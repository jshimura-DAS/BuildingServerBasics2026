<?php
header('Content-Type: text/html; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	$name = $_POST['name'] ?? '';
	$birthDate = $_POST['birthDate'] ?? '';

	// === クライアント側検証の再確認（サーバー側セキュリティ） ===

	// 1. 名前のチェック
	if (empty($name)) {
		$result = "エラー";
		$message = "名前が入力されていません";
	} elseif (strlen($name) < 2) {
		$result = "エラー";
		$message = "名前は2文字以上で入力してください";
	}
	// 2. 生年月日のチェック
	elseif (empty($birthDate)) {
		$result = "エラー";
		$message = "生年月日が入力されていません";
	}
	// 3. 日付形式のチェック
	elseif (!isValidDate($birthDate)) {
		$result = "エラー";
		$message = "無効な日付形式です";
	}
	// 4. 生年月日が今日以降ではないかチェック
	elseif (strtotime($birthDate) >= strtotime(date('Y-m-d'))) {
		$result = "エラー";
		$message = "生年月日は今日より前の日付を入力してください";
	}
	else {
		// すべての検証を通過したので、CSVファイルに保存
		$csvFile = '/var/www/data/users.csv';
		$timestamp = date('Y-m-d H:i:s');
		$csvLine = $name . ',' . $birthDate . ',' . $timestamp . "\n";

		// ファイルが存在しない場合は、ヘッダーを先に作成
		if (!file_exists($csvFile)) {
			file_put_contents($csvFile, "名前,生年月日,登録日時\n");
		}

		// ファイルに追記
		if (file_put_contents($csvFile, $csvLine, FILE_APPEND)) {
			// ファイルパーミッションを確認して修正
			if (!is_readable($csvFile)) {
				chmod($csvFile, 0644);
			}

			$result = "成功";
			$message = "登録が完了しました";
		} else {
			$result = "エラー";
			$message = "ファイルへの保存に失敗しました";
		}
	}

	// HTMLレスポンスを返す
	echo "
<!DOCTYPE html>
<html lang='ja'>
<head>
    <meta charset='UTF-8'>
    <title>登録結果</title>
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

        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

            table th, table td {
                border: 1px solid #ddd;
                padding: 10px;
                text-align: left;
            }

            table th {
                background-color: #f2f2f2;
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
</head>
<body>
    <h1>登録結果</h1>
    <div class='result " . ($result === "成功" ? "success" : "error") . "'>
        <h2>" . htmlspecialchars($result) . "</h2>
        <p>" . htmlspecialchars($message) . "</p>
    </div>

    " . ($result === "成功" ? "
    <table>
        <tr>
            <th>項目</th>
            <th>内容</th>
        </tr>
        <tr>
            <td>名前</td>
            <td>" . htmlspecialchars($name) . "</td>
        </tr>
        <tr>
            <td>生年月日</td>
            <td>" . htmlspecialchars($birthDate) . "</td>
        </tr>
        <tr>
            <td>登録日時</td>
            <td>" . $timestamp . "</td>
        </tr>
    </table>
    " : "") . "

    <a href='RegistUser.html'>戻る</a>
</body>
</html>
	";
} else {
	echo "POSTリクエストが必要です";
}

/**
 * 日付が有効か判定する関数
 * @param string $date 日付文字列 (YYYY-MM-DD)
 * @return bool 有効な日付の場合 true
 */
function isValidDate($date) {
    //return true;    // ここでは常にtrueを返すようにしています。必要に応じて実装してください。	
    // 以下、日付の妥当性をチェックするコード例です。必要に応じて有効化してください。
    $dateArray = explode('-', $date);
	if (count($dateArray) !== 3) {
		return false;
	}

	$year = intval($dateArray[0]);
	$month = intval($dateArray[1]);
	$day = intval($dateArray[2]);

	// checkdate()はPHP組み込み関数で、日付の妥当性をチェック
	return checkdate($month, $day, $year);
}
?>