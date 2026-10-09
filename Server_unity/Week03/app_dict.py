# app_dict.py
from flask import Flask

app = Flask(__name__)

players = [
	{"name": "A", "score": 950},
	{"name": "B", "score": 1330},
	{"name": "C", "score": 1210},
]


@app.route("/ranking")
def ranking():
	sorted_players = sorted(players, key=lambda p: p["score"], reverse=True)
	lines = []
	for i, p in enumerate(sorted_players, start=1):
		lines.append(f"{i}位 {p['name']} : {p['score']}")
	return "<br>".join(lines)


if __name__ == "__main__":
	app.run(host="0.0.0.0", port=5000, debug=True)