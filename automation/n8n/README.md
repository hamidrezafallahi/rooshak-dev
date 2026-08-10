# SEO Automation Pack (n8n + OnlineShop SeoOps API)

Hybrid model for this shop:

- **n8n** orchestrates schedules, LLM reasoning, and alerts
- **ASP.NET `SeoOps` API** reads inventory / probes public SEO URLs
- **Human gate** before publish, meta apply, or content refresh

This is intentional. Full auto-publish SEO is not the goal.

## What’s included

| Workflow file | Schedule (Asia/Tehran) | Job | Human gate |
|---|---|---|---|
| `ai-blog-seo-daily.workflow.json` | Daily 08:00 | Generate blog draft | Activate in admin |
| `seo-site-health-daily.workflow.json` | Daily 07:00 | Probe sitemap/robots/home/blog/API | Alert only |
| `seo-weekly-digest.workflow.json` | Mon 09:00 | Inventory + LLM priorities | Review digest |
| `seo-meta-optimizer-weekly.workflow.json` | Tue 10:00 | Weak title/meta suggestions | Apply manually |
| `seo-internal-links-weekly.workflow.json` | Wed 11:00 | Internal link suggestions | Edit content manually |
| `seo-content-refresh-weekly.workflow.json` | Thu 12:00 | Refresh briefs for stale blogs | Edit + republish manually |

## Architecture

```text
Cron (n8n)
  → Login as ContentEditor bot
  → SeoOps / Blogs API
  → optional LLM summarize/suggest
  → report via Telegram (recommended) and/or SEO_ALERT_WEBHOOK_URL
  → YOU review in Admin / n8n Executions
```

## Reporting (recommended)

**Best for this project: Telegram bot** (better than WhatsApp for ops alerts).

| Channel | Pros | Cons | Verdict |
|---|---|---|---|
| **Telegram bot** | Free, reliable in Iran, simple API, works from VPS | Need bot + chat id once | **Use this** |
| Slack/Discord webhook | Nice for teams | Less common personal ops | Optional secondary |
| WhatsApp Cloud API | Familiar UX | Meta Business setup, paid, fragile | Avoid for now |
| Email | Universal | Noisy, slow for ops | Optional weekly only |

Setup:

1. Talk to `@BotFather` → create bot → copy token
2. Send any message to your bot
3. Open `https://api.telegram.org/bot<TOKEN>/getUpdates` → copy `chat.id`
4. Put in `.env`:

```bash
TELEGRAM_BOT_TOKEN=123456:ABC...
TELEGRAM_CHAT_ID=6987...
# optional fallback / team channel
SEO_ALERT_WEBHOOK_URL=
```

5. Recreate n8n: `docker compose -f docker-compose.dev.yml up -d --force-recreate n8n`

Every workflow ends with **Build Notify Targets → Notify Report**.

## Slug policy (AI blog)

Persian URL slugs like `ظروف-کریستال-پذیرایی` are rejected/collide often.

Workflow now:
- maps topics to ASCII slugs (`crystal-hospitality-ware`, …)
- allocates unique suffix (`-2`, `-3`, …)
- **locks** that slug (LLM cannot override it)

## Local setup

### 1) Env

Copy from `.env.example` and set at least:

```bash
LLM_API_KEY=...                 # or OPENROUTER_API_KEY / GROQ_API_KEY
LLM_API_URL=https://api.groq.com/openai/v1/chat/completions
LLM_MODEL=llama-3.3-70b-versatile

CONTENT_BOT_EMAIL=content-bot@onlineshop.local
CONTENT_BOT_PASSWORD=ContentBot@123
API_BASE_URL=http://backend:8080/api

# Reachable FROM the backend container (for SeoOps health probes)
SeoOps__SitePublicUrl=http://nginx
SeoOps__ApiPublicUrl=http://backend:8080

# Reports: Telegram (recommended) and/or generic webhook JSON { "text": "..." }
TELEGRAM_BOT_TOKEN=
TELEGRAM_CHAT_ID=
SEO_ALERT_WEBHOOK_URL=
SEO_STALE_DAYS=90
```

### 2) Start stack

```bash
docker compose -f docker-compose.dev.yml up -d
# after C# SeoOps changes:
docker compose -f docker-compose.dev.yml up -d --build backend
```

Open:

- Site: `http://localhost`
- API: `http://localhost:8080`
- n8n: `http://localhost:5678`

### 3) Import all workflows

In n8n → **Workflows → Import from File**, import every `*.workflow.json` in this folder.

Then open each workflow and click **Test workflow** once.

Activate only after a successful manual run.

## Backend API used by automations

All require `ContentEditor` / `Admin` / `SuperAdmin` JWT (bot account is seeded).

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/SeoOps/health` | Probe public SEO endpoints |
| GET | `/api/SeoOps/snapshot?staleDays=90&take=20` | Weekly inventory digest |
| GET | `/api/SeoOps/meta-audit?take=30` | Weak title/meta blogs |
| GET | `/api/SeoOps/refresh-candidates?staleDays=90` | Stale active blogs |
| GET | `/api/SeoOps/internal-links/{blogId}` | Link suggestions |
| GET | `/api/Blogs/getslugs?includeInactive=true` | Draft-safe slug de-dupe |
| POST | `/api/Blogs/validate-content` | Quality gates for AI drafts |
| POST | `/api/Blogs` | Create inactive draft |

## Recommended weekly human checklist

1. **Mon** — read Weekly Digest, pick top 3 priorities
2. **Tue** — apply best meta suggestions in admin (or reject)
3. **Wed** — add 2–5 internal links from suggestions
4. **Thu** — refresh 1 stale article from briefs
5. **Daily** — if health alert fires, fix broken SEO URL first
6. **Daily AI blog** — review draft tone/facts → activate only if good

## What is intentionally NOT automated

- Auto-publish blog/product pages
- Mass outreach / link spam
- Blind title overwrite without review
- Claiming Google Search Console actions without OAuth setup

### Optional next upgrade (GSC)

When you are ready, add Google Search Console OAuth in n8n and feed clicks/CTR into:

- Weekly Digest
- Meta Optimizer prioritization

Until then, this pack already covers the highest-ROI ops loop for this codebase.

## Quality / safety rules

- Drafts stay `IsActive=false` until a human activates them
- Meta/link/refresh workflows only **suggest**
- Alerts are optional via `SEO_ALERT_WEBHOOK_URL`
- If webhook is empty, results still appear in n8n Executions

## Troubleshooting

| Symptom | Fix |
|---|---|
| Health fails on frontend URLs | Set `SeoOps__SitePublicUrl=http://nginx` (Docker network), rebuild/restart backend |
| Login fails | Check `CONTENT_BOT_*` and that ContentEditor seed ran |
| LLM 401 | Set `LLM_API_KEY` / `OPENROUTER_API_KEY` / `GROQ_API_KEY`, recreate n8n |
| Duplicate blog slugs | Use `getslugs?includeInactive=true` (fixed in API) |
| No alerts | Set `SEO_ALERT_WEBHOOK_URL` or read n8n execution output |

## Bot account (seeded)

| Field | Default |
|---|---|
| Email | `content-bot@onlineshop.local` |
| Password | `ContentBot@123` |
| Role | `ContentEditor` |
