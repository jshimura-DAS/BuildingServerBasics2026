# list_sample.py
scores = [1200, 980, 1430, 1100]

for i in range(2):
    print(f"要素{i}={scores[i]}")

print(f"最初の要素={scores[0]}")  # 先頭要素
print(f"最後の要素={scores[-1]}")  # 末尾要素

scores.append(1250)          # 末尾追加
top_score = max(scores)      # 最大値
avg_score = sum(scores) / len(scores)

print("scores:", scores[:2])
print("top:", top_score)
print("average:", round(avg_score, 1))