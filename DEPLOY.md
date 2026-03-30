# MamVibe — Production Deployment Guide

## 1. Infrastructure You Need

| What           | Recommendation                  | Cost         |
|----------------|---------------------------------|--------------|
| VPS server     | Hetzner CX22 (2 vCPU / 4 GB)   | ~€4/month    |
| Domain         | Namecheap or Cloudflare Domains | ~€10/year    |
| DNS + SSL      | Cloudflare (free plan)          | Free         |
| SMTP email     | Brevo (free up to 300/day)      | Free         |

---

## 2. One-Time Server Setup

```bash
# SSH into your VPS (Ubuntu 22.04)
apt update && apt upgrade -y
apt install -y docker.io docker-compose-plugin git curl

# Allow your user to run Docker without sudo
usermod -aG docker $USER
newgrp docker
```

---

## 3. Cloudflare DNS + SSL (Free HTTPS)

1. Add your domain to Cloudflare (free plan)
2. Create an **A record**: `@` → your VPS IP address, **Proxy ON** (orange cloud)
3. Create an **A record**: `www` → same VPS IP, **Proxy ON**
4. SSL/TLS mode → set to **Full**

Cloudflare terminates HTTPS for free. Your server only needs to listen on port 80.

---

## 4. Clone the Repo on the VPS

```bash
git clone https://github.com/BojidarDermendjiev/MamVibe.git
cd MamVibe
```

---

## 5. Create the .env File

```bash
cp .env.example .env
nano .env   # fill in every value
```

Generate secrets:
```bash
# JWT secret (min 32 chars)
openssl rand -base64 48

# IBAN encryption key (exactly 32 bytes)
openssl rand -base64 32

# Database password
openssl rand -hex 24
```

---

## 6. External Services — Setup Checklist

### Google OAuth
1. Go to https://console.cloud.google.com → APIs & Services → Credentials
2. Create OAuth 2.0 Client ID → type: Web application
3. Authorised JavaScript origins: `https://mamvibe.com`
4. Authorised redirect URIs: `https://mamvibe.com`
5. Copy Client ID and Client Secret → `.env`

### Stripe
1. Go to https://dashboard.stripe.com → Developers → API keys
2. Copy `sk_live_...` and `pk_live_...` → `.env`
3. Go to Developers → Webhooks → Add endpoint:
   - **Endpoint URL:** `https://mamvibe.com/api/payments/webhook`
   - **Event:** `checkout.session.completed`
4. Copy the `whsec_...` signing secret → `STRIPE_WEBHOOK_SECRET` in `.env`

### Cloudflare Turnstile
1. Go to https://dash.cloudflare.com → Turnstile → Add site
2. Domain: `mamvibe.com`
3. Copy Site Key and Secret Key → `.env`

### Brevo (SMTP email)
1. Go to https://app.brevo.com → SMTP & API → SMTP
2. Create a new SMTP key
3. Host: `smtp-relay.brevo.com`, Port: `587`
4. Copy username and SMTP key → `.env`

### Econt
- Change base URL from demo to production: `https://ee.econt.com/services`
  (already set in `docker-compose.prod.yml`)

---

## 7. All API Endpoints That Must Work in Production

| Method | Endpoint                              | Needs                        |
|--------|---------------------------------------|------------------------------|
| POST   | /api/auth/register                    | SMTP (welcome email)         |
| POST   | /api/auth/login                       | —                            |
| POST   | /api/auth/refresh                     | —                            |
| POST   | /api/auth/google                      | Google OAuth                 |
| POST   | /api/auth/forgot-password             | SMTP                         |
| POST   | /api/auth/reset-password              | —                            |
| GET    | /api/items                            | —                            |
| POST   | /api/items                            | —                            |
| PUT    | /api/items/{id}                       | —                            |
| DELETE | /api/items/{id}                       | —                            |
| POST   | /api/photos/upload                    | Local disk (Docker volume)   |
| POST   | /api/payments/checkout/{itemId}       | Stripe API                   |
| POST   | /api/payments/webhook                 | Stripe → your server         |
| POST   | /api/payments/onspot/{itemId}         | —                            |
| POST   | /api/payments/booking/{itemId}        | —                            |
| POST   | /api/payments/donation/checkout       | Stripe API                   |
| GET    | /api/payments/my-payments             | —                            |
| POST   | /api/purchaserequests                 | SignalR notify               |
| POST   | /api/purchaserequests/{id}/accept     | SignalR notify               |
| POST   | /api/purchaserequests/{id}/decline    | SignalR notify               |
| POST   | /api/shipping/create                  | Econt / Speedy / BoxNow      |
| POST   | /api/shipping/calculate               | Econt / Speedy / BoxNow      |
| GET    | /api/shipping/offices                 | Econt / Speedy / BoxNow      |
| GET    | /api/shipping/{id}/track              | Econt / Speedy / BoxNow      |
| POST   | /api/turnstile/verify                 | Cloudflare Turnstile         |
| GET    | /api/users/profile                    | —                            |
| PUT    | /api/users/profile                    | —                            |
| GET    | /api/messages                         | —                            |
| POST   | /api/messages                         | —                            |
| GET    | /api/categories                       | —                            |
| POST   | /api/feedback                         | —                            |
| GET    | /api/admin/users                      | Admin only                   |
| POST   | /api/admin/users/{id}/block           | Admin only                   |
| GET    | /api/admin/dashboard                  | Admin only                   |
| WS     | /hubs/chat                            | SignalR WebSocket             |
| GET    | /health                               | Health check                  |

---

## 8. Deploy

```bash
# First deploy
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build

# Check logs
docker compose logs -f api
docker compose logs -f frontend

# Check health
curl http://localhost/health
```

---

## 9. Every Future Update

```bash
cd /root/MamVibe
git pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

---

## 10. Post-Deploy Checklist

```
[ ] https://mamvibe.com loads the frontend
[ ] https://mamvibe.com/health returns "healthy"
[ ] Register a test user → check welcome email arrives
[ ] Google login works
[ ] Create a listing + upload a photo
[ ] Stripe test checkout completes → check webhook fires (docker logs api)
[ ] Switch Stripe to LIVE mode (sk_live_ / pk_live_)
[ ] SignalR chat works (browser DevTools → no WebSocket errors)
[ ] Purchase request flow works end-to-end
[ ] Admin panel accessible at /admin with seeded admin account
[ ] Set ADMIN_SEED_ENABLED=false in .env and redeploy
```

---

## 11. Important Warnings

| Warning | Detail |
|---------|--------|
| IBAN_ENCRYPTION_KEY | Never change after first deploy — existing encrypted IBANs will become unreadable |
| ADMIN_SEED_ENABLED | Set to `false` after first deploy |
| Stripe webhook secret | Regenerate `whsec_...` every time you register a new webhook endpoint |
| Photo storage | Photos stored in Docker volume — back it up; consider migrating to S3/R2 for multi-server setup |
| Econt URL | `docker-compose.prod.yml` uses `ee.econt.com` (production), not `demo.econt.com` |
| .env file | Never commit `.env` to git — it contains all secrets |
