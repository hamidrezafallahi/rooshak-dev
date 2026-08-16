#!/usr/bin/env python3
"""P018 probe: live orders workflows, methods, Hermes list-orders. No secrets."""
from pathlib import Path
import json
import subprocess
import urllib.request

def load_env(path):
    vals = {}
    for raw in Path(path).read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        k, v = line.split("=", 1)
        vals[k] = v.strip().strip('"').strip("'")
    return vals

env = load_env("/opt/shop/.env")
key = env.get("N8N_API_KEY") or ""
req = urllib.request.Request(
    "http://127.0.0.1:5678/api/v1/workflows?limit=250",
    headers={"X-N8N-API-KEY": key},
)
with urllib.request.urlopen(req, timeout=20) as resp:
    payload = json.loads(resp.read().decode())
items = payload.get("data") if isinstance(payload, dict) else payload

print("=== workflows with order in name ===")
for wf in items or []:
    name = (wf.get("name") or "")
    wid = wf.get("id")
    if "order" in name.lower() or "order" in str(wid).lower():
        print("WF", wid, name, "active", wf.get("active"))
        detail_req = urllib.request.Request(
            f"http://127.0.0.1:5678/api/v1/workflows/{wid}",
            headers={"X-N8N-API-KEY": key},
        )
        with urllib.request.urlopen(detail_req, timeout=20) as dresp:
            detail = json.loads(dresp.read().decode())
        data = detail.get("data") if isinstance(detail.get("data"), dict) and "nodes" in (detail.get("data") or {}) else detail
        nodes = data.get("nodes") or []
        names = [n.get("name") or "" for n in nodes]
        blob = json.dumps(nodes)
        print("  nodes", ",".join(names))
        print("  has_login", "Identity" in blob or "Login" in blob)
        print("  uses_x_api_key", "X-Api-Key" in blob)
        methods = []
        for token in ["PUT", "PATCH", "DELETE", "POST", "GET"]:
            if f'"{token}"' in blob or f"'{token}'" in blob or f"method\": \"{token}" in blob or f"/{token}" in blob:
                pass
        print("  has_PUT", "PUT" in blob)
        print("  has_PATCH", "PATCH" in blob)
        print("  has_DELETE", "DELETE" in blob)
        print("  hits_Orders", "/Orders" in blob or "/orders" in blob)
        print("  webhook_path")
        for n in nodes:
            params = n.get("parameters") or {}
            if n.get("type") == "n8n-nodes-base.webhook":
                print("   ", params.get("path"), params.get("httpMethod"))
        Path(f"/tmp/p018-{wid}.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")

print("=== disk order json ===")
print(subprocess.check_output("ls /opt/shop/automation/n8n/*order* 2>/dev/null || true", shell=True, text=True))

print("=== hermes list-orders hours=24 ===")
upd = subprocess.run(
    [
        "docker", "exec", "shop-hermes-prod",
        "bash", "/opt/data/skills/shop/shop-owner/scripts/list-orders.sh",
        "--hours", "24",
    ],
    capture_output=True,
    text=True,
)
print("exit", upd.returncode)
print("stdout_head", (upd.stdout or "")[:800])
print("stderr_head", (upd.stderr or "")[:400])
