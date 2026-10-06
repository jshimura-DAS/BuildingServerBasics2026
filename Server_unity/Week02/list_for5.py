# list_for5.py
scores = [1200, 980, 1430, 1100]

bonus_scores = [s + 100 for s in scores]
high_scores = [s for s in scores if s >= 1100]

print(bonus_scores)
print(high_scores)