/**
 * MomVibe Platform — k6 Load Test
 * Simulates 500–1000 concurrent users across three realistic traffic profiles.
 *
 * Profiles (adjustable via env vars):
 *   anonymous_browse  60 % — unauthenticated catalog browsing
 *   auth_users        28 % — logged-in users browsing + interacting
 *   active_traders    12 % — power users: offers, messages, purchase requests
 *
 * Install:
 *   winget install k6                 (Windows)
 *   choco install k6                  (Chocolatey)
 *   brew install k6                   (macOS)
 *
 * Run (local dev — no auth scenarios):
 *   k6 run load-tests/k6/load-test.js
 *
 * Run (with authenticated scenarios — supply a users file):
 *   k6 run --env USERS_FILE=./load-tests/k6/test-users.json load-tests/k6/load-test.js
 *
 * Run against production:
 *   k6 run --env BASE_URL=https://api.momvibe.bg --env USERS_FILE=./test-users.json load-tests/k6/load-test.js
 *
 * Scale peak VUs:
 *   k6 run --env PEAK_ANON=300 --env PEAK_AUTH=150 --env PEAK_TRADER=50 load-tests/k6/load-test.js
 *
 * ⚠  Rate-limit note:
 *   The backend enforces 200 req/min per IP for anonymous traffic.
 *   When all VUs share one IP (local run), ~200+ VUs will receive HTTP 429.
 *   The thresholds below treat 429 as "expected under load" — they are NOT
 *   counted as failures (success_rate), but ARE tracked in rate_limit_hits.
 */

import http from 'k6/http';
import { sleep, check, group } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { SharedArray } from 'k6/data';

// ── Custom metrics ─────────────────────────────────────────────────────────────
const rateLimitHits   = new Counter('rate_limit_hits');
const loginErrors     = new Counter('login_errors');
const successRate     = new Rate('success_rate');
const itemsLoadMs     = new Trend('items_load_ms',   true);
const itemDetailMs    = new Trend('item_detail_ms',  true);
const authMs          = new Trend('auth_ms',         true);
const offersMs        = new Trend('offers_ms',       true);

// ── Config ─────────────────────────────────────────────────────────────────────
const BASE_URL    = __ENV.BASE_URL    || 'http://localhost:5038';
const PEAK_ANON   = parseInt(__ENV.PEAK_ANON   || '600');
const PEAK_AUTH   = parseInt(__ENV.PEAK_AUTH   || '280');
const PEAK_TRADER = parseInt(__ENV.PEAK_TRADER || '120');

// Test user pool — loaded from file if USERS_FILE env var is set, otherwise hardcoded defaults.
// Create real test accounts matching these credentials in your dev DB.
const testUsers = new SharedArray('users', function () {
  if (__ENV.USERS_FILE) return JSON.parse(open(__ENV.USERS_FILE));
  return [
    { email: 'loadtest1@test.com', password: 'LoadTest@123' },
    { email: 'loadtest2@test.com', password: 'LoadTest@123' },
    { email: 'loadtest3@test.com', password: 'LoadTest@123' },
    { email: 'loadtest4@test.com', password: 'LoadTest@123' },
    { email: 'loadtest5@test.com', password: 'LoadTest@123' },
  ];
});

// ── Load stages helper — proportional ramp up / hold / ramp down ───────────────
function stages(peak) {
  return [
    { duration: '30s',  target: Math.round(peak * 0.05) || 1 },  // warm-up
    { duration: '60s',  target: Math.round(peak * 0.30) || 1 },  // ramp 1
    { duration: '60s',  target: Math.round(peak * 0.70) || 1 },  // ramp 2
    { duration: '90s',  target: peak || 1 },                      // full peak
    { duration: '120s', target: peak || 1 },                      // sustain peak
    { duration: '60s',  target: Math.round(peak * 0.40) || 1 },  // ramp down
    { duration: '30s',  target: 0 },                              // drain
  ];
}

// Build scenario map — only include scenarios with peak > 0
function buildScenarios() {
  const s = {};
  if (PEAK_ANON > 0) {
    s.anonymous_browse = {
      executor: 'ramping-vus', exec: 'anonymousBrowse',
      startVUs: 0, stages: stages(PEAK_ANON),
    };
  }
  if (PEAK_AUTH > 0) {
    s.auth_users = {
      executor: 'ramping-vus', exec: 'authenticatedBrowse',
      startTime: '30s', startVUs: 0, stages: stages(PEAK_AUTH),
    };
  }
  if (PEAK_TRADER > 0) {
    s.active_traders = {
      executor: 'ramping-vus', exec: 'activeTrader',
      startTime: '60s', startVUs: 0, stages: stages(PEAK_TRADER),
    };
  }
  return s;
}

// ── Options ────────────────────────────────────────────────────────────────────
export const options = {
  scenarios: buildScenarios(),
  thresholds: {
    // Overall HTTP latency
    http_req_duration:  ['p(95)<1000', 'p(99)<3000'],
    // Allow up to 10 % raw failures (expected: some 429s when running from one IP)
    http_req_failed:    ['rate<0.10'],
    // Custom success rate (excludes 429 from failure count)
    success_rate:       ['rate>0.90'],
    // Endpoint-specific latency
    items_load_ms:      ['p(95)<800'],
    item_detail_ms:     ['p(95)<600'],
    auth_ms:            ['p(95)<500'],
    offers_ms:          ['p(95)<700'],
  },
};

// ── Shared helpers ─────────────────────────────────────────────────────────────

const BASE_HEADERS = {
  'Content-Type': 'application/json',
  Accept: 'application/json',
};

function authHeaders(token) {
  return { ...BASE_HEADERS, Authorization: `Bearer ${token}` };
}

/** Random think time to simulate human pacing */
function think(minSec = 1, maxSec = 3) {
  sleep(randomIntBetween(minSec * 10, maxSec * 10) / 10);
}

/**
 * Record result into custom metrics.
 * 429 Rate-Limited responses are tracked separately but NOT counted as failures
 * because they are expected when many anonymous VUs share a single IP address.
 */
function record(res, trendMetric) {
  const ok      = res.status >= 200 && res.status < 300;
  const limited = res.status === 429;
  const gone    = res.status === 404;

  if (limited) rateLimitHits.add(1);

  // 429 and 404 are "acceptable" for load test purposes
  successRate.add(ok || limited || gone ? 1 : 0);

  if (trendMetric && ok) trendMetric.add(res.timings.duration);
  return ok;
}

/** POST /api/v1/auth/login — returns access token or null */
function login(user) {
  const res = http.post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ email: user.email, password: user.password }),
    { headers: BASE_HEADERS, tags: { endpoint: 'auth_login' } },
  );
  authMs.add(res.timings.duration);

  if (res.status === 200) {
    try {
      const body = res.json();
      return body.accessToken || body.token || null;
    } catch { return null; }
  }

  loginErrors.add(1);
  return null;
}

/** Extract a random item id from a paginated list response */
function pickItemId(res) {
  if (res.status !== 200) return null;
  try {
    const body  = res.json();
    const items = body.data || body.items || (Array.isArray(body) ? body : null);
    if (Array.isArray(items) && items.length > 0) {
      return items[randomIntBetween(0, items.length - 1)].id;
    }
  } catch { /* ignore */ }
  return null;
}

// ── Scenario 1: Anonymous Browsing ────────────────────────────────────────────
// Represents guest users scrolling the catalog, opening product pages.
export function anonymousBrowse() {
  group('anonymous_browse', () => {

    // Categories endpoint — should always be instant (1-hour output cache)
    group('categories', () => {
      const res = http.get(`${BASE_URL}/api/v1/categories`, {
        headers: BASE_HEADERS, tags: { endpoint: 'categories' },
      });
      check(res, { 'categories 200': (r) => r.status === 200 });
      record(res, null);
    });

    think(0.5, 1.5);

    // Paginated item list
    let itemId = null;
    group('items_list', () => {
      const page = randomIntBetween(1, 10);
      const res = http.get(
        `${BASE_URL}/api/v1/items?page=${page}&pageSize=20`,
        { headers: BASE_HEADERS, tags: { endpoint: 'items_list' } },
      );
      check(res, { 'items 200': (r) => r.status === 200 || r.status === 429 });
      record(res, itemsLoadMs);
      itemId = pickItemId(res);
    });

    think(1, 2.5);

    if (itemId) {
      group('item_detail', () => {
        const res = http.get(
          `${BASE_URL}/api/v1/items/${itemId}`,
          { headers: BASE_HEADERS, tags: { endpoint: 'item_detail' } },
        );
        check(res, { 'item detail ok': (r) => [200, 404, 429].includes(r.status) });
        record(res, itemDetailMs);
      });

      think(0.5, 1.5);

      // Increment view counter (rate limited: 30/min/IP)
      group('item_view', () => {
        const res = http.post(
          `${BASE_URL}/api/v1/items/${itemId}/view`,
          null,
          { headers: BASE_HEADERS, tags: { endpoint: 'item_view' } },
        );
        record(res, null);
      });
    }

    // 1 in 5 users checks public stats
    if (randomIntBetween(1, 5) === 1) {
      think(0.3, 0.8);
      group('public_stats', () => {
        const res = http.get(`${BASE_URL}/api/v1/stats/public`, {
          headers: BASE_HEADERS, tags: { endpoint: 'stats_public' },
        });
        record(res, null);
      });
    }
  });

  think(1, 3);
}

// ── Scenario 2: Authenticated Browse ─────────────────────────────────────────
// Represents active members browsing, liking items, checking messages.
export function authenticatedBrowse() {
  const user  = testUsers[(__VU - 1) % testUsers.length];
  const token = login(user);

  if (!token) {
    think(5, 10); // back off if login fails (e.g. user not in DB)
    return;
  }

  group('auth_browse', () => {
    // Verify session
    group('get_me', () => {
      const res = http.get(`${BASE_URL}/api/v1/auth/me`, {
        headers: authHeaders(token), tags: { endpoint: 'auth_me' },
      });
      check(res, { 'get me 200': (r) => r.status === 200 });
      record(res, null);
    });

    think(0.5, 1.5);

    // Paginated catalog
    let itemId = null;
    group('items_list_auth', () => {
      const page = randomIntBetween(1, 8);
      const res = http.get(
        `${BASE_URL}/api/v1/items?page=${page}&pageSize=20`,
        { headers: authHeaders(token), tags: { endpoint: 'items_list_auth' } },
      );
      check(res, { 'items auth ok': (r) => r.status === 200 || r.status === 429 });
      record(res, itemsLoadMs);
      itemId = pickItemId(res);
    });

    think(1, 2);

    if (itemId) {
      group('item_detail_auth', () => {
        const res = http.get(
          `${BASE_URL}/api/v1/items/${itemId}`,
          { headers: authHeaders(token), tags: { endpoint: 'item_detail_auth' } },
        );
        check(res, { 'item detail auth ok': (r) => [200, 404, 429].includes(r.status) });
        record(res, itemDetailMs);
      });

      // 1 in 3 users likes the item
      if (randomIntBetween(1, 3) === 1) {
        think(0.5, 1);
        group('like_item', () => {
          const res = http.post(
            `${BASE_URL}/api/v1/items/${itemId}/like`,
            null,
            { headers: authHeaders(token), tags: { endpoint: 'like_item' } },
          );
          record(res, null);
        });
      }
    }

    think(1, 2);

    // Check own listings
    group('my_items', () => {
      const res = http.get(
        `${BASE_URL}/api/v1/users/my-items?page=1&pageSize=10`,
        { headers: authHeaders(token), tags: { endpoint: 'my_items' } },
      );
      check(res, { 'my items 200': (r) => r.status === 200 || r.status === 429 });
      record(res, null);
    });

    think(1, 2);

    // Inbox
    group('messages', () => {
      const res = http.get(`${BASE_URL}/api/v1/messages`, {
        headers: authHeaders(token), tags: { endpoint: 'messages' },
      });
      check(res, { 'messages 200': (r) => r.status === 200 || r.status === 429 });
      record(res, null);
    });
  });

  think(2, 4);
}

// ── Scenario 3: Active Trader ─────────────────────────────────────────────────
// Represents power users managing offers, purchase requests, and invoices.
export function activeTrader() {
  const user  = testUsers[(__VU - 1) % testUsers.length];
  const token = login(user);

  if (!token) {
    think(5, 10);
    return;
  }

  const hdrs = { headers: authHeaders(token) };

  group('active_trader', () => {
    // Offers inbox
    group('offers_received', () => {
      const res = http.get(`${BASE_URL}/api/v1/offers/received`, {
        ...hdrs, tags: { endpoint: 'offers_received' },
      });
      check(res, { 'offers received ok': (r) => r.status === 200 || r.status === 429 });
      record(res, offersMs);
    });

    think(0.5, 1.5);

    group('offers_sent', () => {
      const res = http.get(`${BASE_URL}/api/v1/offers/sent`, {
        ...hdrs, tags: { endpoint: 'offers_sent' },
      });
      check(res, { 'offers sent ok': (r) => r.status === 200 || r.status === 429 });
      record(res, offersMs);
    });

    think(0.5, 1);

    // Purchase requests — both sides
    group('purchase_requests_buyer', () => {
      const res = http.get(`${BASE_URL}/api/v1/purchase-requests/buyer`, {
        ...hdrs, tags: { endpoint: 'pr_buyer' },
      });
      check(res, { 'pr buyer ok': (r) => r.status === 200 || r.status === 429 });
      record(res, null);
    });

    think(0.3, 0.8);

    group('purchase_requests_seller', () => {
      const res = http.get(`${BASE_URL}/api/v1/purchase-requests/seller`, {
        ...hdrs, tags: { endpoint: 'pr_seller' },
      });
      check(res, { 'pr seller ok': (r) => r.status === 200 || r.status === 429 });
      record(res, null);
    });

    think(1, 2);

    // Browse for potential offers
    group('browse_for_offers', () => {
      const page = randomIntBetween(1, 5);
      const res = http.get(
        `${BASE_URL}/api/v1/items?page=${page}&pageSize=20`,
        { ...hdrs, tags: { endpoint: 'items_trader' } },
      );
      record(res, itemsLoadMs);
    });

    think(1, 2);

    // E-bills / invoices
    group('ebills', () => {
      const res = http.get(`${BASE_URL}/api/v1/ebills`, {
        ...hdrs, tags: { endpoint: 'ebills' },
      });
      check(res, { 'ebills ok': (r) => r.status === 200 || r.status === 429 });
      record(res, null);
    });

    // 1 in 4 traders checks their ratings
    if (randomIntBetween(1, 4) === 1) {
      think(0.5, 1);
      group('ratings_received', () => {
        const res = http.get(`${BASE_URL}/api/v1/ratings/received`, {
          ...hdrs, tags: { endpoint: 'ratings' },
        });
        record(res, null);
      });
    }
  });

  think(2, 5);
}
