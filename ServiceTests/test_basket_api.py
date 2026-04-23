import pytest
import requests

base_URI = "http://localhost:5041/basket/api/pytest"
added_GUIDS = []

def test_basket_adds_new_item():
    ticket_id = "b0d8a7f4-a123-4c1f-b2ad-cf78867f9c52"
    added_GUIDS.append(ticket_id)

    res = requests.post(f"{base_URI}?ticket_id={ticket_id}")
    assert res.status_code == 201

def test_basket_add_item_handles_bad_data():
    ticket_id = "not real"

    res = requests.post(f"{base_URI}?ticket_id={ticket_id}")
    assert res.status_code == 422

def test_basket_gets_all_items():
    res = requests.get(f"{base_URI}")

    assert res.status_code == 200

def test_basket_updates_items():
    new_id = "a5c9d4e2-1100-4480-ba87-6f2b3bb6d8c3"

    res = requests.put(f"{base_URI}/{added_GUIDS[0]}?new_id={new_id}",)
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