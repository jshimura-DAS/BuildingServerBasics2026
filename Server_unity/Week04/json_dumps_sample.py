# json_dumps_sample.py
import json

player = {
    "name": "PlayerA",
    "level": 7,
    "alive": True,
    "guild": None,
    "items": ["Potion", "Key"],
}

json_text = json.dumps(player, ensure_ascii=False, indent=2)

print(type(json_text))
print(json_text)