#!/usr/bin/env python3
import json
from pathlib import Path
wf = json.loads(Path("/tmp/p016-offer-wf.json").read_text(encoding="utf-8"))
data = wf.get("data") if isinstance(wf, dict) and "nodes" not in wf else wf
for n in (data or {}).get("nodes") or []:
    if n.get("name") == "Update Offer":
        code = (n.get("parameters") or {}).get("jsCode") or ""
        print(code[1800:])
        print("---LEN", len(code))
