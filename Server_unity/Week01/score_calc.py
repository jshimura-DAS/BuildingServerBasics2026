base_score = 1200
bonus = 350
penalty = 1000
#整数型変数で確保して代入
final_score = base_score + bonus - penalty
print(f"最終スコアは{final_score}点")

if final_score >= 1000:
    print("Rank: A")
else:
    print("Rank: B")
    
# ランクに限らず実行される
print("スコア計算が完了しました。")
# 文字列を代入し上書きする
final_score = "おしまい"
print(f"最終スコアは{final_score}です。")

