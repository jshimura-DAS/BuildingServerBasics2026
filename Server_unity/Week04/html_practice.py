# html_practice.py
import json
from flask import Flask

app = Flask(__name__)

player = {
    "name": "PlayerA",
    "level": 7,
    "alive": True,
    "guild": None,
    "score": 1250,  
    "items": ["Potion", "Key"],
}

json_data = json.dumps(player, ensure_ascii=False, indent=2)

css_header= "<style>body { font-family: Arial, sans-serif; } h1{color: #ccc; background-color: #333; padding: 10px;} td,tr{border: 1px solid #ccc; padding: 5px;}</style>"


@app.route("/profile")
def profile():
    profile = json.loads(json_data) 
    return css_header+f"<h1>{profile['name']}</h1><table><tr><th>レベル</th><td> {profile['level']}</td></tr><tr><th>スコア</th><td>{profile['score']}</td></tr></table><h2>所持アイテム</h2><ul><li>{profile['items'][0]}</li><li>{profile['items'][1]}</li></ul>"


if __name__ == "__main__":
	app.run(host="0.0.0.0", port=5000, debug=True)