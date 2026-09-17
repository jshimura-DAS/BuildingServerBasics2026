# ステップバイステップ PHP チャットアプリケーション実装ガイド

## 目次
1. [プロジェクト概要](#プロジェクト概要)
2. [ファイル構成比較](#ファイル構成比較)
3. [RegistUser2 から Chat への進化](#registuser2-から-chat-への進化)
4. [GET と POST の分岐](#get-と-post-の分岐)
5. [データの保存と展開](#データの保存と展開)
6. [実装の詳細](#実装の詳細)
7. [実装演習](#実装演習)

---

## プロジェクト概要

### RegistUser2 (ユーザー登録)
- **目的**: 名前と生年月日を1回だけ登録
- **処理フロー**: HTML フォーム → POST → PHP 検証 → CSV 保存 → 結果表示
- **特徴**: リクエスト1回 = 1つのアクション（登録処理）

### Chat (チャット)
- **目的**: 名前とコメントを何度も投稿し、すべての投稿を表示
- **処理フロー**: 
  - POST リクエスト（フォーム送信）→ メッセージ保存 → 結果ページ表示
  - GET リクエスト（ページロード/更新） → メッセージ読み込み → HTML で展開
- **特徴**: リクエストの種類によって異なる処理を実行

---

## ファイル構成比較

### RegistUser2.php（単純な登録システム）
```
RegistUser2.html（フォーム）
		↓ [POST リクエスト]
RegistUser2.php（POST のみ処理）
	1. 入力値を検証
	2. CSV に保存
	3. 結果ページを表示（完了）
		↓ [POST の処理は終わり]
```

### Chat.php（双方向通信システム）
```
Chat.html（フォーム + メッセージ表示エリア）
	├─ [POST リクエスト - メッセージ投稿]
	│   ↓
	│ Chat.php（POST 処理）
	│   1. 入力値を検証
	│   2. CSV に保存
	│   3. 結果ページを表示
	│
	└─ [GET リクエスト?action=get - メッセージ取得]
		↓ [JavaScript の fetch() が定期実行]
	  Chat.php（GET 処理）
		1. CSV を読み込み
		2. メッセージを解析
		3. HTML タグを生成
		4. ブラウザに返却（自動更新）
```

---

## RegistUser2 から Chat への進化

### 段階1：RegistUser2.php と同じ基本構造

```php
<?php
header('Content-Type: text/html; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// POST処理：ユーザーからの送信を受け取る
	$name = $_POST['name'] ?? '';
	$birthDate = $_POST['birthDate'] ?? '';

	// 検証
	if (empty($name) || empty($birthDate)) {
		echo "エラー";
		exit;
	}

	// CSV に保存
	$csv = "名前,$birthDate\n";
	file_put_contents('/var/www/data/users.csv', $csv, FILE_APPEND);

	echo "登録完了";
}
?>
```

**問題点**: 登録のみで、過去のデータを表示できない

---

### 段階2：登録 + 過去データ表示（RegistUser2 の改善版）

```php
<?php
header('Content-Type: text/html; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// POST 処理：新しいデータを保存
	$name = $_POST['name'] ?? '';
	// ... 保存処理
}

// 過去のデータを読み込んで表示
$data = file_get_contents('/var/www/data/users.csv');
echo "<h2>登録済みユーザー</h2>";
echo "<pre>" . htmlspecialchars($data) . "</pre>";
?>
```

**改善点**: データ表示機能を追加
**問題点**: 常に全データを表示。ポーリング（自動更新）ができない

---

### 段階3：Chat.php の実装（GET/POST を明確に分離）

```php
<?php
header('Content-Type: text/html; charset=utf-8');

$action = $_GET['action'] ?? '';  // ← GET パラメータで処理を分岐

if ($action === 'get') {
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// GET 処理：データを読み込んで HTML で返す
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━
	displayMessages();  // ← メッセージを HTML タグで返す

} elseif ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// POST 処理：フォームデータを受け取って保存
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━
	$name = $_POST['name'] ?? '';
	$comment = $_POST['comment'] ?? '';

	// 検証
	// ...

	// CSV に保存
	// ...

	// 結果ページ表示
	displayResult("成功", "メッセージが投稿されました");
}
?>
```

---

## GET と POST の分岐

### 重要な違い

| 項目 | POST | GET |
|------|------|-----|
| 用途 | **データ送信**（登録、投稿） | **データ取得**（一覧表示、検索） |
| リクエスト方法 | フォーム送信、`fetch()` POST | URL パラメータ、`fetch()` GET、ブラウザロード |
| 取得方法 | `$_POST['名前']` | `$_GET['parameter']`（またはパラメータ） |
| セキュリティ | URL に見えない（相対的に安全） | URL に見える（注意が必要） |
| 用例 | チャットメッセージ投稿 | メッセージ一覧取得 |

### コード例：Post vs Get

#### POST リクエスト（フォーム送信）
```html
<!-- HTML フォーム -->
<form action="http://localhost/chat.php" method="POST">
	<input type="text" name="name" placeholder="名前">
	<textarea name="comment" placeholder="コメント"></textarea>
	<button type="submit">送信</button>
</form>
```

```php
<?php
// PHP 側（POST 処理）
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	$name = $_POST['name'];      // ← $_POST から取得
	$comment = $_POST['comment'];

	// データ保存処理...
}
?>
```

#### GET リクエスト（JavaScript の fetch）
```javascript
// JavaScript 側
fetch('chat.php?action=get')  // ← URL にパラメータを追加
	.then(response => response.text())
	.then(html => {
		document.getElementById('messages').innerHTML = html;
	});
```

```php
<?php
// PHP 側（GET 処理）
$action = $_GET['action'];  // ← $_GET から取得

if ($action === 'get') {
	// メッセージ読み込み処理...
	echo "<div>メッセージ</div>";
}
?>
```

### 処理フローの全体図

```
┌─────────────────────────────────────────────────────────────┐
│ ブラウザで chat.html を開く                                   │
└──────────────────────┬──────────────────────────────────────┘
					   │
		┌──────────────┴──────────────┐
		│                             │
	ページロード時               ユーザー操作
	（DOMLoaded）             （フォーム送信）
		│                             │
		↓                             ↓
  [GET リクエスト]            [POST リクエスト]
  chat.php?action=get        chat.php（フォームデータ）
		│                             │
		├─ PHP: displayMessages()   ├─ PHP: handlePostMessage()
		│   ├─ CSV読込               │   ├─ 入力値検証
		│   ├─ 解析                  │   ├─ CSV保存
		│   └─ HTML生成 ─────────┐   │   └─ 結果ページ表示
		│                       │   │
		└──────────────┬────────┘   │
					   │            │
				 [HTMLで返却]     [新ページ表示]
					   │            │
			 document.innerHTML   =  │
			 メッセージが表示    ユーザー → リダイレクト
									  │
								   [GET リクエスト]
								   chat.html 読込
									  │
						  （ここから また自動更新が始まる）
```

---

## データの保存と展開

### 保存：POST リクエスト → CSV 記録

#### ステップ1：HTML フォームから送信
```html
<!-- chat.html より -->
<form action="http://localhost/chat.php" method="POST" onsubmit="return validateForm()">
	<input type="text" id="name" name="name" placeholder="山田太郎">
	<textarea id="comment" name="comment" placeholder="メッセージ"></textarea>
	<button type="submit">送信</button>
</form>
```

#### ステップ2：PHP で受け取り、検証、保存
```php
<?php
// chat.php より（POST 処理）
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	$name = $_POST['name'] ?? '';
	$comment = $_POST['comment'] ?? '';

	// 検証
	$errors = array();
	if (empty($name)) {
		$errors[] = "名前が空";
	}
	if (empty($comment)) {
		$errors[] = "コメントが空";
	}

	if (!empty($errors)) {
		displayResult("エラー", implode("<br>", $errors));
		return;
	}

	// CSV にデータを追加
	$timestamp = date('Y-m-d H:i:s');

	// CSV エスケープ処理（ダブルクォートを2つにする）
	$csvName = '"' . str_replace('"', '""', $name) . '"';
	$csvComment = '"' . str_replace('"', '""', $comment) . '"';

	// CSV 形式でファイルに追記
	$csvLine = $timestamp . ',' . $csvName . ',' . $csvComment . "\n";
	file_put_contents('/var/www/data/chat.csv', $csvLine, FILE_APPEND);

	displayResult("成功", "メッセージが投稿されました");
}
?>
```

**CSV ファイルの内容:**
```csv
入力日,入力時刻,名前,コメント
"2024-01-17 10:30:45","山田太郎","こんにちは"
"2024-01-17 10:31:15","鈴木花子","返信です"
```

---

### 展開：GET リクエスト → CSV 読込 → HTML で表示

#### ステップ1：JavaScript が定期的に GET リクエストを送信
```javascript
// chat.html より
function autoRefresh() {
	fetch('chat.php?action=get')  // ← GET リクエスト送信
		.then(response => response.text())
		.then(html => {
			// サーバーから返ってきた HTML を画面に表示
			document.getElementById('chatMessages').innerHTML = html;
		});
}

// ページロード時と5秒ごとに実行
window.addEventListener('DOMContentLoaded', function() {
	autoRefresh();
	setInterval(autoRefresh, 5000);  // 5秒ごと
});
```

#### ステップ2：PHP が CSV を読み込んで HTML に変換
```php
<?php
// chat.php より（GET 処理）
function displayMessages() {
	$chatFile = '/var/www/data/chat.csv';

	if (!file_exists($chatFile)) {
		echo "<div class='no-messages'>メッセージがまだありません</div>";
		return;
	}

	// CSV ファイルを開く
	$handle = fopen($chatFile, 'r');
	$messages = array();
	$isHeader = true;

	// CSV を1行ずつ読み込む
	while (($line = fgets($handle)) !== false) {
		// ヘッダー行はスキップ
		if ($isHeader) {
			$isHeader = false;
			continue;
		}

		// CSV を解析（ダブルクォート処理に対応）
		$data = parseCSVLine($line);

		// フィールドが3つ以上あれば、メッセージとして保存
		if (count($data) >= 3) {
			$messages[] = array(
				'datetime' => $data[0],
				'name' => $data[1],
				'comment' => $data[2]
			);
		}
	}
	fclose($handle);

	// 新しいメッセージが上に来るように逆順
	$messages = array_reverse($messages);

	// HTML を生成して画面に返す
	foreach ($messages as $msg) {
		echo "
		<div class='message'>
			<div class='message-header'>
				<span class='message-name'>" . htmlspecialchars($msg['name']) . "</span>
				<span class='message-time'>" . htmlspecialchars($msg['datetime']) . "</span>
			</div>
			<div class='message-content'>" . nl2br(htmlspecialchars($msg['comment'])) . "</div>
		</div>
		";
	}
}
?>
```

**生成される HTML:**
```html
<div class='message'>
	<div class='message-header'>
		<span class='message-name'>山田太郎</span>
		<span class='message-time'>2024-01-17 10:30:45</span>
	</div>
	<div class='message-content'>こんにちは</div>
</div>
<div class='message'>
	<div class='message-header'>
		<span class='message-name'>鈴木花子</span>
		<span class='message-time'>2024-01-17 10:31:15</span>
	</div>
	<div class='message-content'>返信です</div>
</div>
```

---

## 実装の詳細

### 1. CSV パーサー：ダブルクォートに対応

Why？ CSV 形式は、カンマやダブルクォートが含まれる文字列を安全に保存するため、以下のルールがあります：

- フィールドはダブルクォートで囲む
- ダブルクォートは2つに置換してエスケープ

**例:**
```
名前が 「山田太郎」、コメントが 「彼曰く"完璧"」の場合

保存時:
"山田太郎","彼曰く""完璧"""

解析時:
- ダブルクォートで囲まれている → 削除
- 連続するダブルクォート「""」→ 「"」に変換
結果:
山田太郎
彼曰く"完璧"
```

```php
function parseCSVLine($line) {
	$fields = array();
	$field = '';
	$insideQuotes = false;

	$line = rtrim($line, "\r\n");
	$length = strlen($line);

	// 1文字ずつ処理
	for ($i = 0; $i < $length; $i++) {
		$char = $line[$i];

		if ($char === '"') {
			if ($insideQuotes) {
				// ダブルクォート内かクォートの終わり
				if (isset($line[$i + 1]) && $line[$i + 1] === '"') {
					// 連続するダブルクォート → 1つの「"」に変換
					$field .= '"';
					$i++;  // ← 次の「"」はスキップ
				} else {
					// クォートの終わり
					$insideQuotes = false;
				}
			} else {
				// クォートの開始
				$insideQuotes = true;
			}
		} elseif ($char === ',' && !$insideQuotes) {
			// カンマはフィールド区切り（ただしクォート内は除く）
			$fields[] = $field;
			$field = '';
		} else {
			// 通常の文字
			$field .= $char;
		}
	}

	// 最後のフィールドを追加
	$fields[] = $field;

	// ダブルクォート処理：外側のクォートを削除
	$cleanedFields = array();
	foreach ($fields as $f) {
		if (strlen($f) >= 2 && $f[0] === '"' && $f[strlen($f) - 1] === '"') {
			$f = substr($f, 1, -1);
		}
		$cleanedFields[] = trim($f);
	}

	return $cleanedFields;
}
```

### 2. セキュリティ対策：XSS 防止

**htmlspecialchars() の重要性:**

ユーザーが入力した文字列には悪意あるコードが含まれている可能性があります。

```
ユーザーが以下を入力した場合:
<script>alert('ハッキング');</script>

htmlspecialchars() なし:
<script>alert('ハッキング');</script>
→ ブラウザがスクリプトを実行！危険

htmlspecialchars() あり:
&lt;script&gt;alert(&#039;ハッキング&#039;);&lt;/script&gt;
→ テキストとして表示される（安全）
```

**正しい使用方法:**
```php
// ❌ 危険
echo "<div>" . $userInput . "</div>";

// ✅ 安全
echo "<div>" . htmlspecialchars($userInput) . "</div>";

// ✅ 改行も反映したい場合
echo "<div>" . nl2br(htmlspecialchars($userInput)) . "</div>";
```

### 3. ファイル権限の管理

```php
// ファイルを作成する時
file_put_contents($chatFile, $header);
chmod($chatFile, 0666);  // 読み書き可能に

// ファイルが読み込めない場合は権限を修正
if (!is_readable($chatFile)) {
	chmod($chatFile, 0666);
}

// ファイルが書き込めない場合は権限を修正
if (!is_writable($chatFile)) {
	chmod($chatFile, 0666);
}
```

---

## 実装の詳細（フロー整理）

### 完全なリクエスト/レスポンス フロー

```
【初期状態】ユーザーが chat.html を開く
	↓
	DOMContentLoaded イベント発火
	↓
	autoRefresh() 実行
	↓
┌─────────────────────────────────────────────────┐
│ [GET リクエスト #1]                             │
│ fetch('chat.php?action=get')                    │
│     ↓                                            │
│ PHP: displayMessages()                          │
│   1. chat.csv を開く                            │
│   2. 1行ずつ読み込み                           │
│   3. parseCSVLine() で解析                      │
│   4. HTML を生成                                │
│   5. echo で返却                                │
│     ↓                                            │
│ [HTML レスポンス]                                │
│ <div class='message'>...{メッセージ1}...</div> │
│ <div class='message'>...{メッセージ2}...</div> │
│     ↓                                            │
│ JavaScript: .innerHTML に代入                    │
│     ↓                                            │
│ 画面に表示されるメッセージ                       │
└─────────────────────────────────────────────────┘
	↓ 5秒待機
	↓ [GET リクエスト #2] ... 繰り返す

【ユーザーがメッセージを投稿】
	↓
	フォーム送信
	↓
┌─────────────────────────────────────────────────┐
│ [POST リクエスト]                                │
│ form action="chat.php" method="POST"            │
│   ┌─ name=「山田太郎」                          │
│   └─ comment=「こんにちは」                    │
│     ↓                                            │
│ PHP: handlePostMessage()                        │
│   1. $_POST から取得                            │
│   2. 入力値を検証                               │
│   3. csvEscape() でエスケープ                   │
│   4. file_put_contents() で追記                 │
│   5. displayResult() で結果ページを表示         │
│     ↓                                            │
│ [HTML レスポンス]                                │
│ <h1>投稿結果</h1>                               │
│ <p>メッセージが投稿されました</p>              │
│ <a href='chat.html'>チャットに戻る</a>         │
│     ↓                                            │
│ ユーザーが「チャットに戻る」をクリック          │
└─────────────────────────────────────────────────┘
	↓
	chat.html 読込
	↓
	DOMContentLoaded イベント発火
	↓
	autoRefresh() で最新メッセージを取得（上のフローに戻る）
```

---

## 実装演習

### 演習1：RegistUser2.php をベースに Chat.php を作成する

**タスク:**
1. RegistUser2.php をコピーして新しいファイルを作成
2. 以下の6つの関数を追加実装

**必要な関数:**

```php
// 1. GET/POST 分岐の追加
$action = $_GET['action'] ?? '';
if ($action === 'get') {
	// TODO: displayMessages() を呼び出す
} elseif ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// TODO: handlePostMessage() を呼び出す
}

// 2. handlePostMessage() 関数
// 入力値の受け取り → 検証 → CSV保存 → 結果表示

// 3. displayMessages() 関数
// CSV読込 → 行解析 → HTML生成 → 出力

// 4. parseCSVLine() 関数
// CSV行をパース

// 5. csvEscape() 関数
// CSV用にエスケープ

// 6. displayResult() 関数
// 結果ページのHTML生成
```

### 演習2：CSV 解析ロジックの動作確認

**テストケース:**

```
入力: "山田太郎","こんにちは"
期待: ["山田太郎", "こんにちは"]

入力: "鈴木花子","彼曰く""完璧""です"
期待: ["鈴木花子", """彼曰く"完璧"です"]

入力: "李明","2024-01-17 10:30:45"
期待: ["李明", "2024-01-17 10:30:45"]
```

### 演習3：JavaScript の自動更新

**実装要件:**

```javascript
// 1. DOMContentLoaded で初回取得
window.addEventListener('DOMContentLoaded', function() {
	// TODO: autoRefresh() を実行
});

// 2. 5秒ごとに実行
// TODO: setInterval(autoRefresh, 5000)

// 3. autoRefresh() で GET リクエスト
function autoRefresh() {
	fetch('chat.php?action=get')
		.then(response => response.text())
		.then(html => {
			document.getElementById('chatMessages').innerHTML = html;
		});
}
```

---

## デバッグのポイント

### サーバー側の確認

```bash
# ログを確認
tail -f /var/log/apache2/error.log
tail -f /var/log/nginx/error.log

# CSV ファイルを確認
cat /var/www/data/chat.csv
hexdump -C /var/www/data/chat.csv  # 改行文字まで確認

# ファイル権限を確認
ls -la /var/www/data/chat.csv
```

### ブラウザ側の確認

```javascript
// F12 開発者ツール → Console でデバッグ
fetch('chat.php?action=get')
	.then(r => r.text())
	.then(html => {
		console.log('受け取った HTML:', html);
		console.log('HTML 長:', html.length);
	});
```

### よくあるエラーと解決方法

| エラー | 原因 | 解決方法 |
|--------|------|---------|
| 「メッセージがまだあります」 | parseCSVLine() のバグ | CSV パーサーをテスト |
| メッセージが重複表示 | array_reverse() がない | メッセージ順序の確認 |
| メッセージが保存されない | ファイル権限不足 | chmod 確認 |
| XSS 脆弱性 | htmlspecialchars() がない | 全エスケープを確認 |

---

## まとめ

### Chat.php の実装のキーポイント

1. **GET/POST の分岐**
   - `$action = $_GET['action'] ?? ''` で分岐
   - POST: データ受け取り → 保存 → 結果ページ
   - GET: データ読込 → HTML生成 → 返却

2. **CSV の安全な保存**
   - `csvEscape()` でダブルクォート処理
   - `htmlspecialchars()` で XSS 対策

3. **CSV の解析**
   - `parseCSVLine()` で RFC 4180 対応
   - クォート内のカンマを区切り文字にしない

4. **HTML 動的生成**
   - PHP で echo で HTML タグを生成
   - JavaScript で fetch してキャッシュして表示

5. **自動更新の実装**
   - `setInterval()` で定期GET リクエスト
   - `.innerHTML = html` で画面更新

この実装パターンは、Web アプリケーション開発の基本的なフローです。
