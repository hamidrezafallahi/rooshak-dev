#!/usr/bin/env python3
"""Probe ProductOffers list shape for low-stock scan. No secrets."""
import json
import subprocess
import urllib.request


def main():
    # from host via published n8n? use docker exec backend wget
    raw = subprocess.check_output(
        [
            "docker",
            "exec",
            "shop-backend-prod",
            "wget",
            "-qO-",
            "http://127.0.0.1:8080/api/ProductOffers?page=1&pageSize=50",
        ],
        text=True,
    )
    payload = json.loads(raw)
    data = payload.get("data") if isinstance(payload, dict) else payload
    records = []
    if isinstance(data, dict):
        records = data.get("records") or data.get("Records") or []
        print("keys", sorted(data.keys())[:20])
        print("totalPages", data.get("totalPages") or data.get("TotalPages"))
        print("totalCount", data.get("totalCount") or data.get("TotalCount"))
    elif isinstance(data, list):
        records = data
    print("record_count", len(records))
    for o in records[:30]:
        print(
            {
                "id": o.get("id"),
                "productId": o.get("productId"),
                "name": o.get("productName") or o.get("name"),
                "inventory": o.get("inventory"),
                "isActive": o.get("isActive"),
                "isDeleted": o.get("isDeleted"),
            }
        )

    # also by-product for 7
    raw7 = subprocess.check_output(
        [
            "docker",
            "exec",
            "shop-backend-prod",
            "wget",
            "-qO-",
            "http://127.0.0.1:8080/api/ProductOffers/by-product/7",
        ],
        text=True,
    )
    print("by_product_7", raw7[:500])


if __name__ == "__main__":
    main()
