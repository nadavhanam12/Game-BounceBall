using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IinputManager : MonoBehaviour
{
    private GameCanvasScript m_gameCanvas;
    protected bool m_initialized = false;

    public void Init(GameCanvasScript gameCanvas)
    {
        m_gameCanvas = gameCanvas;

        m_initialized = true;
    }

    void Update()
    {
        if (m_initialized)
        {
            GetInputs();
        }
    }

    protected abstract void GetInputs();

    protected void OnKickRegularInput()
    {
        //print("OnKickRegularInput");
        m_gameCanvas.OnKickRegularInput();
    }
    protected void OnKickSpecialInput()
    {
        //print("OnKickSpecialInput");
        m_gameCanvas.OnKickSpecialInput();
    }
    protected void OnMoveLeftInputPressed()
    {
        //print("OnMoveLeftInputPressed");
        m_gameCanvas.OnMoveLeftInputPressed();
    }
    protected void OnMoveRightInputPressed()
    {
        //print("OnMoveRightInputPressed");
        m_gameCanvas.OnMoveRightInputPressed();
    }
    protected void OnJumpInput()
    {
        //print("OnJumpInput");
        m_gameCanvas.OnJumpInput();
    }
    protected void OnInputEnd()
    {
        //print("OnInputEnd");
        m_gameCanvas.OnInputEnd();
    }
}
