using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBase : MonoBehaviour
{
    public Role role;
    public Grabber grabber;
    public GameObject grabberGO;
    public int currentPickedHoleId = -1;
    public int firstMovePointId = -1;
    public BoardHole initHole;
    public BoardHole currentPickedHole;
    public Transform boardHoleParent;
    public List<BoardHole> boardHoles = new List<BoardHole>();

    public enum Role
    {
        PLAYER = 0,
        AI = 1
    }
}
