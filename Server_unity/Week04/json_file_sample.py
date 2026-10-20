# json_file_sample.py
import json

ranking = [
    {"name": "A", "score": 950},
    {"name": "B", "score": 1330},
    {"name": "C", "score": 1210},
]

with open("ranking.json", "w", encoding="utf-8") as f:
    json.dump(ranking, f, ensure_ascii=False, indent=2)

with open("ranking.json", "r", encoding="utf-8") as f:
    loaded = json.load(f)


best = max(loaded, key=lambda p: p["score"])
print("1位:", best["name"], best["score"])