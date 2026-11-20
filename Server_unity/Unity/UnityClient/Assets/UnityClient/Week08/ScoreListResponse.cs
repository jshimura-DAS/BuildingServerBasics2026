// ScoreListResponse.cs
// 5-4: /api/sample-list のJSONに対応するクラス（配列はオブジェクトの中に入れる）
using System.Collections.Generic;

[System.Serializable]
public class ScoreItem
{
    public int id;
    public string name;
    public int score;
}

[System.Serializable]
public class ScoreListResponse
{
    public string result;
    public List<ScoreItem> scores;
}
