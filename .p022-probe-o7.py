#!/usr/bin/env python3
import json, subprocess

def get(url):
    raw = subprocess.check_output(["docker","exec","shop-backend-prod","wget","-qO-",url], text=True)
    print(url, raw[:400])
    return json.loads(raw)

get("http://127.0.0.1:8080/api/ProductOffers/7")
get("http://127.0.0.1:8080/api/Products/7")
