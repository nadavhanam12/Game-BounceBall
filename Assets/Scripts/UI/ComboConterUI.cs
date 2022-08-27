using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboConterUI : MonoBehaviour
{

    [SerializeField] TMP_Text m_text;
    private int m_curTweenId = -1;
    private Vector3 m_textInitScale;
    private int m_curCombo;

    [Range(0.05f, 1f)] public float m_timeToTween = 0.2f;





    public void Init()
    {
        m_textInitScale = transform.localScale;
        m_curCombo = 0;

    }
    public void IncrementCombo()
    {
        m_curCombo++;
        SetCombo(m_curCombo);
    }

    public void SetCombo(int curCombo)
    {
        m_curCombo = curCombo;
        m_text.text = m_curCombo.ToString();
        if ((m_curTweenId == -1) && (m_curCombo != 0))
        {
            ScaleUp();
        }
    }
    private void ScaleUp()
    {
        m_curTweenId =
        LeanTween.scale(m_text.gameObject, m_textInitScale * 3, m_timeToTween)
                .setOnComplete(ScaleDown)
                .id;
    }
    private void ScaleDown()
    {
        m_curTweenId =
        LeanTween.scale(m_text.gameObject, m_textInitScale, m_timeToTween)
                .setOnComplete(FinishTween)
                .id;
    }

    private void FinishTween()
    {
        m_curTweenId = -1;
    }
    public int GetCurCombo()
    {
        return m_curCombo;
    }

}
