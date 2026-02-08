#!/usr/bin/env bash
set -euo pipefail

# ──────────────────────────────────────────────
# Longstone — Deploy to UGREEN NAS
# ──────────────────────────────────────────────

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

info()  { echo -e "${BLUE}[INFO]${NC}  $*"; }
ok()    { echo -e "${GREEN}[OK]${NC}    $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC}  $*"; }
err()   { echo -e "${RED}[ERROR]${NC} $*" >&2; }
die()   { err "$@"; exit 1; }

# Load .env if present
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

if [[ -f "$REPO_ROOT/.env" ]]; then
    info "Loading .env from $REPO_ROOT/.env"
    set -a
    # shellcheck source=/dev/null
    source "$REPO_ROOT/.env"
    set +a
fi

# Configuration (override via .env or environment)
NAS_HOST="${NAS_HOST:?NAS_HOST is required — set in .env or environment}"
NAS_USER="${NAS_USER:?NAS_USER is required — set in .env or environment}"
NAS_DEPLOY_PATH="${NAS_DEPLOY_PATH:-/volume1/docker/longstone}"
IMAGE_NAME="${IMAGE_NAME:-longstone}"

GIT_SHA="$(git -C "$REPO_ROOT" rev-parse --short HEAD)"
IMAGE_TAG="${IMAGE_NAME}:${GIT_SHA}"
IMAGE_FILE="${IMAGE_NAME}-${GIT_SHA}.tar"

info "Deploying ${IMAGE_TAG} to ${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_PATH}"
echo ""

# ── Step 1: Build Docker image ──────────────
info "Building Docker image..."
docker build -t "$IMAGE_TAG" -t "${IMAGE_NAME}:latest" "$REPO_ROOT" \
    || die "Docker build failed"
ok "Image built: ${IMAGE_TAG}"

# ── Step 2: Save image to tarball ────────────
info "Saving image to ${IMAGE_FILE}..."
docker save "$IMAGE_TAG" "${IMAGE_NAME}:latest" -o "/tmp/${IMAGE_FILE}" \
    || die "Docker save failed"
IMAGE_SIZE=$(du -h "/tmp/${IMAGE_FILE}" | cut -f1)
ok "Image saved: ${IMAGE_SIZE}"

# ── Step 3: Copy image to NAS ────────────────
info "Copying image to NAS..."
scp "/tmp/${IMAGE_FILE}" "${NAS_USER}@${NAS_HOST}:/tmp/${IMAGE_FILE}" \
    || die "SCP failed — check NAS_HOST and NAS_USER"
ok "Image copied to NAS"

# ── Step 4: Copy compose + config files ──────
info "Syncing deployment files..."
ssh "${NAS_USER}@${NAS_HOST}" "mkdir -p ${NAS_DEPLOY_PATH}" \
    || die "Failed to create deploy directory on NAS"
scp "$REPO_ROOT/docker-compose.hub.yml" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_PATH}/docker-compose.yml" \
    || die "Failed to copy docker-compose.yml"
scp "$REPO_ROOT/tailscale-serve.json" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_PATH}/tailscale-serve.json" \
    || die "Failed to copy tailscale-serve.json"
if [[ -f "$REPO_ROOT/.env" ]]; then
    scp "$REPO_ROOT/.env" "${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_PATH}/.env" \
        || warn "Failed to copy .env — you may need to create it manually on the NAS"
fi
ok "Deployment files synced"

# ── Step 5: Load image on NAS ────────────────
info "Loading image on NAS..."
ssh "${NAS_USER}@${NAS_HOST}" "docker load -i /tmp/${IMAGE_FILE} && rm /tmp/${IMAGE_FILE}" \
    || die "Docker load failed on NAS"
ok "Image loaded on NAS"

# ── Step 6: Start services ───────────────────
info "Starting services..."
ssh "${NAS_USER}@${NAS_HOST}" "cd ${NAS_DEPLOY_PATH} && docker compose up -d" \
    || die "Docker compose up failed"
ok "Services started"

# ── Step 7: Health check ─────────────────────
info "Waiting for health check..."
MAX_RETRIES=10
RETRY_INTERVAL=5

for i in $(seq 1 $MAX_RETRIES); do
    if ssh "${NAS_USER}@${NAS_HOST}" "docker compose -f ${NAS_DEPLOY_PATH}/docker-compose.yml ps --format json" 2>/dev/null \
        | grep -q '"Health":"healthy"'; then
        ok "Health check passed"
        break
    fi
    if [[ $i -eq $MAX_RETRIES ]]; then
        warn "Health check did not pass after ${MAX_RETRIES} attempts"
        warn "Check logs: ssh ${NAS_USER}@${NAS_HOST} 'cd ${NAS_DEPLOY_PATH} && docker compose logs longstone'"
        break
    fi
    info "  Attempt ${i}/${MAX_RETRIES} — waiting ${RETRY_INTERVAL}s..."
    sleep $RETRY_INTERVAL
done

# ── Cleanup local tarball ────────────────────
rm -f "/tmp/${IMAGE_FILE}"

echo ""
ok "Deployment complete!"
info "Image:  ${IMAGE_TAG}"
info "NAS:    ${NAS_USER}@${NAS_HOST}:${NAS_DEPLOY_PATH}"
info "Logs:   ssh ${NAS_USER}@${NAS_HOST} 'cd ${NAS_DEPLOY_PATH} && docker compose logs -f longstone'"
