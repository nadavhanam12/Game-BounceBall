using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerAbstract;

[System.Serializable]
public class PlayerArgs
{
    [HideInInspector]
    public GameType GameType;
    public string Name;
    public PlayerIndex PlayerIndex;

    public Color Color = Color.red;
    public Texture Image;


    [HideInInspector] public bool AutoPlay = false;

    [HideInInspector] public PlayerScript PlayerScript;

    [HideInInspector] public int CurScore = 0;

    [HideInInspector] public int CurComboIndex = -1;
    [HideInInspector] public int CurCombo = 0;
    [HideInInspector] public GameBounds Bounds;
    [HideInInspector] public GameBallsManager BallsManager;
    [HideInInspector] public PickablesManager PickablesManager;
    [HideInInspector] public PlayerStats playerStats;

    public delegate void OnToggleBombUI(bool toShow);
    public OnToggleBombUI ToggleBombUI;

}
