// Quick smoke test — run the full suite instead:
//   k6 run load-tests/k6/load-test.js
//   .\load-tests\run.ps1 -PeakAnon 600 -PeakAuth 280 -PeakTrader 120

import http from 'k6/http';
import { sleep, check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5038';

export const options = {
  vus: 10,
  duration: '30s',
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed:   ['rate<0.01'],
  },
};

export default function () {
  const res = http.get(`${BASE_URL}/api/v1/items?page=1&pageSize=20`);
  check(res, {
    'status 200':    (r) => r.status === 200,
    'response <1s':  (r) => r.timings.duration < 1000,
  });
  sleep(1);
}
