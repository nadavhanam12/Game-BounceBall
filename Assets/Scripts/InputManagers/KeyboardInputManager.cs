using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputManager : IinputManager
{
    protected override void GetInputs()
    {
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.LeftArrow))
        {
            OnMoveLeftInputPressed();
        }
        if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.RightArrow))
        {
            OnMoveRightInputPressed();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            OnJumpInput();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            OnKickRegularInput();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnKickSpecialInput();
        }
    }
}
