using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public ActorBase belongToActor;
    public Collider collider;
    public List<Seed> seeds = new List<Seed>();

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
}
