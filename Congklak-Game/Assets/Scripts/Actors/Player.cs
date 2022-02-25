using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;
using DG.Tweening;
using Random = UnityEngine.Random;


public class Player : ActorBase
{
    public States states;
    public static event Action<States> OnBeforeStateUpdate;
    public static event Action<States> OnAfterStateUpdate;

    [Header("Player UI")]
    public Button moveButton;


    private void OnDisable()
    {
        UIManager.Instance.moveButton.onClick.RemoveAllListeners();
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
                HandleOnProcessingTurn();
                break;
            case States.FREE_TURN:
                HandleOnGettingFreeTurn();
                break;
            case States.MOVETHRU_TURN:
                HandleOnGettingMovethruTurn();
                break;
            case States.SHOOTING:
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


    public void HandleOnMove()
    {
        if (states == States.FREE_TURN)
        {
            if (currentPickedHole != null)
            {
                UIManager.Instance.moveButton.enabled = false;

                foreach(BoardHole hole in boardHoles)
                {
                    hole.touchable = false;
                }

                if (grabberGO == null)
                {
                    grabberGO = LeanPool.Spawn(grabber.gameObject, transform.parent);
                    grabberGO.transform.position = currentPickedHole.gameObject.transform.position;


                    grabber = grabberGO.GetComponent<Grabber>();
                    grabber.belongToActor = GetComponent<ActorBase>();
                    GameplayManager.Instance.currentGrabber = grabber;

                    StartCoroutine(grabber.GrabSeeds(grabber.MoveToMovementPoint));
                }

                else
                {
                    grabber.firstMove = true;
                    grabberGO.transform.position = currentPickedHole.gameObject.transform.position;
                    StartCoroutine(grabber.GrabSeeds(grabber.MoveToMovementPoint));
                }
                
            }
        }
    }

    private void HandleOnInit()
    {
        Debug.Log("init player");
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

    private void HandleOnProcessingTurn()
    {
        if (currentPickedHole.containedSeeds.Count > 1)
        {

        }

        else
        {
            if (grabber.rotateCycleDone)
            {

            }

            else
            {

            }
        }
    }

    private void HandleOnGettingFreeTurn()
    {
        GameplayManager.Instance.currentActor = GetComponent<ActorBase>();
        UIManager.Instance.moveButton.enabled = true;

        foreach (BoardHole hole in boardHoles)
        {
            hole.touchable = true;
        }
    }

    private void HandleOnGettingMovethruTurn()
    {
        grabber.firstMove = true;
        UIManager.Instance.moveButton.enabled = false;
        foreach (BoardHole hole in boardHoles)
        {
            hole.touchable = false;
        }

        grabberGO.transform.position = currentPickedHole.gameObject.transform.position;
        StartCoroutine(grabber.GrabSeeds(grabber.MoveToMovementPoint));
    }

    private void HandleOnEndingTurn()
    {
        GameplayManager.Instance.currentActor = null;
        UIManager.Instance.moveButton.enabled = false;

        foreach (BoardHole hole in boardHoles)
        {
            hole.touchable = false;
        }
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
        FREE_TURN = 2,
        MOVETHRU_TURN = 3,
        SHOOTING = 4,
        END_TURN = 5,
        WIN = 6,
        GAME_OVER = 7
    }
}
