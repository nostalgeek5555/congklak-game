using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Lean.Pool;
using DG.Tweening;
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
    public Grabber currentGrabber;
    public Player player;
    public Ai ai;
    
    public List<BoardHole> allBoardHoles = new List<BoardHole>();
    

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
                HandleOnGameStart();
                break;
            case GameState.SHIFT_TURN:
                HandleOnShiftTurn();
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

        //LevelManager.Instance.allHoles = new List<BoardHole>();
        allBoardHoles = new List<BoardHole>();

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
                                //LevelManager.Instance.allHoles.Add(boardHole);
                                allBoardHoles.Add(boardHole);
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
                                allBoardHoles.Add(boardHole);
                                //LevelManager.Instance.allHoles.Add(boardHole);
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
        ai.StateController(Ai.States.INIT);
        LevelManager.Instance.PopulateLevel();
    }

    public void HandleOnGameStart()
    {
        int randomTurn = 0;//Random.RandomRange(0, 1);

        if (randomTurn == 0)
        {
            Debug.Log($"player getting first turn");
            player.StateController(Player.States.FREE_TURN);
            UIManager.Instance.InitUI();
        }

        else
        {
            Debug.Log($"enemy ai getting first turn");
        }
    }

    public void HandleOnShiftTurn()
    {

    }


    #region GAME_CORE_LOOP
    public void HandleOnSeedDropped(BoardHole hole)
    {
        if (currentGrabber.seeds.Count > 0)
        {
            currentGrabber.currentHoleIndex++;
            currentGrabber.MoveToMovementPoint(currentGrabber.currentHoleIndex, 1f);
        }

        else
        {
            currentActor.initHole.touchedSign.SetActive(false);
            currentActor.currentPickedHole.touchedSign.SetActive(false);
            currentActor.currentPickedHole = hole;
            currentActor.currentPickedHole.touchedSign.SetActive(true);

            //check if current hole only contain 1 seed
            if (hole.containedSeeds.Count == 1)
            {
                if (currentActor.boardHoles.Contains(hole))
                {
                    switch (hole.type)
                    {
                        case BoardHole.Type.ORDINARY:
                            //check if this hole belong to current playing actor and check if grabber has rotating one cycle, if yes then do SHOOTING method
                            if (currentGrabber.rotateCycleDone)
                            {
                                switch (currentActor.role)
                                {
                                    case ActorBase.Role.PLAYER:
                                        int facingHoleIndex = (ai.boardHoles.Count - 1) - player.boardHoles.IndexOf(hole);
                                        BoardHole facingHole = ai.boardHoles[facingHoleIndex];

                                        Debug.Log("start procedure for shooting");
                                        if (facingHole.containedSeeds.Count > 0)
                                        {
                                            Debug.Log($"contain seed {facingHole.containedSeeds.Count} start shooting");
                                            player.StateController(Player.States.SHOOTING);
                                        }

                                        else
                                        {
                                            Debug.Log("end turn");
                                            player.StateController(Player.States.END_TURN);
                                        }
                                        
                                        break;
                                    case ActorBase.Role.AI:
                                        break;
                                    default:
                                        break;
                                }
                                Debug.Log("run shooting method");
                            }

                            else
                            {
                                Debug.Log("shift turn");
                            }
                            break;

                        case BoardHole.Type.BASE:
                            if (IsAllSeedDroppedInBase())
                            {
                                Debug.Log("Start calculating final result for all actors");
                            }

                            else
                            {
                                switch (currentActor.role)
                                {
                                    case ActorBase.Role.PLAYER:
                                        Player player = currentActor.GetComponent<Player>();
                                        player.StateController(Player.States.FREE_TURN);
                                        break;

                                    case ActorBase.Role.AI:
                                        break;
                                    default:
                                        break;
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }

                else
                {
                    Debug.Log("shift turn to another actor");
                }
                
            }

            else
            {
                switch (hole.type)
                {
                    case BoardHole.Type.ORDINARY:
                        switch (currentActor.role)
                        {
                            case ActorBase.Role.PLAYER:
                                Player player = currentActor.GetComponent<Player>();
                                player.StateController(Player.States.MOVETHRU_TURN);
                                break;

                            case ActorBase.Role.AI:
                                break;
                            default:
                                break;
                        }
                        break;
                    case BoardHole.Type.BASE:
                        switch (currentActor.role)
                        {
                            case ActorBase.Role.PLAYER:
                                Player player = currentActor.GetComponent<Player>();
                                player.StateController(Player.States.FREE_TURN);
                                break;

                            case ActorBase.Role.AI:
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public bool IsAllSeedDroppedInBase()
    {
        int emptyHole = 0;

        for (int i = 0; i < allBoardHoles.Count; i++)
        {
            BoardHole hole = allBoardHoles[i];
            if (hole.empty)
            {
                emptyHole++;
            }
        }


        if (emptyHole == allBoardHoles.Count)
        {
            Debug.Log($"total empty hole {emptyHole}");
            return true;
        }

        else
        {
            Debug.Log($"total empty hole {emptyHole}");
            return false;
        }
    }

    #endregion
    public void StartGameplay()
    {
        StateController(GameState.START_GAME);
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
