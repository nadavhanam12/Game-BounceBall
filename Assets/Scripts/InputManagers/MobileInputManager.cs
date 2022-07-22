using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputManager : AbstractTouchInputManager
{
    protected override void GetInputs()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleInput(touch);
        }
    }

}
