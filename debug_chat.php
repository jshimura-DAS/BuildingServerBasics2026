#!/usr/bin/env php
<?php
/**
 * chat.csv のデバッグスクリプト
 * コマンドライン: php debug_chat.php
 */

echo "=== Chat.csv Debug Tool ===\n\n";

$chatFile = '/var/www/data/chat.csv';

echo "1. ファイル存在確認\n";
echo "   ファイルパス: $chatFile\n";
echo "   存在: " . (file_exists($chatFile) ? "yes" : "no") . "\n";

if (!file_exists($chatFile)) {
	echo "\nファイルが存在しません。終了します。\n";
	exit(1);
}

echo "\n2. ファイル権限確認\n";
$perms = fileperms($chatFile);
echo "   権限（8進数）: " . substr(sprintf('%o', $perms), -4) . "\n";
echo "   読み込み可能: " . (is_readable($chatFile) ? "yes" : "no") . "\n";
echo "   書き込み可能: " . (is_writable($chatFile) ? "yes" : "no") . "\n";

echo "\n3. ファイルサイズ\n";
echo "   サイズ: " . filesize($chatFile) . " bytes\n";

echo "\n4. ファイル内容（最初の1000文字）\n";
$content = file_get_contents($chatFile);
if (strlen($content) > 1000) {
	echo substr($content, 0, 1000) . "...\n";
} else {
	echo $content . "\n";
}

echo "\n5. CSV解析\n";
$handle = fopen($chatFile, 'r');
$lineNum = 0;
$dataLines = 0;

if ($handle) {
	$isHeader = true;
	while (($line = fgets($handle)) !== false) {
		$lineNum++;
		echo "   Line $lineNum: ";

		if ($isHeader) {
			echo "(header) ";
			$isHeader = false;
		} else {
			$dataLines++;
		}

		// 改行を除去して表示
		$displayLine = trim($line);
		if (strlen($displayLine) > 80) {
			echo substr($displayLine, 0, 80) . "...\n";
		} else {
			echo $displayLine . "\n";
		}
	}
	fclose($handle);
}

echo "\n6. 統計\n";
echo "   総行数: $lineNum\n";
echo "   データ行数: $dataLines\n";

echo "\n=== End Debug ===\n";
?>
