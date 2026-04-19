#!/bin/bash
# MamVibe — One-time VPS setup script
# Tested on Ubuntu 24.04 LTS (Hetzner CX22)
#
# Run as root immediately after provisioning:
#   curl -fsSL https://raw.githubusercontent.com/BojidarDermendjiev/MamVibe/main/scripts/server-setup.sh | bash
# OR copy it manually and run:
#   bash server-setup.sh

set -euo pipefail

# ── Config ────────────────────────────────────────────────────────────
APP_USER="mamvibe"
DEPLOY_DIR="/srv/mamvibe"
REPO_URL="https://github.com/BojidarDermendjiev/MamVibe.git"
# ─────────────────────────────────────────────────────────────────────

if [[ $EUID -ne 0 ]]; then
  echo "Error: run this script as root (sudo bash server-setup.sh)"
  exit 1
fi

echo ""
echo "======================================================"
echo "  MamVibe — Server Setup"
echo "  $(date)"
echo "======================================================"
echo ""

# ── 1. System update ──────────────────────────────────────────────────
echo "[1/7] Updating system packages..."
apt-get update -qq
apt-get upgrade -y -qq
apt-get install -y -qq curl git ufw fail2ban

# ── 2. Create non-root deploy user ───────────────────────────────────
echo "[2/7] Creating user '$APP_USER'..."
if id "$APP_USER" &>/dev/null; then
  echo "      User '$APP_USER' already exists — skipping."
else
  adduser --disabled-password --gecos "" "$APP_USER"
  usermod -aG sudo "$APP_USER"
  echo "      User '$APP_USER' created and added to sudo."
fi

# Copy root's authorized_keys to the new user so your SSH key works
if [[ -f /root/.ssh/authorized_keys ]]; then
  mkdir -p "/home/$APP_USER/.ssh"
  cp /root/.ssh/authorized_keys "/home/$APP_USER/.ssh/authorized_keys"
  chown -R "$APP_USER:$APP_USER" "/home/$APP_USER/.ssh"
  chmod 700 "/home/$APP_USER/.ssh"
  chmod 600 "/home/$APP_USER/.ssh/authorized_keys"
  echo "      SSH authorized_keys copied from root."
fi

# ── 3. Harden SSH ────────────────────────────────────────────────────
echo "[3/7] Hardening SSH..."
SSHD_CONFIG="/etc/ssh/sshd_config"

# Disable root login
sed -i 's/^#\?PermitRootLogin.*/PermitRootLogin no/' "$SSHD_CONFIG"
# Disable password auth (key-only)
sed -i 's/^#\?PasswordAuthentication.*/PasswordAuthentication no/' "$SSHD_CONFIG"
# Disable empty passwords
sed -i 's/^#\?PermitEmptyPasswords.*/PermitEmptyPasswords no/' "$SSHD_CONFIG"

systemctl reload sshd
echo "      Root login disabled, password auth disabled."

# ── 4. Firewall (UFW) ────────────────────────────────────────────────
echo "[4/7] Configuring firewall..."
ufw --force reset
ufw default deny incoming
ufw default allow outgoing
ufw allow 22/tcp   comment "SSH"
ufw allow 80/tcp   comment "HTTP  (Cloudflare → Nginx)"
ufw allow 443/tcp  comment "HTTPS (future direct TLS)"
ufw --force enable
echo "      UFW enabled: ports 22, 80, 443 open."

# ── 5. Fail2ban ──────────────────────────────────────────────────────
echo "[5/7] Configuring fail2ban..."
cat > /etc/fail2ban/jail.local << 'EOF'
[DEFAULT]
bantime  = 3600
findtime = 600
maxretry = 5

[sshd]
enabled = true
port    = ssh
EOF

systemctl enable fail2ban
systemctl restart fail2ban
echo "      Fail2ban enabled — SSH brute-force protection active."

# ── 6. Docker ────────────────────────────────────────────────────────
echo "[6/7] Installing Docker..."
if command -v docker &>/dev/null; then
  echo "      Docker already installed — skipping."
else
  curl -fsSL https://get.docker.com | sh
  echo "      Docker installed."
fi

usermod -aG docker "$APP_USER"
systemctl enable docker
echo "      '$APP_USER' added to docker group."

# ── 7. Clone repo ────────────────────────────────────────────────────
echo "[7/7] Cloning MamVibe repository..."
if [[ -d "$DEPLOY_DIR/.git" ]]; then
  echo "      Repo already exists at $DEPLOY_DIR — pulling latest."
  git -C "$DEPLOY_DIR" pull
else
  git clone "$REPO_URL" "$DEPLOY_DIR"
  echo "      Cloned to $DEPLOY_DIR"
fi

# Fix ownership so APP_USER can work in the deploy dir
chown -R "$APP_USER:$APP_USER" "$DEPLOY_DIR"

# Make all scripts executable
find "$DEPLOY_DIR/scripts" -name "*.sh" -exec chmod +x {} \;
echo "      Scripts marked executable."

# ── Done ─────────────────────────────────────────────────────────────
echo ""
echo "======================================================"
echo "  Setup complete!"
echo "======================================================"
echo ""
echo "Next steps:"
echo "  1. Log out of root and SSH in as '$APP_USER'"
echo "  2. cd $DEPLOY_DIR"
echo "  3. cp .env.example .env && nano .env   (fill in credentials)"
echo "  4. ./scripts/setup-cron.sh             (install backup cron)"
echo "  5. docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build"
echo ""
echo "Point your Cloudflare A record → $(curl -4s ifconfig.me 2>/dev/null || echo '<this-server-ip>')"
echo ""
