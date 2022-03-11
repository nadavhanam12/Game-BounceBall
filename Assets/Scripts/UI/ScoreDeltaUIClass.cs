using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerScript;

public class ScoreDeltaUIClass : MonoBehaviour
{
    TMP_Text ScoreText;

    ScoreTween m_scoreTween1;

    ScoreTween m_scoreTween2;

    private int m_curScore;

    private bool m_isCounting;

    private bool m_isScoreScaledUp;
    private float m_initScoreFontSize;
    private float m_curScoreFontSize;

    private bool isGamePaused;

    private bool m_initialized;

    private Color m_colorPlayer1;
    private Color m_colorPlayer2;

    private Color m_colorTie = Color.gray;

    int ScoreScaleDownSpeed;

    float ScoreScaleUpFactor;

    private PlayerIndex m_playerInLead;
    private int m_curDelta;

    private Color m_curColor;

    [SerializeField] private Slider m_leftSlider;
    [SerializeField] private Slider m_rightSlider;
    [SerializeField] private TMP_Text m_timeTextSeconds;
    [SerializeField] private TMP_Text m_timeTextMilliseconds;

    private float m_maxScore = 5000f;
    private float m_timeToPlay = 60f;

    private float timeRemaining;



    void Awake()
    {
        ScoreText = this.GetComponent<TMP_Text>();
        m_initScoreFontSize = ScoreText.fontSize;
        m_curScoreFontSize = m_initScoreFontSize;
        m_isCounting = false;
        m_isScoreScaledUp = false;
        m_initialized = false;
        ScoreTween[] tweens = this.GetComponentsInChildren<ScoreTween>(true);
        m_scoreTween1 = tweens[0];
        m_scoreTween2 = tweens[1];
        m_curDelta = 0;

    }


    public void SetTime(float timeLeft)
    {

    }

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;

    }

    public void Init(ScoreUiArgs args)
    {
        if (!m_initialized)
        {
            m_initialized = true;

            ScoreScaleDownSpeed = args.ScoreScaleDownSpeed;
            ScoreScaleUpFactor = args.ScoreScaleUpFactor;

            m_timeToPlay = args.TimeToPlay;

            m_playerInLead = PlayerIndex.First;
            //m_curColor = m_colorPlayer1;
            m_curColor = m_colorTie;
            ScoreText.text = 0.ToString();
            ScoreText.color = m_curColor;

            InitSliders();
            InitTimer();
        }


    }

    void InitTimer()
    {
        timeRemaining = m_timeToPlay;
        DisplayTime(timeRemaining);
    }


    void Update()
    {
        if (!isGamePaused)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                isGamePaused = true;
                EventManager.Broadcast(EVENT.EventOnTimeOver);
            }
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay >= 0)
        {
            //float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            float milliSeconds = (timeToDisplay % 1) * 100;
            m_timeTextSeconds.text = string.Format("{0:00}", seconds);
            m_timeTextMilliseconds.text = string.Format("{0:00}", milliSeconds);
            //m_timeText.text = string.Format("{0:00}", seconds);
        }
    }


    void InitSliders()
    {
        m_leftSlider.value = 0f;
        m_leftSlider.maxValue = 1.0f;
        m_rightSlider.value = 0f;
        m_rightSlider.maxValue = 1.0f;

    }

    public void SetColorsArgs(Color colorPlayer1, Color colorPlayer2)
    {

        m_colorPlayer1 = colorPlayer1;
        m_colorPlayer2 = colorPlayer2;
        //ScoreText.color = m_colorTie;
    }

    public PlayerIndex GetCurPlayerInLead()
    {
        return m_playerInLead;
    }
    public int GetCurDelta()
    {
        return m_curDelta;
    }


    public void setScore(int scoreDelta, PlayerIndex playerInLead)
    {
        int addedScore;

        if (playerInLead == m_playerInLead)
        {
            addedScore = scoreDelta - m_curDelta;

        }
        else
        {
            addedScore = scoreDelta;
            m_playerInLead = playerInLead;
            ScoreText.text = 0.ToString();
        }
        m_curDelta = scoreDelta;

        UpdateCurColor();


        ScoreText.color = m_curColor;



        //ActivateScoreTween(addedScore.ToString(), m_curColor);
        m_curScore = scoreDelta;
        //Score.text = m_curScore.ToString();


        UpdateProgressBar();


        if (!m_isCounting)
        {
            StartCoroutine(UpdateScore());
        }

        if (m_curScoreFontSize <= m_initScoreFontSize * 2f)
        {
            m_curScoreFontSize = m_curScoreFontSize * ScoreScaleUpFactor;

        }
        if (!m_isScoreScaledUp)
        {
            StartCoroutine(ScoreScaleUp());
        }
    }

    private void UpdateCurColor()
    {
        if (m_curDelta == 0)
        {
            m_curColor = m_colorTie;
        }
        else
        {
            m_curColor = (m_playerInLead == PlayerIndex.First ? m_colorPlayer1 : m_colorPlayer2);
        }
    }


    private void UpdateProgressBar()
    {
        Slider curSlider = (m_playerInLead == PlayerIndex.First) ? m_leftSlider : m_rightSlider;
        curSlider.value = (float)((float)m_curScore / m_maxScore);

    }

    private void ActivateScoreTween(string str, Color playerColor)
    {
        if (!m_scoreTween1.IsRunning())
        {
            m_scoreTween1.SetColor(playerColor);
            m_scoreTween1.Activate(str);
        }
        else if (!m_scoreTween2.IsRunning())
        {
            m_scoreTween2.SetColor(playerColor);
            m_scoreTween2.Activate(str);
        }


    }

    IEnumerator UpdateScore()
    {
        m_isCounting = true;
        int startScore = int.Parse(ScoreText.text);
        int delta;
        bool CountUp = true;

        while (m_isCounting)
        {
            delta = m_curScore - startScore;
            if (delta == 0)
            {
                m_isCounting = false;
            }
            else
            {
                CountUp = delta > 0;
                if (delta < 20)
                {
                    if (CountUp)
                    {
                        startScore++;
                    }

                    else
                    {
                        startScore--;
                    }

                }
                else
                {
                    if (CountUp)
                    {
                        startScore += 5;
                    }

                    else
                    {
                        startScore -= 5;
                    }
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
            if (m_curScoreFontSize <= m_initScoreFontSize)
            {
                m_curScoreFontSize = m_initScoreFontSize;
                m_isScoreScaledUp = false;
            }
            else
            {
                //m_curScoreFontSize -= 0.001f * ScoreScaleDownSpeed;
                m_curScoreFontSize -= ScoreScaleDownSpeed;
                ScoreText.fontSize = m_curScoreFontSize;
            }

            //yield return new WaitForSeconds(1 - CountingSpeed);
            yield return null;

        }
    }

}
