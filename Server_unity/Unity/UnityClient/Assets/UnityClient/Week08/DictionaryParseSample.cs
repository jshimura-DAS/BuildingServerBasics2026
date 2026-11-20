// DictionaryParseSample.cs
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DictionaryParseSample : MonoBehaviour
{
    private void Start()
    {
        string json = "{\"result\":\"ok\",\"id\":12,\"name\":\"PlayerA\",\"score\":1250}";

        Dictionary<string, object> dict;
        try
        {
            dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        catch (JsonException e)
        {
            Debug.LogError("JSONの形式が不正です: " + e.Message);
            return;
        }

        foreach (KeyValuePair<string, object> pair in dict)
        {
            Debug.Log($"{pair.Key} = {pair.Value} ({pair.Value?.GetType().Name})");
        }
    }
}
