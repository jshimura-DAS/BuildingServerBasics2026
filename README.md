# BuildingServerBasics2026

## &lt;form&gt;タグの役割、&lt;input type="text"&gt;やname属性、送信ボタンの仕組み

### &lt;form&gt;タグの役割
`<form>`タグはHTMLにおいてユーザー入力を処理するための基本的な要素です。主な役割は以下の通りです：
- ユーザーが入力したデータをサーバーに送信するための枠組みを提供する
- フォーム内のすべての入力要素を一つのグループとしてまとめる
- 送信先のサーバーアドレス（action属性）と送信方法（method属性）を指定する
- フォーム検証やイベント処理の対象となる

#### &lt;form&gt;タグの具体例
```html
<form action="/submit" method="POST">
  <!-- フォーム内の入力要素がここに入る -->
</form>
```
- `action="/submit"` - フォームデータの送信先URL
- `method="POST"` - データ送信方法（POST、またはGET）

#### &lt;form&gt;タグの実践例（ローカルサーバーでの動作）

ローカルネットワーク上で以下のサーバーが動作している場合を想定します：
- **サーバーIP**: `192.168.1.100`
- **ポート番号**: `8080`
- **PHPファイル**: `submit.php`

```html
<form action="http://192.168.1.100:8080/submit.php" method="POST">
  <input type="text" name="username" placeholder="ユーザー名">
  <input type="email" name="email" placeholder="メールアドレス">
  <button type="submit">送信</button>
</form>
```

**フロー説明**:
1. ユーザーがブラウザ（例：`192.168.1.50`）でフォームを入力
2. 送信ボタンをクリック
3. ブラウザが `http://192.168.1.100:8080/submit.php` へPOSTリクエストを送信
4. `192.168.1.100` のマシンのポート `8080` で動作するPHPサーバーが受信
5. `submit.php` が実行される

**submit.php の実装例**:
```php
<?php
// POST データを取得
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $username = $_POST['username'] ?? 'Unknown';
    $email = $_POST['email'] ?? 'Unknown';

    // データの処理（例：ファイル保存、データベース登録）
    $log_message = "[" . date('Y-m-d H:i:s') . "] Username: $username, Email: $email\n";
    file_put_contents('/var/log/submissions.log', $log_message, FILE_APPEND);

    // レスポンスを返す
    echo "<h1>登録完了</h1>";
    echo "<p>ユーザー名: " . htmlspecialchars($username) . "</p>";
    echo "<p>メール: " . htmlspecialchars($email) . "</p>";
    echo "<a href='index.html'>戻る</a>";
} else {
    echo "不正なリクエストです。";
}
?>
```

**ローカルサーバーの起動例（PHPビルトインサーバー）**:
```bash
# ローカルマシン(192.168.1.100)で実行
php -S 192.168.1.100:8080 -t /path/to/webroot
```

### &lt;input type="text"&gt;の役割
`<input type="text">`は単一行のテキスト入力フィールドを作成します：
- ユーザーがテキストを自由に入力できるフィールドを表示する
- 短い文字列（名前、メールアドレス、検索キーワードなど）を入力するのに適している
- 複数行の入力が必要な場合は`&lt;textarea&gt;`タグを使用する

#### &lt;input type="text"&gt;の具体例
```html
<input type="text" name="fullname" placeholder="山田太郎">
<input type="text" name="email" placeholder="user@example.com">
<input type="text" name="search" placeholder="検索キーワードを入力">
```
- `placeholder` - 入力フィールド内に薄く表示されるヒントテキスト

### name属性の役割
`name`属性はフォーム送信時に非常に重要な役割を果たします：
- 入力されたデータを識別するためのキーとなる
- サーバー側で`name`属性の値をキーにしてデータを受け取ることができる
- 例えば`&lt;input type="text" name="username"&gt;`の場合、送信されるデータは`username=入力値`という形式になる
- name属性を指定しないと、そのフィールドのデータはサーバーに送信されない

#### name属性の具体例
```html
<form action="/user/register" method="POST">
  <input type="text" name="username" placeholder="ユーザー名">
  <input type="text" name="email" placeholder="メールアドレス">
</form>
```
送信時のデータ例：
- `username=taro&email=taro@example.com`

### 送信ボタンの仕組み
送信ボタンはフォームデータをサーバーに送信するトリガーとなります：
- `&lt;button type="submit"&gt;送信&lt;/button&gt;`または`&lt;input type="submit" value="送信"&gt;`で作成される
- ユーザーがボタンをクリックすると、フォーム内のすべての入力データが収集される
- `&lt;form&gt;`タグのaction属性で指定されたURLに、method属性で指定された方法（通常はGETまたはPOST）でデータが送信される
- サーバーはこれらのデータを受け取り、処理を実行する

#### 送信ボタンの具体例
```html
<form action="/login" method="POST">
  <input type="text" name="username" placeholder="ユーザー名を入力">
  <input type="text" name="password" placeholder="パスワードを入力">
  <button type="submit">ログイン</button>
</form>
```

### 完全なフォームの例
```html
<form action="/contact" method="POST">
  <label for="name">名前:</label>
  <input type="text" id="name" name="name" placeholder="山田太郎" required>
  
  <label for="email">メールアドレス:</label>
  <input type="text" id="email" name="email" placeholder="user@example.com" required>
  
  <label for="message">メッセージ:</label>
  <textarea id="message" name="message" rows="5" placeholder="メッセージを入力してください"></textarea>
  
     <button type="submit">送信する</button>
     <button type="reset">リセット</button>
  </form>
  ```
  - `id属性` - CSSやJavaScriptで要素を識別するために使用
  - `required属性` - 入力が必須であることを指定
  - `&lt;label&gt;`タグ - 入力フィールドのラベルを関連付ける
  - `type="reset"` - フォームの入力値をすべてクリアするボタン

  ## フォーム送信のシーケンスとGET/POST の違い

  ### フォーム送信の全体シーケンス

  ```
  ブラウザからサーバーへの通信フロー：

  1. [ブラウザ] HTMLを表示
  2. [ユーザー] 入力フィールドに情報を入力
  3. [ユーザー] 送信ボタンをクリック
  4. [ブラウザ] フォームデータを収集（name属性をキーとして）
  5. [ブラウザ] HTTPリクエストを生成（GETまたはPOST）
  6. [ブラウザ] サーバーにリクエストを送信
     ↓
  7. [サーバー] HTTPリクエストを受け取る
  8. [サーバー] name属性をキーにしてデータを抽出
  9. [サーバー] データを処理・検証・保存
  10. [サーバー] 処理結果に応じてHTMLを生成
     ↓
  11. [サーバー] HTTPレスポンス（HTML）を返信
  12. [ブラウザ] 受け取ったHTMLをレンダリング
  13. [ユーザー] 新しいページが表示される
  ```

  ### GET vs POST の違い

  #### GET メソッド
  - **データの場所**: URLに付加される
  - **URL例**: `/login?username=taro&password=pass123`
  - **可視性**: URLバーに表示される（見える）
  - **セキュリティ**: ⚠️ 低い（パスワードなどは不適切）
  - **データ量**: 制限あり（URL長の制限、通常2000文字程度）
  - **キャッシュ**: されやすい
  - **べき等性**: ✅ あり（何度実行しても同じ結果）
  - **主な用途**: 検索、フィルタリング、ページネーション、ブックマーク可能

  #### POST メソッド
  - **データの場所**: リクエストボディに含まれる
  - **URL例**: `/login`（URLにデータは表示されない）
  - **可視性**: 見えない（隠れている）
  - **セキュリティ**: ✅ 高い（URLに表示されないため機密情報に適している）
  - **データ量**: 制限ほぼなし（大容量データに対応）
  - **キャッシュ**: されない
  - **べき等性**: ❌ なし（実行するたび結果が変わる可能性）
  - **主な用途**: ログイン、登録、ファイルアップロード、データベース変更

  ### GET を使う場合の例

  ```html
  <!-- 検索フォーム（データが少なく、繰り返し実行しても問題ない） -->
  <form action="/search" method="GET">
    <input type="text" name="keyword" placeholder="検索キーワード">
    <button type="submit">検索</button>
  </form>
  ```

  **実行フロー**:
  1. ユーザーが「サーバー構築」と入力して送信
  2. URLが `https://example.com/search?keyword=サーバー構築` に変わる
  3. サーバー側で `$_GET['keyword']` で値を取得 → `サーバー構築`
  4. 検索結果を含むHTMLを返す

  **メリット**:
  - URLを友人にシェアできる（同じ検索結果を共有可能）
  - ブックマークでき、後で同じ検索結果を再表示可能

  ### POST を使う場合の例

  ```html
  <!-- ログインフォーム（パスワードなど機密情報を含む） -->
  <form action="/login" method="POST">
    <label for="username">ユーザー名:</label>
    <input type="text" id="username" name="username" placeholder="ユーザー名">

    <label for="password">パスワード:</label>
    <input type="password" id="password" name="password" placeholder="パスワード">

    <button type="submit">ログイン</button>
  </form>
  ```

  **実行フロー**:
  1. ユーザーが情報を入力して送信
  2. URLは `https://example.com/login` のまま（データは見えない）
  3. リクエストボディ内に `username=taro&password=pass123` が含まれる
  4. サーバー側で `$_POST['username']`、`$_POST['password']` で値を取得
  5. 認証処理を実行し、結果に応じてHTMLを返す

  **メリット**:
  - パスワードなど機密情報がURLに表示されない
  - 大量のデータを送信できる

  ### 使い分けの実践例

  | シーン | 推奨 | 理由 |
  |-------|------|------|
  | 🔍 検索・フィルタリング | GET | 結果が確定的で、URLを共有・ブックマークできる |
  | 🔐 ログイン・パスワード | POST | 機密情報は非表示にする |
  | 📝 ユーザー登録 | POST | データ量が多く、機密情報を含む可能性 |
  | 📤 ファイルアップロード | POST | 大容量データに対応 |
  | 💳 決済・重要な操作 | POST | データが変更されるため、べき等性がない必要 |
  | 🔗 ページネーション | GET | ページ番号をURLに含めて共有可能 |
  | 📋 フィルター検索 | GET | 複数条件をURLで管理、ブックマーク可能 |


  ## 実際にやってみよう

  ### うるう年判定アプリケーション

  年、月、日を入力して、その年がうるう年かどうかを判定するアプリケーションの実装例です。POST と GET の2つの方法を紹介します。

  #### POST を使う場合

  **フォーム（HTML）**:
  ```html
  <!DOCTYPE html>
  <html lang="ja">
  <head>
      <meta charset="UTF-8">
      <title>うるう年判定（POST）</title>
  </head>
  <body>
      <h1>うるう年判定ツール（POST例）</h1>
      <form action="http://192.168.1.100:8080/leap_year.php" method="POST">
          <label for="year">年:</label>
          <input type="number" id="year" name="year" placeholder="例: 2024" required>

          <label for="month">月:</label>
          <input type="number" id="month" name="month" min="1" max="12" placeholder="例: 2" required>

          <label for="day">日:</label>
          <input type="number" id="day" name="day" min="1" max="31" placeholder="例: 29" required>

          <button type="submit">判定する</button>
      </form>
  </body>
  </html>
  ```

  **サーバー側のPHP（leap_year.php）**:
  ```php
  <?php
  header('Content-Type: text/html; charset=utf-8');

  if ($_SERVER['REQUEST_METHOD'] === 'POST') {
      $year = intval($_POST['year'] ?? 0);
      $month = intval($_POST['month'] ?? 0);
      $day = intval($_POST['day'] ?? 0);

      // うるう年の判定ロジック
      $isLeapYear = isLeapYear($year);

      // 日付の妥当性チェック
      if ($month < 1 || $month > 12) {
          $message = "月の値が不正です（1-12）";
          $result = "エラー";
      } else if ($day < 1 || $day > 31) {
          $message = "日の値が不正です（1-31）";
          $result = "エラー";
      } else if ($month == 2 && $day > ($isLeapYear ? 29 : 28)) {
          $message = "この月の日数を超えています";
          $result = "エラー";
      } else {
          $leapText = $isLeapYear ? "うるう年です" : "平年です";
          $message = "{$year}年{$month}月{$day}日は存在します。{$year}年は{$leapText}";
          $result = "成功";
      }

      // HTMLレスポンスを返す
      echo "
      <!DOCTYPE html>
      <html lang='ja'>
      <head>
          <meta charset='UTF-8'>
          <title>判定結果</title>
      </head>
      <body>
          <h1>判定結果（POSTで送信）</h1>
          <p><strong>ステータス:</strong> $result</p>
          <p><strong>メッセージ:</strong> $message</p>
          <p><strong>送信データ:</strong></p>
          <ul>
              <li>年: $year</li>
              <li>月: $month</li>
              <li>日: $day</li>
          </ul>
          <a href='index.html'>戻る</a>
      </body>
      </html>
      ";
  } else {
      echo "POSTリクエストが必要です";
  }

  /**
   * うるう年判定関数
   * @param int $year 年
   * @return bool うるう年の場合true
   */
  function isLeapYear($year) {
      // うるう年の判定ルール：
      // 1. 400で割り切れる → うるう年
      // 2. 100で割り切れる → 平年
      // 3. 4で割り切れる → うるう年
      // 4. それ以外 → 平年

      if ($year % 400 == 0) {
          return true;
      } elseif ($year % 100 == 0) {
          return false;
      } elseif ($year % 4 == 0) {
          return true;
      } else {
          return false;
      }
  }
  ?>
  ```

  **実行例（POST）**:
  - フォームで「2024年2月29日」を入力
  - URLは `http://192.168.1.100:8080/leap_year.php` のまま（データはURLに見えない）
  - レスポンス:
  ```
  ステータス: 成功
  メッセージ: 2024年2月29日は存在します。2024年はうるう年です
  送信データ:
  - 年: 2024
  - 月: 2
  - 日: 29
  ```

  ---

  #### GET を使う場合

  **フォーム（HTML）**:
  ```html
  <!DOCTYPE html>
  <html lang="ja">
  <head>
      <meta charset="UTF-8">
      <title>うるう年判定（GET）</title>
  </head>
  <body>
      <h1>うるう年判定ツール（GET例）</h1>
      <form action="http://192.168.1.100:8080/leap_year_get.php" method="GET">
          <label for="year">年:</label>
          <input type="number" id="year" name="year" placeholder="例: 2024" required>

          <label for="month">月:</label>
          <input type="number" id="month" name="month" min="1" max="12" placeholder="例: 2" required>

          <label for="day">日:</label>
          <input type="number" id="day" name="day" min="1" max="31" placeholder="例: 29" required>

          <button type="submit">判定する</button>
      </form>
  </body>
  </html>
  ```

  **サーバー側のPHP（leap_year_get.php）**:
  ```php
  <?php
  header('Content-Type: text/html; charset=utf-8');

  if ($_SERVER['REQUEST_METHOD'] === 'GET') {
      $year = intval($_GET['year'] ?? 0);
      $month = intval($_GET['month'] ?? 0);
      $day = intval($_GET['day'] ?? 0);

      // うるう年の判定ロジック
      $isLeapYear = isLeapYear($year);

      // 日付の妥当性チェック
      if ($month < 1 || $month > 12) {
          $message = "月の値が不正です（1-12）";
          $result = "エラー";
      } else if ($day < 1 || $day > 31) {
          $message = "日の値が不正です（1-31）";
          $result = "エラー";
      } else if ($month == 2 && $day > ($isLeapYear ? 29 : 28)) {
          $message = "この月の日数を超えています";
          $result = "エラー";
      } else {
          $leapText = $isLeapYear ? "うるう年です" : "平年です";
          $message = "{$year}年{$month}月{$day}日は存在します。{$year}年は{$leapText}";
          $result = "成功";
      }

      // HTMLレスポンスを返す
      echo "
      <!DOCTYPE html>
      <html lang='ja'>
      <head>
          <meta charset='UTF-8'>
          <title>判定結果</title>
      </head>
      <body>
          <h1>判定結果（GETで送信）</h1>
          <p><strong>ステータス:</strong> $result</p>
          <p><strong>メッセージ:</strong> $message</p>
          <p><strong>現在のURL:</strong></p>
          <code>{$_SERVER['REQUEST_URI']}</code>
          <p><strong>送信データ:</strong></p>
          <ul>
              <li>年: $year</li>
              <li>月: $month</li>
              <li>日: $day</li>
          </ul>
          <a href='index.html'>戻る</a>
      </body>
      </html>
      ";
  } else {
      echo "GETリクエストが必要です";
  }

  /**
   * うるう年判定関数
   * @param int $year 年
   * @return bool うるう年の場合true
   */
  function isLeapYear($year) {
      if ($year % 400 == 0) {
          return true;
      } elseif ($year % 100 == 0) {
          return false;
      } elseif ($year % 4 == 0) {
          return true;
      } else {
          return false;
      }
  }
  ?>
  ```

  **実行例（GET）**:
  - フォームで「2024年2月29日」を入力
  - URLが `http://192.168.1.100:8080/leap_year_get.php?year=2024&month=2&day=29` に変わる（データがURL内に見える）
  - レスポンス:
  ```
  ステータス: 成功
  メッセージ: 2024年2月29日は存在します。2024年はうるう年です
  現在のURL:
  http://192.168.1.100:8080/leap_year_get.php?year=2024&month=2&day=29
  送信データ:
  - 年: 2024
  - 月: 2
  - 日: 29
  ```

  #### POST と GET の動作の違い

  | 項目 | POST 例 | GET 例 |
  |------|--------|-------|
  | **送信先URL** | `http://192.168.1.100:8080/leap_year.php` | `http://192.168.1.100:8080/leap_year_get.php?year=2024&month=2&day=29` |
  | **URLに表示される** | データなし | データが全て表示される |
  | **ブックマーク可能** | ❌ 不可（毎回フォーム入力が必要） | ✅ 可能（URLをブックマーク→同じ結果を再表示） |
  | **用途** | 重要な処理に適している | 検索・フィルタリングに適している |

