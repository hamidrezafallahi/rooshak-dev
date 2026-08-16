#!/usr/bin/env python3
"""P016 probe: current offer-update path. No secrets printed."""
from pathlib import Path
import json
import subprocess
import urllib.error
import urllib.request

def load_env(path):
    vals = {}
    for raw in Path(path).read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, val = line.split("=", 1)
        vals[key] = val.strip().strip('"').strip("'")
    return vals

env = load_env("/opt/shop/.env")
key = env.get("N8N_API_KEY") or ""
base = "http://127.0.0.1:5678/api/v1"

def req(method, path):
    r = urllib.request.Request(
        base + path,
        method=method,
        headers={"X-N8N-API-KEY": key, "Content-Type": "application/json"},
    )
    try:
        with urllib.request.urlopen(r, timeout=30) as resp:
            return resp.status, json.loads(resp.read().decode() or "null")
    except urllib.error.HTTPError as exc:
        return exc.code, exc.read().decode(errors="replace")[:200]

print("=== n8n offer workflows ===")
status, payload = req("GET", "/workflows?limit=250")
print("list_status", status)
items = payload.get("data") if isinstance(payload, dict) else payload or []
offer_ids = []
for w in items:
    name = w.get("name") or ""
    if "offer" in name.lower() or "Offer" in name:
        print("workflow", w.get("id"), name, "active", w.get("active"))
        offer_ids.append(w.get("id"))

for wid in offer_ids:
    status, detail = req("GET", f"/workflows/{wid}")
    wf = detail.get("data") if isinstance(detail, dict) and "data" in detail else detail
    nodes = (wf or {}).get("nodes") or []
    print("nodes_for", wid)
    for n in nodes:
        ntype = n.get("type") or ""
        nname = n.get("name") or ""
        params = n.get("parameters") or {}
        url = params.get("url") or ""
        method = params.get("method") or params.get("httpMethod") or ""
        print(" -", nname, ntype, method, url[:80] if isinstance(url, str) else "")

print("=== hermes update-offer against offer 7 ===")
result = subprocess.run(
    [
        "docker", "exec", "shop-hermes-prod",
        "bash", "/opt/data/skills/shop/shop-owner/scripts/update-offer.sh",
        "--offer-id", "7", "--price", "10001",
    ],
    capture_output=True,
    text=True,
)
print("exit", result.returncode)
out = (result.stdout or "").strip()
err = (result.stderr or "").strip()
print("stdout_head", out[:600])
print("stderr_head", err[:300])
print("P016_PROBE_OK")
