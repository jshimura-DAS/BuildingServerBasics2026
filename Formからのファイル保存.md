# Formからのファイル保存

## ユーザー登録とCSVファイル保存

### 概要

名前と生年月日をフォーム入力し、データの妥当性をクライアント＆サーバー側で確認した後、CSV形式でファイルに保存するアプリケーションです。

**ポイント**:
- **クライアント側検証**: 基本的な形式チェック（HTML5の `type="date"` で無効な日付は送信不可）
- **サーバー側検証**: 実際に存在する日付かを確認
- **ファイル操作**: PHP のファイルIO関数を使用してCSVに保存
- **データ追記**: 既存ファイルがあれば追記、なければ作成

### RegistUser.html（ユーザー登録フォーム）

```html
<!DOCTYPE html>
<html lang="ja">
<head>
	<meta charset="UTF-8">
	<title>ユーザー登録</title>
	<style>
		body {
			font-family: Arial, sans-serif;
			max-width: 500px;
			margin: 50px auto;
			padding: 20px;
		}
		.form-group {
			margin-bottom: 15px;
		}
		label {
			display: block;
			margin-bottom: 5px;
			font-weight: bold;
		}
		input[type="text"],
		input[type="date"] {
			width: 100%;
			padding: 8px;
			border: 1px solid #ccc;
			border-radius: 4px;
			box-sizing: border-box;
		}
		button {
			background-color: #4CAF50;
			color: white;
			padding: 10px 20px;
			border: none;
			border-radius: 4px;
			cursor: pointer;
			font-size: 16px;
		}
		button:hover {
			background-color: #45a049;
		}
		.error {
			color: red;
			font-size: 12px;
			margin-top: 3px;
		}
	</style>
	<script>
		// クライアント側の検証
		function validateForm() {
			const name = document.getElementById('name').value.trim();
			const birthDate = document.getElementById('birthDate').value;

			// 名前が空でないか確認
			if (!name) {
				alert('名前を入力してください');
				return false;
			}

			// 名前が3文字以上か確認
			if (name.length < 2) {
				alert('名前は2文字以上で入力してください');
				return false;
			}

			// 生年月日が入力されているか確認
			if (!birthDate) {
				alert('生年月日を入力してください');
				return false;
			}

			// 生年月日が今日以降ではないか確認
			const birthDateObj = new Date(birthDate);
			const today = new Date();
			if (birthDateObj >= today) {
				alert('生年月日は今日より前の日付を入力してください');
				return false;
			}

			return true;
		}
	</script>
</head>
<body>
	<h1>ユーザー登録</h1>
	<form action="http://192.168.1.100:8080/RegistUser.php" method="POST" onsubmit="return validateForm()">
		<div class="form-group">
			<label for="name">名前:</label>
			<input type="text" id="name" name="name" placeholder="山田太郎" required>
			<p class="error">※ 2文字以上で入力してください</p>
		</div>

		<div class="form-group">
			<label for="birthDate">生年月日:</label>
			<input type="date" id="birthDate" name="birthDate" required>
			<p class="error">※ 今日より前の日付を入力してください</p>
		</div>

		<button type="submit">登録する</button>
		<button type="reset">クリア</button>
	</form>
</body>
</html>
```

### RegistUser.php（サーバー側処理）

```php
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
		$csvFile = __DIR__ . '/users.csv';
		$timestamp = date('Y-m-d H:i:s');

		// CSVの1行を作成
		$csvLine = $name . ',' . $birthDate . ',' . $timestamp . "\n";

		// ファイルに追記
		if (file_put_contents($csvFile, $csvLine, FILE_APPEND)) {
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
```

### ファイルIO関数の解説

PHP でファイル操作を行う主な関数：

#### 1. `file_put_contents()` - ファイルに内容を書き込む（最も簡単）

```php
// ファイルに1行追記
file_put_contents($filepath, $data, FILE_APPEND);
```

**パラメータ**:
- `$filepath`: ファイルパス
- `$data`: 書き込む内容
- `FILE_APPEND`: ファイルの末尾に追記するフラグ（省略時は上書き）

**戻り値**: 書き込んだバイト数（失敗時は false）

**メリット**: コードが短く、簡単
**デメリット**: 大量のファイル操作には向かない

---

#### 2. `fopen()` + `fwrite()` + `fclose()` - より詳細な制御

```php
// ファイルを開く
$file = fopen($filepath, 'a');  // 'a' = 追記モード

// ファイルに書き込む
if ($file) {
	fwrite($file, $data);
	fclose($file);
}
```

**fopen() のモード**:
- `'r'`: 読み込み（ファイルが存在しなければエラー）
- `'w'`: 書き込み（既存ファイルは上書き）
- `'a'`: 追記（ファイルが存在しなければ作成）
- `'x'`: 書き込み（ファイルが存在すればエラー）

**メリット**: より柔軟な制御が可能
**デメリット**: コードが長い

---

#### 3. `file_get_contents()` - ファイル全体を読み込む

```php
// ファイル全体を文字列として読み込む
$content = file_get_contents($filepath);
```

---

#### 4. `file()` - ファイルを行ごとに配列に読み込む

```php
// ファイルを行ごとに配列に格納
$lines = file($filepath);
foreach ($lines as $line) {
	echo trim($line) . "<br>";
}
```

---

#### 5. `checkdate()` - 日付の妥当性をチェック

```php
// PHP 組み込み関数
checkdate($month, $day, $year);  // true or false

// 例：2024年2月29日は有効
checkdate(2, 29, 2024);  // true

// 例：2023年2月29日は無効（平年）
checkdate(2, 29, 2023);  // false
```

---

### CSV ファイルの形式

RegistUser.php で保存されるCSVファイル（users.csv）の形式：

```
名前,生年月日,登録日時
山田太郎,1990-05-15,2024-01-20 14:30:45
鈴木花子,1985-12-20,2024-01-20 14:35:22
佐藤次郎,1995-03-10,2024-01-20 14:40:10
```

**読み込み例**:
```php
$lines = file(__DIR__ . '/users.csv');
foreach ($lines as $line) {
	$data = str_getcsv($line);  // CSV を配列に分割
	echo "名前: " . $data[0] . ", 生年月日: " . $data[1] . "<br>";
}
```

---

### セキュリティと注意点

#### 1. **入力値の検証** - SQLインジェクション等を防ぐ
```php
// 名前に特殊文字が含まれていないかチェック
$name = preg_replace('/[^ぁ-んァ-ヴー一-龥a-zA-Z0-9 ・\-]/u', '', $name);
```

#### 2. **出力時の HTML エスケープ** - XSS 対策
```php
// ユーザー入力をそのまま出力しない
echo htmlspecialchars($name, ENT_QUOTES, 'UTF-8');
```

#### 3. **ファイルパーミッション** - ファイルの読み書き権限を確認
```php
// ファイルが書き込み可能か確認
if (is_writable(dirname($csvFile))) {
	// 書き込み可能
}
```

#### 4. **ファイルアップロード対策** - CSV ヘッダーの管理
```php
// ファイルが存在しない場合はヘッダーを追加
if (!file_exists($csvFile)) {
	file_put_contents($csvFile, "名前,生年月日,登録日時\n");
}
file_put_contents($csvFile, $csvLine, FILE_APPEND);
```

---

## Linux + Apache 環境でのファイル保存パス

### 概要

Linux サーバー上で Apache を実行している場合、CSV などのデータファイルをどこに保存するかは、**セキュリティ**と**アクセス権限**の観点から重要な決定です。

### 一般的なディレクトリ構成

```
/var/www/
├── html/                    ← Apacheドキュメントルート（ウェブからアクセス可能）
│   ├── index.php
│   ├── RegistUser.html
│   ├── RegistUser.php
│   └── ...
├── data/                    ← データ保存用（ウェブからアクセス不可）
│   ├── users.csv
│   └── ...
└── log/                     ← ログファイル用
```

### ファイル保存先の選択

#### **推奨: ドキュメントルート外に保存する** ✅

```php
<?php
// ファイル保存パス（ドキュメントルート外）
$csvFile = '/var/www/data/users.csv';

// または、相対パスで親ディレクトリを指定
$csvFile = dirname(__DIR__) . '/data/users.csv';
?>
```

**メリット**:
- ウェブブラウザから直接アクセスできない（セキュリティが高い）
- ユーザーが任意に CSV をダウンロード/削除できない
- データの整合性が保たれやすい

**注意点**:
- `/var/www/data/` ディレクトリが存在する必要がある
- ディレクトリの書き込み権限を Apache プロセスに付与する必要がある

---

#### **非推奨: ドキュメントルート内に保存する** ❌

```php
<?php
// ファイル保存パス（ドキュメントルート内）
$csvFile = __DIR__ . '/users.csv';  // /var/www/html/users.csv
?>
```

**デメリット**:
- ウェブブラウザから直接 CSV ファイルへアクセス可能
- `http://example.com/users.csv` でダウンロード可能
- 個人情報の漏洩リスクが高い
- 保存されたファイル一覧を確認される可能性

---

### ディレクトリの準備と権限設定

#### 1. データ保存ディレクトリを作成

```bash
# サーバーのコマンドラインで実行
sudo mkdir -p /var/www/data
sudo chmod 755 /var/www/data
```

#### 2. Apache プロセスの所有者を確認

```bash
# Apache プロセスの実行ユーザーを確認（通常は www-data）
ps aux | grep apache

# または
ps aux | grep httpd
```

#### 3. ディレクトリの書き込み権限を設定

```bash
# Apache（www-data ユーザー）に書き込み権限を付与
sudo chown www-data:www-data /var/www/data
sudo chmod 755 /var/www/data
```

#### 4. ファイルの権限確認

```bash
# 作成されたファイルの権限を確認
ls -la /var/www/data/users.csv

# 出力例：
# -rw-r--r-- 1 www-data www-data 1234 Jan 20 14:30 /var/www/data/users.csv
```

---

### RegistUser.php での実装例

```php
<?php
header('Content-Type: text/html; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
	$name = $_POST['name'] ?? '';
	$birthDate = $_POST['birthDate'] ?? '';

	// 検証処理（省略）...

	if ($allValidationPassed) {
		// ===== 推奨: ドキュメントルート外に保存 =====
		$csvFile = '/var/www/data/users.csv';

		// または、相対パスを使用
		// $csvFile = dirname(dirname(__FILE__)) . '/data/users.csv';

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

	// レスポンス出力...
}
?>
```

---

### ファイルパスの使い分け

| パス | 用途 | セキュリティ | 備考 |
|------|------|------------|------|
| `/var/www/html/` | ウェブページ、画像、CSS | ⚠️ 低 | ウェブからアクセス可能 |
| `/var/www/data/` | ユーザーデータ（CSV等） | ✅ 高 | ウェブからアクセス不可 |
| `/tmp/` または `/var/tmp/` | 一時ファイル | ⚠️ 中 | サーバー再起動で消えることがある |
| `/home/username/` | ユーザーホーム | ⚠️ 中 | 権限管理が複雑 |

---

### トラブルシューティング

#### 問題: 「Permission denied」エラーが出る

```
Warning: file_put_contents(/var/www/data/users.csv): failed to open stream: Permission denied
```

**原因**: Apache プロセスにファイルの書き込み権限がない

**解決策**:
```bash
# ディレクトリの所有者と権限を確認
ls -ld /var/www/data

# 所有者を www-data に変更
sudo chown www-data:www-data /var/www/data

# 権限を 755 に設定
sudo chmod 755 /var/www/data

# 既存ファイルの権限も確認
sudo chown www-data:www-data /var/www/data/users.csv
sudo chmod 644 /var/www/data/users.csv
```

---

#### 問題: 「No such file or directory」エラーが出る

```
Warning: file_put_contents(/var/www/data/users.csv): failed to open stream: No such file or directory
```

**原因**: `/var/www/data/` ディレクトリが存在しない

**解決策**:
```bash
# ディレクトリを作成
sudo mkdir -p /var/www/data

# 権限を設定
sudo chmod 755 /var/www/data
```

---

### ベストプラクティス

```php
<?php
// 1. ファイルパスの定義（定数化）
define('DATA_DIR', '/var/www/data');
define('CSV_FILE', DATA_DIR . '/users.csv');

// 2. ディレクトリの存在確認
if (!is_dir(DATA_DIR)) {
	mkdir(DATA_DIR, 0755, true);
}

// 3. ディレクトリの書き込み可能確認
if (!is_writable(DATA_DIR)) {
	die('ディレクトリが書き込み可能ではありません');
}

// 4. ファイルに追記
$csvLine = $name . ',' . $birthDate . ',' . date('Y-m-d H:i:s') . "\n";
$bytesWritten = file_put_contents(CSV_FILE, $csvLine, FILE_APPEND | LOCK_EX);

if ($bytesWritten === false) {
	die('ファイルへの書き込みに失敗しました');
}

// 5. ファイルパーミッションの確認
if (fileperms(CSV_FILE) & 0x0004) {
	// ワールド読み込み可能なので、権限を制限
	chmod(CSV_FILE, 0640);
}
?>
```

**ポイント**:
- `LOCK_EX` フラグでファイルロックを使用（複数プロセスからのアクセスを安全に）
- ディレクトリとファイルの権限を適切に管理
- ファイルパスを定数化して、コード内で統一

