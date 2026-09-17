# コード例解説：GET/POST の分岐と HTML 展開の詳細

## 目次
1. [リクエストの種類判定](#リクエストの種類判定)
2. [POST リクエストの処理フロー](#post-リクエストの処理フロー)
3. [GET リクエストの処理フロー](#get-リクエストの処理フロー)
4. [HTML 動的生成の詳細](#html-動的生成の詳細)
5. [実装例の比較](#実装例の比較)

---

## リクエストの種類判定

### PHPでのリクエスト判定方法

```php
<?php
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法1：REQUEST_METHOD で POST/GET を判定
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	echo "ユーザーが フォームを送信しました（POST）";
} elseif ($_SERVER['REQUEST_METHOD'] === 'GET') {
	echo "通常のページロードまたはリンククリック（GET）";
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法2：GET パラメータで処理を細分化
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// URL: chat.php?action=get
$action = $_GET['action'] ?? 'default';

switch ($action) {
	case 'get':
		echo "メッセージ一覧を取得";
		break;
	case 'search':
		echo "メッセージを検索";
		break;
	default:
		echo "デフォルト処理";
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法3：Chat.php のように組み合わせ
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

$action = $_GET['action'] ?? '';

if ($action === 'get') {
	// GET パラメータで 「get」が指定 → データ取得処理
	displayMessages();

} elseif ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// POST リクエスト → データ保存処理
	handlePostMessage();

} else {
	// どちらでもない → エラーメッセージ
	echo "不正なリクエストです";
}
?>
```

### リクエストの種類を視覚化

```
【ブラウザからのリクエスト】

1. ページをロード
   URL: http://localhost/chat.html
   ↓
   REQUEST_METHOD = GET （ページロードのため）
   GET パラメータは없음
   ※ HTTP ヘッダのみ送信

2. メッセージ投稿フォーム送信
   <form action="chat.php" method="POST">
   ↓
   REQUEST_METHOD = POST
   POST ボディに: name=「山田太郎」&comment=「こんにちは」

3. JavaScript で GET リクエスト実行
   fetch('chat.php?action=get')
   ↓
   REQUEST_METHOD = GET
   GET パラメータ: ?action=get
   ※ Request Headers のみで、Body はない
```

---

## POST リクエストの処理フロー

### 完全な POST 処理の流れ

```php
<?php
// ═══════════════════════════════════════════════════════
// ステップ1：リクエストが POST かどうか確認
// ═══════════════════════════════════════════════════════

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// ───────────────────────────────────
	// ステップ2：フォームデータを取得
	// ───────────────────────────────────

	// $_POST は連想配列で、キーがフォームの name 属性
	$name = $_POST['name'] ?? '';      // HTML: <input name="name">
	$comment = $_POST['comment'] ?? '';  // HTML: <textarea name="comment"></textarea>

	// 例外処理（取得できない場合は空文字列）
	// ?? は null 合体演算子


	// ───────────────────────────────────
	// ステップ3：入力値を検証
	// ───────────────────────────────────

	$errors = array();  // エラーメッセージを格納

	// 名前の検証
	if (empty($name)) {
		$errors[] = "名前が入力されていません";
	} elseif (strlen($name) < 2) {
		$errors[] = "名前は2文字以上で入力してください";
	} elseif (strlen($name) > 50) {
		$errors[] = "名前は50文字以下で入力してください";
	}

	// コメントの検証
	if (empty($comment)) {
		$errors[] = "コメントが入力されていません";
	} elseif (strlen($comment) > 1000) {
		$errors[] = "コメントは1000文字以下で入力してください";
	}

	// エラーがあればここで結果を返して処理終了
	if (!empty($errors)) {
		displayResult("エラー", implode("<br>", $errors));
		return;  // 以下の処理に進まない
	}


	// ───────────────────────────────────
	// ステップ4：タムスタンプを追加
	// ───────────────────────────────────

	$timestamp = date('Y-m-d H:i:s');  // 現在の日時を取得
	// 例: "2024-01-17 14:30:45"


	// ───────────────────────────────────
	// ステップ5：CSV フォーマットで保存用に整形
	// ───────────────────────────────────

	// CSV は以下のフォーマット：
	// タムスタンプ,名前,コメント\n

	// だが、コメントに「,」や「"」が含まれていると
	// CSV パーサーが誤解することがあるので、エスケープが必要

	// 例：
	// 入力: コメント = 私が言った「完璧」です
	// エスケープ後:
	//   "私が言った""完璧""です"  （ダブルクォート対応）

	$csvName = csvEscape($name);       // "山田太郎"
	$csvComment = csvEscape($comment);   // "こんにちは"

	// CSV 行を作成
	$csvLine = $timestamp . ',' . $csvName . ',' . $csvComment . "\n";
	// 結果: "2024-01-17 14:30:45","山田太郎","こんにちは"\n


	// ───────────────────────────────────
	// ステップ6：ファイルが存在しなければ作成
	// ───────────────────────────────────

	$chatFile = '/var/www/data/chat.csv';

	if (!file_exists($chatFile)) {
		// ファイルが存在しない場合：ヘッダー行を作成
		$header = "入力日,入力時刻,名前,コメント\n";
		$headerResult = file_put_contents($chatFile, $header);

		if ($headerResult === false) {
			error_log("Failed to create header in chat file");
			displayResult("エラー", "ファイルの初期化に失敗しました");
			return;
		}

		// ファイル作成直後：権限を設定（読み書き可能に）
		chmod($chatFile, 0666);
	}


	// ───────────────────────────────────
	// ステップ7：CSV 行をファイルに追記
	// ───────────────────────────────────

	// file_put_contents() は、以下の使い方で追記できる
	// - 第3引数: FILE_APPEND
	// - オプション: LOCK_EX（ファイルロック）

	$writeResult = file_put_contents(
		$chatFile,            // ファイルパス
		$csvLine,             // 追記する内容
		FILE_APPEND | LOCK_EX // フラグ：追記 + ロック
	);

	if ($writeResult === false) {
		error_log("Failed to write to chat file");
		displayResult("エラー", "メッセージの保存に失敗しました");
		return;
	}


	// ───────────────────────────────────
	// ステップ8：ファイル権限の確認
	// ───────────────────────────────────

	if (file_exists($chatFile) && !is_writable($chatFile)) {
		chmod($chatFile, 0666);  // 権限を修正
	}


	// ───────────────────────────────────
	// ステップ9：成功ページを表示
	// ───────────────────────────────────

	displayResult("成功", "メッセージが投稿されました");
}
?>
```

### POST リクエストの実装補助関数

```php
<?php
/**
 * CSV エスケープ処理
 * @param string $field フィールド値
 * @return string エスケープされた値
 */
function csvEscape($field) {
	// ステップ1：ダブルクォートを2つに置換
	// 例: "完璧" → ""完璧""
	$field = str_replace('"', '""', $field);

	// ステップ2：フィールド全体をダブルクォートで囲む
	// 例: ""完璧"" → """完璧"""
	return '"' . $field . '"';
}

// 使用例
$input = 'コメント: 完璧なチャット';
$escaped = csvEscape($input);
// 結果: "コメント: 完璧なチャット"

$input2 = 'She said "Hello"';
$escaped2 = csvEscape($input2);
// 結果: "She said ""Hello"""
?>
```

---

## GET リクエストの処理フロー

### 完全な GET 処理の流れ

```php
<?php
// ═══════════════════════════════════════════════════════
// ステップ1：GET パラメータを確認
// ═══════════════════════════════════════════════════════

$action = $_GET['action'] ?? '';
// URL が「chat.php?action=get」の場合、$action = 'get'
// ?action=get がない場合、$action = ''（空文字列）

if ($action === 'get') {
	// ───────────────────────────────────
	// ステップ2：CSV ファイルを開く
	// ───────────────────────────────────

	$chatFile = '/var/www/data/chat.csv';

	if (!file_exists($chatFile)) {
		// ファイルがない場合
		echo "<div class='no-messages'>メッセージがまだありません</div>";
		return;
	}

	// ファイルポインタを取得
	$handle = fopen($chatFile, 'r');
	// 'r' = 読み込みモード

	if (!$handle) {
		// ファイルを開けなかった場合
		error_log("Failed to open chat file: $chatFile");
		echo "<div class='no-messages'>ファイルを開けません</div>";
		return;
	}


	// ───────────────────────────────────
	// ステップ3：CSV をメモリに読み込む
	// ───────────────────────────────────

	$messages = array();  // メッセージの配列
	$isHeader = true;     // ヘッダーフラグ
	$lineCount = 0;       // デバッグ用：行番号

	// fgets() で1行ずつ読み込む
	while (($line = fgets($handle)) !== false) {
		$lineCount++;

		// ヘッダー行（1行目）をスキップ
		if ($isHeader) {
			// 1行目は「入力日,入力時刻,名前,コメント」
			// この行は無視して、次の行から処理
			$isHeader = false;
			continue;  // ← continue で残りをスキップして次の反復へ
		}

		// 空行をスキップ
		if (trim($line) === '') {
			continue;
		}

		// ───────────────────────────────
		// ステップ4：CSV 行をパース
		// ───────────────────────────────

		$data = parseCSVLine($line);
		// 入力:  "2024-01-17 14:30:45","山田太郎","こんにちは"
		// 出力:  ["2024-01-17 14:30:45", "山田太郎", "こんにちは"]

		// ───────────────────────────────
		// ステップ5：データをメモリに格納
		// ───────────────────────────────

		if (count($data) >= 3) {
			// フィールドが3個以上あれば有効
			$messages[] = array(
				'datetime' => $data[0],     // 日時
				'name' => $data[1],         // 名前
				'comment' => $data[2]       // コメント
			);
		} else {
			// フィールド数が不足している場合
			error_log("Line $lineCount has insufficient fields");
		}
	}

	// ファイルを閉じる
	fclose($handle);


	// ───────────────────────────────────
	// ステップ6：メッセージを逆順に並べ替え
	// ───────────────────────────────────

	// CSV は古いメッセージから順に追記されているので、
	// 配列も古い順になっている
	// 画面には新しいメッセージを上に表示したいので、
	// array_reverse() で逆順にする

	$messages = array_reverse($messages);
	// 例：
	// 逆順前: [メッセージ1, メッセージ2, メッセージ3]
	// 逆順後: [メッセージ3, メッセージ2, メッセージ1]


	// ───────────────────────────────────
	// ステップ7：メッセージが存在するか確認
	// ───────────────────────────────────

	if (empty($messages)) {
		// メッセージがなければ
		echo "<div class='no-messages'>メッセージがまだありません</div>";
		return;
	}


	// ───────────────────────────────────
	// ステップ8：メッセージを HTML に変換して出力
	// ───────────────────────────────────

	foreach ($messages as $msg) {
		// ダブルクォーテーションで HTML 文字列を構築
		echo "
		<div class='message'>
			<div class='message-header'>
				<span class='message-name'>" . htmlspecialchars($msg['name']) . "</span>
				<span class='message-time'>" . htmlspecialchars($msg['datetime']) . "</span>
			</div>
			<div class='message-content'>" . nl2br(htmlspecialchars($msg['comment'])) . "</div>
		</div>
		";

		// 補足：
		// htmlspecialchars() = XSS 対策（ユーザー入力をエスケープ）
		// nl2br() = 改行を <br> に変換（複数行コメント対応）
	}
}
?>
```

### GET リクエスト処理の補助関数

```php
<?php
/**
 * CSV 行をパース（RFC 4180準拠）
 * @param string $line CSV形式の行
 * @return array 解析されたフィールド配列
 */
function parseCSVLine($line) {
	$fields = array();
	$field = '';
	$insideQuotes = false;

	// 改行文字を削除（改行を含めずに長さ計算）
	$line = rtrim($line, "\r\n");
	$length = strlen($line);

	// 1文字ずつ処理
	for ($i = 0; $i < $length; $i++) {
		$char = $line[$i];

		// ━━━━━━━━━━━━━━━━━━━
		// ケース1：ダブルクォート
		// ━━━━━━━━━━━━━━━━━
		if ($char === '"') {
			if ($insideQuotes) {
				// クォート内にいる場合
				if (isset($line[$i + 1]) && $line[$i + 1] === '"') {
					// 次の文字も「"」の場合 → エスケープ処理
					// 「""」=エスケープされた「"」を意味する
					$field .= '"';
					$i++;  // 次の「"」をスキップ
				} else {
					// 次の文字が「"」でない → クォートの終わり
					$insideQuotes = false;
				}
			} else {
				// クォート外 → クォートの開始
				$insideQuotes = true;
			}
		}

		// ━━━━━━━━━━━━━━━━━━━
		// ケース2：カンマ（フィールド区切り）
		// ━━━━━━━━━━━━━━━━━
		elseif ($char === ',' && !$insideQuotes) {
			// クォート外のカンマのみ区切り文字として機能
			// クォート内のカンマは無視される
			$fields[] = $field;
			$field = '';
		}

		// ━━━━━━━━━━━━━━━━━━━
		// ケース3：通常の文字
		// ━━━━━━━━━━━━━━━━━
		else {
			$field .= $char;
		}
	}

	// 最後のフィールドを追加
	$fields[] = $field;

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// 後処理：外側のダブルクォートを削除
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// 入力:  ["山田太郎", ""コメント""]
	// 出力:  [山田太郎, "コメント"]

	$cleanedFields = array();
	foreach ($fields as $f) {
		if (strlen($f) >= 2 && $f[0] === '"' && $f[strlen($f) - 1] === '"') {
			// 最初と最後が「"」で始まると終わる場合は削除
			$f = substr($f, 1, -1);
		}
		// 前後の空白も削除
		$cleanedFields[] = trim($f);
	}

	return $cleanedFields;
}
?>
```

---

## HTML 動的生成の詳細

### PHP で HTML を生成する仕組み

```php
<?php
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法1：echo で直接出力
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━

$name = "山田太郎";
$comment = "こんにちは";

// 通常の出力
echo "名前: $name";
echo "コメント: $comment";

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法2：ダブルクォーテーション文字列で HTML を生成
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━

echo "
<div class='message'>
	<div class='message-header'>
		<span class='message-name'>$name</span>
	</div>
	<div class='message-content'>$comment</div>
</div>
";

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法3：ヒアドキュメント（複数行）
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━

echo <<<HTML
<div class='message'>
	<div class='message-header'>
		<span class='message-name'>$name</span>
	</div>
	<div class='message-content'>$comment</div>
</div>
HTML;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 方法4：連結演算子（.）で組み立て
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━

$html = "<div class='message'>";
$html .= "<div class='message-header'>";
$html .= "<span class='message-name'>" . htmlspecialchars($name) . "</span>";
$html .= "</div>";
$html .= "</div>";
echo $html;
?>
```

### JavaScriptで受け取ったHTMLを画面に表示

```javascript
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ステップ1：サーバーからHTMLを取得
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

fetch('chat.php?action=get')
	// chat.php から返ってくるレスポンスを受け取る

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// ステップ2：テキストとして解析
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	.then(response => response.text())
	// response.text() = レスポンスボディを文字列に変換
	// 返り値:
	// <div class='message'>...(メッセージ1)...</div>
	// <div class='message'>...(メッセージ2)...</div>

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// ステップ3：DOMに代入して画面に反映
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	.then(html => {
		// html 変数に、PHP が echo した HTML 文字列が格納されている

		// ID が 「chatMessages」の要素を取得
		const chatMessages = document.getElementById('chatMessages');

		// .innerHTML で要素の内容を置き換え
		// （古い内容は削除される）
		chatMessages.innerHTML = html;

		// ブラウザが HTML を解析して画面に表示
	})

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// ステップ4：エラーハンドリング
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	.catch(error => {
		console.error('エラー:', error);
		chatMessages.innerHTML = "<div class='no-messages'>読み込みに失敗しました</div>";
	});
```

### HTML 展開の具体例

```
【PHP の出力】
echo "
<div class='message'>
	<div class='message-header'>
		<span class='message-name'>山田太郎</span>
		<span class='message-time'>2024-01-17 10:30:45</span>
	</div>
	<div class='message-content'>こんにちは</div>
</div>
";

echo "
<div class='message'>
	<div class='message-header'>
		<span class='message-name'>鈴木花子</span>
		<span class='message-time'>2024-01-17 10:31:15</span>
	</div>
	<div class='message-content'>返信です</div>
</div>
";

↓↓↓ ネットワークで送信 ↓↓↓

【JavaScript（ブラウザ）が受け取る HTML 文字列】
"
<div class='message'>
	...
</div>
<div class='message'>
	...
</div>
"

↓↓↓ .innerHTML = html ↓↓↓

【ブラウザで表示される画面】
┌─────────────────────────────────┐
│ チャット                        │
├─────────────────────────────────┤
│ 山田太郎                       │
│ 2024-01-17 10:30:45            │
│                                 │
│ こんにちは                     │
├─────────────────────────────────┤
│ 鈴木花子                       │
│ 2024-01-17 10:31:15            │
│                                 │
│ 返信です                       │
└─────────────────────────────────┘
```

---

## 実装例の比較

### RegistUser2.php（登録のみ）

```php
<?php
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// POST のみ処理
	// データ受け取り → 検証 → 保存 → 結果表示
	// 以上で処理完了
}
// GET リクエストは処理しない
?>
```

### Chat.php（登録 + 表示）

```php
<?php
$action = $_GET['action'] ?? '';

if ($action === 'get') {
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// 【GET 処理】データの取得と HTML 生成
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// CSV を読み込み
	$handle = fopen('/var/www/data/chat.csv', 'r');

	// 1行ずつ処理
	while (($line = fgets($handle)) !== false) {
		$data = parseCSVLine($line);
		// メッセージを HTML に変換
		echo "<div class='message'>...</div>";
	}

	fclose($handle);
	// GET 処理完了

} elseif ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	// 【POST 処理】データの受け取りと保存
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	$name = $_POST['name'] ?? '';
	$comment = $_POST['comment'] ?? '';

	// 検証と保存
	file_put_contents('/var/www/data/chat.csv', $csvLine, FILE_APPEND);

	// 結果ページを表示
	echo "<h1>投稿完了</h1>";
	// POST 処理完了
}
?>
```

---

## まとめ

### Key Concepts

| 概念 | RegistUser2 | Chat |
|------|------------|------|
| リクエスト種別 | POST のみ | POST + GET |
| データ流 | ユーザー → サーバー | 双方向 |
| 画面更新 | 1回限定 | 連続更新 |
| 用途 | 初期登録 | メッセージ投稿・表示 |

### 実装の流れ

1. **リクエスト判定** → GET/POST を分岐
2. **POST処理** → データ受け取り → 検証 → CSV保存 → 結果表示
3. **GET処理** → CSV読込 → 解析 → HTML生成 → 返却
4. **JavaScript** → 定期GET実行 → HTMLを.innerHTML で表示

このパターンは、リアルタイムチャットアプリケーションの基本です。
