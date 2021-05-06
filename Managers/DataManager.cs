using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;

    public static DataManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<DataManager>();
            return instance;
        }
    }
    public PlayerData playerData;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void WritePlayerData(string pname, int pmoney, int pBestScore)
    {
        playerData.playername = pname;
        playerData.money = pmoney;
        playerData.BestScore = pBestScore;
    }

    public void SavePlayerDatatoJson()
    {
        string jsonData = JsonUtility.ToJson(playerData, true);
        string path = Path.Combine(Application.dataPath, "playerData.json");
        File.WriteAllText(path, jsonData);
    }

    public bool LoadPlayerDataFromJson()
    {
        string path = Path.Combine(Application.dataPath, "playerData.json");
        FileInfo fileinfo = new FileInfo(path);
        if (!fileinfo.Exists)
        {
            Debug.Log("데이터 없음");
            return false;
        }
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);
        return true;
    }
}

[System.Serializable]
public class PlayerData
{
    public string playername;
    public int money;
    public int BestScore;
}
