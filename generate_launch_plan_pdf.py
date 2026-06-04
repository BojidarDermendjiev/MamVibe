from fpdf import FPDF
import datetime

MAUVE  = (148, 92, 103)
INDIGO = (63, 75, 127)
DARK   = (54, 65, 83)
GRAY   = (107, 114, 128)
WHITE  = (255, 255, 255)
GREEN  = (22, 163, 74)
AMBER  = (180, 120, 20)
RED    = (185, 28, 28)
LIGHT_GREEN = (240, 253, 244)
LIGHT_AMBER = (255, 251, 235)
LIGHT_RED   = (254, 242, 242)

class PDF(FPDF):
    def header(self):
        self.set_fill_color(*MAUVE)
        self.rect(0, 0, 210, 28, "F")
        self.set_font("Helvetica", "B", 15)
        self.set_text_color(*WHITE)
        self.set_xy(12, 7)
        self.cell(0, 8, "MamVibe  -  Pre-Launch Production Checklist", ln=False)
        self.set_font("Helvetica", "", 8)
        self.set_xy(12, 17)
        self.cell(0, 5, f"Generated {datetime.date.today().strftime('%d %B %Y')}  |  Web Production Launch Plan", ln=False)
        self.set_text_color(*DARK)

    def footer(self):
        self.set_y(-12)
        self.set_font("Helvetica", "", 7)
        self.set_text_color(*GRAY)
        self.cell(0, 5, f"MamVibe Launch Plan  |  Page {self.page_no()}  |  Confidential", align="C")

pdf = PDF()
pdf.set_auto_page_break(auto=True, margin=15)
pdf.add_page()
pdf.set_margins(12, 32, 12)

def section_title(title, color=INDIGO):
    pdf.ln(5)
    pdf.set_fill_color(*color)
    pdf.set_text_color(*WHITE)
    pdf.set_font("Helvetica", "B", 10)
    pdf.cell(0, 7, f"  {title}", ln=True, fill=True)
    pdf.set_text_color(*DARK)
    pdf.ln(1)

def checklist_row(num, task, detail, status="blocker", shade=False):
    colors = {
        "blocker": (LIGHT_RED,   RED,   "MUST DO"),
        "should":  (LIGHT_AMBER, AMBER, "SHOULD DO"),
        "done":    (LIGHT_GREEN, GREEN, "DONE"),
    }
    bg, fg, label = colors[status]

    if shade:
        pdf.set_fill_color(248, 248, 252)
    else:
        pdf.set_fill_color(*WHITE)

    row_start_y = pdf.get_y()

    # Number
    pdf.set_font("Helvetica", "B", 8)
    pdf.set_text_color(*GRAY)
    pdf.cell(8, 6, str(num), fill=True)

    # Status badge
    pdf.set_fill_color(*fg)
    pdf.set_text_color(*WHITE)
    pdf.set_font("Helvetica", "B", 6)
    pdf.cell(18, 6, label, fill=True, align="C")

    # Task
    if shade:
        pdf.set_fill_color(248, 248, 252)
    else:
        pdf.set_fill_color(*WHITE)
    pdf.set_text_color(*DARK)
    pdf.set_font("Helvetica", "B", 8.5)
    pdf.cell(70, 6, task[:52], fill=True)

    # Detail
    pdf.set_font("Helvetica", "", 7.5)
    pdf.set_text_color(*GRAY)
    pdf.cell(0, 6, detail[:85], fill=True, ln=True)
    pdf.set_text_color(*DARK)

def note(text):
    pdf.set_font("Helvetica", "I", 7.5)
    pdf.set_text_color(*GRAY)
    pdf.cell(0, 5, text, ln=True)
    pdf.set_text_color(*DARK)
    pdf.ln(1)

# ── SUMMARY BOX ──────────────────────────────────────────────────────────────
pdf.set_fill_color(245, 245, 255)
pdf.set_draw_color(*INDIGO)
pdf.rect(12, 33, 186, 16, "DF")
pdf.set_font("Helvetica", "B", 9)
pdf.set_text_color(*INDIGO)
pdf.set_xy(16, 35)
pdf.cell(0, 5, "Audit Result:  The codebase is PRODUCTION-READY.", ln=True)
pdf.set_font("Helvetica", "", 8.5)
pdf.set_text_color(*DARK)
pdf.set_x(16)
pdf.cell(0, 5, "Everything blocking launch is external service setup and configuration - no new code needed (except GDPR deletion endpoints).", ln=True)
pdf.set_text_color(*DARK)
pdf.ln(3)

# ── SECTION 1: BLOCKERS ──────────────────────────────────────────────────────
section_title("SECTION 1  -  MUST DO BEFORE LAUNCH  (Hard Blockers)", RED)
pdf.set_font("Helvetica", "", 7.5)
pdf.set_text_color(*GRAY)
pdf.cell(0, 5, "  These require your action outside the repo. None require new code.", ln=True)
pdf.set_text_color(*DARK)
pdf.ln(1)

blockers = [
    ("Buy domain + connect to Cloudflare",    "Set A record -> VPS IP, Proxy ON, SSL mode = Full (free TLS)"),
    ("Rent a VPS",                            "Hetzner CX32 min. (4 vCPU / 8 GB). App has no host yet."),
    ("Copy .env.example -> .env, fill values","JWT_SECRET, DB password, Stripe live keys, SMTP, Anthropic, Turnstile, n8n"),
    ("Generate strong secrets",               "openssl rand -hex 32 for JWT_SECRET, IBAN_ENCRYPTION_KEY, N8N_ENCRYPTION_KEY"),
    ("Stripe live account + webhook",         "Switch sk_test_ to sk_live_. Add webhook: https://yourdomain.com/api/payments/webhook"),
    ("Configure SMTP provider",               "Password reset and e-bill emails fail silently without it. Brevo free = 300/day"),
    ("Set ADMIN_SEED_ENABLED=true first deploy", "Creates admin account. Set to false immediately after first run."),
    ("Set ALLOWED_HOSTS to real domain",      "Currently defaults to localhost - production will reject all requests"),
    ("Set FRONTEND_URL to real HTTPS domain", "CORS will block the frontend if this is wrong"),
    ("Register Cloudflare Turnstile site",    "Get real Site Key + Secret Key. Registration form is broken without it"),
]
for i, (task, detail) in enumerate(blockers):
    checklist_row(i + 1, task, detail, status="blocker", shade=i % 2 == 0)

pdf.ln(2)
note("  Priority order: Tasks 1-4 on Day 1 (infrastructure) -> Tasks 5-10 on Day 1 (services) -> First deploy.")

# ── SECTION 2: SHOULD DO ─────────────────────────────────────────────────────
section_title("SECTION 2  -  SHOULD DO BEFORE LAUNCH  (Not Blocking but Important)", AMBER)
pdf.set_font("Helvetica", "", 7.5)
pdf.set_text_color(*GRAY)
pdf.cell(0, 5, "  Missing features or config in the codebase that matter for a professional launch.", ln=True)
pdf.set_text_color(*DARK)
pdf.ln(1)

should_dos = [
    (11, "Database backup strategy",            "No automated backup. Daily pg_dump cron off-server. Data loss risk is real."),
    (12, "GDPR data export/deletion endpoints", "Privacy policy promises these rights. Add POST /api/users/export and DELETE /api/users/me"),
    (13, "Prometheus alerting rules",           "Grafana deployed but no alert rules. You won't be notified if server goes down."),
    (14, "Replace placeholder meta tags",       "GTM-XXXXXXX and google-site-verification still placeholders in index.html"),
    (15, "Submit sitemap to Google Search Console", "sitemap.xml exists but not submitted. Affects crawl speed."),
    (16, "Test full payment flow end-to-end",   "With real Stripe live keys before going public."),
    (17, "n8n workflow backup",                 "Workflows live in Docker volume. Document how to export/restore them."),
]
for i, (num, task, detail) in enumerate(should_dos):
    checklist_row(num, task, detail, status="should", shade=i % 2 == 0)

# ── SECTION 3: ALREADY DONE ──────────────────────────────────────────────────
pdf.add_page()
section_title("SECTION 3  -  ALREADY DONE IN CODEBASE  (No Action Needed)", GREEN)
pdf.set_font("Helvetica", "", 7.5)
pdf.set_text_color(*GRAY)
pdf.cell(0, 5, "  Full audit of backend, frontend, Docker, CI/CD, SEO, and legal confirms these are production-ready.", ln=True)
pdf.set_text_color(*DARK)
pdf.ln(1)

done_items = [
    ("Security",         [
        "HTTPS + HSTS (1 year, includeSubDomains, preload)",
        "CSP + X-Frame-Options + X-Content-Type-Options + Referrer-Policy + Permissions-Policy",
        "Rate limiting: 5 policies (global 200/min, auth 30/min, upload 20/min, assistant 20/min, ebill 3/min)",
        "AllowedHosts lockdown via env var (Host Header Injection protection)",
        "CORS - strict: only FrontendUrl allowed, fail-fast on non-HTTPS in production",
        "JWT: 15-min access tokens + 7-day refresh tokens with rotation",
        "Password policy: 8+ chars, digit, uppercase, lowercase, non-alphanumeric",
        "Cloudflare Turnstile on registration (bot protection)",
        "Blocked user middleware with distributed cache (Redis-backed)",
        "MetricsProtectionMiddleware: /metrics returns 404 for non-internal IPs",
        "DataProtection keys persisted to Docker volume (sessions survive restarts)",
    ]),
    ("Performance",      [
        "Composite DB indexes on AuditLog, ItemModerationLog, Feedback, DoctorReview, ChildFriendlyPlace",
        "Output caching: 30s base policy, 1h for Categories endpoint",
        "Redis distributed cache (IDistributedCache) with in-memory fallback for dev",
        "SignalR Redis backplane (horizontal scaling ready)",
        "PgBouncer connection pooler (transaction mode, 500 max clients, 20 pool size)",
        "Response compression: Brotli + Gzip, enabled for HTTPS",
    ]),
    ("Infrastructure",   [
        "docker-compose.prod.yml with resource limits (API: 2 CPU/1 GB, Postgres: 1 CPU/512 MB)",
        "Multi-stage Dockerfiles, non-root appuser, pre-created dataprotection-keys/ and logs/",
        "GitHub Actions CI/CD: build, lint, TypeScript check, test, Docker build",
        "Prometheus + Loki + Grafana fully wired (monitoring ports hidden in prod)",
        "Health check endpoint (/health) used by Docker healthchecks",
        "Loki log retention: 31 days via compactor config",
        "Doppler integration (doppler.yaml) as .env alternative",
    ]),
    ("Frontend",         [
        "TypeScript build compiles without errors (verified in CI)",
        "All console.log/warn/error guarded by import.meta.env.DEV (stripped in prod builds)",
        "React ErrorBoundary in App.tsx",
        "404 NotFoundPage + all loading states / skeleton components",
        "robots.txt (disallows /admin, /api, /hubs) + sitemap.xml",
        "SEO: usePageSEO on 40+ pages, OG tags, Twitter Card, hreflang EN/BG",
        "Favicon, meta description, OG image in index.html",
        "frontend/.env.local.example for developer onboarding",
    ]),
    ("Legal",            [
        "Privacy Policy page (/privacy) - 11 sections, GDPR Art. 6 legal basis table, data processors table",
        "Terms & Conditions page (/terms) - 14 sections, Bulgarian governing law, escrow rules",
        "Cookie Policy page (/cookies) - single cookie fully documented with all attributes",
        "Cookie consent banner (CookieConsent component in MainLayout)",
        "Footer legal links: Privacy Policy - Terms & Conditions - Cookie Policy (EN + BG)",
    ]),
    ("Payments & Email", [
        "Stripe test mode auto-detection (falls back to test payments if keys not set)",
        "Stripe webhook endpoint with signature verification (HMAC-SHA256)",
        "E-bill issuance: auto-generated on purchase, idempotent, resendable, 5-year retention",
        "TakeANap fiscal receipts integrated (Bulgarian compliance)",
        "Password reset flow complete (forgot-password -> email -> reset-password)",
        "SMTP configured with env var injection (Brevo recommended)",
    ]),
    ("Admin & Data",     [
        "Admin panel: Dashboard, Users, Items, Shipping, Community, Audit Logs",
        "Admin seed: ADMIN_SEED_ENABLED=true creates admin on first deploy",
        "Category + community seed data runs in ALL environments (not dev-only)",
        "Demo users/items seed remains development-only",
        "21+ EF Core migrations, all up to date",
        "API connects through PgBouncer:6432 in production",
    ]),
]

for area, items in done_items:
    pdf.set_fill_color(*DARK)
    pdf.set_text_color(*WHITE)
    pdf.set_font("Helvetica", "B", 8)
    pdf.cell(0, 6, f"   {area}", ln=True, fill=True)
    pdf.set_text_color(*DARK)
    for i, item in enumerate(items):
        pdf.set_fill_color(240, 253, 244) if i % 2 == 0 else pdf.set_fill_color(255, 255, 255)
        pdf.set_font("Helvetica", "", 8)
        pdf.set_text_color(22, 163, 74)
        pdf.cell(10, 5, "  OK", fill=True)
        pdf.set_text_color(*DARK)
        pdf.cell(0, 5, item, ln=True, fill=True)
    pdf.ln(1)

# ── SECTION 4: PRIORITY TIMELINE ─────────────────────────────────────────────
section_title("SECTION 4  -  RECOMMENDED LAUNCH TIMELINE", MAUVE)

timeline = [
    ("Day 1 - Morning",   "Tasks 1-4",  "Rent VPS, connect domain to Cloudflare, copy .env, generate secrets"),
    ("Day 1 - Afternoon", "Tasks 5-10", "Stripe live keys + webhook, SMTP, Turnstile, ALLOWED_HOSTS, FRONTEND_URL"),
    ("Day 1 - Evening",   "First deploy","docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build"),
    ("Day 1 - Evening",   "Task 7",     "Verify admin account created, then set ADMIN_SEED_ENABLED=false and redeploy"),
    ("Day 2",             "Tasks 14-16","Fix placeholder meta tags, submit sitemap, test full payment flow end-to-end"),
    ("Week 1",            "Tasks 11, 13","Set up pg_dump cron backup + Grafana alerting rules"),
    ("Week 2",            "Task 12",    "Implement GDPR data export (GET /api/users/export) and deletion (DELETE /api/users/me)"),
    ("Week 2",            "Task 17",    "Document n8n workflow backup/restore procedure"),
]

w = [34, 22, 130]
pdf.set_fill_color(*DARK)
pdf.set_text_color(*WHITE)
pdf.set_font("Helvetica", "B", 8)
for label, w_ in zip(["When", "Tasks", "Action"], w):
    pdf.cell(w_, 6, f"  {label}", fill=True)
pdf.ln()

for i, (when, tasks, action) in enumerate(timeline):
    pdf.set_fill_color(250, 245, 255) if i % 2 == 0 else pdf.set_fill_color(*WHITE)
    pdf.set_font("Helvetica", "B" if i < 4 else "", 8)
    pdf.set_text_color(*MAUVE if i < 4 else DARK)
    pdf.cell(34, 5.5, f"  {when}", fill=True)
    pdf.set_text_color(*DARK)
    pdf.set_font("Helvetica", "B", 8)
    pdf.cell(22, 5.5, f"  {tasks}", fill=True)
    pdf.set_font("Helvetica", "", 7.5)
    pdf.cell(0, 5.5, action, ln=True, fill=True)

# ── BOTTOM SUMMARY ────────────────────────────────────────────────────────────
pdf.ln(5)
pdf.set_fill_color(*MAUVE)
y = pdf.get_y()
pdf.rect(12, y, 186, 20, "F")
pdf.set_text_color(*WHITE)
pdf.set_font("Helvetica", "B", 10)
pdf.set_xy(14, y + 3)
pdf.cell(0, 6, "  10 blockers to resolve  |  7 items to do week 1-2  |  Everything else is done", ln=True)
pdf.set_font("Helvetica", "", 8.5)
pdf.set_xy(14, y + 11)
pdf.cell(0, 6, "  All blocking items are external service setup. Zero new code required for launch.", ln=True)

out = r"C:\Users\bozhi\Desktop\MamVibe-PreLaunch-Plan.pdf"
pdf.output(out)
print(f"Saved: {out}")
