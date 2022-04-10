using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NormalScoreUI : MonoBehaviour
{

    [SerializeField] TMP_Text m_text;
    private int m_curTweenId = -1;
    private Vector3 m_textInitScale;

    [Range(0.05f, 1f)] public float m_timeToTween = 0.2f;





    public void Init()
    {
        m_textInitScale = transform.localScale;
        SetScore(0);

    }

    public void SetScore(int curScore)
    {
        if (m_text.text != curScore.ToString())
        {
            m_text.text = curScore.ToString();
            if (m_curTweenId == -1)
            {
                ScaleUp();
            }
        }

    }
    private void ScaleUp()
    {
        m_curTweenId =
        LeanTween.scale(m_text.gameObject, m_textInitScale * 2, m_timeToTween)
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

}
