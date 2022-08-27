using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputManager : AbstractTouchInputManager
{

    private TouchPointer m_touchPointer;
    Vector3 touchPosition3D;
    Vector2 touchPosition2D;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected override void Start()
    {
        base.Start();
        m_touchPointer = FindObjectOfType<TouchPointer>(true);
        m_touchPointer.Init();
        touchPosition3D = Vector3.zero;
        touchPosition2D = Vector2.zero;
    }
    protected override void GetInputs()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleInput(touch);
            touchPosition3D.x = touch.position.x;
            touchPosition3D.y = touch.position.y;
            touchPosition3D = m_camera.ScreenToWorldPoint(touchPosition3D);
            touchPosition2D.x = touchPosition3D.x;
            touchPosition2D.y = touchPosition3D.y;
            m_touchPointer.Activate(touchPosition2D);
        }
        else
        {
            m_touchPointer.StopTouch();
        }
    }

}
