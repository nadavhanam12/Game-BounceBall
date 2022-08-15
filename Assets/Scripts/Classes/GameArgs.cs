using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;


public enum GameType
{
    OnePlayer = 0,
    TalTalGame = 3

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
