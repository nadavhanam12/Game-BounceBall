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
            GetInputs();
    }

    protected abstract void GetInputs();

    protected void OnKickSpecialInput()
    {
        m_gameCanvas.OnKickSpecialInput();
    }
    protected void OnJumpInput()
    {
        m_gameCanvas.OnJumpInput();
    }
    protected void OnDoubleTap()
    {
        OnKickSpecialInput();
    }
    protected void MovePlayerToPosition(Vector2 position)
    {
        m_gameCanvas.MovePlayerToPosition(position);
    }
    protected void EndInput()
    {
        m_gameCanvas.OnEndInput();
    }
    protected void MoveRight()
    {
        m_gameCanvas.MoveRight();
    }
    protected void MoveLeft()
    {
        m_gameCanvas.MoveLeft();
    }



}
