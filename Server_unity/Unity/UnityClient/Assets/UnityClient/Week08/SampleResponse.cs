// SampleResponse.cs
// 5-2: /api/sample のJSONに対応するクラス
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int id;
    public string name;
    public int score;
    public bool cleared;
}

[System.Serializable]
public class SampleResponse
{
    public string result;
    public PlayerData player;
    public List<string> items;
}
