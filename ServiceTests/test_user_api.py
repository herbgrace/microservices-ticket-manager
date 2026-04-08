import pytest
import requests

base_URI = "http://localhost:8085/api/users"
created_GUIDS = []

def test_api_adds_new_user():
    obj = {
        "username": "another user",
        "email": "fakeEmail@gmail.com",
        "password": "secure"
    }

    res = requests.post(base_URI, json=obj)
    created_GUIDS.append(res.json()["userGuid"])

    assert res.status_code == 201

def test_api_handles_bad_data():
    obj = {
        "username": "another user",
        "password": "secure"
    }

    res = requests.post(base_URI, json=obj)
    assert res.status_code == 400

def test_api_can_login_user():
    obj = {
        "email": "fakeEmail@gmail.com",
        "password": "secure"
    }

    res = requests.post(f"{base_URI}/login", json=obj)
    assert res.status_code == 200

def test_api_can_get_single_user():
    res = requests.get(f"{base_URI}/{created_GUIDS[0]}")
    assert res.status_code == 200

def test_api_can_get_all_users():
    res = requests.get(base_URI)
    assert res.status_code == 200

def test_api_can_update_user():
    obj = {
        "userGuid": created_GUIDS[0],
        "username": "another user UPDATED!!!",
        "email": "fakeEmail@gmail.com",
        "password": "securely updated!!",
        "createdDate": "2026-04-07T23:57:57.8084731"
    }

    res = requests.put(f"{base_URI}/{created_GUIDS[0]}", json=obj)
    assert res.status_code == 200

def test_api_can_delete_user():
    for item in created_GUIDS:
        res = requests.delete(f"{base_URI}/{item}")
        assert res.status_code == 200
    created_GUIDS.clear()