using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DimensionTypes { DIMENSION2D, DIMENSION3D }
public class GameManager : DontDestorySingleton<GameManager>
{
    SceneData _currentSceneData;
    public SceneData CurrentSceneData { get { return _currentSceneData; } }

    public Character PlayerCharacter;
    public float DataSaveDelay = 3f;

    float _saveCheckTime;
    Health _playerHealth;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _currentSceneData = DataManager.Instance.GetSceneDataFromName(scene.name);
        CreatePlayerCharacter();
    }

    void CreatePlayerCharacter()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            GameObject prefab = !_currentSceneData.Is3D ? Resources.Load<GameObject>("Characters/Player/Prefab/Player2D") : Resources.Load<GameObject>("Characters/Player/Prefab/Player3D");
            playerObject = Instantiate(prefab, _currentSceneData.SpawnPos, Quaternion.Euler(_currentSceneData.SpawnPos));
        }
        PlayerCharacter = playerObject.GetComponent<Character>();
        _playerHealth = PlayerCharacter.GetComponent<Health>();
        _playerHealth.MaximumHP = PlayerData.Instance.MaxSleepPower;
        _playerHealth.SetHealth(PlayerData.Instance.SleepPower);
    }

    private void Update()
    {
        if (_currentSceneData.SceneID.Equals(0)) return;

        if(Time.unscaledTime - _saveCheckTime >= DataSaveDelay)
        {
            SavePlayerData();
            _saveCheckTime = Time.unscaledTime;
        }

        PlayerData.Instance.SleepPower -= Time.deltaTime;
        _playerHealth.SetHealth(PlayerData.Instance.SleepPower);
    }

    void SavePlayerData()
    {
        if (!PlayerData.HasInstance) return;

        //임시로 데이터 저장
        PlayerData.Instance.LocationInfo.PlayerPos = PlayerCharacter.transform.position;
        PlayerData.Instance.LocationInfo.PlayerRot = PlayerCharacter.transform.rotation.eulerAngles;

        var json = JsonUtility.ToJson(PlayerData.Instance);
        //var path = $"{Application.persistentDataPath}/SavePlayerData.Json";
        var path = $"{Application.dataPath}/SavePlayerData.Json";
        var fileStream = new FileStream(path, FileMode.Create);
        using (var writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
