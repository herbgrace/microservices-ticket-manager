import pytest
import requests

base_URI = "http://localhost:8080/api/tickets"
added_GUIDS = []

def test_catalog_adds_new_item():
    obj = {
        "event":"A7X Concert",
        "price":100.25,
        "description":"Metal Concert full of Avenged Sevenfold's best hits.",
        "eventDate":"2026-08-25"
    }

    res = requests.post(base_URI, json=obj)
    added_GUIDS.append(res.json()["id"])

    assert res.status_code == 201

def test_catalog_gets_all_catalog_items():
    res = requests.get(base_URI)

    assert res.status_code == 200

def test_catalog_gets_single_item():
    res = requests.get(f"{base_URI}/{added_GUIDS[0]}")

    assert res.status_code == 200

def test_catalog_searches_item_events():
    res = requests.get(f"{base_URI}/search/A7X")

    assert res.status_code == 200

def test_catalog_searches_item_descriptions():
    res = requests.get(f"{base_URI}/search/Sevenfold")

    assert res.status_code == 200

def test_catalog_updates_items():
    new_obj = {
        "event":"UPDATED A7X Concert",
        "price":10000.25,
        "description":"Metal Concert full of Avenged Sevenfold's worst hits.",
        "eventDate":"2026-08-25"
    }

    res = requests.put(f"{base_URI}/{added_GUIDS[0]}", json=new_obj)

    assert res.status_code == 200

def test_catalog_deletes_items():
    for item in added_GUIDS:
        res = requests.delete(f"{base_URI}/{item}")

        assert res.status_code == 204