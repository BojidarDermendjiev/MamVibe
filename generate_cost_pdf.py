from fpdf import FPDF
import datetime

MAUVE  = (148, 92, 103)
INDIGO = (63, 75, 127)
DARK   = (54, 65, 83)
GRAY   = (107, 114, 128)
WHITE  = (255, 255, 255)

class PDF(FPDF):
    def header(self):
        self.set_fill_color(*MAUVE)
        self.rect(0, 0, 210, 28, "F")
        self.set_font("Helvetica", "B", 16)
        self.set_text_color(*WHITE)
        self.set_xy(12, 8)
        self.cell(0, 8, "MamVibe  -  Infrastructure Cost Summary", ln=False)
        self.set_font("Helvetica", "", 8)
        self.set_xy(12, 17)
        self.cell(0, 5, f"Generated {datetime.date.today().strftime('%d %B %Y')}", ln=False)
        self.set_text_color(*DARK)

    def footer(self):
        self.set_y(-12)
        self.set_font("Helvetica", "", 7)
        self.set_text_color(*GRAY)
        self.cell(0, 5, f"MamVibe Cost Summary  |  Page {self.page_no()}  |  Prices approximate - verify with providers", align="C")

pdf = PDF()
pdf.set_auto_page_break(auto=True, margin=15)
pdf.add_page()
pdf.set_margins(12, 32, 12)

def section_title(title):
    pdf.ln(4)
    pdf.set_fill_color(*INDIGO)
    pdf.set_text_color(*WHITE)
    pdf.set_font("Helvetica", "B", 10)
    pdf.cell(0, 7, f"  {title}", ln=True, fill=True)
    pdf.set_text_color(*DARK)
    pdf.ln(1)

def table_header(cols, widths):
    pdf.set_fill_color(*DARK)
    pdf.set_text_color(*WHITE)
    pdf.set_font("Helvetica", "B", 8)
    for col, w in zip(cols, widths):
        pdf.cell(w, 6, col, border=0, fill=True, align="L")
    pdf.ln()
    pdf.set_text_color(*DARK)

def table_row(cells, widths, shade=False, bold_last=False):
    pdf.set_fill_color(245, 247, 250) if shade else pdf.set_fill_color(*WHITE)
    for i, (cell, w) in enumerate(zip(cells, widths)):
        pdf.set_font("Helvetica", "B" if (bold_last and i == len(cells)-1) else "", 8)
        pdf.cell(w, 5.5, str(cell), border=0, fill=True, align="L")
    pdf.ln()

def note(text):
    pdf.set_font("Helvetica", "I", 7.5)
    pdf.set_text_color(*GRAY)
    pdf.cell(0, 5, text, ln=True)
    pdf.set_text_color(*DARK)
    pdf.ln(1)

# ── 1. FIXED COSTS ────────────────────────────────────────────────────────────
section_title("1.  Fixed Monthly Costs")
cols = ["Service", "Purpose", "Plan / Tier", "Est. Cost/mo"]
w = [40, 68, 54, 24]
table_header(cols, w)
rows = [
    ("Domain (.com)", "mamvibe.com registration", "Annual, amortised", "~EU1.20"),
    ("VPS - Hetzner CX32", "Entire stack (launch)", "4 vCPU / 8 GB RAM", "~EU8-10"),
    ("VPS - Hetzner CX42", "Production headroom", "8 vCPU / 16 GB RAM", "~EU17"),
    ("n8n (self-hosted)", "18 automation workflows", "Runs on VPS", "EU0"),
    ("n8n Cloud (optional)", "18 automation workflows", "Starter cloud plan", "~$20"),
    ("Doppler", "Secrets management", "Free (1 project)", "EU0"),
    ("Cloudflare Turnstile", "Bot protection", "Free <= 1M req/mo", "EU0"),
    ("GitHub Actions", "CI/CD pipeline", "Free (2,000 min/mo)", "EU0"),
    ("Expo EAS Build", "Mobile builds", "Free (30 builds/mo)", "EU0"),
    ("Expo EAS Production", "Unlimited mobile builds", "Production plan", "~$29"),
    ("PostgreSQL + Redis", "Database + cache", "Self-hosted on VPS", "EU0"),
    ("Prometheus + Loki + Grafana", "Full observability stack", "Self-hosted on VPS", "EU0"),
    ("PgBouncer + Nginx", "Pooler + reverse proxy", "Self-hosted on VPS", "EU0"),
]
for i, r in enumerate(rows):
    table_row(r, w, shade=i % 2 == 0)
note("  * Cloudflare, GitHub Actions, Doppler free tiers are generous enough for early-stage production.")

# ── 2. VARIABLE COSTS ─────────────────────────────────────────────────────────
section_title("2.  Variable / Usage-Based Costs")
cols2 = ["Service", "Purpose", "Pricing Model", "Est. Monthly"]
w2 = [36, 54, 68, 28]
table_header(cols2, w2)
rows2 = [
    ("Stripe", "Card payments + wallet top-ups", "1.5% + EU0.25 per EU card charge", "GMV-based"),
    ("Anthropic Claude", "AI listing, moderation, pricing", "Per token (input + output)", "~EU10-40"),
    ("SMTP / Email", "Transactional emails", "Free tier / Brevo EU25/mo", "EU0-25"),
    ("TakeANap", "Bulgarian fiscal receipts", "Subscription / per-receipt", "Contact them"),
    ("Econt / Speedy / BoxNow", "Courier API access", "Free API - buyers pay per parcel", "EU0 for API"),
    ("Google OAuth", "Sign in with Google", "Free for standard usage", "EU0"),
]
for i, r in enumerate(rows2):
    table_row(r, w2, shade=i % 2 == 0)
note("  * Stripe fees are paid on each transaction - not a fixed platform overhead.")

# ── 3. CLAUDE API DETAIL ──────────────────────────────────────────────────────
section_title("3.  Claude API Pricing Detail")
cols3 = ["Model", "Input / 1M tokens", "Output / 1M tokens", "Best Use Case"]
w3 = [56, 32, 34, 64]
table_header(cols3, w3)
rows3 = [
    ("claude-haiku-4-5-20251001", "$0.80", "$4.00", "High-volume moderation (cheapest)"),
    ("claude-sonnet-4-6  <- default", "$3.00", "$15.00", "Listing suggestions + price hints"),
    ("claude-opus-4-7", "$15.00", "$75.00", "Complex reasoning (avoid for bulk)"),
]
for i, r in enumerate(rows3):
    table_row(r, w3, shade=i % 2 == 0)
note("  * 500 listings/mo moderated on Haiku + Sonnet for suggestions: roughly EU15-30/mo.")

# ── 4. SCENARIO TOTALS ────────────────────────────────────────────────────────
section_title("4.  Monthly Total by Stage")
cols4 = ["Stage", "Server", "n8n", "Email", "Claude", "Expo", "TOTAL"]
w4 = [44, 22, 18, 16, 18, 16, 52]
table_header(cols4, w4)
rows4 = [
    ("Dev / Early launch", "EU8", "EU0", "EU0", "~EU10", "EU0", "~EU19-22 / month"),
    ("Production (growth)", "EU17", "$20", "$20", "~EU30", "$29", "~EU100-120 / month"),
    ("Scale (multi-instance)", "2xEU17", "$20", "$25", "~EU50", "$29", "~EU180-200 / month"),
]
for i, r in enumerate(rows4):
    table_row(r, w4, shade=i % 2 == 0, bold_last=True)

pdf.ln(2)
note("  + Stripe fees on top: at EU10,000 GMV/mo (~1.5% + EU0.25/tx) = ~EU175-200 in payment fees.")
note("  + Multi-instance scaling already supported via Redis SignalR backplane - just add another VPS.")

# ── 5. FREE SERVICES ──────────────────────────────────────────────────────────
section_title("5.  What You Get for Free")
free_items = [
    "PostgreSQL 16  -  self-hosted on VPS",
    "Redis 7 (distributed cache + SignalR backplane)  -  self-hosted",
    "PgBouncer connection pooler  -  self-hosted",
    "Prometheus + Loki + Grafana (full observability)  -  self-hosted",
    "Nginx reverse proxy  -  self-hosted",
    "Cloudflare Turnstile bot protection  -  free tier (1M checks/mo)",
    "GitHub Actions CI/CD  -  free tier (2,000 min/mo)",
    "Econt / Speedy / BoxNow API access  -  free (buyers pay per parcel)",
    "Google OAuth  -  free for standard marketplace usage",
    "n8n 18 workflows  -  free when self-hosted on VPS",
]
for i, item in enumerate(free_items):
    pdf.set_fill_color(240, 253, 244) if i % 2 == 0 else pdf.set_fill_color(255, 255, 255)
    pdf.set_font("Helvetica", "", 8.5)
    pdf.set_text_color(22, 163, 74)
    pdf.cell(10, 5.5, "  OK", fill=True)
    pdf.set_text_color(*DARK)
    pdf.cell(0, 5.5, item, ln=True, fill=True)

# ── SUMMARY BOX ───────────────────────────────────────────────────────────────
pdf.ln(5)
pdf.set_fill_color(*MAUVE)
y = pdf.get_y()
pdf.rect(12, y, 186, 14, "F")
pdf.set_text_color(*WHITE)
pdf.set_font("Helvetica", "B", 11)
pdf.set_xy(12, y + 3)
pdf.cell(0, 8, "  Bottom line:  ~EU20-25 / month to run the full MamVibe stack at launch", ln=True)

out = r"C:\Users\bozhi\Downloads\MamVibe-Cost-Summary.pdf"
pdf.output(out)
print(f"Saved: {out}")
