using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;
using static PlayerScript;

[System.Serializable]
public class ComboData
{
    public int ComboRequired;
    public int ScoreBonus;
}

[System.Serializable]
public class ComboDataContainer : MonoBehaviour
{
    public int RegularHitScore = 100;
    public List<ComboData> ScoreBonusList;



    public ComboData GetNextCombo(int curComboIndex)
    {
        //print("curComboIndex: " + curComboIndex);
        if (ScoreBonusList.Count > curComboIndex + 1)
            return ScoreBonusList[curComboIndex + 1];
        else
            return ScoreBonusList[curComboIndex];

    }

}

