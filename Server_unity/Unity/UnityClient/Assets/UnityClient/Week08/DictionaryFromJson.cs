// DictionaryFromJson.cs
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DictionaryFromJson : MonoBehaviour
{
    private void Start()
    {
        string json = "{\"name\":\"PlayerA\",\"score\":1250,\"cleared\":true}";

        Dictionary<string, object> dict =
            JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        string name = (string)dict["name"];
        long score = (long)dict["score"];   // 整数は long になる
        bool cleared = (bool)dict["cleared"];

        Debug.Log($"name={name}, score={score}, cleared={cleared}");
    }
}
