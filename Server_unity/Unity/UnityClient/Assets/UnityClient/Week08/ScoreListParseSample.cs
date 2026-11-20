// ScoreListParseSample.cs
// 5-4: 配列を含むJSONをクラスへ変換する（ScoreListResponse.cs のクラスを使用）
using UnityEngine;

public class ScoreListParseSample : MonoBehaviour
{
    private void Start()
    {
        string json = "{\"result\":\"ok\",\"scores\":[{\"id\":1,\"name\":\"PlayerA\",\"score\":1250},{\"id\":2,\"name\":\"PlayerB\",\"score\":980}]}";

        ScoreListResponse list = JsonUtility.FromJson<ScoreListResponse>(json);
        foreach (ScoreItem item in list.scores)
        {
            Debug.Log($"{item.id}: {item.name} {item.score}");
        }
    }
}
