using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;


[System.Serializable]
public class ComboData
{
    public int Index;
    public int ComboRequired;
    public int ScoreAdded;
    public Sprite BallTexture;
    public ComboData(int index)
    {
        Index = index;
        ComboRequired = Index * 4;
        ScoreAdded = Index * 50;

    }


}
