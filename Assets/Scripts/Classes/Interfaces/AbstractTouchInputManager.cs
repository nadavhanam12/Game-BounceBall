using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTouchInputManager : IinputManager
{
    private bool m_isPressedDown = false;
    private float m_sensitivityX = 0;
    private float m_sensitivityY = 30;
    private Vector2 m_firstTouchPosition;
    protected Camera m_camera;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected virtual void Start()
    {
        m_camera = Camera.main;
    }

    protected void HandleInput(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
            m_firstTouchPosition = touch.position;
        if (touch.phase == TouchPhase.Ended)
        {
            EndInput();
            return;
        }
        CheckMovingTouch(touch);
    }

    private void CheckMovingTouch(Touch touch)
    {
        Vector2 delta = touch.deltaPosition;
        if (Math.Abs(delta.y) > 100 || Math.Abs(delta.x) > 100)
        {
            //print(delta);
            return;
        }
        //Vector2 delta = touch.position - m_firstTouchPosition;
        if (Math.Abs(delta.y) < m_sensitivityY)
        {
            Vector2 pos = GetTouchWorldPosition(touch);
            MovePlayerToPosition(pos);
            return;
        }
        if (delta.y > 0)
            OnJumpInput();
        else
            OnKickSpecialInput();

    }

    Vector2 GetTouchWorldPosition(Touch touch)
    {
        Vector3 touchPosition3D = Vector3.zero;
        Vector2 touchPosition2D = Vector2.zero;
        touchPosition3D.x = touch.position.x;
        touchPosition3D.y = touch.position.y;
        touchPosition3D = m_camera.ScreenToWorldPoint(touchPosition3D);
        touchPosition2D.x = touchPosition3D.x;
        touchPosition2D.y = touchPosition3D.y;
        return touchPosition2D;
    }
}
