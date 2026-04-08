import pytest
import requests

base_URI = "http://localhost:8082/api/basket/pytest"
added_GUIDS = []

def test_basket_adds_new_item():
    obj = {
        "id": "b0d8a7f4-a123-4c1f-b2ad-cf78867f9c52",
        "event": "Spring Fest Concert",
        "price": 59.99,
        "description": "Outdoor live concert featuring local bands.",
        "event_date": "2026-05-15"
    }
    added_GUIDS.append(obj["id"])

    res = requests.post(base_URI, json=obj)
    assert res.status_code == 201

def test_basket_add_item_handles_bad_data():
    obj = {
        "id": "not_a_GUID",
        "event": "fake",
        "price": -200,
        "description": "Party in the void between time",
        "event_date": "always but never"
    }

    res = requests.post(base_URI, json=obj)
    assert res.status_code == 422

def test_basket_gets_all_items():
    res = requests.get(f"{base_URI}")

    assert res.status_code == 200

def test_basket_updates_items():
    new_obj = {
        "id": "b0d8a7f4-a123-4c1f-b2ad-cf78867f9c52",
        "event": "UPDATED Spring Fest Concert",
        "price": 2009.99,
        "description": "Outdoor live concert featuring BAD local bands.",
        "event_date": "2026-05-15"
    }

    res = requests.put(f"{base_URI}/{added_GUIDS[0]}", json=new_obj)
    assert res.status_code == 200

def test_basket_update_item_handles_bad_data():
    new_obj = {
        "id": "wrong",
        "event": "UPDATED Spring Fest Concert",
        "price": 2009.99,
        "description": "Outdoor live concert featuring BAD local bands.",
        "event_date": "2026-05-15"
    }

    res = requests.put(f"{base_URI}/{added_GUIDS[0]}", json=new_obj)
    assert res.status_code == 422

def test_basket_deletes_items():
    res = requests.delete(f"{base_URI}/{added_GUIDS[0]}")
    assert res.status_code == 200

def test_basket_delete_item_handles_bad_data():
    res = requests.delete(f"{base_URI}/notReal")
    assert res.status_code == 422

def test_basket_clears_items():
    res = requests.delete(f"{base_URI}")
    assert res.status_code == 200