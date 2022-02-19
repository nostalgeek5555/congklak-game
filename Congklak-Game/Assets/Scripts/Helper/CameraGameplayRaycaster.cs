using UnityEngine;

public interface ICameraRaycastCollidable
{
    public void OnRaycastHit(CameraGameplayRaycaster initiator);
}

public class CameraGameplayRaycaster : MonoBehaviour
{
    [SerializeField] Camera _myCam = null;
    [SerializeField] LayerMask _raycastMask;

    void OnTouched(TouchInfo touchInfo)
    {
        Ray ray = _myCam.ScreenPointToRay(touchInfo.mostRecent_pixelPos);
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, _raycastMask.value);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 100f);
        Debug.Log($"raycast from camera origin {ray.origin} to {ray.direction}");
        if (!hit)
        {
            return;
        }

        var col = hitInfo.collider.GetComponentInParent<ICameraRaycastCollidable>();
        if (col == null) { return; }
        col.OnRaycastHit(this);
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    CheckTouch(Input.GetTouch(0).position);
                }

                if (Input.touchCount > 1)
                {
                    if (Input.GetTouch(1).phase == TouchPhase.Began)
                    {
                        CheckTouch(Input.GetTouch(1).position);
                    }
                }
            }

        }
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CheckTouch(Input.mousePosition);
            }
        }
    }

    private Ray ray;
    private void CheckTouch(Vector3 pos)
    {
        ray = _myCam.ScreenPointToRay(pos);
        Debug.DrawRay(ray.origin, pos, Color.red, 20f);
        //check if touched lower area near camera
        //if yes,use sphere cast, if no, use raycast
        if (pos.y <= Screen.height * 0.25f)
        {
            if (Physics.SphereCast(ray.origin, 0.75f, ray.direction, out RaycastHit hitInfo, 300, _raycastMask))
            {
                var col = hitInfo.collider.GetComponentInParent<ICameraRaycastCollidable>();
                if (col == null) { return; }
                col.OnRaycastHit(this);
                Debug.Log("use spherecast");
            }
        }
        else
        {
            if (Physics.SphereCast(ray.origin, 0.3f, ray.direction, out RaycastHit hitInfo, 300, _raycastMask))
            {
                var col = hitInfo.collider.GetComponentInParent<ICameraRaycastCollidable>();
                if (col == null) { return; }
                col.OnRaycastHit(this);
                Debug.Log("use raycast");
            }
        }
    }
}
