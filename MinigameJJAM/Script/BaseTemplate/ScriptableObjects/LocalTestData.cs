using starinc.io;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalTestData", menuName = "Scriptable Objects/LocalTestData")]
public class LocalTestData : ScriptableObject
{
    [SerializeField]
    private MinigameData _minigameData;
    public MinigameData MinigameData { get { return _minigameData; } }

    [SerializeField]
    private HighScoreData _scoreData;
    public HighScoreData ScoreData { get { return _scoreData; } }
}
