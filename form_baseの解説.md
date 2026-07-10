C#などのオブジェクト指向言語や一般的なプログラミングの基本（変数、関数、ループ、クラスなど）を理解している方向けに、HTML内におけるJavaScript（ECMAScript）の役割や動き方を解説します。

「C#のあの概念は、JavaScriptだとこれに近い」という対比を交えながら説明していきます。

---

## 1. そもそもJavaScript（ECMAScript）とは？

一言でいうと、「ブラウザ（HTML）を動的に操作するための軽量なスクリプト言語」です。

* **ECMAScript（ES）とは？**
JavaScriptの「標準規格（仕様書）」の名前です。C#でいう「ECMA-334（C#の標準規格）」のようなものです。実務上は **JavaScript ≒ ECMAScript** と思って差し支えありません。「ES6（ES2015）」などの表記は、「C# 6.0」や「C# 10」といった言語のバージョンを指しています。
* **C#との最大の違い**
C#はコンパイルされて静的型付けで動きますが、JavaScriptはブラウザがその場で解釈して実行する**動的型付けのインタプリタ言語**です。

---

## 2. HTML内でのScriptの役割

HTMLが「画面の構造（UIの定義）」、CSSが「見た目の装飾」だとすれば、JavaScriptは「ロジック（イベントハンドラやUIの動的変更）」を担当します。

C#（特にWPFやWinForms、Blazor）の感覚でいうと、**HTMLが `.xaml` や `.designer.cs`（画面定義）で、JavaScriptが `code-behind（.xaml.cs）**` のような関係性です。

### HTMLへの書き方（2パターン）

**① HTML内に直接書く（インライン）**

```html
<script>
    console.log("Hello World!"); // C#の Console.WriteLine() に相当
</script>

```

**② 外部ファイル（.js）を読み込む（推奨）**
C#でクラスファイルを分けるのと同じ感覚です。

```html
<script src="app.js"></script>

```

---

## 3. C#エンジニアが知るべきJavaScriptの4大特徴

### ① 型システム（静的 vs 動的）

C#は `int` や `string` を厳格に定義しますが、JavaScriptはすべて `let` や `const` で宣言し、型は実行時に自動で決まります。

* `const`: 再代入不可（C#の `readonly` や `const` に近い）
* `let`: 再代入可能（普通の変数）
* `var`: 古い書き方（スコープの挙動が怪しいため、現在は**原則使わない**）

```javascript
// C#： string name = "Alice";
const name = "Alice";

// C#： int counter = 0; counter++;
let counter = 0;
counter++;

```

### ② DOM（Document Object Model）操作

HTML内の要素（タグ）は、ブラウザによってツリー構造のオブジェクト（DOM）に変換されています。JavaScriptはこのオブジェクトを操作して画面を書き換えます。

C#のコントロール操作（`myButton.Text = "Click me";`）と全く同じ感覚です。

```javascript
// 1. HTMLからIDが "submit-btn" の要素を取得（インスタンスの取得）
const button = document.getElementById("submit-btn");

// 2. 文字列を変更
button.textContent = "送信する";

// 3. スタイル（CSS）を直接書き換える
button.style.backgroundColor = "blue";

```

### ③ イベントリスナー（イベントハンドラ）

「ボタンが押されたとき」などの処理の登録方法です。C#の `button.Click += OnButtonClick;`（マルチキャストデリゲート）と概念は同じです。

JavaScriptでは、関数を引数としてそのまま渡す（ラムダ式のような形）のが一般的です。

```javascript
const button = document.getElementById("submit-btn");

// C#の button.Click += (s, e) => { ... } に相当
button.addEventListener("click", (event) => {
    alert("ボタンがクリックされました！"); // ポップアップを表示
});

```

### ④ 非同期処理（Promise と async/await）

JavaScriptはシングルスレッドで動くため、API通信などで画面がフリーズしないよう非同期処理が多用されます。
嬉しいことに、**JavaScriptの `async/await` は、C#の `async/await` とほぼ同じ感覚で使えます。**

* C#の `Task<T>
    ` ＝ JavaScriptの `Promise`

    ```javascript
    // C#： async Task FetchDataAsync() { ... await client.GetAsync(url); }
    async function fetchData() {
    try {
    const response = await fetch("https://api.example.com/data");
    const data = await response.json(); // JSONのデシリアライズ
    console.log(data);
    } catch (error) {
    console.error("エラー発生", error); // C#の catch (Exception ex)
    }
    }

    ```

    ---

    ## 4. C#脳で読むと「おや？」となりやすい注意点

    1. **比較演算子は `===` を使う**
    JavaScriptには `==` と `===` があります。`==` は「型を自動変換して比較」してしまうため、`"5" == 5` が `true` になります。C#と同じ厳格な比較をしたい場合は、**常に `===`（厳密等価演算子）を使ってください。**
    2. **`null` と `undefined` がある**
    * `null`: 「値がない」状態（明示的に空にされている）。C#の `null` と同等。
    * `undefined`: 「変数は定義されているが、まだ何も代入されていない」状態（未定義）。


    3. **すべてがオブジェクト**
    JavaScriptにはプリミティブ型（数値や文字列）もありますが、それらも自動的にオブジェクトのようにラップされ、メソッド（例: `string.length` など）を呼ぶことができます。

    ---

    ## まとめ

    HTML内のScript（JavaScript）は、「ブラウザが用意してくれた画面オブジェクト（DOM）を、イベント駆動でこねくり回すための言語」です。

    言語仕様のクセ（動的型付けゆえの緩さ）はありますが、`async/await` や `ラムダ式（アロー関数）`、`クラスの概念` など、近代的な機能はC#と非常に親和性が高いです。まずは「`document.getElementById` で要素を掴んで、イベントを登録する」という流れを押さえるのが上達の近道です。
