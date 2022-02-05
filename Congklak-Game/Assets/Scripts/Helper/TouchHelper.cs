using System.Linq;
using System.Collections.Generic;
using UnityEngine;


// attach on some gameObject, and it will function as a singleton.
// Processes touches & keeps track of the ORDER the touches were spotted.
//
// If your Ui object received a touch, that object can tell the TouchHelper to invoke a certain function
// when TouchHelper no longer sees that touch.
//
// This way, even if the touch MOVES OUT of your UI element's region, you can track it's presence.
// You will know if the touch is still pressed, and will be notified when the TouchHelper no longer sees that touch.
// You can also get info about the touch - the curve-distance it travelled, when it started, etc.
//
// FRAME RATE INDEPENDENT :DDD  (will properly de-register touches even during low fps - no zombie touches left behind)
//
// Also, tracks left, right, middle mouse buttons (but if there are touches CURRENTLY pressed, will ignore mouse down)
//
// 
// PLEASE PUT ME IN CREDITS OF YOUR GAME
// Igor Aherne 9/Oct/2017  facebook.com/igor.aherne.
public class TouchHelper : MonoBehaviour
{
    class pair<T1, T2>
    {
        public T1 obj1;
        public T2 obj2;
        public pair(T1 obj1, T2 obj2) { this.obj1 = obj1; this.obj2 = obj2; }
    }

    [Header("singleton, tracks the current touches anywhere on the screen")]

    #region singleton stuff
    private static TouchHelper _instance;
    public static TouchHelper instance
    {
        get { return _instance; }
    }
    #endregion

    #region internal vars
    //all the touches which were in the previous frame. Gets refreshed every Update().
    //Will allow us to determine the most recent touch (sits at the end of the list)
    List<TouchInfo> _mostRecentTouchIDs = new List<TouchInfo>(10);


    // invoked as soon as there is any new touch observed.
    // The argument of your callback function is the Touch Id
    //
    // NOTICE - using separate collection for _toAdd and _toRemove  to guarantee to the user that his requested callbacks
    // will NOT be called at the current frame, but on the next one:
    List<TouchInfoArg_FuncPtr> _onAnyTouchDown = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onAnyTouchDown_toAdd = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onAnyTouchDown_toRmv = new List<TouchInfoArg_FuncPtr>(100);

    List<TouchInfoArg_FuncPtr> _onAnyTouchUp = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onAnyTouchUp_toAdd = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onAnyTouchUp_toRmv = new List<TouchInfoArg_FuncPtr>(100);

    //when user wishes us to auto-remove his callback after invoking it once:
    List<TouchInfoArg_FuncPtr> _onAnyTouchDownONCE = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onAnyTouchDownONCE_toAdd = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onAnyTouchDownONCE_toRmv = new List<TouchInfoArg_FuncPtr>(100);

    //when the last touch is about to be lifted
    List<TouchInfoArg_FuncPtr> _onLastTouchUp = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onLastTouchUp_toAdd = new List<TouchInfoArg_FuncPtr>(100);
    List<TouchInfoArg_FuncPtr> _onLastTouchUp_toRmv = new List<TouchInfoArg_FuncPtr>(100);



    //supply your callback and specify which touch to pay attention to.
    // When the specified touch is no longer found pressed, we will invoke the provided function, & will auto-remove the entry.
    Dictionary<int, TouchInfoArg_FuncPtr> _touchID_to_onTouchEnd_ONCE = new Dictionary<int, TouchInfoArg_FuncPtr>(100);
    List<pair<int, TouchInfoArg_FuncPtr>> _touchID_to_OnTouchEnd_toAdd = new List<pair<int, TouchInfoArg_FuncPtr>>(100);
    List<pair<int, TouchInfoArg_FuncPtr>> _touchID_to_OnTouchEnd_toRmv = new List<pair<int, TouchInfoArg_FuncPtr>>(100);


    // starting from the moment when touches began, what is the max number of touches that were pressed together?
    // Will be reset to 1 after [no touches seen, then first touch observed]
    // 
    // Helps identify how many touches user intended to press together, during the most recent interaction.
    // For example, some of your elements might respond to "on touch up" only 
    // if user didn't press 3 touches together, previously.
    int _maxNumTouches_recentInteraction;
    public int maxNumTouches_recentInteraction { get { return _maxNumTouches_recentInteraction; } }

    float _lastUpdateTime = -1;
    #endregion

    public static int TOUCH_NONE = -9999;//not  -1 because -1,-2,-3 is pointer id in PC.


    //returns null if such touch is not tracked yet.
    // Otherwise returns information about travelled distance, time of the touch, etc.
    // we will invoke the provided function, & will remove the entry.
    public TouchInfo getCurrent_TouchInfo_forTouch(int touchID)
    {
        Update_ifNeeded();
        return _mostRecentTouchIDs.FirstOrDefault(t => t.touchID == touchID);
    }


    public TouchInfo tryGetTouch_fromScreenCoord(Vector2 position)
    {
        Update_ifNeeded();
        return _mostRecentTouchIDs.FirstOrDefault(t => t.mostRecent_pixelPos == position);
    }


    //useful when your several mouse buttons are pressed (-1, -2, etc).
    //Unlike traditional touches, such "touches" always have the same position.
    public List<TouchInfo> getAllTouches_fromScreenCoord(Vector2 position)
    {
        Update_ifNeeded();
        List<TouchInfo> matchingTouches = _mostRecentTouchIDs.Where(t => t.mostRecent_pixelPos == position).ToList();
        // Debug.Log("you requested touches for position: " + position.x + ", " + position.y +" Current touches are:");
        // string ts = "";
        // foreach(var ti in matchingTouches){
        //     ts += (ti.mostRecent_pixelPos.x.ToString() + ", " + ti.mostRecent_pixelPos.y.ToString() + ";  ");
        // }
        //Debug.Log( ts );

        return matchingTouches;
    }


    #region on-touch callbacks (register)
    // The argument of your callback function is the Touch Id.
    public void OnAnyTouchDown_ONCE(TouchInfoArg_FuncPtr toCall)
    {
        _onAnyTouchDownONCE_toAdd.Add(toCall);
    }

    //For your convenience ther is also  OnAnyTouchDown_ONCE()  which auto-unsubscribes.
    //This one doesn't  -don't forget to unsubscribe some time later!
    public void OnAnyTouchDown(TouchInfoArg_FuncPtr toCall)
    {
        _onAnyTouchDown_toAdd.Add(toCall);
    }


    //don't forget to unsubscribe some time later!
    public void OnAnyTouchUp(TouchInfoArg_FuncPtr toCall)
    {
        _onAnyTouchUp_toAdd.Add(toCall);
    }

    // adds your function to the list of other functions associated with the required touch ID.
    // When this touch ID is no longer observed (was lifted), all callbacks are invoked, and callback
    // etc (for this touch) are auto-removed.
    public void OnTouchUp_ONCE(TouchInfoArg_FuncPtr toCall, int whenThisTouchEnded)
    {
        var p = new pair<int, TouchInfoArg_FuncPtr>(whenThisTouchEnded, toCall);
        _touchID_to_OnTouchEnd_toAdd.Add(p);
    }

    public void OnLastTouchUp(TouchInfoArg_FuncPtr toCall)
    {
        _onLastTouchUp_toAdd.Add(toCall);
    }
    #endregion on-touch callbacks (register)


    #region on-touch callbacks (unregister)
    public void rmv_onAnyTouchDownONCE(TouchInfoArg_FuncPtr toRmv)
    {
        _onAnyTouchDownONCE_toRmv.Add(toRmv);
    }

    public void rmv_OnAnyTouchDown(TouchInfoArg_FuncPtr toRmv)
    {
        _onAnyTouchDown_toRmv.Add(toRmv);
    }


    public void rmv_OnAnyTouchUp(TouchInfoArg_FuncPtr toRmv)
    {
        _onAnyTouchUp_toRmv.Add(toRmv);
    }


    public void rmv_OnTouchUp(int touchId, TouchInfoArg_FuncPtr func)
    {
        _touchID_to_OnTouchEnd_toRmv.Add(new pair<int, TouchInfoArg_FuncPtr>(touchId, func));
    }


    public void rmv_OnLastTouchUp(TouchInfoArg_FuncPtr toRmv)
    {
        _onLastTouchUp_toRmv.Add(toRmv);
    }
    #endregion


    void Awake()
    {
        //assign self as a singleton instance, if there is nothing in _instance already:
        if (_instance != null)
        {
#if UNITY_EDITOR
            //MiscTools.Editor_Misc.printf_inEditor_duplicateSingleton_Removed( this,
            //                                                              _instance.gameObject );
#endif
            DestroyImmediate(this.gameObject);
            return;
        }
        _instance = this;

        transform.SetParent(null); //make a root, so DontDestroyOnLoad dones't complain in xCode
        //DontDestroyOnLoad(this.gameObject);
    }


    private void OnDestroy()
    {
        if (_instance != this) { return; }

        Update_ifNeeded();

        for (int i = 0; i < _mostRecentTouchIDs.Count; ++i)
        {
            TouchInfo tinf = _mostRecentTouchIDs[i];

            _onAnyTouchUp.ForEach(o => o.Invoke(tinf));
            if (i == _mostRecentTouchIDs.Count - 1)
            {
                _onLastTouchUp.ForEach(o => o.Invoke(tinf));
            }
        }//end for most recent touch IDs
    }


    private void Update()
    {
        Update_ifNeeded();
    }


    // Invoke this every time one of our "get" functions is called. 
    // Such a "get" function might be called from OnPointerEnter() etc,  -invoked by Unity's EventSystem.
    // Event System is called BEFORE any scripts, regardless of compilation order, so important we are up-to-date.
    void Update_ifNeeded()
    {
        if (_lastUpdateTime == Time.unscaledTime) { return; }
        _lastUpdateTime = Time.unscaledTime;

        Rmv_and_Add_callbacks();
        //refresh our structure of touches. We maintain it to be able to tell which of the touches were applied last.
        RefreshMostRecentTouches();
        // once we ensured all of our touches are present in Input.touches and we are tracking all touchs from Input.touches,
        // refresh information on each such a touch (travelled distance, most recent position, etc) 
        UpdateExistingTouches();
    }


    private void Rmv_and_Add_callbacks()
    {
        #region adding
        foreach (var func in _onAnyTouchDownONCE_toAdd)
        {
            if (_onAnyTouchDownONCE.Contains(func)) { continue; }
            _onAnyTouchDownONCE.Add(func);
        }

        foreach (var func in _onAnyTouchDown_toAdd)
        {
            if (_onAnyTouchDown.Contains(func)) { continue; }
            _onAnyTouchDown.Add(func);
        }

        foreach (var func in _onAnyTouchUp_toAdd)
        {
            if (_onAnyTouchUp.Contains(func)) { continue; }
            _onAnyTouchUp.Add(func);
        }

        foreach (var kvp in _touchID_to_OnTouchEnd_toAdd)
        {
            int touch = kvp.obj1;
            var func = kvp.obj2;
            if (_touchID_to_onTouchEnd_ONCE.ContainsKey(touch) == false)
            {
                _touchID_to_onTouchEnd_ONCE.Add(touch, func);
                continue;
            }

            //checking for 'null' to avoid error in future with .GetINvocationList()
            if (_touchID_to_onTouchEnd_ONCE[touch] == null)
            {
                _touchID_to_onTouchEnd_ONCE[touch] += func;
                continue;
            }

            if (_touchID_to_onTouchEnd_ONCE[touch].GetInvocationList().Contains(func))
            {
                //requested function is already present amonst callbacks for this specific touch. 
                //Don't allow duplicate callbacks for this touch!  - just in case user made a mistake :)
                continue;
            }
            _touchID_to_onTouchEnd_ONCE[touch] += func;
        }

        foreach (var func in _onLastTouchUp_toAdd)
        {
            if (_onLastTouchUp.Contains(func)) { continue; }
            _onLastTouchUp.Add(func);
        }
        #endregion

        // now remove callbacks.  NOTICE:  Doing it AFTER adding, in case user 
        // "changed his mind" on the same frame after registering for a callback.
        #region removing
        foreach (var func in _onAnyTouchDownONCE_toRmv)
        {
            _onAnyTouchDownONCE.Remove(func);
        }

        foreach (var func in _onAnyTouchDown_toRmv)
        {
            _onAnyTouchDown.Remove(func);
        }

        foreach (var func in _onAnyTouchUp_toRmv)
        {
            _onAnyTouchUp.Remove(func);
        }

        foreach (var kvp in _touchID_to_OnTouchEnd_toRmv)
        {
            int touch = kvp.obj1;
            var func = kvp.obj2;
            if (_touchID_to_onTouchEnd_ONCE.ContainsKey(touch) == false) { continue; }
            _touchID_to_onTouchEnd_ONCE[touch] -= func;
        }

        foreach (var func in _onLastTouchUp_toRmv)
        {
            _onLastTouchUp.Remove(func);
        }
        #endregion

        _onAnyTouchDownONCE_toAdd.Clear();
        _onAnyTouchDown_toAdd.Clear();
        _onAnyTouchUp_toAdd.Clear();
        _touchID_to_OnTouchEnd_toAdd.Clear();
        _onLastTouchUp_toAdd.Clear();

        _onAnyTouchDownONCE_toRmv.Clear();
        _onAnyTouchDown_toRmv.Clear();
        _onAnyTouchUp_toRmv.Clear();
        _touchID_to_OnTouchEnd_toRmv.Clear();
        _onLastTouchUp_toRmv.Clear();
    }



    //updates our _mostRecentTouchIds, allowing us to remember which touch was "pressed the latest", even if some of the 
    //old touches are lifted-off.
    private void RefreshMostRecentTouches()
    {

        //find all the touches in our list which are no longer in the Input.
        #region
        List<TouchInfo> touch_ids_toRMV = null;
        foreach (TouchInfo touchInfo in _mostRecentTouchIDs)
        {

            if (isShouldDestroy_TouchInfo(touchInfo) == false)
            {
                continue;//the touch tracked by us is still in the Input.touches,  so let it be.
            }

            touch_ids_toRMV = touch_ids_toRMV ?? new List<TouchInfo>(5);
            touch_ids_toRMV.Add(touchInfo);
        }//end foreach of our currently observed touchInfos
        #endregion

        //discard such no longer observed touches:
        #region
        if (touch_ids_toRMV != null)
        {
            for (int i = 0; i < touch_ids_toRMV.Count; ++i)
            {
                TouchInfo touchInfo = touch_ids_toRMV[i];
                //Invoke() all delegates when any touch went up:
                _onAnyTouchUp.ForEach(c => c(touchInfo));

                if (_touchID_to_onTouchEnd_ONCE.ContainsKey(touchInfo.touchID))
                {
                    //Invoke() all delegates who track the ending of this specific touch:
                    _touchID_to_onTouchEnd_ONCE[touchInfo.touchID]?.Invoke(touchInfo);
                    _touchID_to_onTouchEnd_ONCE.Remove(touchInfo.touchID);
                }

                //remove AFTER callback invocations
                _mostRecentTouchIDs.Remove(touchInfo);//remove "this index value", not "AT this index".

            }//end for each touch-to-forget

            //if there are now no more touches, invoke  OnLastTouchUp() callbacks:
            if (_mostRecentTouchIDs.Count == 0)
            {
                // Just use ix 0 (even though multiple touchesmight have been lifted)
                // This is to avoid passing copies of the list.
                TouchInfo removed_tInfo = touch_ids_toRMV[0];
                _onLastTouchUp.ForEach(c => c(removed_tInfo));
            }
        }//end if (touch_ids_toRMV != null)

        #endregion


        //find all the touches in the Input.touches which are not yet included into our list of recent touches.
        //Such new touches will of course become the "most recent" ones.
        #region
        grow_MaxNumRecentTouches_ifNeeded();

        bool wasTouchDown = false;

        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch touch = Input.touches[i];

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                continue;//don't add a touch that was ended
            }

            if (_mostRecentTouchIDs.Any(tInfo => tInfo.touchID == touch.fingerId))
            {
                continue; //already tracking this touch
            }

            //else, Input has a touch that we are not tracking yet. Store it:
            TouchInfo touchInfo = new TouchInfo(touch.fingerId, touch.position);
            _mostRecentTouchIDs.Add(touchInfo);
            //notify any new subscribers about this new touch:
            _onAnyTouchDown.ForEach(func => func(touchInfo));
            _onAnyTouchDownONCE.ForEach(func => func(touchInfo));

            wasTouchDown = true;
        }//end for-every touch in Input

        if (wasTouchDown)
        {//notice, only touch down if touch was appropriate
            _onAnyTouchDownONCE.Clear();
        }

        if (Input.touchCount > 0)
        {
            // DON'T PROCESS mouseDown when there are touches 
            // (because mouseDowns always launch if there are touches)
            return;
        }

        bool wasMouseDown = false;

        //else, include mouse as well:
        if (Input.GetMouseButtonDown(0))
        {
            TouchInfo touchInfo = new TouchInfo(-1, Input.mousePosition);
            _mostRecentTouchIDs.Add(touchInfo);
            _onAnyTouchDown.ForEach(func => func(touchInfo));
            _onAnyTouchDownONCE.ForEach(func => func(touchInfo));
            wasMouseDown |= true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            TouchInfo touchInfo = new TouchInfo(-2, Input.mousePosition);
            _mostRecentTouchIDs.Add(touchInfo);
            _onAnyTouchDown.ForEach(func => func(touchInfo));
            _onAnyTouchDownONCE.ForEach(func => func(touchInfo));
            wasMouseDown |= true;
        }

        if (Input.GetMouseButtonDown(2))
        {
            TouchInfo touchInfo = new TouchInfo(-3, Input.mousePosition);
            _mostRecentTouchIDs.Add(touchInfo);
            _onAnyTouchDown.ForEach(func => func(touchInfo));
            _onAnyTouchDownONCE.ForEach(func => func(touchInfo));
            wasMouseDown |= true;
        }
        #endregion

        if (wasMouseDown)
        {
            _onAnyTouchDownONCE.Clear();
        }
    }//end ()



    void grow_MaxNumRecentTouches_ifNeeded()
    {
        int num = Input.touchCount;

        // ONLY PROCESS mouse when there are no touches 
        // (because mouse always launch if there are touches, which is undesirable):
        if (Input.touchCount == 0)
        {
            if (Input.GetMouseButton(0)) { num++; }
            if (Input.GetMouseButton(1)) { num++; }
            if (Input.GetMouseButton(2)) { num++; }
        }

        if (num == 0)
        {//no more touches, so reset the counter
            _maxNumTouches_recentInteraction = 0;
            return;
        }

        ////else there are some touches.  See if there were no touches before:
        //if(_mostRecentTouchIDs.Count ==0){
        //    _maxNumTouches_recentInteraction  = num;  
        //}

        //if current number of touches is largest since the last reset, copy into the counter:
        if (num > _maxNumTouches_recentInteraction)
        {
            _maxNumTouches_recentInteraction = num;
        }
    }



    //true if the Input.touches has no touch with needed fingerID
    // or if that touch has just Began 
    bool isShouldDestroy_TouchInfo(TouchInfo touchInfo)
    {

#if UNITY_EDITOR
        if (touchInfo.touchID < 0)
        {
            //this is a mouse, check if mouse is lifed. Notice, when Unity notices it has a touchpad connected,
            //it will make left mouse -1, right mouse -2, middle mouse -3
            if (Input.GetMouseButtonUp(Mathf.Abs(touchInfo.touchID + 1)))
            {
                return true;
            }
            else
            {
                return false;//don't destoroy the touchInfo
            }
        }
#endif


        foreach (Touch t in Input.touches)
        {
            if (t.fingerId != touchInfo.touchID)
            {
                continue;
            }
            // Notice, if there is Touch in input, AND that  didn't End.
            //
            // If Input's touch DID end, then we will need to remove currently tracked touchInfo 
            //
            // This is important when user touches very quickly during low framerate, and two separate touches appear in two consecutive frames.
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                continue;
            }

            //shouldn't be destroyed, because Input.touches contains a matching Touch:
            return false;
        }//end foreach


        //the touchInfo must be destroyed
        return true;
    }




    //call after we've ensured our tracked touchInfos correspond to Input.touches.
    void UpdateExistingTouches()
    {
        float inverseScreenWidth = 1.0f / Screen.width;//multiplication will be faster than division
        //loop:
        for (int i = 0; i < _mostRecentTouchIDs.Count; ++i)
        {
            //get info:
            TouchInfo touchInfo = _mostRecentTouchIDs[i];

#if UNITY_EDITOR
            //process mouse click, for debugging in-editor 
            //(unity makes Lmouse -1, Rmouse -2, MiddleMouse -3  when it notices it has touchpad connected)
            if (touchInfo.touchID < 0)
            {
                Vector2 newPos = Input.mousePosition;
                //update this touchInfo with info (increase the distance travelled, update most recent pos, etc):
                touchInfo.totalMovedDist_rel_screenWidth += Vector2.Distance(touchInfo.mostRecent_pixelPos,
                                                                              newPos) * inverseScreenWidth;
                touchInfo.mostRecent_pixelPos = newPos;
                continue;
            }
#endif

            Vector2 newPosition = Input.touches.First(t => t.fingerId == touchInfo.touchID).position;

            //update this touchInfo with info (increase the distance travelled, update most recent pos, etc):
            touchInfo.totalMovedDist_rel_screenWidth += Vector2.Distance(touchInfo.mostRecent_pixelPos,
                                                                          newPosition) * inverseScreenWidth;
            touchInfo.mostRecent_pixelPos = newPosition;
        }//end foreach of our currrently observed touchInfos
    }


}




public class TouchInfo
{
    public int touchID = -9999; //can contain PointerEventData.touchID, etc.
    public float startTime_unscaled = Time.unscaledTime;

    public Vector2 start_pixelPos;
    public Vector2 mostRecent_pixelPos;

    //allows us to see if the related touch moved at least a little, throughout its existance.
    //Relative to screen for example, (4000.3pixels/Screen.width)
    //
    // Notice, it's not a straight line between start and end - it will have length of a curve, because
    // it's updated every frame from current position, by TouchHelper.instance
    public float totalMovedDist_rel_screenWidth = 0.0f;

    //uses screen's DPI, to determine the displacement from start to the most recent position.
    public float startPosToRecent_inchesMoved
    {
        get { return (mostRecent_pixelPos - start_pixelPos).magnitude / Screen.dpi; }
    }

    public TouchInfo(int touchID, Vector2 mostRecentPos)
    {
        this.touchID = touchID;
        this.start_pixelPos = this.mostRecent_pixelPos = mostRecentPos;
    }
}




public delegate void TouchInfoArg_FuncPtr(TouchInfo touchInfo);