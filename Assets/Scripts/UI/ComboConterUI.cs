using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboConterUI : MonoBehaviour
{

    [SerializeField] TMP_Text m_text;





    public void Init()
    {


    }

    public void SetCombo(int curCombo)
    {
        m_text.text = curCombo.ToString();
    }

}
