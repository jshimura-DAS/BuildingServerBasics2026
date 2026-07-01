# BuildingServerBasics2026

## <form>タグの役割、<input type="text">やname属性、送信ボタンの仕組み

### <form>タグの役割
`<form>`タグはHTMLにおいてユーザー入力を処理するための基本的な要素です。主な役割は以下の通りです：
- ユーザーが入力したデータをサーバーに送信するための枠組みを提供する
- フォーム内のすべての入力要素を一つのグループとしてまとめる
- 送信先のサーバーアドレス（action属性）と送信方法（method属性）を指定する
- フォーム検証やイベント処理の対象となる

#### <form>タグの具体例
```html
<form action="/submit" method="POST">
  <!-- フォーム内の入力要素がここに入る -->
</form>
```
- `action="/submit"` - フォームデータの送信先URL
- `method="POST"` - データ送信方法（POST、またはGET）

### <input type="text">の役割
`<input type="text">`は単一行のテキスト入力フィールドを作成します：
- ユーザーがテキストを自由に入力できるフィールドを表示する
- 短い文字列（名前、メールアドレス、検索キーワードなど）を入力するのに適している
- 複数行の入力が必要な場合は`<textarea>`タグを使用する

#### <input type="text">の具体例
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
- 例えば`<input type="text" name="username">`の場合、送信されるデータは`username=入力値`という形式になる
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
- `<button type="submit">送信</button>`または`<input type="submit" value="送信">`で作成される
- ユーザーがボタンをクリックすると、フォーム内のすべての入力データが収集される
- `<form>`タグのaction属性で指定されたURLに、method属性で指定された方法（通常はGETまたはPOST）でデータが送信される
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
- `<label>タグ` - 入力フィールドのラベルを関連付ける
- `type="reset"` - フォームの入力値をすべてクリアするボタン
