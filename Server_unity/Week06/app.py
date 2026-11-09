# app.py
import sqlite3
from flask import Flask, request, jsonify
from db import get_connection, init_db
from validators import validate_score_payload

app = Flask(__name__)
app.config["MAX_CONTENT_LENGTH"] = 1024 * 10  # リクエストボディの上限（10KB）
app.json.ensure_ascii = False  # 日本語を\uXXXXに変換せず、そのまま返す
init_db()


def error_response(message, status):
	return jsonify({"result": "error", "message": message}), status


@app.route("/api/scores", methods=["POST"])
def register_score():
	# 1. Content-Typeの確認
	if not request.is_json:
		return error_response("Content-Typeはapplication/jsonにしてください。", 415)

	# 2. JSONのパース（壊れていればNone）
	data = request.get_json(silent=True)
	if data is None:
		return error_response("JSONの形式が不正です。", 400)

	# 3. 必須・型・範囲の検証
	error_message = validate_score_payload(data)
	if error_message is not None:
		return error_response(error_message, 400)

	name = data["name"].strip()
	score = data["score"]

	# 4. DBへ保存（接続の失敗を含め、sqlite3.Errorは500で返す）
	conn = None
	try:
		conn = get_connection()
		cursor = conn.execute(
			"INSERT INTO scores (name, score) VALUES (?, ?)",
			(name, score),
		)
		conn.commit()
		new_id = cursor.lastrowid
	except sqlite3.Error:
		return error_response("サーバー内部でエラーが発生しました。", 500)
	finally:
		if conn is not None:
			conn.close()

	return jsonify({
		"result": "ok",
		"id": new_id,
		"name": name,
		"score": score,
	}), 201


@app.errorhandler(405)
def method_not_allowed(e):
	return error_response("このURLでは許可されていないメソッドです。", 405)


@app.errorhandler(413)
def too_large(e):
	return error_response("リクエストが大きすぎます。", 413)


@app.errorhandler(500)
def server_error(e):
	return error_response("サーバー内部でエラーが発生しました。", 500)


if __name__ == "__main__":
	app.run(host="0.0.0.0", port=5000, debug=True)
