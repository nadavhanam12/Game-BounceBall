using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;
using static PlayerScript;

[System.Serializable]
public class ComboDataContainer : MonoBehaviour
{

    public Dictionary<KickType, int> m_scoreDictionary = new Dictionary<KickType, int>()
     {
        {KickType.Regular, 100},
        {KickType.Up, 250},
        {KickType.Power, 250},
    };


    [SerializeField] private ComboData ComboIndex0 = new ComboData(0);
    [SerializeField] private ComboData ComboIndex1 = new ComboData(1);
    [SerializeField] private ComboData ComboIndex2 = new ComboData(2);
    [SerializeField] private ComboData ComboIndex3 = new ComboData(3);
    [SerializeField] private ComboData ComboIndex4 = new ComboData(4);


    public ComboData InitPlayerCombo()
    {
        return ComboIndex0;
    }

    public ComboData LowerPlayerCombo(ComboData curCombo)
    {
        //print(curCombo.ComboRequired);
        if (curCombo.ComboRequired == ComboIndex0.ComboRequired)
        {
            return ComboIndex0;
        }
        else if (curCombo.ComboRequired == ComboIndex1.ComboRequired)
        {
            return ComboIndex0;
        }
        else if (curCombo.ComboRequired == ComboIndex2.ComboRequired)
        {
            return ComboIndex1;
        }
        else if (curCombo.ComboRequired == ComboIndex3.ComboRequired)
        {
            return ComboIndex2;
        }
        else if (curCombo.ComboRequired == ComboIndex4.ComboRequired)
        {
            return ComboIndex3;
        }
        return ComboIndex0;
    }

    public List<Sprite> GetBallTextures()
    {
        List<Sprite> ballTextures = new List<Sprite>();
        ballTextures.Add(ComboIndex0.BallTexture);
        ballTextures.Add(ComboIndex1.BallTexture);
        ballTextures.Add(ComboIndex2.BallTexture);
        ballTextures.Add(ComboIndex3.BallTexture);
        ballTextures.Add(ComboIndex4.BallTexture);
        return ballTextures;
    }


    public ComboData GetComboState(int curCombo)
    {
        if (curCombo < ComboIndex1.ComboRequired)
        {
            return ComboIndex0;
        }
        else if (curCombo < ComboIndex2.ComboRequired)
        {
            return ComboIndex1;
        }
        else if (curCombo < ComboIndex3.ComboRequired)
        {
            return ComboIndex2;
        }
        else if (curCombo < ComboIndex4.ComboRequired)
        {
            return ComboIndex3;
        }
        else
        {
            return ComboIndex4;
        }
    }
}

