using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoardMovementPoint : MonoBehaviour
{
    public int movementPointID;
    public SphereCollider sphereCollider;


    public void OnTriggerEnter(Collider other)
    {
        
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(sphereCollider.bounds.center, sphereCollider.radius);

        Handles.color = Color.yellow;
        Handles.ArrowHandleCap(0, transform.position, transform.rotation * Quaternion.Euler(-90, 0, 0), 0.15f, EventType.Repaint);
    }

}
