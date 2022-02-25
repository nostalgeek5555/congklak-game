using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public Collider collider;
    public Rigidbody rigidbody;

    public bool grabbed;

    public void Init()
    {
        grabbed = false;
    }
}
