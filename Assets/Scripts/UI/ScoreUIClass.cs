using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameManagerScript;

public class ScoreUIClass : MonoBehaviour
{
    ScoreTween m_scoreTween1;
    ScoreTween m_scoreTween2;


    private bool isGamePaused;

    private bool m_initialized;




    void Awake()
    {
        //ScoreText = this.GetComponent<TMP_Text>();
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

    public void Init()
    {
        if (!m_initialized)
        {
            Awake();
            m_initialized = true;
            //m_scoreTween1.SetColor(m_color);
            //m_scoreTween2.SetColor(m_color);
        }


    }


    public void setScore(int score)
    {
        ActivateScoreTween(score.ToString());

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

}
