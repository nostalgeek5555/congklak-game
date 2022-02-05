using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public List<GameModeSO> gameModeList;
    public Dictionary<string, GameModeSO> gameModeTable;
    public List<string> addedGameModeKeys;

    public List<SeedSO> seedList;
    public Dictionary<string, SeedSO> seedTable;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
        }

        LoadResources();
    }

    public void LoadGameMode()
    {
        //load game modes
        gameModeList = new List<GameModeSO>(Resources.LoadAll<GameModeSO>("Game Mode"));
        gameModeTable = new Dictionary<string, GameModeSO>();

        for (int i = 0; i < gameModeList.Count; i++)
        {
            GameModeSO gameModeSO = gameModeList[i];
            string key = gameModeSO.playerModeType + "|" + gameModeSO.modeType;

            gameModeTable.Add(key, gameModeSO);
            Debug.Log($"game mode added {gameModeTable[key].playerModeType}");
        }
    }

    public void LoadResources()
    {
        LoadGameMode();

        //load all seed types
        seedList = new List<SeedSO>(Resources.LoadAll<SeedSO>("Seed"));
        seedTable = new Dictionary<string, SeedSO>();

        for (int i = 0; i < seedList.Count; i++)
        {
            SeedSO seedSO = seedList[i];
            seedTable.Add(seedSO.seedId, seedSO);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        addedGameModeKeys = gameModeTable.Keys.ToList();
    }
#endif
}
