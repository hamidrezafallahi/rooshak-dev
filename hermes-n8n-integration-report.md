# Hermes + Telegram + OpenRouter + n8n Integration
## Complete Setup and Current Status Report

**Date:** 2026-08-14  
**Environment:** Linux VPS / Ubuntu  
**Hermes:** Installed directly on VPS  
**n8n:** Docker container  
**LLM Provider:** OpenRouter  
**Primary Objective:** Allow Hermes to interact with the local n8n instance and eventually discover and execute n8n workflows through Telegram.

---

# 1. Executive Summary

The overall objective is to build an AI automation system on the VPS where:

1. The user communicates with Hermes through Telegram.
2. Hermes uses an LLM through OpenRouter for reasoning.
3. Hermes has access to the VPS terminal and other tools.
4. Hermes can communicate with the locally running n8n instance.
5. Hermes can inspect available n8n workflows.
6. Hermes can execute appropriate workflows.
7. The workflow results can be returned to the user through Telegram.

The intended architecture is:

```text
                         ┌──────────────────┐
                         │     Telegram     │
                         │      User        │
                         └────────┬─────────┘
                                  │
                                  ▼
                         ┌──────────────────┐
                         │  Hermes Agent    │
                         │   VPS / Linux    │
                         └────────┬─────────┘
                                  │
                    ┌─────────────┴─────────────┐
                    │                           │
                    ▼                           ▼
           ┌──────────────────┐       ┌──────────────────┐
           │    OpenRouter    │       │   Local Tools    │
           │    Free Model    │       │    Terminal      │
           └──────────────────┘       └────────┬─────────┘
                                               │
                                               ▼
                                      ┌──────────────────┐
                                      │       n8n        │
                                      │     :5678        │
                                      └────────┬─────────┘
                                               │
                                               ▼
                                      ┌──────────────────┐
                                      │ Store / Backend  │
                                      │ APIs / Workflows  │
                                      └──────────────────┘


                                      The infrastructure is mostly working.

The current blocker is n8n REST API authentication.

2. Original Goal

The original idea was to have an AI agent running on the VPS that can be controlled through Telegram.

For example, the owner could send something like:

Check today's orders.

or:

Run the workflow that checks failed payments.

or:

Connect to n8n and tell me what workflows are available.

Hermes should then reason about the request and use n8n when appropriate.

The desired final flow is:

Telegram
   │
   │ User request
   ▼
Hermes
   │
   │ Reasoning
   ▼
OpenRouter
   │
   │ Tool decision
   ▼
n8n REST API
   │
   │ Execute workflow
   ▼
Workflow
   │
   ▼
Shop / Backend / Database / External APIs
   │
   ▼
Workflow Result
   │
   ▼
Hermes
   │
   ▼
Telegram
3. Current VPS Architecture

The VPS contains both the shop infrastructure and Hermes.

Current architecture:

Ubuntu VPS
│
├── Hermes
│   └── Installed directly on Linux
│
├── Docker
│   │
│   ├── shop-n8n-prod
│   │   └── n8nio/n8n:latest
│   │
│   ├── shop-nginx-prod
│   │   └── nginx:1.27-alpine
│   │
│   ├── shop-frontend-prod
│   │   └── Docker image from Docker Hub
│   │
│   ├── shop-backend-prod
│   │   └── Docker image from Docker Hub
│   │
│   ├── shop-postgres-prod
│   │   └── postgres:16-alpine
│   │
│   ├── shop-certbot-renew
│   │   └── certbot/certbot:latest
│   │
│   └── nginx_reloader
│
└── Network / Ports
    ├── 80    → nginx
    ├── 443   → nginx
    ├── 8080  → backend (localhost only)
    └── 5678  → n8n (localhost only)
4. Hermes Installation

Hermes is installed directly on the VPS.

It is not running inside Docker.

Therefore Hermes can directly access services exposed on the VPS localhost interface.

For n8n:

http://127.0.0.1:5678

or:

http://localhost:5678

Both point to the Docker-published n8n port.

This means there is no need to expose n8n publicly just so Hermes can access it.

5. n8n Installation

n8n is running inside Docker.

Current container:

Container:
shop-n8n-prod


Image:
n8nio/n8n:latest


Port:
127.0.0.1:5678 -> 5678/tcp

This was confirmed with:

docker ps --format 'table {{.Names}}\t{{.Image}}\t{{.Ports}}'

Relevant output:

shop-n8n-prod    n8nio/n8n:latest    127.0.0.1:5678->5678/tcp
6. n8n Version

The n8n version was checked using:

docker exec shop-n8n-prod n8n --version

Result:

2.34.5

Therefore the currently running n8n version is:

n8n 2.34.5
7. n8n Network Configuration

The Docker port mapping is:

127.0.0.1:5678 -> 5678

This means n8n is accessible from:

VPS itself:
http://127.0.0.1:5678

but is NOT directly exposed as:

http://PUBLIC_VPS_IP:5678

This is desirable from a security perspective.

The intended architecture is:

Hermes
   │
   ▼
127.0.0.1:5678
   │
   ▼
Docker
   │
   ▼
n8n

There is no need to expose port 5678 to the Internet for Hermes.

8. Remote Access to n8n UI

Because n8n is bound to VPS localhost, the n8n UI can be accessed remotely using an SSH tunnel.

From the local computer:

ssh -L 127.0.0.1:5678:127.0.0.1:5678 root@YOUR_VPS_IP

Keep this SSH connection open.

Then open locally:

http://127.0.0.1:5678

The connection path is:

Local Computer
     │
     │ SSH tunnel
     ▼
VPS 127.0.0.1:5678
     │
     ▼
Docker n8n

This allows access to the n8n UI without publicly exposing port 5678.

9. n8n Environment Configuration

The following n8n environment values were inspected:

N8N_LISTEN_ADDRESS=0.0.0.0
N8N_HOST=localhost
WEBHOOK_URL=http://127.0.0.1:5678/
N8N_BLOCK_ENV_ACCESS_IN_NODE=false
N8N_WEBHOOK_URL=http://127.0.0.1:5678/
N8N_SECURE_COOKIE=false
N8N_PORT=5678
N8N_PROTOCOL=http
N8N_RELEASE_TYPE=stable

The license key was present but intentionally redacted.

The n8n container itself is listening on:

0.0.0.0:5678

inside the container.

Docker exposes it only on:

127.0.0.1:5678

on the VPS.

10. Initial Groq Configuration

The original Hermes configuration used Groq.

The main model was:

model:
  default: openai/gpt-oss-120b
  provider: custom:groq

Groq endpoint:

https://api.groq.com/openai/v1

The selected model was:

openai/gpt-oss-120b
11. Groq Problem

The Groq request failed with:

HTTP 413

The important error was:

Request too large for model `openai/gpt-oss-120b`


Limit:
8000 TPM


Requested:
19095 tokens

Hermes also reported:

Context overflow


Auto-compaction is disabled

The problem was therefore not basic connectivity.

The problem was the request/token size exceeding the available Groq token-per-minute limit.

12. Decision to Abandon Groq

Because the Groq setup was causing request-size/TPM problems, the decision was made to stop using Groq.

The target became:

OpenRouter
   +
Free model

The goal was to have:

No direct Groq dependency.
Free/low-cost model availability.
Easy model switching.
OpenAI-compatible API.
Compatibility with Hermes.
13. OpenRouter Configuration

Hermes was changed to use OpenRouter.

The general configuration is:

model:
  default: openrouter/free
  provider: openrouter
  max_tokens: 4096

OpenRouter is used for the primary model.

The auxiliary configuration was also changed.

Compression:

auxiliary:
  compression:
    provider: openrouter
    model: openrouter/free

Title generation:

auxiliary:
  title:
    provider: openrouter
    model: openrouter/free

Fallback:

fallback_model:
  provider: openrouter
  model: openrouter/free
14. OpenRouter API Key

The OpenRouter API key is stored in the Hermes environment file:

~/.hermes/.env

The environment variable is:

OPENROUTER_API_KEY=...

The actual secret must not be exposed in logs or reports.

15. OpenRouter Test

Hermes was tested with:

hello, respond with exactly: OpenRouter works

Hermes responded:

OpenRouter works

Therefore the following chain is confirmed:

Hermes
   │
   ▼
OpenRouter
   │
   ▼
Free Model
   │
   ▼
Response

Status:

OpenRouter integration: WORKING
16. Hermes → n8n Connectivity Test

Hermes was asked to verify whether n8n was reachable on port 5678.

Hermes executed a local curl request against:

http://localhost:5678

The result was:

HTTP 200

The VPS itself was also tested directly:

curl -I http://127.0.0.1:5678

Result:

HTTP/1.1 200 OK

Therefore:

Hermes
   │
   ▼
localhost:5678
   │
   ▼
n8n

is confirmed to work.

Status:

Hermes → n8n network connectivity: WORKING
17. n8n REST API Investigation

The next objective was to determine whether Hermes could use the n8n REST API.

The workflow endpoint was tested:

http://127.0.0.1:5678/rest/workflows

Without authentication, n8n returned:

{
  "status": "error",
  "message": "Unauthorized"
}

This established that:

REST API endpoint:
reachable


Authentication:
required
18. N8N_API_KEY Configuration

A new environment variable was added to:

~/.hermes/.env

The variable is:

N8N_API_KEY=...

It was verified with:

[ -n "$N8N_API_KEY" ] && echo "N8N_API_KEY is set"

Result:

N8N_API_KEY is set

Therefore the environment variable exists in the current shell.

However, existence of the variable does not mean that the value is a valid n8n API key.

19. n8n API Authentication Test

The following request was tested:

curl -i \
  -H "X-N8N-API-KEY: $N8N_API_KEY" \
  http://127.0.0.1:5678/rest/workflows

Result:

HTTP/1.1 401 Unauthorized

Response:

{
  "status": "error",
  "message": "Unauthorized"
}

Therefore:

n8n REST API:
        reachable          YES
        endpoint exists    YES
        authentication     REQUIRED
        current API key    NOT ACCEPTED
20. Important Shell Command Issue

There was a shell syntax issue while loading the environment file.

The correct command is:

set -a
source ~/.hermes/.env
set +a

These are three separate commands.

The following is incorrect:

set -a source ~/.hermes/.env set +a

The latter is interpreted as a single shell command.

21. .env Syntax Problem

When the .env file was sourced:

source ~/.hermes/.env

the shell returned:

DM: command not found

This means the .env file contains at least one malformed line.

Shell-compatible environment files should contain entries like:

NAME=value

For example:

OPENROUTER_API_KEY=xxxxx
N8N_API_KEY=xxxxx

A line such as:

DM: something

is not valid shell environment-variable syntax.

The malformed line should be fixed or removed.

22. Current System Diagram

The current system is:

                         TELEGRAM
                            │
                            │
                            ▼
                    ┌───────────────┐
                    │     HERMES    │
                    │               │
                    │ Installed     │
                    │ directly on   │
                    │ VPS           │
                    └───────┬───────┘
                            │
                            │ LLM
                            ▼
                    ┌───────────────┐
                    │  OPENROUTER   │
                    │               │
                    │ Free Model    │
                    └───────┬───────┘
                            │
                            │
                    Hermes tools
                            │
                            ▼
                 ┌─────────────────────┐
                 │  VPS localhost      │
                 │  127.0.0.1:5678     │
                 └──────────┬──────────┘
                            │
                            ▼
                    ┌───────────────┐
                    │     DOCKER    │
                    │               │
                    │shop-n8n-prod  │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │      n8n      │
                    │    v2.34.5    │
                    └───────┬───────┘
                            │
                            │ REST API
                            ▼
                    ┌───────────────┐
                    │   Workflows   │
                    └───────┬───────┘
                            │
                            ▼
                 Shop / Backend / APIs

The only broken link is currently:

Hermes → n8n REST API authentication
23. Current Status Matrix
Component	Status	Notes
VPS	✅	Working
Ubuntu	✅	Working
Docker	✅	Working
Hermes	✅	Installed directly on VPS
Hermes Telegram integration	✅	Working
Hermes terminal	✅	Working
OpenRouter	✅	Working
OpenRouter free model	✅	Working
Groq	❌	Abandoned because of TPM/request-size issue
n8n	✅	Running
n8n Docker container	✅	shop-n8n-prod
n8n version	✅	2.34.5
n8n port	✅	127.0.0.1:5678
Hermes → n8n connectivity	✅	HTTP 200
Remote n8n UI	✅	SSH tunnel works
n8n REST API	✅	Endpoint reachable
n8n API authentication	❌	Current API key rejected
N8N_API_KEY environment variable	✅	Exists
.env syntax	⚠️	Contains malformed line(s)
Hermes → authenticated n8n API	⏳	Blocked by authentication
Hermes → list workflows	⏳	Not yet possible
Hermes → execute workflows	⏳	Not yet possible
Telegram → n8n automation	⏳	Final objective
24. What Has Been Successfully Completed

The following parts are already complete:

Infrastructure
VPS is running.
Docker is running.
n8n is running.
n8n is bound to localhost.
Hermes is installed directly on the VPS.
Hermes can access n8n through localhost.
LLM
Groq was tested.
Groq's TPM limitation caused HTTP 413.
Groq was abandoned.
OpenRouter was configured.
OpenRouter free model is working.
n8n Connectivity
Port 5678 is reachable locally.
Hermes can reach n8n.
n8n returns HTTP 200 from its UI.
REST API endpoint exists.
Authentication is correctly enforced.
Remote Administration
n8n can be accessed from a local computer using an SSH tunnel.
Port 5678 does not need to be publicly exposed.
25. What Has NOT Been Completed

The following tasks remain:

25.1 Fix n8n API authentication

Current:

Hermes
   ↓
X-N8N-API-KEY
   ↓
n8n
   ↓
401 Unauthorized

Target:

Hermes
   ↓
X-N8N-API-KEY
   ↓
n8n
   ↓
200 OK
25.2 List n8n workflows

Once authentication works:

GET /rest/workflows

should return workflow information.

Hermes should then be able to discover what automations exist.

25.3 Determine how Hermes should execute workflows

After discovering workflows, the next task is to determine the appropriate n8n API call for executing a workflow.

The desired chain is:

User
  ↓
Telegram
  ↓
Hermes
  ↓
Understand request
  ↓
Find appropriate workflow
  ↓
Execute workflow
  ↓
Receive result
  ↓
Return result to Telegram
25.4 Add safety boundaries

Before allowing Hermes to execute arbitrary workflows, it should be restricted appropriately.

For example:

Read-only workflows
        ↓
Allowed


Safe automation workflows
        ↓
Allowed


Destructive workflows
        ↓
Require confirmation

This is particularly important because Hermes has terminal access and n8n may have access to:

Database
Backend APIs
Files
External APIs
Credentials
Store operations
26. Recommended Security Model

The recommended architecture is:

Internet
   │
   ├── HTTPS → nginx
   │
   └── Telegram → Hermes
                     │
                     ▼
                  n8n API
                     │
                     ▼
              localhost only

Avoid exposing:

0.0.0.0:5678

to the public Internet unless there is a specific reason.

Current:

127.0.0.1:5678

is preferable.

27. Immediate Next Step

The immediate task is not changing Hermes, OpenRouter, Docker, or the network.

The immediate task is:

FIX N8N API AUTHENTICATION

First access the n8n UI remotely:

ssh -L 127.0.0.1:5678:127.0.0.1:5678 root@YOUR_VPS_IP

Then open:

http://127.0.0.1:5678

Create/obtain a valid n8n API key from the n8n UI.

Then update:

~/.hermes/.env

with:

N8N_API_KEY=YOUR_VALID_N8N_API_KEY

Fix any malformed .env lines.

Then load:

set -a
source ~/.hermes/.env
set +a

Finally test:

curl -i \
  -H "X-N8N-API-KEY: $N8N_API_KEY" \
  http://127.0.0.1:5678/rest/workflows

The desired response is:

HTTP/1.1 200 OK
28. Final Target Architecture

Once authentication is fixed, the intended final system is:

                         ┌───────────────┐
                         │    TELEGRAM   │
                         │     USER      │
                         └───────┬───────┘
                                 │
                                 │ Natural language
                                 ▼
                         ┌───────────────┐
                         │    HERMES     │
                         │  AI AGENT     │
                         └───────┬───────┘
                                 │
                   ┌─────────────┼─────────────┐
                   │             │             │
                   ▼             ▼             ▼
             OpenRouter      Terminal       n8n API
                   │                           │
                   │                           ▼
                   │                     Workflows
                   │                           │
                   │                           ▼
                   │                  Shop / Backend
                   │                           │
                   └──────────────┬────────────┘
                                  │
                                  ▼
                              Result
                                  │
                                  ▼
                              Hermes
                                  │
                                  ▼
                              Telegram
29. Final Conclusion

The project is mostly operational.

The current state is:

Telegram                 ✅
       ↓
Hermes                   ✅
       ↓
OpenRouter               ✅
       ↓
Free LLM                 ✅
       ↓
Hermes Terminal          ✅
       ↓
VPS localhost:5678       ✅
       ↓
Docker n8n               ✅
       ↓
n8n 2.34.5               ✅
       ↓
REST API                 ✅
       ↓
Authentication           ❌  ← CURRENT BLOCKER
       ↓
Workflow discovery       ⏳
       ↓
Workflow execution       ⏳
       ↓
Telegram automation      ⏳

The system does not need to be rebuilt.

The current blocker is specifically:

n8n REST API authentication

Once the API key is accepted and:

GET /rest/workflows

returns 200 OK, the next phase is to integrate workflow discovery and execution into Hermes.