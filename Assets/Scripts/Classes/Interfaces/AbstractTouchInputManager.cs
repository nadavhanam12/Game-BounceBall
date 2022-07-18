using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTouchInputManager : IinputManager
{
    protected abstract Touch GetCurTouch();
    private bool m_isPressedDown = false;
    private float m_sensitivityX = 20;
    private float m_sensitivityY = 100;
    private Vector2 m_firstTouchPosition;

    protected override void GetInputs()
    {
        // Handle screen touches.
        Touch touch = GetCurTouch();
        int touchCount = touch.tapCount;

        switch (touch.phase)
        {

            case TouchPhase.Began:
                m_firstTouchPosition = touch.position;
                break;
            case TouchPhase.Ended:
                if (m_isPressedDown)
                {
                    m_isPressedDown = false;
                    OnInputEnd();
                    break;
                }
                OnKickRegularInput();
                break;
            case TouchPhase.Moved:
                m_isPressedDown = true;
                CheckMovingTouch(touch);
                break;
            case TouchPhase.Stationary:
                if (m_isPressedDown)
                    CheckMovingTouch(touch);
                break;

        }
    }

    private void CheckMovingTouch(Touch touch)
    {
        //Vector2 delta = touch.deltaPosition;
        Vector2 delta = touch.position - m_firstTouchPosition;
        //print(delta);

        if (Math.Abs(delta.x) > Math.Abs(delta.y))
        {
            if (Math.Abs(delta.x) < m_sensitivityX)
                return;
            if (delta.x > 0)
                OnMoveRightInputPressed();
            else
                OnMoveLeftInputPressed();
        }
        else
        {
            if (Math.Abs(delta.y) < m_sensitivityY)
                return;
            if (delta.y > 0)
                OnJumpInput();
            else
                OnKickSpecialInput();
        }
    }
}
