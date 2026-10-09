# app_routing.py
from flask import Flask

app = Flask(__name__)


@app.route("/")
def index():
	return "Hello World"


@app.route("/about")
def about():
	return "これはFlask学習用のサーバーです"


@app.route("/hello/<name>")
def hello_name(name):
	return f"Hello, {name}!"


if __name__ == "__main__":
	app.run(host="0.0.0.0", port=5000, debug=True)