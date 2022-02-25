using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : ActorBase
{
    public States states;
    public static event Action<States> OnBeforeStateUpdate;
    public static event Action<States> OnAfterStateUpdate;


    public enum States
    {
        INIT = 0,
        PROCESSING_TURN = 1,
        FREE_TURN = 2,
        MOVETHRU_TURN = 3,
        SHOOTING = 4,
        END_TURN = 5,
        WIN = 6,
        GAME_OVER = 7
    }

    public void StateController(States _states)
    {
        OnBeforeStateUpdate?.Invoke(_states);

        states = _states;

        switch (_states)
        {
            case States.INIT:
                HandleOnInit();
                break;
            case States.PROCESSING_TURN:
                //HandleOnProcessingTurn();
                break;
            case States.FREE_TURN:
                //HandleOnGettingFreeTurn();
                break;
            case States.MOVETHRU_TURN:
                //HandleOnGettingMovethruTurn();
                break;
            case States.SHOOTING:
                //HandleOnGetShooting();
                break;
            case States.END_TURN:
                //HandleOnEndingTurn();
                break;
            case States.WIN:
                //HandleOnWin();
                break;
            case States.GAME_OVER:
                //HandleOnGameOver();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_states), _states, "state out of reach");
        }

        OnAfterStateUpdate?.Invoke(_states);
    }

    private void HandleOnInit()
    {
        Debug.Log("init ai");
        currentPickedHole = null;

        if (boardHoleParent.childCount > 0)
        {
            for (int i = 0; i < boardHoleParent.childCount; i++)
            {
                BoardHole boardHole = boardHoleParent.GetChild(i).GetComponent<BoardHole>();
                boardHoles.Add(boardHole);
            }
        }
    }
}
