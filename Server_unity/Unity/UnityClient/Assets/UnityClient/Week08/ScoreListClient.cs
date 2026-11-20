// ScoreListClient.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreListClient : MonoBehaviour
{
    [SerializeField] private string url = "http://172.25.21.149:5000/api/sample-list";
    [SerializeField] private TMP_Text resultText;

    private bool isFetching = false;

    // ボタンのOnClickから呼ぶ
    public void OnFetchClicked()
    {
        if (isFetching)
        {
            return;
        }

        StartCoroutine(FetchList());
    }

    private IEnumerator FetchList()
    {
        isFetching = true;
        resultText.text = "取得中...";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                resultText.text = $"通信エラー({request.responseCode}): {request.error}";
            }
            else
            {
                resultText.text = ParseAndFormat(request.downloadHandler.text);
            }
        }

        isFetching = false;
    }

    private string ParseAndFormat(string json)
    {
        ScoreListResponse response;
        try
        {
            response = JsonUtility.FromJson<ScoreListResponse>(json);
        }
        catch (ArgumentException e)
        {
            return "JSONの形式が不正です: " + e.Message;
        }

        if (response == null || response.scores == null)
        {
            return "想定した形式のデータではありません。";
        }

        StringBuilder sb = new StringBuilder();
        foreach (ScoreItem item in response.scores)
        {
            sb.AppendLine($"{item.id}. {item.name} : {item.score}");
        }
        return sb.ToString();
    }
}
