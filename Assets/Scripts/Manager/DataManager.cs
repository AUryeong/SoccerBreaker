using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] // ����ȭ

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
            DontDestroyOnLoad(gameObject); // DataManager�� Scene�� �ٲ��� �����־�� ��.
    }
    private void Start() 
    {
        LoadGameData(); 
    } 
    
    public void LoadGameData() 
    {
        string s = PlayerPrefs.GetString("SaveData", "null"); // ���� �ý���, ������� ���� ���� �ý����� �۵� ����
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
        SaveGameData(); // ������� OnApplicationQuit�� �۵����ϹǷ� Pause�� ���������� ��ġ ���ῡ ���� �ý��� �־��ֱ�
        Application.Quit();
    }
    
    public void SaveGameData()
    {
        PlayerPrefs.SetString("SaveData", JsonUtility.ToJson(gameData));
    }

    private void OnApplicationPause(bool pause) // ������� OnApplicationQuit�� �۵����ϹǷ� Pause�� ����
    {
        if (pause)
            SaveGameData();
    } 
} 