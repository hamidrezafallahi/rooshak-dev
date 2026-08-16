#!/usr/bin/env python3
"""P026 readiness: Gate C dependency + pipeline green. No secrets."""
import json
import subprocess


def wget(url):
    try:
        return subprocess.check_output(
            ["docker", "exec", "shop-backend-prod", "wget", "-qO-", url],
            text=True,
            stderr=subprocess.DEVNULL,
        )
    except subprocess.CalledProcessError:
        return None


def main():
    products = []
    for i in range(1, 25):
        raw = wget(f"http://127.0.0.1:8080/api/Products/{i}")
        if not raw:
            continue
        try:
            payload = json.loads(raw)
        except Exception:
            continue
        data = payload.get("data") if isinstance(payload, dict) else payload
        if not isinstance(data, dict) or not data.get("id"):
            continue
        products.append(
            {
                "id": data.get("id"),
                "name": data.get("name"),
                "brand": data.get("brandName"),
                "cat": data.get("categoryName"),
            }
        )

    testish = []
    realish = []
    for p in products:
        name = str(p.get("name") or "")
        if any(
            k in name
            for k in (
                "P015",
                "P019",
                "P025",
                "Gate B",
                "audit draft",
                "تست اتوماسیون",
            )
        ):
            testish.append(p)
        else:
            realish.append(p)

    print("product_count", len(products))
    print("test_drafts", json.dumps(testish, ensure_ascii=False))
    print("non_test_products", json.dumps(realish, ensure_ascii=False))

    # containers
    for c in ("shop-hermes-prod", "shop-n8n-prod", "shop-backend-prod"):
        st = subprocess.check_output(
            ["docker", "inspect", "-f", "{{.State.Status}}", c], text=True
        ).strip()
        print("container", c, st)

    # Gate C not signed if no owner-real telegram create beyond seeded catalog 1-6
    # Seeded 1-5 are store catalog; 6 is automation test; 7+ are agent drafts.
    owner_gate_c_candidates = [
        p
        for p in products
        if int(p["id"]) >= 7
        and not any(
            k in str(p.get("name") or "")
            for k in ("P015", "P019", "P025", "Gate B", "audit", "vessel draft")
        )
    ]
    print("gate_c_candidates", json.dumps(owner_gate_c_candidates, ensure_ascii=False))
    if owner_gate_c_candidates:
        print("P026_GATE_C_MAYBE_READY")
    else:
        print("P026_BLOCKED_ON_GATE_C")


if __name__ == "__main__":
    main()
