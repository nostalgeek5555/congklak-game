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
    public List<BoardHole> allHoles = new List<BoardHole>();

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

        if (allHoles.Count > 0)
        {
            for (int i = 0; i < allHoles.Count; i++)
            {
                BoardHole boardHole = allHoles[i];
                if (boardHole.type == BoardHole.Type.ORDINARY)
                {
                    boardHole.containedSeeds = new List<Seed>();
                    for (int k = 0; k < totalSeedEachCup; k++)
                    {
                        GameObject seedGO = LeanPool.Spawn(seedSO.seedModel, boardHole.gameObject.transform);
                        Seed seed = seedGO.GetComponent<Seed>();
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
        

        foreach (BoardHole hole in allHoles)
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
