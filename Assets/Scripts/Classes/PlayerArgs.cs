using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;

[System.Serializable]
public class PlayerArgs
{
    public string Name;
    public PlayerIndex PlayerIndex;

    public Color Color = Color.red;

    [HideInInspector] public bool AutoPlay = false;

    [HideInInspector] public PlayerScript PlayerScript;

    [HideInInspector] public int CurScore;

    [HideInInspector] public int CurCombo;
    [HideInInspector] public ComboData CurComboData;

    [HideInInspector] public GameBounds Bounds;
    [HideInInspector] public GameBallsManager BallsManager;
    [HideInInspector] public PlayerStats playerStats;

}
