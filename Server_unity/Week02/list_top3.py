# list_top3.py
scores = [1200, 980, 1430, 1100, 1250, 900]

top3 = sorted(scores, reverse=True)[:3]
print("上位3件:", top3)