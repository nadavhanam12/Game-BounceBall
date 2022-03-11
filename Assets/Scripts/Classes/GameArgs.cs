using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;


public enum GameType
{
    OnePlayer = 0,
    TwoPlayer = 1,
    TurnsGame = 2

}
public class GameArgs
{
    public GameType GameType;
    public MainMenu MainMenu;
    public Texture Background;

    public GameArgs(GameType gameType, MainMenu mainMenu)
    {
        GameType = gameType;
        MainMenu = mainMenu;
    }


}
