# Formからのファイル保存

## 1. 基本的なファイル操作方法

PHPでCSV保存に使う基本関数です。

### `file_put_contents()`（推奨）

```php
$bytes = file_put_contents($filePath, $line, FILE_APPEND | LOCK_EX);
if ($bytes === false) {
	echo '保存に失敗しました';
}
```

- `FILE_APPEND`: 追記
- `LOCK_EX`: 同時書き込みの競合を抑止

### `fopen()` / `fwrite()` / `fclose()`

```php
$fp = fopen($filePath, 'a');
if ($fp) {
	fwrite($fp, $line);
	fclose($fp);
}
```

---

## 2. 保存先ディレクトリの設定

保存先を `/var/www/data` にする例です。

```php
<?php
$saveDir = '/var/www/data';
$csvFile = $saveDir . '/users.csv';

if (!is_dir($saveDir)) {
	mkdir($saveDir, 0755, true);
}

if (!is_writable($saveDir)) {
	die('保存先ディレクトリに書き込み権限がありません');
}
```

Ubuntu側の準備例:

```bash
sudo mkdir -p /var/www/data
sudo chown www-data:www-data /var/www/data
sudo chmod 755 /var/www/data
```

---

## 3. フォームとPHPの例（日付の入力チェックはなし）

この段階では、日付の妥当性チェックは行いません。

### RegistUser.html

```html
<!DOCTYPE html>
<html lang="ja">
<head>
	<meta charset="UTF-8">
	<title>ユーザー登録</title>
</head>
<body>
	<h1>ユーザー登録</h1>
	<form action="RegistUser.php" method="POST">
		<div>
			<label for="name">名前:</label>
			<input type="text" id="name" name="name" required>
		</div>

		<div>
			<label for="birthDate">生年月日:</label>
			<input type="date" id="birthDate" name="birthDate" required>
		</div>

		<button type="submit">登録</button>
	</form>
</body>
</html>
```

### RegistUser.php

```php
<?php
header('Content-Type: text/html; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
	exit('POSTリクエストが必要です');
}

$name = trim($_POST['name'] ?? '');
$birthDate = $_POST['birthDate'] ?? '';

if ($name === '') {
	exit('名前が入力されていません');
}

$saveDir = '/var/www/data';
$csvFile = $saveDir . '/users.csv';

if (!is_dir($saveDir)) {
	mkdir($saveDir, 0755, true);
}

if (!is_writable($saveDir)) {
	exit('保存先ディレクトリに書き込み権限がありません');
}

$timestamp = date('Y-m-d H:i:s');
$csvLine = $name . ',' . $birthDate . ',' . $timestamp . "\n";

if (!file_exists($csvFile)) {
	file_put_contents($csvFile, "名前,生年月日,登録日時\n");
}

$ok = file_put_contents($csvFile, $csvLine, FILE_APPEND | LOCK_EX);

echo $ok === false ? '保存に失敗しました' : '登録が完了しました';
```

---

## 4. 入力チェックを追加する方法（日付の妥当性を確認する部分を追加する方法）

`RegistUser.php` に、次の2点を追加します。

1. `isValidDate()` 関数を追加
2. 保存前に `isValidDate($birthDate)` を使って判定

### 追加する関数

```php
function isValidDate($date) {
	$parts = explode('-', $date);
	if (count($parts) !== 3) {
		return false;
	}

	$year = (int)$parts[0];
	$month = (int)$parts[1];
	$day = (int)$parts[2];

	return checkdate($month, $day, $year);
}
```

### 保存前チェックに追加

```php
if (!isValidDate($birthDate)) {
	exit('無効な日付形式です');
}
```

必要に応じて、未来日を不可にするチェックも追加できます。

```php
if (strtotime($birthDate) >= strtotime(date('Y-m-d'))) {
	exit('生年月日は今日より前の日付を入力してください');
}
```
