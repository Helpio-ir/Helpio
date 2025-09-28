#!/usr/bin/env bash
set -Eeuo pipefail

if [[ ${EUID:-$(id -u)} -ne 0 ]]; then
  echo "[FATAL] این اسکریپت باید با کاربر ریشه (root) اجرا شود." >&2
  exit 1
fi

umask 027

log() {
  local level="$1"; shift
  printf '[%s] [%s] %s\n' "$(date --iso-8601=seconds)" "$level" "$*"
}

trap 'log FATAL "خطا در اجرای اسکریپت در خط ${LINENO}."' ERR

update_system_packages() {
  log INFO "به‌روزرسانی مخازن و بسته‌های سیستم"
  apt-get update
  DEBIAN_FRONTEND=noninteractive apt-get -y upgrade
  apt-get -y autoremove
}

install_base_packages() {
  log INFO "نصب بسته‌های پایه مورد نیاز"
  local packages=(curl git ufw nginx certbot python3-certbot-nginx fail2ban ca-certificates gnupg lsb-release)
  DEBIAN_FRONTEND=noninteractive apt-get install -y "${packages[@]}"
}

configure_firewall() {
  log INFO "پیکربندی فایروال UFW"
  ufw allow OpenSSH
  ufw allow http
  ufw allow https
  ufw --force enable
}

require_env() {
  local var_name="$1";
  if [[ -z "${!var_name:-}" ]]; then
    log FATAL "متغیر محیطی ${var_name} تنظیم نشده است.";
    exit 1;
  fi
}

# ----- متغیرهای پیکربندی -----
APP_USER="${HELPIO_APP_USER:-helpio}"
APP_GROUP="$APP_USER"
APP_ROOT="${HELPIO_APP_ROOT:-/opt/helpio}"
SRC_DIR="$APP_ROOT/src"
COMPOSE_DIR="$SRC_DIR/deploy/docker"
COMPOSE_FILE="$COMPOSE_DIR/docker-compose.yml"
COMPOSE_ENV_FILE="$COMPOSE_DIR/.env"
ENV_DIR="/etc/helpio"
LOG_DIR="/var/log/helpio"
SQL_CONTAINER_NAME="${HELPIO_SQL_CONTAINER_NAME:-helpio-sql}"
SQL_VOLUME_NAME="${HELPIO_SQL_VOLUME_NAME:-helpio-sql-data}"
SQL_DB_NAME="${HELPIO_SQL_DB_NAME:-HelpioDB}"
SQL_APP_USER="${HELPIO_SQL_APP_USER:-helpio_app}"
SQL_PORT="${HELPIO_SQL_PORT:-1433}"
DOTNET_EF_VERSION="${HELPIO_DOTNET_EF_VERSION:-9.0.4}"
TLS_ENABLED="${HELPIO_ENABLE_TLS:-true}"
API_DOMAIN="${HELPIO_API_DOMAIN:-}"
DASH_DOMAIN="${HELPIO_DASHBOARD_DOMAIN:-}"
WEB_DOMAIN="${HELPIO_WEB_DOMAIN:-}"
GIT_REPO="${HELPIO_GIT_REPO:-https://github.com/Helpio-ir/Helpio.git}"
GIT_BRANCH="${HELPIO_GIT_BRANCH:-}"
CERTBOT_EMAIL="${HELPIO_CERTBOT_EMAIL:-}"
APP_ENVIRONMENT="${HELPIO_ENVIRONMENT:-Production}"
API_PORT="${HELPIO_API_INTERNAL_PORT:-5000}"
DASH_PORT="${HELPIO_DASH_INTERNAL_PORT:-5002}"
WEB_PORT="${HELPIO_WEB_INTERNAL_PORT:-5003}"

require_env HELPIO_SQL_SA_PASSWORD
require_env HELPIO_SQL_APP_PASSWORD
if [[ -z "$GIT_REPO" ]]; then
  log FATAL "آدرس مخزن گیت (HELPIO_GIT_REPO) باید مشخص شود."; exit 1;
fi

if [[ "$TLS_ENABLED" == "true" ]]; then
  if [[ -z "$API_DOMAIN" || -z "$DASH_DOMAIN" || -z "$WEB_DOMAIN" ]]; then
    log FATAL "برای فعال‌سازی TLS باید متغیرهای HELPIO_API_DOMAIN، HELPIO_DASHBOARD_DOMAIN و HELPIO_WEB_DOMAIN تنظیم شوند."; exit 1;
  fi
  if [[ -z "$CERTBOT_EMAIL" ]]; then
    log FATAL "برای دریافت گواهی TLS آدرس ایمیل (HELPIO_CERTBOT_EMAIL) لازم است."; exit 1;
  fi
fi

SQL_SA_PASSWORD="$HELPIO_SQL_SA_PASSWORD"
SQL_APP_PASSWORD="$HELPIO_SQL_APP_PASSWORD"

API_BASE_URL="${HELPIO_API_BASE_URL:-}"
if [[ -z "$API_BASE_URL" ]]; then
  if [[ "$TLS_ENABLED" == "true" ]]; then
    API_BASE_URL="https://${API_DOMAIN}/"
  else
    API_BASE_URL="http://127.0.0.1:${API_PORT}/"
  fi
fi

create_system_user() {
  if ! id "$APP_USER" &>/dev/null; then
    log INFO "ایجاد کاربر سیستمی ${APP_USER}"
    useradd --system --create-home --shell /usr/sbin/nologin "$APP_USER"
  else
    log INFO "کاربر ${APP_USER} از قبل وجود دارد."
  fi
}

ensure_directories() {
  log INFO "ایجاد ساختار دایرکتوری"
  mkdir -p "$SRC_DIR" "$ENV_DIR" "$LOG_DIR/app" "$LOG_DIR/nginx"

  chown -R "$APP_USER":"$APP_GROUP" "$APP_ROOT"
  chown "$APP_USER":"$APP_GROUP" "$LOG_DIR/app"
  chown www-data:adm "$LOG_DIR/nginx" || true
  chmod 750 "$LOG_DIR/app" "$LOG_DIR/nginx"

  chown root:"$APP_GROUP" "$ENV_DIR"
  chmod 750 "$ENV_DIR"
}

install_docker() {
  if ! command -v docker &>/dev/null; then
    log INFO "نصب Docker CE"
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor >/etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" >/etc/apt/sources.list.d/docker.list
    apt-get update
    DEBIAN_FRONTEND=noninteractive apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
  else
    log INFO "Docker از قبل نصب شده است."
  fi
  systemctl enable --now docker
  usermod -aG docker "$APP_USER" || true
}

resolve_git_branch() {
  if [[ -n "$GIT_BRANCH" ]]; then
    if git ls-remote --exit-code --heads "$GIT_REPO" "$GIT_BRANCH" >/dev/null 2>&1; then
      log INFO "استفاده از شاخه مشخص‌شده: ${GIT_BRANCH}"
      return
    fi
    log FATAL "شاخه ${GIT_BRANCH} در مخزن ${GIT_REPO} یافت نشد. مقدار HELPIO_GIT_BRANCH را اصلاح کنید."
    exit 1
  fi

  log INFO "تشخیص شاخه پیش‌فرض مخزن ${GIT_REPO}"
  local symref=""
  symref=$(git ls-remote --symref "$GIT_REPO" HEAD 2>/dev/null | awk '/^ref:/ {sub("refs/heads/", "", $2); print $2; exit}' || true)

  if [[ -n "$symref" ]]; then
    GIT_BRANCH="$symref"
  else
    for candidate in main master; do
      if git ls-remote --exit-code --heads "$GIT_REPO" "$candidate" >/dev/null 2>&1; then
        GIT_BRANCH="$candidate"
        break
      fi
    done
  fi

  if [[ -z "$GIT_BRANCH" ]]; then
    log FATAL "عدم توانایی در تشخیص شاخه مخزن؛ لطفاً HELPIO_GIT_BRANCH را به‌صورت صریح مشخص کنید."
    exit 1
  fi

  log INFO "شاخه تشخیص داده‌شده: ${GIT_BRANCH}"
}

clone_or_update_repo() {
  resolve_git_branch
  if [[ ! -d "$SRC_DIR/.git" ]]; then
    log INFO "کلون کردن مخزن ${GIT_REPO} (شاخه ${GIT_BRANCH})"
    sudo -u "$APP_USER" -H git clone --depth 1 --branch "$GIT_BRANCH" "$GIT_REPO" "$SRC_DIR"
  else
    log INFO "به‌روزرسانی مخزن موجود (شاخه ${GIT_BRANCH})"
    sudo -u "$APP_USER" -H git -C "$SRC_DIR" fetch --all --prune
    if ! sudo -u "$APP_USER" -H git -C "$SRC_DIR" rev-parse --verify "origin/${GIT_BRANCH}" >/dev/null 2>&1; then
      log FATAL "شاخه origin/${GIT_BRANCH} در مخزن یافت نشد. لطفاً HELPIO_GIT_BRANCH را به مقدار صحیح تنظیم کنید."
      exit 1
    fi
    sudo -u "$APP_USER" -H git -C "$SRC_DIR" checkout "$GIT_BRANCH"
    sudo -u "$APP_USER" -H git -C "$SRC_DIR" pull --ff-only
  fi
}

run_migrations() {
  log INFO "اجرای مایگریشن‌های پایگاه‌داده در کانتینر"
  local conn="Server=${SQL_CONTAINER_NAME},1433;Database=${SQL_DB_NAME};User Id=${SQL_APP_USER};Password=${SQL_APP_PASSWORD};Encrypt=True;TrustServerCertificate=True;"
  local ef_commands="set -Eeuo pipefail
dotnet tool list --global | grep -q 'dotnet-ef' || dotnet tool install --global dotnet-ef --version ${DOTNET_EF_VERSION}
export PATH=\"\$PATH:/root/.dotnet/tools\"
dotnet restore src/Presentation/Helpio.API/Helpio.API.csproj
dotnet ef database update --project src/Infrastructure/Helpio.Infrastructure/Helpio.Infrastructure.csproj --startup-project src/Presentation/Helpio.API/Helpio.API.csproj --context Helpio.Ir.Infrastructure.Data.ApplicationDbContext"

  sudo -u "$APP_USER" -H env ConnectionStrings__DefaultConnection="${conn}" \
    docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" run --rm --entrypoint bash migrator -c "$ef_commands"
}

generate_compose_env_file() {
  log INFO "ایجاد فایل محیطی برای Docker Compose"
  if [[ ! -d "$COMPOSE_DIR" ]]; then
    log FATAL "پیکربندی Docker Compose در مخزن یافت نشد."; exit 1;
  fi
  if [[ ! -f "$COMPOSE_FILE" ]]; then
    log FATAL "فایل Docker Compose (${COMPOSE_FILE}) یافت نشد."; exit 1;
  fi

  cat >"$COMPOSE_ENV_FILE" <<EOF
HELPIO_SQL_SA_PASSWORD=${SQL_SA_PASSWORD}
HELPIO_SQL_APP_PASSWORD=${SQL_APP_PASSWORD}
SQL_DB_NAME=${SQL_DB_NAME}
SQL_APP_USER=${SQL_APP_USER}
HELPIO_ENVIRONMENT=${APP_ENVIRONMENT}
API_PORT=${API_PORT}
DASH_PORT=${DASH_PORT}
WEB_PORT=${WEB_PORT}
API_BASE_URL=${API_BASE_URL}
SQL_CONTAINER_NAME=${SQL_CONTAINER_NAME}
SQL_VOLUME_NAME=${SQL_VOLUME_NAME}
SQL_PORT=${SQL_PORT}
HOST_PROJECT_DIR=${SRC_DIR}
MIGRATIONS_CONNECTION_STRING=Server=${SQL_CONTAINER_NAME},1433;Database=${SQL_DB_NAME};User Id=${SQL_APP_USER};Password=${SQL_APP_PASSWORD};Encrypt=True;TrustServerCertificate=True;
EOF

  chown "$APP_USER":"$APP_GROUP" "$COMPOSE_ENV_FILE"
  chmod 600 "$COMPOSE_ENV_FILE"
}

persist_runtime_configuration() {
  log INFO "ثبت تنظیمات محیطی در ${ENV_DIR}"
  local runtime_file="${ENV_DIR}/runtime.env"

  cat >"$runtime_file" <<EOF
# Helpio provisioning snapshot (خودکار تولید شده - شامل رمزها نیست)
APP_ROOT=${APP_ROOT}
SRC_DIR=${SRC_DIR}
COMPOSE_FILE=${COMPOSE_FILE}
APP_ENVIRONMENT=${APP_ENVIRONMENT}
TLS_ENABLED=${TLS_ENABLED}
API_DOMAIN=${API_DOMAIN}
DASH_DOMAIN=${DASH_DOMAIN}
WEB_DOMAIN=${WEB_DOMAIN}
API_BASE_URL=${API_BASE_URL}
API_PORT=${API_PORT}
DASH_PORT=${DASH_PORT}
WEB_PORT=${WEB_PORT}
SQL_DB_NAME=${SQL_DB_NAME}
SQL_APP_USER=${SQL_APP_USER}
SQL_CONTAINER_NAME=${SQL_CONTAINER_NAME}
SQL_VOLUME_NAME=${SQL_VOLUME_NAME}
SQL_PORT=${SQL_PORT}
GIT_REPO=${GIT_REPO}
GIT_BRANCH=${GIT_BRANCH}
DOTNET_EF_VERSION=${DOTNET_EF_VERSION}
EOF

  chown root:"$APP_GROUP" "$runtime_file"
  chmod 640 "$runtime_file"
}

validate_helpio_repo_layout() {
  log INFO "اعتبارسنجی ساختار مخزن Helpio"
  local required_paths=(
    "$SRC_DIR/Helpio.sln"
    "$COMPOSE_FILE"
    "$COMPOSE_DIR/api/Dockerfile"
    "$COMPOSE_DIR/dashboard/Dockerfile"
  "$COMPOSE_DIR/web/Dockerfile"
    "$SRC_DIR/src/Presentation/Helpio.API/Helpio.API.csproj"
    "$SRC_DIR/src/Presentation/Helpio.Dashboard/Helpio.Dashboard.csproj"
  "$SRC_DIR/src/Presentation/Helpio.web/Helpio.web.csproj"
    "$SRC_DIR/src/Infrastructure/Helpio.Infrastructure/Helpio.Infrastructure.csproj"
  )

  for path in "${required_paths[@]}"; do
    if [[ ! -e "$path" ]]; then
      log FATAL "مسیر مورد انتظار ${path} یافت نشد. ساختار مخزن با Helpio سازگار نیست.";
      exit 1
    fi
  done
}

start_sql_service() {
  log INFO "راه‌اندازی سرویس SQL Server با Docker Compose"

  local sql_already_running="false"
  local existing_container_id=""
  existing_container_id=$(sudo -u "$APP_USER" -H docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" ps -q sqlserver || true)

  if [[ -n "$existing_container_id" ]]; then
    sql_already_running="true"
    log INFO "کانتینر SQL Server از قبل در حال اجراست (ID: ${existing_container_id})."
  else
    docker volume create "$SQL_VOLUME_NAME" >/dev/null
    sudo -u "$APP_USER" -H docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" up -d sqlserver
  fi

  log INFO "منتظر آماده شدن SQL Server"
  local attempt
  for attempt in {1..30}; do
    if docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" exec -T \
      -e SA_PASSWORD="$SQL_SA_PASSWORD" sqlserver bash -lc '
set -Eeuo pipefail
if [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
  SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
else
  exit 127
fi
"$SQLCMD" -C -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1"
' &>/dev/null; then
      log INFO "SQL Server آماده است."
      break
    fi
    sleep 3
    if [[ $attempt -eq 30 ]]; then
      if [[ "$sql_already_running" == "true" ]]; then
        log FATAL "عدم توانایی در اتصال به SQL Server موجود؛ به نظر می‌رسد رمز SA در کانتینر فعلی با مقدار HELPIO_SQL_SA_PASSWORD متفاوت است. مقدار درست را تنظیم کنید یا کانتینر و ولوم ${SQL_VOLUME_NAME} را حذف کنید."; exit 1;
      fi
      log FATAL "عدم توانایی در اتصال به SQL Server"; exit 1;
    fi
  done

  docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" exec -T \
    -e SA_PASSWORD="$SQL_SA_PASSWORD" sqlserver bash -lc '
set -Eeuo pipefail
if [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
  SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
else
  echo "sqlcmd binary not found inside SQL Server container" >&2
  exit 127
fi
"$SQLCMD" -C -S localhost -U sa -P "$SA_PASSWORD"
' <<SQL
IF DB_ID('${SQL_DB_NAME}') IS NULL
BEGIN
  CREATE DATABASE [${SQL_DB_NAME}];
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = '${SQL_APP_USER}')
BEGIN
  CREATE LOGIN [${SQL_APP_USER}] WITH PASSWORD = '${SQL_APP_PASSWORD}', CHECK_POLICY = ON, CHECK_EXPIRATION = ON;
END;
GO
USE [${SQL_DB_NAME}];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '${SQL_APP_USER}')
BEGIN
  CREATE USER [${SQL_APP_USER}] FOR LOGIN [${SQL_APP_USER}];
END;
GO
ALTER ROLE db_owner ADD MEMBER [${SQL_APP_USER}];
GO
SQL
}

build_application_images() {
  log INFO "بیلد ایمیج‌های برنامه (API، Dashboard، Web)"
  sudo -u "$APP_USER" -H docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" build helpio-api helpio-dashboard helpio-web
}

start_application_containers() {
  log INFO "راه‌اندازی کانتینرهای API، Dashboard و Web"
  sudo -u "$APP_USER" -H docker compose -f "$COMPOSE_FILE" --env-file "$COMPOSE_ENV_FILE" up -d helpio-api helpio-dashboard helpio-web
}

configure_nginx() {
  log INFO "پیکربندی Nginx به عنوان Reverse Proxy"
  local api_conf="/etc/nginx/sites-available/helpio-api.conf"
  local dash_conf="/etc/nginx/sites-available/helpio-dashboard.conf"
  local web_conf="/etc/nginx/sites-available/helpio-web.conf"
  local api_server_name="${API_DOMAIN:-_}"
  local dash_server_name="${DASH_DOMAIN:-_}"
  local web_server_name="${WEB_DOMAIN:-_}"

  cat >"$api_conf" <<EOF
server {
    listen 80;
  server_name ${api_server_name};

    location /healthz {
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_pass http://127.0.0.1:${API_PORT}/health;
    }

    location / {
        proxy_pass http://127.0.0.1:${API_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }

    access_log ${LOG_DIR}/nginx-api.access.log;
    error_log ${LOG_DIR}/nginx-api.error.log;
}
EOF

  cat >"$dash_conf" <<EOF
server {
    listen 80;
  server_name ${dash_server_name};

    location / {
        proxy_pass http://127.0.0.1:${DASH_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }

    access_log ${LOG_DIR}/nginx-dashboard.access.log;
    error_log ${LOG_DIR}/nginx-dashboard.error.log;
}
EOF

  cat >"$web_conf" <<EOF
server {
    listen 80;
  server_name ${web_server_name};

    location / {
        proxy_pass http://127.0.0.1:${WEB_PORT};
        proxy_http_version 1.1;
  proxy_set_header Upgrade \$http_upgrade;
  proxy_set_header Connection keep-alive;
  proxy_set_header Host \$host;
  proxy_set_header X-Real-IP \$remote_addr;
  proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
  proxy_set_header X-Forwarded-Proto \$scheme;
    }

    access_log ${LOG_DIR}/nginx-web.access.log;
    error_log ${LOG_DIR}/nginx-web.error.log;
}
EOF

  ln -sf "$api_conf" /etc/nginx/sites-enabled/helpio-api.conf
  ln -sf "$dash_conf" /etc/nginx/sites-enabled/helpio-dashboard.conf
  ln -sf "$web_conf" /etc/nginx/sites-enabled/helpio-web.conf

  rm -f /etc/nginx/sites-enabled/default || true
  nginx -t
  systemctl reload nginx
}

obtain_certificates() {
  if [[ "$TLS_ENABLED" != "true" ]]; then
    log INFO "دریافت گواهی TLS غیرفعال است."
    return
  fi

  log INFO "دریافت گواهی‌های Let's Encrypt"
  certbot --nginx --non-interactive --agree-tos --redirect --no-eff-email \
    -m "$CERTBOT_EMAIL" -d "$API_DOMAIN" --deploy-hook "systemctl reload nginx"
  certbot --nginx --non-interactive --agree-tos --redirect --no-eff-email \
    -m "$CERTBOT_EMAIL" -d "$DASH_DOMAIN" --deploy-hook "systemctl reload nginx"
  certbot --nginx --non-interactive --agree-tos --redirect --no-eff-email \
    -m "$CERTBOT_EMAIL" -d "$WEB_DOMAIN" --deploy-hook "systemctl reload nginx"
  systemctl reload nginx
}

harden_ssh_fail2ban() {
  log INFO "فعال‌سازی Fail2ban برای SSH"
  cat >/etc/fail2ban/jail.d/defaults-debian.conf <<'EOF'
[sshd]
enabled = true
port    = ssh
logpath = %(sshd_log)s
backend = systemd
maxretry = 5
findtime = 15m
bantime = 1h
EOF
  systemctl enable --now fail2ban
}

main() {
  update_system_packages
  install_base_packages
  configure_firewall
  create_system_user
  ensure_directories
  install_docker
  clone_or_update_repo
  validate_helpio_repo_layout
  generate_compose_env_file
  persist_runtime_configuration
  start_sql_service
  run_migrations
  build_application_images
  start_application_containers
  configure_nginx
  obtain_certificates
  harden_ssh_fail2ban
  log INFO "نصب به پایان رسید. لطفاً لاگ‌ها و وضعیت سرویس‌ها را بررسی کنید."
}

main "$@"
