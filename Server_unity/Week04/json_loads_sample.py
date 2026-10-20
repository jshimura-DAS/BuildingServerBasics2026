# json_loads_sample.py
import json

# JSON文字列を作成
json_text = '{"name": "PlayerA", "level": 7, "items": ["Potion", "Key"]}'

# JSON文字列を辞書に変換
player = json.loads(json_text)

print(type(player))
print(player["name"])
print(player["items"][0])