// ScoreClient.cs
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreClient : MonoBehaviour
{
    [SerializeField] private string url = "http://172.25.21.149:5000/api/scores";
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField scoreField;
    [SerializeField] private TMP_Text resultText;

    private bool isSending = false;

    [System.Serializable]
    private class ScoreRequest
    {
        public string name;
        public int score;
    }

    [System.Serializable]
    private class ScoreResponse
    {
        public string result;
        public int id;
        public string name;
        public int score;
        public string message;
    }

    // ボタンのOnClickから呼ぶ
    public void OnSubmitClicked()
    {
        if (isSending)
        {
            return;
        }

        if (!int.TryParse(scoreField.text, out int score))
        {
            resultText.text = "scoreは整数で入力してください。";
            return;
        }

        StartCoroutine(PostScore(nameField.text, score));
    }

    private IEnumerator PostScore(string playerName, int score)
    {
        isSending = true;
        resultText.text = "送信中...";

        string json = JsonUtility.ToJson(new ScoreRequest { name = playerName, score = score });
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            HandleResponse(request);
        }

        isSending = false;
    }

    private void HandleResponse(UnityWebRequest request)
    {
        // 通信自体の失敗（接続できない・タイムアウトなど）
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            resultText.text = "サーバーに接続できません: " + request.error;
            return;
        }

        string responseText = request.downloadHandler.text;
        ScoreResponse response = JsonUtility.FromJson<ScoreResponse>(responseText);

        // HTTPエラー（4xx / 5xx）はProtocolErrorになる
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string message = response != null && !string.IsNullOrEmpty(response.message)
                ? response.message
                : request.error;
            resultText.text = $"エラー({request.responseCode}): {message}";
            return;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            resultText.text = $"登録成功: id={response.id}, name={response.name}, score={response.score}";
            return;
        }

        resultText.text = "予期しないエラー: " + request.error;
    }
}
