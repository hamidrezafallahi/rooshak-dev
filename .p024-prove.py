#!/usr/bin/env python3
"""P024 proof: گلدون matches live گلدان category; taxonomy count unchanged; create rejects fake ids."""
import json
import subprocess
import urllib.request


def count_categories():
    raw = subprocess.check_output(
        [
            "docker",
            "exec",
            "shop-backend-prod",
            "wget",
            "-qO-",
            "http://127.0.0.1:8080/api/Categories?page=1&pageSize=100&onlyActives=true",
        ],
        text=True,
    )
    payload = json.loads(raw)
    data = payload.get("data") if isinstance(payload, dict) else payload
    records = data.get("records") if isinstance(data, dict) else data
    return len(records or []), [r.get("persianName") for r in (records or [])]


def hermes_match(query):
    # skill dir inside hermes container
    cmd = [
        "docker",
        "exec",
        "shop-hermes-prod",
        "bash",
        "-lc",
        f'bash /opt/data/skills/shop/shop-owner/scripts/match-taxonomy.sh --kind category --query "{query}"',
    ]
    out = subprocess.check_output(cmd, text=True)
    return json.loads(out)


def hermes_create_fake():
    cmd = [
        "docker",
        "exec",
        "shop-hermes-prod",
        "bash",
        "-lc",
        "bash /opt/data/skills/shop/shop-owner/scripts/create-product.sh "
        "--name 'P024 should fail' --price 1000 --inventory 1 --category-id 99999 --brand-id 2 || true",
    ]
    out = subprocess.check_output(cmd, text=True, stderr=subprocess.STDOUT)
    return out.strip()


def main():
    before, names = count_categories()
    print("category_count_before", before)
    print("has_goldan", any("گلدان" in (n or "") for n in names))

    match = hermes_match("گلدون")
    print("match_ok", match.get("ok"))
    print("created_flag", match.get("created"))
    best = match.get("best") or {}
    print("best_id", best.get("id"))
    print("best_name", best.get("name"))
    print("best_score", best.get("score"))

    after, _ = count_categories()
    print("category_count_after", after)

    fake = hermes_create_fake()
    print("fake_create_out", fake[:240].replace("\n", " | "))

    if not match.get("ok"):
        raise SystemExit("P024_FAIL: match-taxonomy failed")
    if match.get("created") is not False:
        raise SystemExit("P024_FAIL: created must be false")
    if before != after:
        raise SystemExit("P024_FAIL: category count changed")
    name = str(best.get("name") or "")
    if "گلدان" not in name:
        raise SystemExit(f"P024_FAIL: expected گلدان match, got {name!r}")
    if "99999" not in fake and "not in live taxonomy" not in fake:
        # script prints JSON error
        if "live taxonomy" not in fake:
            raise SystemExit(f"P024_FAIL: create should reject unknown categoryId: {fake!r}")
    print("P024_PROOF_OK")


if __name__ == "__main__":
    main()
