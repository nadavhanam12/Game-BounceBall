using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameManagerScript;

public class ScoreUIClass : MonoBehaviour
{
    [SerializeField] TMP_Text ScoreText;

    ScoreTween m_scoreTween1;

    ScoreTween m_scoreTween2;

    private int m_curScore;

    private bool m_isCounting;

    private bool m_isScoreScaledUp;
    private Vector3 m_initScoreScale;
    private Vector3 m_curScale;

    private bool isGamePaused;

    private bool m_initialized;

    private Color m_color;

    int ScoreScaleDownSpeed;

    float ScoreScaleUpFactor;


    void Awake()
    {
        //ScoreText = this.GetComponent<TMP_Text>();
        m_initScoreScale = ScoreText.gameObject.transform.localScale;
        m_curScale = m_initScoreScale;
        m_isCounting = false;
        m_isScoreScaledUp = false;
        m_initialized = false;
        ScoreTween[] tweens = this.GetComponentsInChildren<ScoreTween>(true);
        m_scoreTween1 = tweens[0];
        m_scoreTween2 = tweens[1];
        m_scoreTween1.Init();
        m_scoreTween2.Init();
    }

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;

    }

    public void Init(ScoreUiArgs args)
    {
        if (!m_initialized)
        {
            Awake();
            m_initialized = true;
            m_color = args.ScoreColor;
            ScoreText.color = m_color;
            ScoreText.text = "0";
            m_scoreTween1.SetColor(m_color);
            m_scoreTween2.SetColor(m_color);

            ScoreScaleDownSpeed = args.ScoreScaleDownSpeed;
            ScoreScaleUpFactor = args.ScoreScaleUpFactor;
        }


    }

    public int GetScore()
    {
        return m_curScore;
    }

    public void setScore(int score)
    {
        int addedScore = score - m_curScore;
        ActivateScoreTween(addedScore.ToString());
        m_curScore = score;
        //Score.text = m_curScore.ToString();
        if (!m_isCounting)
        {
            StartCoroutine(UpdateScore());
        }

        if (m_curScale.x <= m_initScoreScale.x * 2f)
        {
            m_curScale.x = m_curScale.x * ScoreScaleUpFactor;
            m_curScale.y = m_curScale.y * ScoreScaleUpFactor;
        }
        if (!m_isScoreScaledUp)
        {
            StartCoroutine(ScoreScaleUp());
        }
    }

    private void ActivateScoreTween(string str)
    {
        if (!m_scoreTween1.IsRunning())
        {
            m_scoreTween1.Activate(str);
        }
        else if (!m_scoreTween2.IsRunning())
        {
            m_scoreTween2.Activate(str);
        }


    }

    IEnumerator UpdateScore()
    {
        m_isCounting = true;
        int startScore = int.Parse(ScoreText.text);
        int delta;

        while (m_isCounting)
        {
            delta = m_curScore - startScore;
            if (delta <= 0)
            {
                m_isCounting = false;
            }
            else
            {
                if (delta < 20)
                {
                    startScore++;

                }
                else
                {
                    startScore += 5;
                }

                ScoreText.text = startScore.ToString();
            }

            //yield return new WaitForSeconds(1 - CountingSpeed);
            yield return null;

        }
    }

    IEnumerator ScoreScaleUp()
    {
        m_isScoreScaledUp = true;
        while (m_isScoreScaledUp)
        {
            if ((m_curScale.x <= m_initScoreScale.x) && (m_curScale.y <= m_initScoreScale.y))
            {
                m_curScale = m_initScoreScale;
                m_isScoreScaledUp = false;
            }
            else
            {
                m_curScale.x -= 0.001f * ScoreScaleDownSpeed;
                m_curScale.y -= 0.001f * ScoreScaleDownSpeed;
                ScoreText.gameObject.transform.localScale = m_curScale;
            }

            //yield return new WaitForSeconds(1 - CountingSpeed);
            yield return null;

        }
    }

}
