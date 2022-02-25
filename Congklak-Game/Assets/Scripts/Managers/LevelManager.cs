using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Congklak Board Environments")]
    public List<ActorBase> actorList = new List<ActorBase>();
    public List<Transform> actorCupZones = new List<Transform>();

    [Header("Level Properties")]
    public int totalSeedEachCup;
    public int remainingSeeds;
    public SeedSO seedSO;
    
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

    }

    public void PopulateLevel()
    {
        Debug.Log("populate level");
        StartCoroutine(PopulateAllSeeds(GameplayManager.Instance.StartGameplay));
    }

    public IEnumerator PopulateAllSeeds(Action OnFinishPopulate = null)
    {
        bool populateAllSeed = false;

        seedSO = ResourceManager.Instance.seedTable["seed0"];

        if (GameplayManager.Instance.allBoardHoles.Count > 0)
        {
            for (int i = 0; i < GameplayManager.Instance.allBoardHoles.Count; i++)
            {
                BoardHole boardHole = GameplayManager.Instance.allBoardHoles[i];
                if (boardHole.type == BoardHole.Type.ORDINARY)
                {
                    boardHole.containedSeeds = new List<Seed>();
                    boardHole.empty = false;
                    for (int k = 0; k < totalSeedEachCup; k++)
                    {
                        GameObject seedGO = LeanPool.Spawn(seedSO.seedModel, boardHole.gameObject.transform);
                        Seed seed = seedGO.GetComponent<Seed>();
                        seed.Init();
                        boardHole.containedSeeds.Add(seed);
                        boardHole.totalCurrentSeeds++;
                    }
                }
            }

            populateAllSeed = true;
        }

        yield return new WaitUntil(() => populateAllSeed == true);

        yield return new WaitForSeconds(0.5f);
        Debug.Log($"all holes has been filled with seeds");


        foreach (BoardHole hole in GameplayManager.Instance.allBoardHoles)
        {
            if (hole.type == BoardHole.Type.ORDINARY)
            {
                for (int l = 0; l < remainingSeeds; l++)
                {
                    GameObject seedGO = LeanPool.Spawn(seedSO.seedModel, hole.gameObject.transform);
                    Seed seed = seedGO.GetComponent<Seed>();
                    hole.containedSeeds.Add(seed);
                    hole.totalCurrentSeeds++;
                }
            }
        }

        OnFinishPopulate?.Invoke();
    }

    

}
