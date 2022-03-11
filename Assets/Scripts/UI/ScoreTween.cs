using TMPro;
using UnityEngine;


public class ScoreTween : MonoBehaviour
{

    private TMP_Text m_text;
    private bool m_isRunning = false;

    private Vector3 m_initPosition;

    private Vector3 m_initScale;
    private Color m_initColor;

    private float m_playTime = 1.5f;

    private int m_highMove = 600;

    public void Init()
    {
        m_text = GetComponentInChildren<TMP_Text>(true);
        m_initPosition = gameObject.transform.localPosition;
        m_initScale = gameObject.transform.localScale;
        this.gameObject.SetActive(false);
    }

    public void SetColor(Color color)
    {
        m_initColor = color;
        m_text.color = m_initColor;

    }

    public void Activate(string str)
    {
        if (!m_isRunning)
        {
            InitValues();
            m_isRunning = true;

            m_text.text = "+" + str;
            this.gameObject.SetActive(true);
            //.setEase(LeanTweenType.easeOutQuad)
            LeanTween.moveLocalY(gameObject, m_initPosition.y + m_highMove, m_playTime);
            LeanTweenExt.MyLeanAlphaText(m_text, 0, m_playTime).setEase(LeanTweenType.easeOutQuad).setOnComplete(Deactivate);
        }

    }

    private void InitValues()
    {
        m_text.color = m_initColor;
        gameObject.transform.localPosition = m_initPosition;
        gameObject.transform.localScale = m_initScale;

    }

    public bool IsRunning()
    {
        return m_isRunning;
    }


    public void Deactivate()
    {
        m_isRunning = false;
        this.gameObject.SetActive(false);

    }


}

