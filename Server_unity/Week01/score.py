# ここはコメントです
# 1) 分岐（if / elif / else）
score = 30

if score >= 80:
    print("合格（A判定）")
elif score >= 60:
    print("合格（B判定）")
else:
    print("不合格")

score = "点数"

# 2) ループ（for）
print("----- ループ（for） -----")
scores = [1200,980,1430]
for s in scores:
    print(s)
