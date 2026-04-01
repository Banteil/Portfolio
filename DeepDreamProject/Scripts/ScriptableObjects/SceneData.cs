using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Data/SceneData")]
public class SceneData : ScriptableObject
{
    public int SceneID;
    public string SceneName;
    public string DisplayName;
    [TextArea]
    public string Description;
    public bool Is3D;
    public Vector3 SpawnPos;
    public Vector3 SpawnRot;
}
