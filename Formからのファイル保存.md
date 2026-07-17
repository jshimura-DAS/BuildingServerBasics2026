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

$writeError = '';
set_error_handler(function ($severity, $message) use (&$writeError) {
	$writeError = $message;
	return true;
});

$ok = file_put_contents($csvFile, $csvLine, FILE_APPEND | LOCK_EX);
restore_error_handler();

if ($ok === false) {
	echo '保存に失敗しました';
	if ($writeError !== '') {
		echo '<br>ファイルシステムエラー: ' . $writeError;
	}
} else {
	echo '<h2>登録が完了しました</h2>';
	echo '<table border="1" cellpadding="6">';
	echo '<tr><th>項目</th><th>内容</th></tr>';
	echo '<tr><td>名前</td><td>' . $name . '</td></tr>';
	echo '<tr><td>生年月日</td><td>' . $birthDate . '</td></tr>';
	echo '<tr><td>登録日時</td><td>' . $timestamp . '</td></tr>';
	echo '</table>';
}

?>
```

※このサンプルは説明を優先しており、表示部分にXSS対策を入れていません。
実運用用に編集する際は、後述の「## 5. XSS対策」「## 6. RegistUser.php のサンプルコードをXSS対策済み版にする方法」を参照し、`$name` などの表示用変数を `htmlspecialchars()` 適用済みの変数へ置き換えてください。

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
## 5. XSS対策

フォーム入力値をそのままHTMLに出力すると、悪意あるスクリプトが実行される可能性があります。
表示時は `htmlspecialchars()` で必ずエスケープします。

### 基本方針

- **保存時**: 元データをそのまま保存
- **表示時**: `htmlspecialchars()` でエスケープ

### 表示時の実装例

```php
$safeName = htmlspecialchars($name, ENT_QUOTES, 'UTF-8');
$safeBirthDate = htmlspecialchars($birthDate, ENT_QUOTES, 'UTF-8');

echo "<p>名前: {$safeName}</p>";
echo "<p>生年月日: {$safeBirthDate}</p>";
```

### CSV表示時の例

```php
$lines = file($csvFile, FILE_SKIP_EMPTY_LINES);
foreach ($lines as $line) {
	$cols = explode(',', trim($line));
	echo '<tr>';
	foreach ($cols as $col) {
		echo '<td>' . htmlspecialchars($col, ENT_QUOTES, 'UTF-8') . '</td>';
	}
	echo '</tr>';
}
```

### 補足

- `strip_tags()` だけでは不十分です（文脈に応じたエスケープが必要）。
- HTML出力以外（JavaScript文字列、URL、属性値）では、それぞれに合った対策を行ってください。

---

## 6. RegistUser.php のサンプルコードをXSS対策済み版にする方法

以下の方針で `RegistUser.php` を調整します。

1. **入力値の保存前エスケープはしない**（元データを保持）
2. **画面表示時のみ** `htmlspecialchars()` を適用する
3. `htmlspecialchars()` は `ENT_QUOTES` と `UTF-8` を指定する

### 追記・変更ポイント

```php
// 保存処理はそのまま（エスケープしない）
$timestamp = date('Y-m-d H:i:s');
$csvLine = $name . ',' . $birthDate . ',' . $timestamp . "\n";
```

```php
// 表示前にエスケープ
$safeResult = htmlspecialchars($result, ENT_QUOTES, 'UTF-8');
$safeMessage = htmlspecialchars($message, ENT_QUOTES, 'UTF-8');
$safeName = htmlspecialchars($name, ENT_QUOTES, 'UTF-8');
$safeBirthDate = htmlspecialchars($birthDate, ENT_QUOTES, 'UTF-8');
$safeTimestamp = htmlspecialchars($timestamp, ENT_QUOTES, 'UTF-8');
```

```php
// HTML出力部分で安全な変数を使う
echo "<h2>{$safeResult}</h2>";
echo "<p>{$safeMessage}</p>";
echo "<td>{$safeName}</td>";
echo "<td>{$safeBirthDate}</td>";
echo "<td>{$safeTimestamp}</td>";
```

### 最小変更イメージ（結果表示部分）

```php
$safeResult = htmlspecialchars($result, ENT_QUOTES, 'UTF-8');
$safeMessage = htmlspecialchars($message, ENT_QUOTES, 'UTF-8');
$safeName = htmlspecialchars($name, ENT_QUOTES, 'UTF-8');
$safeBirthDate = htmlspecialchars($birthDate, ENT_QUOTES, 'UTF-8');

echo "<div class='result'>";
echo "<h2>{$safeResult}</h2>";
echo "<p>{$safeMessage}</p>";
echo "</div>";

if ($result === '成功') {
	echo "<table>";
	echo "<tr><th>項目</th><th>内容</th></tr>";
	echo "<tr><td>名前</td><td>{$safeName}</td></tr>";
	echo "<tr><td>生年月日</td><td>{$safeBirthDate}</td></tr>";
	echo "</table>";
}
```

この形にすると、既存の保存仕様を変えずに、画面表示時のXSSリスクを抑えられます.

---

## 7. `file_put_contents()` 失敗時に想定される主なエラーと対処方法

### 1. Permission denied（権限不足）

- 想定原因: 保存先ディレクトリやファイルの所有者・パーミッションが不適切
- 対処:
  - `ls -ld /var/www/data` で権限確認
  - `sudo chown www-data:www-data /var/www/data`
  - `sudo chmod 755 /var/www/data`

### 2. No such file or directory（パス不正/ディレクトリ不存在）

- 想定原因: 保存先パスのタイプミス、ディレクトリ未作成
- 対処:
  - `mkdir -p` でディレクトリ作成
  - `realpath()` や `var_dump($csvFile)` で実際の保存先を確認

### 3. Read-only file system（読み取り専用）

- 想定原因: マウント先が読み取り専用
- 対処:
  - `mount` や `findmnt` でマウント状態を確認
  - 必要に応じて書き込み可能な領域へ保存先を変更

### 4. No space left on device / Disk quota exceeded（容量不足/クォータ超過）

- 想定原因: ディスク残量不足、ユーザー割り当て超過
- 対処:
  - `df -h` で空き容量確認
  - 不要ファイル削除、ログローテーション、保存先変更

### 5. Too many open files（ファイルディスクリプタ不足）

- 想定原因: 同時オープン数が上限到達
- 対処:
  - 不要なファイルを確実に閉じる
  - `ulimit -n` を確認し、必要に応じて上限調整

### 6. Is a directory（ファイルパスにディレクトリを指定）

- 想定原因: `$csvFile` がディレクトリを指している
- 対処:
  - `is_dir($csvFile)` で事前チェック
  - ファイル名（例: `users.csv`）まで含めたパスに修正

### 7. Operation not permitted（OSセキュリティ制約）

- 想定原因: SELinux / AppArmor などの制限
- 対処:
  - 監査ログ（`/var/log/audit/audit.log` など）を確認
  - 必要最小限のポリシー調整を実施

### 運用上の推奨

- 画面には概要メッセージのみ表示し、詳細エラーはサーバーログへ出力
- 例:

```php
if ($ok === false) {
	error_log('file_put_contents failed: ' . $writeError . ' path=' . $csvFile);
	echo '保存に失敗しました。管理者に連絡してください。';
}
```


