#!/usr/bin/env python3
"""P022: import/activate low-stock workflows and prove Telegram alert path. No secrets printed."""
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


def api_req(method, path, key, body=None):
    data = None if body is None else json.dumps(body).encode()
    request = urllib.request.Request(
        "http://127.0.0.1:5678/api/v1" + path,
        data=data,
        method=method,
        headers={"X-N8N-API-KEY": key, "Content-Type": "application/json"},
    )
    try:
        with urllib.request.urlopen(request, timeout=60) as resp:
            raw = resp.read().decode()
            return resp.status, json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        raw = e.read().decode()
        try:
            payload = json.loads(raw) if raw else {}
        except Exception:
            payload = {"error": raw[:300]}
        return e.code, payload


def webhook(path, secret, payload):
    payload = dict(payload)
    payload["webhookSecret"] = secret
    data = json.dumps(payload).encode()
    request = urllib.request.Request(
        f"http://127.0.0.1:5678/webhook/{path}",
        data=data,
        method="POST",
        headers={
            "Content-Type": "application/json",
            "X-Shop-Webhook-Secret": secret,
        },
    )
    try:
        with urllib.request.urlopen(request, timeout=90) as resp:
            raw = resp.read().decode()
            return resp.status, json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        raw = e.read().decode()
        try:
            payload = json.loads(raw) if raw else {}
        except Exception:
            payload = {"error": raw[:300]}
        return e.code, payload


def import_workflow(path):
    r = subprocess.run(
        ["docker", "exec", "shop-n8n-prod", "n8n", "import:workflow", f"--input={path}"],
        capture_output=True,
        text=True,
    )
    print("import", Path(path).name, "rc", r.returncode, (r.stdout or r.stderr or "")[:180].replace("\n", " "))
    return r.returncode == 0


def activate(key, workflow_id):
    status, payload = api_req("POST", f"/workflows/{workflow_id}/activate", key)
    print("activate", workflow_id, status, "active" if status < 300 else str(payload)[:160])
    return status < 300


def main():
    env = load_env("/opt/shop/.env")
    key = env.get("N8N_API_KEY") or ""
    secret = env.get("SHOP_OWNER_WEBHOOK_SECRET") or ""
    if not key:
        raise SystemExit("N8N_API_KEY missing")
    if not secret:
        raise SystemExit("SHOP_OWNER_WEBHOOK_SECRET missing")

    for wf in (
        "/workflows/shop-low-stock-alert.workflow.json",
        "/workflows/shop-offer-update.workflow.json",
    ):
        if not import_workflow(wf):
            raise SystemExit(f"import failed: {wf}")

    for wid in ("shop-low-stock-alert", "shop-offer-update"):
        if not activate(key, wid):
            raise SystemExit(f"activate failed: {wid}")

    # Ensure env URL documented (no secret)
    env_path = Path("/opt/shop/.env")
    text = env_path.read_text(encoding="utf-8")
    if "SHOP_OWNER_LOW_STOCK_URL=" not in text:
        env_path.write_text(
            text.rstrip() + "\nSHOP_OWNER_LOW_STOCK_URL=http://n8n:5678/webhook/shop-low-stock-alert\n",
            encoding="utf-8",
        )
        print("env_appended SHOP_OWNER_LOW_STOCK_URL")
    else:
        print("env_has SHOP_OWNER_LOW_STOCK_URL")

    # Raise stock above threshold
    s, j = webhook("shop-offer-update", secret, {"offerId": 7, "inventory": 5})
    print("set_inv_5", s, "ok", j.get("ok"), "alerted", j.get("lowStockAlerted"), "inv", j.get("inventory"))
    if s >= 400 or not j.get("ok"):
        raise SystemExit(f"set inv 5 failed: {j}")

    # Drop below 2 -> immediate Telegram from offer-update
    s, j = webhook("shop-offer-update", secret, {"offerId": 7, "inventory": 1})
    print("set_inv_1", s, "ok", j.get("ok"), "alerted", j.get("lowStockAlerted"), "inv", j.get("inventory"))
    if s >= 400 or not j.get("ok"):
        raise SystemExit(f"set inv 1 failed: {j}")
    if not j.get("lowStockAlerted"):
        raise SystemExit("P022_FAIL: lowStockAlerted expected true after inventory=1")

    # Force scan path
    s, j = webhook("shop-low-stock-alert", secret, {"force": True, "below": 2})
    print(
        "scan_force",
        s,
        "ok",
        j.get("ok"),
        "alertCount",
        j.get("alertCount"),
        "notified",
        j.get("notified"),
        "lowCount",
        j.get("lowCount"),
    )
    if s >= 400 or not j.get("ok"):
        raise SystemExit(f"scan failed: {j}")
    if (j.get("lowCount") or 0) < 1:
        raise SystemExit("P022_FAIL: expected at least one low-stock offer in scan")
    if not j.get("notified") and (j.get("alertCount") or 0) < 1:
        # force=true should notify even if static dedupe saw same inventory
        raise SystemExit("P022_FAIL: scan should notify when force=true and lowCount>0")

    # Restore baseline used in prior phases
    s, j = webhook(
        "shop-offer-update",
        secret,
        {"offerId": 7, "price": 10000, "inventory": 1},
    )
    print("restore", s, "ok", j.get("ok"), "inv", j.get("inventory"), "alerted", j.get("lowStockAlerted"))
    if s >= 400 or not j.get("ok"):
        raise SystemExit(f"restore failed: {j}")

    print("P022_PROOF_OK")


if __name__ == "__main__":
    main()
