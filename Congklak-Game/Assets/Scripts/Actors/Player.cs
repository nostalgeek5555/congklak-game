using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;
using DG.Tweening;

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
    public Transform grabberTransform;

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

    public void HandleOnMove()
    {
        if (states == States.GET_TURN)
        {
            if (currentPickedHole != null)
            {
                GameObject grabberGO = LeanPool.Spawn(grabber.gameObject, transform.parent);
                grabberGO.transform.position = currentPickedHole.gameObject.transform.position;
                grabberTransform = grabberGO.transform;

                StartCoroutine(GrabSeeds(grabberTransform, MoveThroughHoles));
            }
        }
    }

    private IEnumerator GrabSeeds(Transform grabber, Action<Transform> OnFinishGrabSeeds = null)
    {
        bool getAllSeed = false;

        for (int i = 0; i < currentPickedHole.containedSeeds.Count; i++)
        {
            Seed seed = currentPickedHole.containedSeeds[i];
            seed.transform.parent = grabber;
            seed.collider.enabled = false;
            seed.rigidbody.isKinematic = true;
            seed.rigidbody.freezeRotation = true;

            if (i == currentPickedHole.containedSeeds.Count - 1)
            {
                getAllSeed = true;
            }
        }

        yield return new WaitUntil(() => getAllSeed = true);
        Debug.Log($"grab all seed in this hole {getAllSeed}");

        OnFinishGrabSeeds?.Invoke(currentPickedHole.movementPoint);
    }

    public void MoveThroughHoles(Transform movePoint)
    {
        float dist = Vector3.Distance(grabberTransform.position, movePoint.position);
        float timeRequired = dist / 20f * Time.deltaTime;

        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.2f);
        sequence.Join(grabberTransform.DOMove(movePoint.position, 1f, false));

        sequence.AppendCallback(() =>
        {
            Debug.Log("finish move");
        });
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
        UIManager.Instance.moveButton.enabled = true;
    }

    private void HandleOnEndingTurn()
    {
        UIManager.Instance.moveButton.enabled = false;
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
