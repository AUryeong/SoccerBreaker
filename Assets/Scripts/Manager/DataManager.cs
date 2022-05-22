using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] // 직렬화

public class GameData
{
    public int Score;
    public int MaxScore;
    public int BallCount;
    public bool Gaming;
    public Vector3 BallPos;
    public List<BlockPos> addBalls = new List<BlockPos>();
    public List<BrickPos> bricks = new List<BrickPos>();
}

[System.Serializable]
public class BlockPos
{
    public BlockPos(Block block)
    {
        x = block.x;
        y = block.y;
    }
    public byte x;
    public byte y;
}

[System.Serializable]
public class BrickPos : BlockPos
{
    public BrickPos(Brick brick) : base(brick)
    {
        hp = brick.hp;
    }
    public int hp;
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
        {
            DontDestroyOnLoad(gameObject); // DataManager은 Scene이 바껴도 남아있어야 함.
            LoadGameData();
        }
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
        GameManager.Instance.Score = gameData.Score;
        GameManager.Instance.MaxScore = gameData.MaxScore;
        GameManager.Instance.Gaming = gameData.Gaming;
        if (gameData.Gaming)
            GamingLoadData();
    } 

    public void GamingLoadData()
    {
        GameManager.Instance.blocks = new List<Block>();
        foreach (BlockPos addBall in gameData.addBalls)
        {
            GameManager.Instance.GetNewAddBall(addBall.x, addBall.y);
        }
        foreach (BrickPos brick in gameData.bricks)
        {
            GameManager.Instance.GetNewBrick(brick.x, brick.y, brick.hp);
        }
    }

    public void GameQuit()
    {
        SaveGameData(); // 모바일은 OnApplicationQuit가 작동안하므로 Pause로 변경했으니 터치 종료에 저장 시스템 넣어주기
        Application.Quit();
    }
    
    public void SaveGameData()
    {
        gameData.Score = GameManager.Instance.Score;
        gameData.MaxScore = GameManager.Instance.MaxScore;
        gameData.addBalls = new List<BlockPos>();
        gameData.bricks = new List<BrickPos>();
        foreach (Block block in GameManager.Instance.blocks)
        {
            if (block.gameObject.activeSelf)
            {
                Brick brick = block as Brick;
                if (brick != null)
                    gameData.bricks.Add(new BrickPos(brick));
                else
                    gameData.addBalls.Add(new BlockPos(block));
            }
        }
        PlayerPrefs.SetString("SaveData", JsonUtility.ToJson(gameData));
    }

    private void OnApplicationPause(bool pause) // 모바일은 OnApplicationQuit가 작동안하므로 Pause로 변경
    {
        if (pause)
            SaveGameData();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
} 