#!/usr/bin/env python3
"""Check recent catalog drafts / telegram session hints for Gate C (no secrets)."""
import json
import subprocess
from pathlib import Path

# recent products via public API list? might be huge - use postgres through backend search for crystal-ish names is hard
# list product ids 1-20 via GET
found = []
for i in range(1, 20):
    try:
        raw = subprocess.check_output(
            ["docker", "exec", "shop-backend-prod", "wget", "-qO-", f"http://127.0.0.1:8080/api/Products/{i}"],
            text=True, stderr=subprocess.DEVNULL,
        )
    except subprocess.CalledProcessError:
        continue
    try:
        payload = json.loads(raw)
    except Exception:
        continue
    data = payload.get("data") if isinstance(payload, dict) else payload
    if not isinstance(data, dict):
        continue
    name = data.get("name") or ""
    found.append({"id": data.get("id"), "name": name, "price": data.get("price"), "brand": data.get("brandName"), "cat": data.get("categoryName")})

print("products_1_19", json.dumps(found, ensure_ascii=False))

# session files names only
sess = Path("/root/.hermes/sessions")
if sess.exists():
    files = sorted(sess.glob("*"), key=lambda p: p.stat().st_mtime, reverse=True)[:8]
    print("recent_sessions", [f.name for f in files])
else:
    print("recent_sessions", [])
