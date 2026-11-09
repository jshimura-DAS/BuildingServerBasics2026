# test_app.py
import pytest
import db
from app import app

@pytest.fixture
def client(tmp_path, monkeypatch):
	# DBの保存先を一時ファイルに差し替え、テーブルを作成する
	monkeypatch.setattr(db, "DB_PATH", str(tmp_path / "test.db"))
	db.init_db()

	app.config["TESTING"] = True
	with app.test_client() as test_client:
		yield test_client

def test_register_score_success(client):
	response = client.post("/api/scores", json={"name": "PlayerA", "score": 1250})
	assert response.status_code == 201
	body = response.get_json()
	assert body["result"] == "ok"
	assert body["name"] == "PlayerA"
	assert body["score"] == 1250

def test_register_score_strips_name(client):
	response = client.post("/api/scores", json={"name": "  PlayerA  ", "score": 100})
	assert response.status_code == 201
	assert response.get_json()["name"] == "PlayerA"

def test_register_score_missing_name(client):
	response = client.post("/api/scores", json={"score": 1250})
	assert response.status_code == 400
	body = response.get_json()
	assert body["result"] == "error"
	assert "必須" in body["message"]

def test_register_score_negative_score(client):
	response = client.post("/api/scores", json={"name": "PlayerB", "score": -10})
	assert response.status_code == 400

def test_register_score_bool_score(client):
	response = client.post("/api/scores", json={"name": "PlayerB", "score": True})
	assert response.status_code == 400

def test_register_score_name_too_long(client):
	long_name = "A" * 21
	response = client.post("/api/scores", json={"name": long_name, "score": 100})
	assert response.status_code == 400

def test_register_score_wrong_content_type(client):
	response = client.post("/api/scores", data="hello", content_type="text/plain")
	assert response.status_code == 415

def test_register_score_broken_json(client):
	response = client.post(
		"/api/scores", data='{"name": "A",}', content_type="application/json"
	)
	assert response.status_code == 400

def test_method_not_allowed_returns_json(client):
	response = client.get("/api/scores")
	assert response.status_code == 405
	assert response.get_json()["result"] == "error"

def test_japanese_message_is_not_escaped(client):
	response = client.post("/api/scores", json={"score": 1})
	# バイト列にUTF-8の日本語がそのまま含まれ、\uXXXXになっていない
	assert "必須".encode("utf-8") in response.data

def test_db_error_returns_500(client, monkeypatch):
	# DBファイルを開けない状態を作る
	monkeypatch.setattr(db, "DB_PATH", "/nonexistent_dir/test.db")
	response = client.post("/api/scores", json={"name": "A", "score": 1})
	assert response.status_code == 500
	assert response.get_json()["result"] == "error"
