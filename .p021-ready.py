#!/usr/bin/env python3
"""P021 Gate C readiness: taxonomy + create path + skill confirm + no admin."""
from pathlib import Path
import json
import subprocess
import urllib.request

def sh(cmd):
    return subprocess.check_output(cmd, text=True)

print("=== hermes/n8n/backend up ===")
for name in ["shop-hermes-prod", "shop-n8n-prod", "shop-backend-prod"]:
    st = sh(["docker", "inspect", "-f", "{{.State.Status}}", name]).strip()
    print(name, st)

print("=== taxonomy from Hermes ===")
tax = subprocess.run(
    ["docker", "exec", "shop-hermes-prod", "bash",
     "/opt/data/skills/shop/shop-owner/scripts/list-taxonomy.sh"],
    capture_output=True, text=True,
)
print("taxonomy_exit", tax.returncode)
out = (tax.stdout or "")[:800]
print("taxonomy_head", out.replace("\n", " | ")[:500])

print("=== skill P020 + create vessel fields ===")
skill = Path("/opt/shop/automation/hermes/skills/shop/shop-owner/SKILL.md").read_text(encoding="utf-8")
print("has_two_message", "Two-message protocol (P020)" in skill)
print("has_diameter", "--diameter" in skill)
print("has_confirm_create", "اگر درست است بگو: تأیید" in skill)

print("=== create workflow active X-Api-Key ===")
vals = {}
for raw in Path("/opt/shop/.env").read_text(encoding="utf-8").splitlines():
    line = raw.strip()
    if not line or line.startswith("#") or "=" not in line:
        continue
    k, v = line.split("=", 1)
    vals[k] = v.strip().strip('"').strip("'")
key = vals.get("N8N_API_KEY") or ""
req = urllib.request.Request(
    "http://127.0.0.1:5678/api/v1/workflows/shop-product-create",
    headers={"X-N8N-API-KEY": key},
)
with urllib.request.urlopen(req, timeout=20) as resp:
    wf = json.loads(resp.read().decode())
data = wf.get("data") if isinstance(wf.get("data"), dict) and "nodes" in (wf.get("data") or {}) else wf
blob = json.dumps(data.get("nodes") or [])
print("create_active", data.get("active") if isinstance(data, dict) else wf.get("active"))
print("create_uses_api_key", "X-Api-Key" in blob or "CATALOG_API_KEY" in blob)
print("create_no_login", "Identity/login" not in blob)

print("=== drafts 7/8 still inactive in public search ===")
for kw in ["P015%20Gate%20B", "P019%20vessel"]:
    search = sh(["docker", "exec", "shop-backend-prod", "wget", "-qO-",
                 f"http://127.0.0.1:8080/api/Products/search?keyword={kw}"])
    print(kw, "hits", '"id"' in search and "P015" in search or "P019" in search)

print("P021_READY_FOR_OWNER")
