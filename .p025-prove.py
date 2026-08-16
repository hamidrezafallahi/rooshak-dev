#!/usr/bin/env python3
"""P025 proof: create draft via create-product.sh → JSONL audit line → list-catalog-audit."""
import json
import subprocess
import urllib.request


def load_env(path="/opt/shop/.env"):
    vals = {}
    with open(path, encoding="utf-8") as f:
        for raw in f:
            line = raw.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            k, v = line.split("=", 1)
            vals[k] = v.strip().strip('"').strip("'")
    return vals


def hermes(cmd):
    return subprocess.check_output(
        ["docker", "exec", "shop-hermes-prod", "bash", "-lc", cmd],
        text=True,
        stderr=subprocess.STDOUT,
    )


def main():
    # ensure scripts executable on host bind
    subprocess.check_call(
        [
            "chmod",
            "+x",
            "/opt/shop/automation/hermes/skills/shop/shop-owner/scripts/append-catalog-audit.sh",
            "/opt/shop/automation/hermes/skills/shop/shop-owner/scripts/list-catalog-audit.sh",
            "/opt/shop/automation/hermes/skills/shop/shop-owner/scripts/create-product.sh",
        ]
    )

    create = hermes(
        "bash /opt/data/skills/shop/shop-owner/scripts/create-product.sh "
        "--name 'P025 audit draft b' --price 11112 --inventory 1 "
        "--category-id 1 --brand-id 2 --description 'P025 audit only — keep draft'"
    )
    print("create_out_head", create.strip().splitlines()[0][:300] if create.strip() else "")
    created = json.loads(create.strip().splitlines()[0])
    if not created.get("ok"):
        raise SystemExit(f"P025_FAIL: create failed: {created}")
    product_id = created.get("productId")
    print("productId", product_id)

    listed = hermes(
        "bash /opt/data/skills/shop/shop-owner/scripts/list-catalog-audit.sh --tail 5"
    )
    audit = json.loads(listed.strip())
    print("audit_count", audit.get("count"))
    print("audit_file", audit.get("file"))
    lines = audit.get("lines") or []
    hit = None
    for ln in reversed(lines):
        if ln.get("productId") == product_id or ln.get("name") == "P025 audit draft b":
            hit = ln
            break
    if not hit:
        raise SystemExit(f"P025_FAIL: audit line missing for product {product_id}: {lines}")
    print("audit_action", hit.get("action"))
    print("audit_source", hit.get("source"))
    print("audit_ok", hit.get("ok"))
    print("audit_productId", hit.get("productId"))

    # still draft / not public
    raw = subprocess.check_output(
        [
            "docker",
            "exec",
            "shop-backend-prod",
            "wget",
            "-qO-",
            f"http://127.0.0.1:8080/api/Products/search?keyword=P025%20audit",
        ],
        text=True,
    )
    search = json.loads(raw)
    data = search.get("data") if isinstance(search, dict) else search
    records = data if isinstance(data, list) else (data.get("records") if isinstance(data, dict) else [])
    print("public_search_hits", len(records or []))

    if hit.get("source") != "telegram-owner" or hit.get("action") != "catalog.create":
        raise SystemExit("P025_FAIL: unexpected audit fields")
    if not hit.get("ok") or hit.get("productId") != product_id:
        raise SystemExit("P025_FAIL: audit product mismatch")
    print("P025_PROOF_OK")


if __name__ == "__main__":
    main()
