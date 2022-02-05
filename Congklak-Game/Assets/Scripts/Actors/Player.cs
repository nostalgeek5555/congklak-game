using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ActorBase
{
    public States states;
    public static event Action<States> OnBeforeStateUpdate;
    public static event Action<States> OnAfterStateUpdate;

    [Header("Gameplay Properties")]
    public Transform boardHoleParent;
    public List<BoardHole> boardHoles = new List<BoardHole>();
    public int currentPickedHoleId = -1;
    public BoardHole currentPickedHole;

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
                HandleOnProcessingTurn();
                break;
            case States.GET_TURN:
                HandleOnGettingTurn();
                break;
            case States.END_TURN:
                HandleOnEndingTurn();
                break;
            case States.WIN:
                HandleOnWin();
                break;
            case States.GAME_OVER:
                HandleOnGameOver();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_states), _states, "state out of reach");
        }

        OnAfterStateUpdate?.Invoke(_states);
    }

    public List<BoardHole> SortBasedOnActiveHole()
    {
        IEnumerable<BoardHole> sortBoardHoles = boardHoles.OrderBy(hole => hole.id).Where(hole => hole.totalCurrentSeeds > 0);
        List<BoardHole> sortedBoardHoles = sortBoardHoles.ToList();

        return sortedBoardHoles;
    }

    public void PickHole(List<BoardHole> activeBoardHoles, int _id)
    {
        if (activeBoardHoles.Count > 0)
        {
            if (currentPickedHoleId != -1)
            {
                activeBoardHoles[currentPickedHoleId].picked = false;

                BoardHole boardHole = activeBoardHoles[_id];
                boardHole.picked = true;
                currentPickedHole = boardHole;
                currentPickedHoleId = boardHole.id;

                Debug.Log($"pick current hole with id {currentPickedHoleId}");
            }

            else
            {
                BoardHole boardHole = activeBoardHoles[_id];
                boardHole.picked = true;
                currentPickedHole = boardHole;
                currentPickedHoleId = boardHole.id;

                Debug.Log($"pick current hole with id {currentPickedHoleId}");
            }
        }
    }


    private void HandleOnInit()
    {
        Debug.Log("init player");
        if (boardHoleParent.childCount > 0)
        {
            for (int i = 0; i < boardHoleParent.childCount; i++)
            {
                BoardHole boardHole = boardHoleParent.GetChild(i).GetComponent<BoardHole>();
                boardHoles.Add(boardHole);
            }
        }
    }

    private void HandleOnProcessingTurn()
    {

    }

    private void HandleOnGettingTurn()
    {

    }

    private void HandleOnEndingTurn()
    {

    }

    private void HandleOnWin()
    {

    }


    private void HandleOnGameOver()
    {

    }


    public void HandleBeforeStateUpdate(States states)
    {
        Debug.Log("handle before state update");
    }

    public void HandleAfterStateUpdate(States states)
    {
        Debug.Log("handle after state update");
    }



    public enum States
    {
        INIT = 0,
        PROCESSING_TURN = 1,
        GET_TURN = 2,
        END_TURN = 3,
        WIN = 4,
        GAME_OVER = 5
    }
}
