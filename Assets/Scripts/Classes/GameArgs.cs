using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;


public enum GameType
{
    SinglePlayer = 0,
    PvE = 1,
    PvP = 2,


}
public class GameArgs
{
    public GameType GameType;
    public Texture Background;

    public GameArgs(GameType gameType)
    {
        GameType = gameType;
    }


}
