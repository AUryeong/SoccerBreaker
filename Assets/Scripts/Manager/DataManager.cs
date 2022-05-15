using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] // 직렬화

public class GameData
{
    public string name = "";
}
public class DataManager : Singleton<DataManager>
{
    public string GameDataFileName = "SBData.json";

    public GameData _gameData; 
    public GameData gameData 
    {
        get 
        { 
            if(_gameData == null)
            { 
                LoadGameData(); 
            }
            return _gameData;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        if(Instance == this)
            DontDestroyOnLoad(gameObject); // DataManager은 Scene이 바껴도 남아있어야 함.
    }
    private void Start() 
    {
        LoadGameData(); 
    } 
    
    public void LoadGameData() 
    {
        string s = PlayerPrefs.GetString("SaveData", "null"); // 저장 시스템, 모바일은 파일 저장 시스템이 작동 안함
        if (s != "null")
        { 
            _gameData = JsonUtility.FromJson<GameData>(s);
        }
        else 
        {            
            _gameData = new GameData();
        }
    } 

    public void GameQuit()
    {
        SaveGameData(); // 모바일은 OnApplicationQuit가 작동안하므로 Pause로 변경했으니 터치 종료에 저장 시스템 넣어주기
        Application.Quit();
    }
    
    public void SaveGameData()
    {
        PlayerPrefs.SetString("SaveData", JsonUtility.ToJson(gameData));
    }

    private void OnApplicationPause(bool pause) // 모바일은 OnApplicationQuit가 작동안하므로 Pause로 변경
    {
        if (pause)
            SaveGameData();
    } 
} 