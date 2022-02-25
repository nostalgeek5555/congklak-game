using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class BoardHole : MonoBehaviour, IOnTriggerNotifiable, ICameraRaycastCollidable
{
    public int id;
    public Owner owner;
    public Type type;
    public SphereCollider sphereCollider;
    public Transform movementPoint;

    public List<Seed> containedSeeds = new List<Seed>();
    public int totalCurrentSeeds = 0;

    [Header("Interactable Properties")]
    public GameObject touchedSign;
    public bool touchable = false;
    public bool empty = true;
    public bool picked = false;

    public void InitHole()
    {
        if (containedSeeds.Count > 0)
        {
            touchable = true;
            empty = false;
        }
    }

    public void OnRaycastHit(CameraGameplayRaycaster initiator)
    {
        if (owner == Owner.PLAYER && type == Type.ORDINARY && touchable)
        {
            if (GameplayManager.Instance.player != null && GameplayManager.Instance.player.states == Player.States.FREE_TURN)
            {
                touchedSign.SetActive(true);
                if (GameplayManager.Instance.player.currentPickedHole != null)
                {
                    BoardHole boardHole = GameplayManager.Instance.player.currentPickedHole;
                    boardHole.touchedSign.SetActive(false);

                    GameplayManager.Instance.player.currentPickedHole = this;
                    GameplayManager.Instance.player.initHole = this;
                    GameplayManager.Instance.player.currentPickedHoleId = id;
                    GameplayManager.Instance.player.firstMovePointId = id;
                }

                else
                {
                    GameplayManager.Instance.player.currentPickedHole = this;
                    GameplayManager.Instance.player.initHole = this;
                    GameplayManager.Instance.player.currentPickedHoleId = id;
                    GameplayManager.Instance.player.firstMovePointId = id;
                }
            }
        }
    }

    public void ThrowSeed(Transform _target, float _height, float _duration)
    {
        Debug.Log("Throw seed");
        int randomSeed = Random.Range(0, containedSeeds.Count - 1);
        Seed seed = containedSeeds[randomSeed];
        seed.grabbed = false;
        seed.collider.enabled = false;
        seed.rigidbody.isKinematic = true;
        //seed.rigidbody.freezeRotation = true;
        seed.transform.SetParent(_target);

        BoardHole hole = _target.GetComponent<BoardHole>();

        DOTween.To(setter: value =>
        {
            Debug.Log(value);
            seed.transform.position = EMath.Parabola(seed.transform.position, _target.transform.position, _height, value);
        }, startValue: 0, endValue: 1, duration: _duration)
        .SetEase(Ease.OutCirc)
        .OnComplete(() =>
        {
            BoardHole hole = _target.gameObject.GetComponent<BoardHole>();
            hole.containedSeeds.Add(seed);
            seed.collider.enabled = true;
            seed.rigidbody.isKinematic = false;
            seed.rigidbody.freezeRotation = false;
            containedSeeds.Remove(seed);

            if (containedSeeds.Count > 0)
            {
                ThrowSeed(_target, _height, _duration);
            }
            
        });

        //else
        //{
        //    //if (GameplayManager.Instance.currentActor != null)
        //    //{
        //    //    switch (GameplayManager.Instance.currentActor.role)
        //    //    {
        //    //        case ActorBase.Role.PLAYER:
        //    //            GameplayManager.Instance.player.StateController(Player.States.END_TURN);
        //    //            break;
        //    //        case ActorBase.Role.AI:
        //    //            break;
        //    //        default:
        //    //            break;
        //    //    }
        //    //}
        //}
    }


    void IOnTriggerNotifiable.onChild_OnTriggerEnter(Collider myEnteredTrigger, Collider other)
    {
        if (other.TryGetComponent(out Seed seed))
        {
            if (seed.grabbed)
            {
                containedSeeds.Add(seed);
                seed.transform.SetParent(transform);
                GameplayManager.Instance.HandleOnSeedDropped(this);
            }
        }
    }

    void IOnTriggerNotifiable.onChild_OnTriggerStay(Collider myEnteredTrigger, Collider other)
    {

    }

    void IOnTriggerNotifiable.onChild_OnTriggerExit(Collider myEnteredTrigger, Collider other)
    {
        
    }

    public enum Owner
    {
        PLAYER = 0,
        ENEMY = 1,
    }

    public enum Type
    {
        ORDINARY = 0,
        BASE = 1
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sphereCollider.radius - 0.01f);

        Handles.color = Color.blue;
        Handles.ArrowHandleCap(0, transform.position, transform.rotation * Quaternion.Euler(-90, 0, 0), 0.15f, EventType.Repaint);
    }

}


