using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardManager : IinputManager
{
    protected override void GetInputs()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnJumpInput();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnKickSpecialInput();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else
            EndInput();
    }


}
