// DictionaryTryGetValue.cs
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DictionaryTryGetValue : MonoBehaviour
{
    private void Start()
    {
        // scoreのキーが無いJSON
        string json = "{\"name\":\"PlayerA\"}";
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        if (dict.TryGetValue("score", out object value))
        {
            int score = System.Convert.ToInt32(value);
            Debug.Log("score=" + score);
        }
        else
        {
            Debug.LogWarning("scoreがありません");
        }
    }
}
