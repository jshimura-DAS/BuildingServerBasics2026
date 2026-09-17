<?php
header('Content-Type: text/html; charset=utf-8');

// エラーレポートを有効化（デバッグ用）
// 本番環境では display_errors = Off に設定してください
error_reporting(E_ALL & ~E_NOTICE & ~E_DEPRECATED);
ini_set('display_errors', 0);
ini_set('log_errors', 1);

// PHPのバージョン情報と環境情報をログ
error_log("=== Chat.php Debug Info ===");
error_log("PHP Version: " . phpversion());
error_log("Server User: " . get_current_user());
error_log("Current Working Directory: " . getcwd());

// チャットデータを扱う基本処理
$chatFile = '/var/www/data/chat.csv';
$chatDataDir = '/var/www/data';

error_log("Chat file path: " . $chatFile);
error_log("Chat dir exists: " . (is_dir($chatDataDir) ? "yes" : "no"));
error_log("Chat file exists: " . (file_exists($chatFile) ? "yes" : "no"));

// データディレクトリが存在しない場合は作成
if (!is_dir($chatDataDir)) {
	if (!mkdir($chatDataDir, 0777, true)) {
		error_log("Failed to create directory: " . $chatDataDir);
	} else {
		error_log("Directory created: " . $chatDataDir);
	}
}

// ディレクトリの書き込み権限を確認
if (!is_writable($chatDataDir)) {
	error_log("Directory is not writable: " . $chatDataDir);
	// 権限を変更してみる
	if (@chmod($chatDataDir, 0777)) {
		error_log("Successfully changed permissions: " . $chatDataDir);
	} else {
		error_log("Failed to change permissions: " . $chatDataDir);
	}
}

// リクエストの種類を判定
$action = $_GET['action'] ?? '';

if ($action === 'get') {
	// メッセージ一覧を取得して表示
	displayMessages();
} elseif ($_SERVER['REQUEST_METHOD'] === 'POST') {
	// 新しいメッセージを投稿
	handlePostMessage();
} else {
	echo "POSTリクエストまたはGETパラメータが必要です";
}

/**
 * メッセージを投稿処理
 */
function handlePostMessage() {
	global $chatFile, $chatDataDir;

	$name = $_POST['name'] ?? '';
	$comment = $_POST['comment'] ?? '';

	// === サーバー側検証 ===

	$errors = array();

	// 1. 名前のチェック
	if (empty($name)) {
		$errors[] = "名前が入力されていません";
	} elseif (strlen($name) < 2) {
		$errors[] = "名前は2文字以上で入力してください";
	} elseif (strlen($name) > 50) {
		$errors[] = "名前は50文字以下で入力してください";
	}

	// 2. コメントのチェック
	if (empty($comment)) {
		$errors[] = "コメントが入力されていません";
	} elseif (strlen($comment) > 1000) {
		$errors[] = "コメントは1000文字以下で入力してください";
	}

	if (!empty($errors)) {
		// エラーがある場合は結果ページを表示
		displayResult("エラー", implode("<br>", $errors));
		return;
	}

	// CSVファイルにデータを保存
	$timestamp = date('Y-m-d H:i:s');
	$csvLine = $timestamp . ',' . csvEscape($name) . ',' . csvEscape($comment) . "\n";

	// ファイルが存在しない場合は、ヘッダーを先に作成
	if (!file_exists($chatFile)) {
		$header = "入力日,入力時刻,名前,コメント\n";
		$headerResult = file_put_contents($chatFile, $header);
		if ($headerResult === false) {
			error_log("Failed to create header in chat file: " . $chatFile);
			displayResult("エラー", "チャットファイルの初期化に失敗しました。サーバー管理者に連絡してください。");
			return;
		}
		chmod($chatFile, 0666);
	}

	// ファイルに追記
	$writeResult = file_put_contents($chatFile, $csvLine, FILE_APPEND | LOCK_EX);
	if ($writeResult === false) {
		error_log("Failed to write to chat file: " . $chatFile);
		displayResult("エラー", "メッセージの保存に失敗しました。サーバー管理者に連絡してください。");
		return;
	}

	// ファイルの権限を確認
	if (file_exists($chatFile) && !is_writable($chatFile)) {
		chmod($chatFile, 0666);
	}

	displayResult("成功", "メッセージが投稿されました");
}

/**
 * CSV形式でのエスケープ処理
 * @param string $field フィールド値
 * @return string エスケープされた値
 */
function csvEscape($field) {
	// ダブルクォートを2つに置換
	$field = str_replace('"', '""', $field);
	// フィールドをダブルクォートで囲む
	return '"' . $field . '"';
}

/**
 * メッセージ一覧を表示
 */
function displayMessages() {
	global $chatFile;

	if (!file_exists($chatFile)) {
		echo "<div class='no-messages'>メッセージがまだありません</div>";
		return;
	}

	// ファイルの読み込み権限を確認
	if (!is_readable($chatFile)) {
		error_log("Chat file is not readable: " . $chatFile);
		echo "<div class='no-messages'>メッセージファイルが読み込めません</div>";
		return;
	}

	$messages = array();
	$handle = @fopen($chatFile, 'r');

	if (!$handle) {
		error_log("Failed to open chat file: " . $chatFile);
		echo "<div class='no-messages'>メッセージファイルを開けません</div>";
		return;
	}

	$isHeader = true;
	$lineCount = 0;

	while (($line = fgets($handle)) !== false) {
		$lineCount++;

		// ヘッダーをスキップ
		if ($isHeader) {
			$isHeader = false;
			continue;
		}

		// 空行をスキップ
		if (trim($line) === '') {
			continue;
		}

		// CSVを解析
		$data = parseCSVLine($line);

		// デバッグログ
		error_log("Line $lineCount parsed: " . count($data) . " fields");

		if (count($data) >= 3) {
			$messages[] = array(
				'datetime' => isset($data[0]) ? $data[0] : '',
				'name' => isset($data[1]) ? $data[1] : '',
				'comment' => isset($data[2]) ? $data[2] : ''
			);
		} else {
			error_log("Line $lineCount has insufficient fields: " . implode("|", $data));
		}
	}
	fclose($handle);

	// メッセージを逆順にして最新のものから表示
	$messages = array_reverse($messages);

	if (empty($messages)) {
		error_log("No messages found in chat file after parsing");
		echo "<div class='no-messages'>メッセージがまだありません</div>";
		return;
	}

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

/**
 * CSV行を解析する関数（RFC 4180準拠）
 * @param string $line CSV形式の行
 * @return array 解析されたフィールド配列
 */
function parseCSVLine($line) {
	$fields = array();
	$field = '';
	$insideQuotes = false;

	// 改行文字を削除
	$line = rtrim($line, "\r\n");
	$length = strlen($line);

	for ($i = 0; $i < $length; $i++) {
		$char = $line[$i];

		if ($char === '"') {
			if ($insideQuotes) {
				// クォート内かクォートの終わり
				if (isset($line[$i + 1]) && $line[$i + 1] === '"') {
					// 連続するダブルクォート（エスケープされた"）
					$field .= '"';
					$i++;
				} else {
					// クォートの終わり
					$insideQuotes = false;
				}
			} else {
				// クォートの開始
				$insideQuotes = true;
			}
		} elseif ($char === ',' && !$insideQuotes) {
			// カンマで区切る（クォート外の場合のみ）
			$fields[] = $field;
			$field = '';
		} else {
			$field .= $char;
		}
	}

	// 最後のフィールドを追加
	$fields[] = $field;

	// 各フィールドの両端の空白とダブルクォートをトリム
	$cleanedFields = array();
	foreach ($fields as $f) {
		// ダブルクォートで囲まれている場合は削除
		if (strlen($f) >= 2 && $f[0] === '"' && $f[strlen($f) - 1] === '"') {
			$f = substr($f, 1, -1);
		}
		$cleanedFields[] = trim($f);
	}

	return $cleanedFields;
}

/**
 * 結果ページを表示
 * @param string $result 成功/エラー
 * @param string $message メッセージ
 */
function displayResult($result, $message) {
	echo "
<!DOCTYPE html>
<html lang='ja'>
<head>
	<meta charset='UTF-8'>
	<title>投稿結果</title>
	<style>
		body {
			font-family: Arial, sans-serif;
			max-width: 600px;
			margin: 50px auto;
			padding: 20px;
		}

		.result {
			padding: 20px;
			border-radius: 4px;
			margin-bottom: 20px;
		}

		.success {
			background-color: #d4edda;
			border: 1px solid #c3e6cb;
			color: #155724;
		}

		.error {
			background-color: #f8d7da;
			border: 1px solid #f5c6cb;
			color: #721c24;
		}

		a {
			display: inline-block;
			margin-top: 20px;
			padding: 10px 20px;
			background-color: #4CAF50;
			color: white;
			text-decoration: none;
			border-radius: 4px;
		}

		a:hover {
			background-color: #45a049;
		}
	</style>
	<script>
		// 3秒後に自動的にリダイレクト
		setTimeout(function() {
			window.location.href = 'chat.html';
		}, 3000);
	</script>
</head>
<body>
	<h1>投稿結果</h1>
	<div class='result " . ($result === "成功" ? "success" : "error") . "'>
		<h2>" . htmlspecialchars($result) . "</h2>
		<p>" . $message . "</p>
		<p>3秒後に自動的にチャットページに戻ります...</p>
	</div>
	<a href='chat.html'>チャットに戻る</a>
</body>
</html>
	";
}
?>
