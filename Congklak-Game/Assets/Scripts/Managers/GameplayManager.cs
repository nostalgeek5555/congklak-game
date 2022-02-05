using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    public static event Action<GameState> OnBeforeStateUpdate;
    public static event Action<GameState> OnAfterStateUpdate;

    public GameState state;
    public Camera gameplayCamera;

    [Header("Game Mode Properties")]
    public GameModeSO currentGameMode;

    [Header("Actor Properties")]
    public Transform actorParent;
    public ActorBase currentActor;
    public Player player;
    public Ai ai;

    private void OnEnable()
    {
        OnBeforeStateUpdate += HandleBeforeStateUpdate;
    }

    private void OnDisable()
    {
        OnBeforeStateUpdate -= HandleAfterStateUpdate;
    }

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

        StateController(GameState.INIT);
    }

    public void StateController(GameState gameState)
    {
        OnBeforeStateUpdate?.Invoke(gameState);

        state = gameState;

        switch (gameState)
        {
            case GameState.INIT:
                HandleGameInitiation();
                break;
            case GameState.START_GAME:
                break;
            case GameState.WIN:
                break;
            case GameState.GAME_OVER:
                break;
            default:
                break;
                //throw new ArgumentOutOfRangeException(typeof(gamest))
        }


        OnAfterStateUpdate?.Invoke(gameState);
    }

    public void HandleBeforeStateUpdate(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.INIT:
                HandleBeforeGameInitiation();
                break;
            case GameState.START_GAME:
                break;
            case GameState.SHIFT_TURN:
                break;
            case GameState.WIN:
                break;
            case GameState.GAME_OVER:
                break;
            default:
                break; 
        }
    }

    public void HandleAfterStateUpdate(GameState gameState)
    {

    }

    public void HandleBeforeGameInitiation()
    {
        string currentGameModeKey = GameModeSO.PlayerModeType.SinglePlayer + "|" + GameModeSO.ModeType.Offline;
        currentGameMode = ResourceManager.Instance.gameModeTable[currentGameModeKey];

        LevelManager.Instance.allHoles = new List<BoardHole>();

        //init player & enemy
        switch (currentGameMode.playerModeType)
        {
            case GameModeSO.PlayerModeType.SinglePlayer:
                switch (currentGameMode.modeType)
                {
                    case GameModeSO.ModeType.Offline:
                        //instantiate player & enemy ai zone
                        GameObject playerGO = LeanPool.Spawn(currentGameMode.playerGO, actorParent);
                        Player currentPlayer = playerGO.GetComponent<Player>();
                        player = currentPlayer;

                        if (player.transform.childCount > 0)
                        {
                            for (int i = 0; i < player.transform.childCount; i++)
                            {
                                BoardHole boardHole = player.transform.GetChild(i).GetComponent<BoardHole>();
                                LevelManager.Instance.allHoles.Add(boardHole);
                            }
                        }

                        GameObject enemyGO = LeanPool.Spawn(currentGameMode.enemyAiGO, actorParent);
                        Ai currentAi = enemyGO.GetComponent<Ai>();
                        ai = currentAi;

                        if (enemyGO.transform.childCount > 0)
                        {
                            for (int i = 0; i < enemyGO.transform.childCount; i++)
                            {
                                BoardHole boardHole = enemyGO.transform.GetChild(i).GetComponent<BoardHole>();
                                LevelManager.Instance.allHoles.Add(boardHole);
                            }
                        }

                        break;
                    case GameModeSO.ModeType.Online:
                        break;
                    default:
                        break;
                }

                break;
            case GameModeSO.PlayerModeType.Multiplayer:
                break;
            default:
                break;
        }
    }


    public void HandleGameInitiation()
    {
        player.StateController(Player.States.INIT);
        LevelManager.Instance.PopulateLevel();
    }

    public void StartGameplay()
    {
        int randomTurn = Random.RandomRange(0, 1);

        if (randomTurn == 0)
        {
            Debug.Log($"player getting first turn");
            player.StateController(Player.States.GET_TURN);
        }

        else
        {
            Debug.Log($"enemy ai getting first turn");
        }
    }


    public enum GameState
    {
        INIT = 0,
        START_GAME = 1,
        SHIFT_TURN = 2,
        WIN = 3,
        GAME_OVER = 4
    }
}
