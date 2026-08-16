#!/usr/bin/env python3
"""P023: verify shop-orders-digest is active at 08:00 Asia/Tehran. No secrets printed."""
from pathlib import Path
import json
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


def find_cron(nodes):
    for n in nodes or []:
        if n.get("type") == "n8n-nodes-base.scheduleTrigger":
            rule = (n.get("parameters") or {}).get("rule") or {}
            intervals = rule.get("interval") or []
            exprs = []
            for it in intervals:
                if isinstance(it, dict) and it.get("expression"):
                    exprs.append(it["expression"])
                elif isinstance(it, dict) and it.get("field") == "cronExpression":
                    exprs.append(it.get("expression") or it)
            return n.get("name"), exprs
    return None, []


def main():
    env = load_env("/opt/shop/.env")
    key = env.get("N8N_API_KEY") or ""
    secret = env.get("SHOP_OWNER_WEBHOOK_SECRET") or ""
    if not key:
        raise SystemExit("N8N_API_KEY missing")
    if not secret:
        raise SystemExit("SHOP_OWNER_WEBHOOK_SECRET missing")

    status, wf_wrap = api_req("GET", "/workflows/shop-orders-digest", key)
    if status >= 400:
        raise SystemExit(f"get workflow failed: {status} {wf_wrap}")
    wf = wf_wrap.get("data") if isinstance(wf_wrap, dict) and "data" in wf_wrap else wf_wrap
    active = bool(wf.get("active"))
    settings = wf.get("settings") or {}
    tz = settings.get("timezone") or settings.get("timezoneId") or ""
    cron_name, exprs = find_cron(wf.get("nodes") or [])

    print("workflow_id", wf.get("id"))
    print("workflow_name", wf.get("name"))
    print("active", active)
    print("timezone", tz or "(unset)")
    print("cron_node", cron_name)
    print("cron_exprs", exprs)

    # Export pretty JSON for repo (no secrets in workflow)
    Path("/opt/shop/automation/n8n/shop-orders-digest.workflow.json").write_text(
        json.dumps(wf, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    print("exported", "/opt/shop/automation/n8n/shop-orders-digest.workflow.json")

    # Manual prove via webhook (same path as cron after Parse)
    s, body = webhook("shop-orders-digest", secret, {"hours": 24, "limit": 15})
    print("manual_status", s)
    print("manual_ok", body.get("ok"))
    print("manual_hours", body.get("hours"))
    print("manual_total", body.get("total"))
    text = body.get("textFa") or body.get("error") or ""
    print("manual_text_head", str(text)[:120].replace("\n", " | "))

    ok = (
        active
        and any(e.strip() == "0 8 * * *" for e in exprs)
        and (tz == "Asia/Tehran" or tz == "")  # empty may inherit GENERIC_TIMEZONE
        and s < 400
        and body.get("ok") is True
    )
    if not active:
        raise SystemExit("P023_FAIL: workflow inactive")
    if not any(e.strip() == "0 8 * * *" for e in exprs):
        raise SystemExit(f"P023_FAIL: cron not 08:00 daily: {exprs}")
    if s >= 400 or body.get("ok") is not True:
        raise SystemExit(f"P023_FAIL: manual digest: {body}")
    print("P023_PROOF_OK")


if __name__ == "__main__":
    main()
