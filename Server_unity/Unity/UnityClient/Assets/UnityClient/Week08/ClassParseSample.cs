// ClassParseSample.cs
// 5-3: 文字列からクラスへ変換する（SampleResponse.cs のクラスを使用）
using UnityEngine;

public class ClassParseSample : MonoBehaviour
{
    private void Start()
    {
        string json = "{\"result\":\"ok\",\"player\":{\"id\":12,\"name\":\"PlayerA\",\"score\":1250,\"cleared\":true},\"items\":[\"sword\",\"shield\"]}";

        SampleResponse response = JsonUtility.FromJson<SampleResponse>(json);

        Debug.Log(response.result);              // ok
        Debug.Log(response.player.name);         // PlayerA
        Debug.Log(response.player.score + 100);  // 1350（intとして計算できる）
        Debug.Log(response.items[0]);            // sword
    }
}
