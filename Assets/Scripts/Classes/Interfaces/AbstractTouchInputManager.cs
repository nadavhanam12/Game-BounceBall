using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTouchInputManager : IinputManager
{
    private bool m_isPressedDown = false;
    private float m_sensitivityX = 0;
    private float m_sensitivityY = 30;
    private bool m_isMovingRight;
    private Vector2 m_firstTouchPosition;

    protected void HandleInput(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                //print("Began");
                m_firstTouchPosition = touch.position;
                break;
            case TouchPhase.Stationary:
                //print("Stationary");
                if (m_isPressedDown)
                    if (m_isMovingRight)
                        OnMoveRightInputPressed();
                    else
                        OnMoveLeftInputPressed();

                break;
            case TouchPhase.Moved:
                //print("Moved");
                m_isPressedDown = true;
                CheckMovingTouch(touch);
                break;

            case TouchPhase.Ended:
                //print("Ended");
                if (m_isPressedDown)
                {
                    m_isPressedDown = false;
                    //OnInputEnd();
                    // break;
                }
                OnKickRegularInput();
                break;

        }
    }

    private void CheckMovingTouch(Touch touch)
    {
        Vector2 delta = touch.deltaPosition;
        //Vector2 delta = touch.position - m_firstTouchPosition;

        if (Math.Abs(delta.x) > Math.Abs(delta.y))
        {
            if (Math.Abs(delta.x) < m_sensitivityX)
                return;
            if (delta.x > 0)
            {
                m_isMovingRight = true;
                OnMoveRightInputPressed();
            }
            else
            {
                m_isMovingRight = false;
                OnMoveLeftInputPressed();
            }
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
