using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputManager : AbstractTouchInputManager
{
    private Touch m_touch = new Touch();
    protected override Touch GetCurTouch()
    {
        if (Input.touchCount > 0)
            return Input.GetTouch(0);
        return m_touch;
    }

}
