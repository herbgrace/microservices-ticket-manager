import pytest
import requests

orders_base_URI = "http://localhost:5041/orders/api"
users_base_URI = "http://localhost:5041/users/api"
auth_base_URI = "http://localhost:5041/auth/api"
created_user_GUIDS = []
created_order_GUIDS = []
created_JWTS = []

def test_orders_sign_of_life_returns_OK():
    res = requests.get(f"{orders_base_URI}/test")
    assert res.status_code == 200

def test_orders_can_create_order():
    user = {
        "username": "another user",
        "email": "fakeEmail@gmail.com",
        "password": "secure"
    }

    # Create a user to store the tickets
    users_res = requests.post(f"{users_base_URI}", json=user)
    created_user_GUIDS.append(users_res.json()["userGuid"])

    # Create auth token
    auth_res = requests.post(f"{auth_base_URI}/createtoken", json=user)
    created_JWTS.append(auth_res.text)

    obj = {
        "UserGuid": created_user_GUIDS[0],
        "Tickets": [
            {
            "TicketGuid": "b0d8a7f4-a123-4c1f-b2ad-cf78867f9c52",
            "Event": "Spring Fest Concert",
            "Price": 59.99,
            "Description": "Outdoor live concert featuring local bands.",
            "EventDate": "2026-05-15"
            },
            {
                "TicketGuid": "a5c9d4e2-1100-4480-ba87-6f2b3bb6d8c3",
                "Event": "Cinema Night Premiere",
                "Price": 12.5,
                "Description": "Premiere screening with free popcorn.",
                "EventDate": "2026-06-10"
            }
        ]
    }
    headers = {
        "Authorization": f"Bearer {created_JWTS[0]}",
        "Content-Type": "application/json"
    }

    res = requests.post(f"{orders_base_URI}", json=obj, headers=headers)
    created_order_GUIDS.append(res.json()["orderGuid"])
    assert res.status_code == 201

def test_orders_create_order_handles_bad_data():
    obj = {
        "UserGuid": "Fake Guid",
        "Tickets": "Not Real Tickets"
    }
    headers = {
        "Authorization": f"Bearer {created_JWTS[0]}",
        "Content-Type": "application/json"
    }

    res = requests.post(f"{orders_base_URI}", json=obj, headers=headers)
    assert res.status_code == 400

def test_orders_can_get_all_orders():
    res = requests.get(f"{orders_base_URI}")
    assert res.status_code == 200

def test_orders_can_get_orders_with_tickets():
    res = requests.get(f"{orders_base_URI}/with-tickets")
    assert res.status_code == 200

def test_orders_can_get_orders_from_order_guid():
    res = requests.get(f"{orders_base_URI}/{created_order_GUIDS[0]}")
    assert res.status_code == 200

def test_get_orders_from_order_guid_handles_bad_data():
    res = requests.get(f"{orders_base_URI}/FakeGuid")
    assert res.status_code == 404

def test_orders_can_get_orders_from_user_guid():
    res = requests.get(f"{orders_base_URI}/user/{created_user_GUIDS[0]}")
    assert res.status_code == 200

def test_get_orders_from_user_guid_handles_bad_data():
    res = requests.get(f"{orders_base_URI}/user/FakeGuid")
    assert res.status_code == 404

def test_delete_orders_handles_bad_data():
    headers = {
        "Authorization": f"Bearer {created_JWTS[0]}",
        "Content-Type": "application/json"
    }
    res = requests.delete(f"{orders_base_URI}/FakeGuid", headers=headers)
    assert res.status_code == 404

def test_orders_can_delete_orders():
    headers = {
        "Authorization": f"Bearer {created_JWTS[0]}",
        "Content-Type": "application/json"
    }
    for order in created_order_GUIDS:
        res = requests.delete(f"{orders_base_URI}/{order}", headers=headers)
        assert res.status_code == 200
    created_order_GUIDS.clear()

    for user in created_user_GUIDS:
        requests.delete(f"{users_base_URI}/{user}")
    created_user_GUIDS.clear()
    created_JWTS.clear()