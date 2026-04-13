import pytest
import requests

base_URI = "http://localhost:8085/api/users"
base_auth_URI = "http://localhost:8084/api/auth"
created_GUIDS = []
created_JWTS = []

def test_api_adds_new_user():
    obj = {
        "username": "another user",
        "email": "fakeEmail@gmail.com",
        "password": "secure"
    }

    res = requests.post(base_URI, json=obj)
    created_GUIDS.append(res.json()["userGuid"])

    assert res.status_code == 201

def test_api_add_user_handles_bad_data():
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

def test_api_login_user_handles_bad_data():
    obj = {
        "email": "fakeEmail@gmail.com"
    }

    res = requests.post(f"{base_URI}/login", json=obj)
    assert res.status_code == 400

def test_api_login_user_handles_bad_credentials():
    obj = {
        "email": "fakeEmail@gmail.com",
        "password": "wrongPassword"
    }

    res = requests.post(f"{base_URI}/login", json=obj)
    assert res.status_code == 401

def test_api_can_get_single_user():
    res = requests.get(f"{base_URI}/{created_GUIDS[0]}")
    assert res.status_code == 200

def test_api_get_user_handles_bad_data():
    res = requests.get(f"{base_URI}/FakeGUID")
    assert res.status_code == 404

def test_api_can_get_all_users():
    res = requests.get(base_URI)
    assert res.status_code == 200

def test_api_can_update_user():
    original_obj = {
        "email": "fakeEmail@gmail.com",
        "username": "another user",
        "password": "secure"
    }

    obj = {
        "userGuid": created_GUIDS[0],
        "Username": "another user UPDATED!!!",
        "email": "fakeEmail@gmail.com",
        "password": "securely updated!!",
        "createdDate": "2026-04-07T23:57:57.8084731"
    }

    res1 = requests.post(f"{base_auth_URI}/createtoken", json=original_obj)
    created_JWTS.append(res1.text)

    headers = {
        "Authorization": f"Bearer {created_JWTS[0]}",
        "Content-Type": "application/json"
    }

    res2 = requests.put(f"{base_URI}/{created_GUIDS[0]}", headers=headers, json=obj)

    assert res2.status_code == 200

def test_api_update_user_will_fail_without_JWT():
    obj = {
        "userGuid": created_GUIDS[0],
        "username": "another user UPDATED!!!",
        "email": "fakeEmail@gmail.com",
        "password": "securely updated!!",
        "createdDate": "2026-04-07T23:57:57.8084731"
    }

    res = requests.put(f"{base_URI}/{created_GUIDS[0]}", json=obj)
    assert res.status_code == 401

def test_api_update_user_handles_bad_data():
    obj = {
        "userGuid": "fakeGUID",
        "password": "securely updated!!",
        "createdDate": "2026-04-07T23:57:57.8084731"
    }

    headers = {
        "Authorization": f"Bearer {created_JWTS[0]}",
        "Content-Type": "application/json"
    }

    res = requests.put(f"{base_URI}/{created_GUIDS[0]}", json=obj, headers=headers)
    assert res.status_code == 400

def test_api_can_delete_user():
    for item in created_GUIDS:
        res = requests.delete(f"{base_URI}/{item}")
        assert res.status_code == 200
    created_GUIDS.clear()

def test_api_delete_user_handles_bad_data():
    res = requests.delete(f"{base_URI}/FAKE")
    assert res.status_code == 404