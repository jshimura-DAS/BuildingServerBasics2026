# 修正サマリー：Uninitialized string offset エラーの解決

## 問題
**警告:** `Uninitialized string offset 70 in /var/www/html/chat.php on line 233`

## 原因
`parseCSVLine()` 関数で、改行文字を削除する前に計算した文字列長 `$length` を、改行文字削除後も使用していたため、範囲外のインデックスにアクセスしていました。

### 問題のコード
```php
function parseCSVLine($line) {
	$field = '';
	$insideQuotes = false;
	$length = strlen($line);  // ← 改行文字を含めた長さ

	// 改行文字を削除
	$line = rtrim($line, "\r\n");  // ← ここでは長さが変わるが...

	for ($i = 0; $i < $length; $i++) {  // ← 古い長さでアクセス → 範囲外！
		$char = $line[$i];
		// ...
	}
}
```

例：改行文字を含めて全体が70文字だった場合
- 改行文字削除前：`strlen($line) = 70`
- 改行文字削除後：実際は68文字になっているのに、ループは70回実行
- 結果：インデックス68, 69 にアクセス → 範囲外！

## 解決方法
改行文字を削除した**後に**文字列長を計算し直します。

### 修正されたコード
```php
function parseCSVLine($line) {
	$field = '';
	$insideQuotes = false;

	// 改行文字を削除
	$line = rtrim($line, "\r\n");
	$length = strlen($line);  // ← 削除後に長さを計算

	for ($i = 0; $i < $length; $i++) {  // ← 正しい長さでアクセス
		$char = $line[$i];
		// ...
	}
}
```

## 実装済みの修正

### chat.php
- ✅ `parseCSVLine()` 関数を修正
- ✅ エラーレポート設定を改善（E_NOTICE を除外）

### RegistUser2.php
- ✅ エラーレポート設定を一貫性のため改善

## エラーレポート設定の改善

```php
// 修正前
error_reporting(E_ALL);
ini_set('display_errors', 1);

// 修正後
error_reporting(E_ALL & ~E_NOTICE & ~E_DEPRECATED);
ini_set('display_errors', 0);
ini_set('log_errors', 1);
```

### 変更理由
- **E_NOTICE を除外**: 初期化されていない変数などの不要な警告を削減
- **E_DEPRECATED を除外**: 廃止予定機能の警告を削減
- **display_errors = 0**: エラーを画面に表示しない（本番環境安全性向上）
- **log_errors = 1**: エラーをログファイルに記録（トラブルシューティング用）

## テスト方法

### ブラウザのコンソールで確認
1. F12 キーで開発者ツールを開く
2. Console タブを選択
3. ページをリロード
4. エラーが表示されていないことを確認

### サーバーログで確認
```bash
# Apache エラーログを確認
tail -f /var/log/apache2/error.log

# Nginx エラーログを確認
tail -f /var/log/nginx/error.log

# PHP エラーログを確認
tail -f /var/log/php-fpm.log
```

Uninitialized string offset の警告が出力されていないことを確認してください。

## チェックリスト
- [ ] chat.php が最新版になっている
- [ ] RegistUser2.php が最新版になっている
- [ ] ブラウザのコンソールにエラーが表示されていない
- [ ] チャット機能が正常に動作している
- [ ] メッセージ投稿時に警告が出力されていない
