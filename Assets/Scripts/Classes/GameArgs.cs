using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerAbstract;


public enum GameType
{
    SinglePlayer = 0,
    PvE = 1,
    PvP = 2,


}
[Serializable]
public class GameArgs
{
    public GameType GameType;
    public Texture Background;
    public int MatchTime = 600;
    public float CountDownDelay = 1f;
    public bool ShouldPlayTutorial = false;
    public bool ShouldPlayCountdown = false;
    public bool WithKeyboard = true;
    public int ComboKicksAmount = 3;

    public GameArgs(GameType gameType)
    {
        GameType = gameType;
    }


}
