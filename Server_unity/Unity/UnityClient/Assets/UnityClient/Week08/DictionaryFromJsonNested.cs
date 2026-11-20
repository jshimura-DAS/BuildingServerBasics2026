// DictionaryFromJsonNested.cs
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class DictionaryFromJsonNested : MonoBehaviour
{
    private void Start()
    {
        string json = "{\"result\":\"ok\",\"player\":{\"name\":\"PlayerA\",\"score\":1250}}";
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        // playerは JObject になる
        var player = (JObject)dict["player"];
        string name = (string)player["name"];
        int score = (int)player["score"];

        Debug.Log($"player: name={name}, score={score}");
    }
}
