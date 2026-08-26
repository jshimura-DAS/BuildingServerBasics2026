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