import pytest
import requests

users_base_URI = "http://localhost:5041/users/api"
auth_base_URI = "http://localhost:5041/auth/api"
created_GUIDS = []

def test_auth_sign_of_life_returns_OK():
    res = requests.get(f"{auth_base_URI}/test1")
    assert res.status_code == 200

def test_auth_returns_JWT_with_valid_credentials():
    obj = {
        "username": "test user",
        "email": "veryRealEmail@gmail.com",
        "password": "Awesome!"
    }

    users_res = requests.post(users_base_URI, json=obj)
    created_GUIDS.append(users_res.json()["userGuid"])

    res = requests.post(f"{auth_base_URI}/createtoken", json=obj)
    assert res.status_code == 200

def test_auth_does_not_return_JWT_with_invalid_credentials():
    obj = {
        "username": "NotAREALUSER",
        "email": "NO",
        "password": "Bad password"
    }

    res = requests.post(f"{auth_base_URI}/createtoken", json=obj)
    assert res.status_code == 400

def test_auth_create_token_handles_bad_data():
    obj = {
        "username": "what"
    }

    res = requests.post(f"{auth_base_URI}/createtoken", json=obj)
    assert res.status_code == 400

for item in created_GUIDS:
    res = requests.delete(f"{users_base_URI}/{item}")
created_GUIDS.clear()