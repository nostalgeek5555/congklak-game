using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Grabber : MonoBehaviour
{
    public ActorBase belongToActor;
    public Collider collider;
    public List<Seed> seeds = new List<Seed>();

    [Header("Grabber Movement Properties")]
    public bool firstMove = true;
    public bool rotateCycleDone = false;
    public List<BoardHole> mergedHoles = new List<BoardHole>();
    public List<BoardHole> trackableHoles = new List<BoardHole>();
    public int currentHoleIndex;

    public void AddAllSeeds()
    {
        seeds = new List<Seed>();

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Seed seed = transform.GetChild(i).GetComponent<Seed>();
                seeds.Add(seed);
                Debug.Log($"add seed to list {i}");
            }
        }
    }

    //procedures for getting all seeds in current picked hole
    public IEnumerator GrabSeeds(Action<int, float> OnFinishGrabSeeds = null)
    {
        bool getAllSeed = false;

        seeds = new List<Seed>();

        for (int i = 0; i < belongToActor.currentPickedHole.containedSeeds.Count; i++)
        {
            Seed seed = belongToActor.currentPickedHole.containedSeeds[i];
            seed.grabbed = true;
            seed.transform.parent = transform;
            seed.collider.enabled = false;
            seed.rigidbody.isKinematic = true;
            seed.rigidbody.freezeRotation = true;
            seeds.Add(seed);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(seed.transform.DOMove(seed.transform.parent.position, 0.2f, false));
            //belongToActor.currentPickedHole.containedSeeds.Remove(seed);

            if (i == belongToActor.currentPickedHole.containedSeeds.Count - 1)
            {
                getAllSeed = true;
            }
        }

        yield return new WaitUntil(() => getAllSeed = true);
        Debug.Log($"grab all seed in this hole {getAllSeed}");

        belongToActor.currentPickedHole.containedSeeds.Clear();
        int holeID = GameplayManager.Instance.allBoardHoles.IndexOf(belongToActor.currentPickedHole);
        OnFinishGrabSeeds?.Invoke(holeID, 1f);
    }


    //run through all existed board holes starting from current hole
    public void MoveToMovementPoint(int holeID, float moveDuration)
    {
        if (firstMove)
        {
            BoardHole boardHole = GameplayManager.Instance.allBoardHoles[holeID];
            Transform movePoint = boardHole.movementPoint;
            
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.Join(transform.DOMove(movePoint.position, moveDuration, false));

            sequence.AppendCallback(() =>
            {
                firstMove = false;
                trackableHoles = new List<BoardHole>(GetTrackableHoles());
                
                if (IfAnySameHoleWithFirstHole())
                {
                    rotateCycleDone = true;
                }

                else
                {
                    rotateCycleDone = false;
                }

                //after getting trackable holes then start moving along the movement points in trackable holes
                currentHoleIndex = 0;
                MoveToMovementPoint(currentHoleIndex, 1f);
            });
        }

        else
        {
            BoardHole boardHole = trackableHoles[holeID];
            Transform movePoint = boardHole.movementPoint;

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.Join(transform.DOMove(movePoint.position, moveDuration, false));

            sequence.AppendCallback(() =>
            {
                DropSeed();
            });
        }
    }


    //get trackable holes
    public List<BoardHole> GetTrackableHoles()
    {
        //start index from current picked hole
        int holeIndex = GameplayManager.Instance.allBoardHoles.IndexOf(belongToActor.currentPickedHole);
        mergedHoles = new List<BoardHole>();
        List<BoardHole> holes = new List<BoardHole>();

        //merging list from holes that not belong to current actor && has type == ORDINARY with holes that belong to current actor
        IEnumerable<BoardHole> holesNotBelongToActor = GameplayManager.Instance.allBoardHoles.Where(hole => !belongToActor.boardHoles.Contains(hole))
                                                        .Where(hole => hole.type == BoardHole.Type.ORDINARY).OrderBy(hole => hole.id);
        List<BoardHole> sortedHolesNotBelongToActor = holesNotBelongToActor.ToList();
        mergedHoles = new List<BoardHole>(belongToActor.boardHoles.Count + sortedHolesNotBelongToActor.Count);
        mergedHoles.AddRange(belongToActor.boardHoles);
        mergedHoles.AddRange(sortedHolesNotBelongToActor);


        for (int i = 0; i < seeds.Count; i++)
        {
            holeIndex ++;

            if (holeIndex < mergedHoles.Count)
            {
                holes.Add(mergedHoles[holeIndex]);
            }

            else
            {
                holeIndex = 0;
                holes.Add(mergedHoles[holeIndex]);
            }
        }

        return holes;
    }

    public bool IfAnySameHoleWithFirstHole()
    {
        IEnumerable<BoardHole> sameHoleTypes = trackableHoles.Where(hole => hole.id == belongToActor.initHole.id && hole.owner == belongToActor.initHole.owner
                                                && hole.type == belongToActor.initHole.type);
        List<BoardHole> sameHoles = sameHoleTypes.ToList();

        if (sameHoles.Count > 0)
        {
            return true;
        }

        else
        {
            return false;
        }
    }


    //dropping seeds
    private void DropSeed()
    {
        int randomSeed = Random.Range(0, seeds.Count - 1);
        Seed seed = seeds[randomSeed];
        seed.collider.enabled = true;
        seed.rigidbody.isKinematic = false;
        seed.rigidbody.freezeRotation = false;

        seeds.Remove(seed);
    }
}
