# validators.py
def validate_score_payload(data):
	"""検証に問題があれば、エラーメッセージの文字列を返す。問題なければNoneを返す。"""
	if not isinstance(data, dict):
		return "リクエストボディはJSONオブジェクトである必要があります。"

	# 1. 必須項目の存在
	if "name" not in data:
		return "nameは必須です。"
	if "score" not in data:
		return "scoreは必須です。"

	name = data["name"]
	score = data["score"]

	# 2. 型と文字数（strip後の値で判定）
	if not isinstance(name, str):
		return "nameは文字列で指定してください。"
	stripped = name.strip()
	if len(stripped) == 0:
		return "nameに有効な文字を入力してください。"
	if len(stripped) > 20:
		return "nameは20文字以内で指定してください。"

	# 3. 型と範囲
	if isinstance(score, bool) or not isinstance(score, int):
		return "scoreは整数で指定してください。"
	if score < 0 or score > 999999:
		return "scoreは0〜999999の範囲で指定してください。"

	return None
