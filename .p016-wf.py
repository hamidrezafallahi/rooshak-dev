#!/usr/bin/env python3
from pathlib import Path
import json
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
req = urllib.request.Request(
    "http://127.0.0.1:5678/api/v1/workflows/shop-offer-update",
    headers={"X-N8N-API-KEY": key},
)
with urllib.request.urlopen(req, timeout=30) as resp:
    wf = json.loads(resp.read().decode())
data = wf.get("data") if isinstance(wf, dict) and "data" in wf else wf
Path("/tmp/p016-offer-wf.json").write_text(json.dumps(data, indent=2), encoding="utf-8")
print("saved_nodes", len((data or {}).get("nodes") or []))
for n in (data or {}).get("nodes") or []:
    if n.get("name") in ("Update Offer", "Parse Payload", "Login Content Bot"):
        code = (n.get("parameters") or {}).get("jsCode") or ""
        url = (n.get("parameters") or {}).get("url") or ""
        body = (n.get("parameters") or {}).get("jsonBody") or ""
        print("NODE", n.get("name"))
        if code:
            print(code[:2000])
        if url:
            print("url", url)
        if body:
            print("body", str(body)[:500])
