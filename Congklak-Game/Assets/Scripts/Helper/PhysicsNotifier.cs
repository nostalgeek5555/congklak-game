using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sends notification to the component on some other gameObject.
// This removes necessity to have trigger and its component on the same gameObject.
public interface IOnTriggerNotifiable
{
    void onChild_OnTriggerEnter(Collider myEnteredTrigger, Collider other);
    void onChild_OnTriggerStay(Collider myEnteredTrigger, Collider other);
    void onChild_OnTriggerExit(Collider myEnteredTrigger, Collider other);
}

public class PhysicsNotifier : MonoBehaviour
{
    //this component must inherit from  'IOnTriggerNotifiable' interface.
    [SerializeField] Component _toNotify = null;
    [SerializeField] Collider _myCollider = null;

#if UNITY_EDITOR
    void Reset() { _myCollider = GetComponent<Collider>(); }

    private void OnValidate()
    {
        if (_toNotify == null) { return; }

        // needed, because user will most likely drag an entire GAME OBJECT into our variable.
        // This means we would get assigned a Transform component, not the one with the interface.
        // Therefore, after assignment we find such interface ourselves, on the same gameObject:
        _toNotify = _toNotify.GetComponent(typeof(IOnTriggerNotifiable));

        if (_toNotify == null)
        {
            Debug.LogError("PhysicsOnTrigger_notifier couldn't grab hold of compoent you wish to notify."
                          + "\nIt means there is no component implementing the IOnTriggerNotifiable interface.");
        }
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        ((IOnTriggerNotifiable)_toNotify).onChild_OnTriggerEnter(_myCollider, other);
    }

    private void OnTriggerStay(Collider other)
    {
        ((IOnTriggerNotifiable)_toNotify).onChild_OnTriggerStay(_myCollider, other);
    }

    private void OnTriggerExit(Collider other)
    {
        ((IOnTriggerNotifiable)_toNotify).onChild_OnTriggerExit(_myCollider, other);
    }
}
