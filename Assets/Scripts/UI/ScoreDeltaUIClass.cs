using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerScript;

public class ScoreDeltaUIClass : MonoBehaviour
{
    #region serialized

    [SerializeField] private Slider m_leftSlider;
    [SerializeField] private Slider m_rightSlider;
    [SerializeField] private NormalScoreUI m_leftNormalScore;
    [SerializeField] private NormalScoreUI m_rightNormalScore;
    [SerializeField] private TMP_Text m_timeTextSeconds;
    [SerializeField] private TMP_Text m_timeTextMilliseconds;

    #endregion

    #region private

    private bool isGamePaused;
    private bool m_initialized = false;

    private Color m_colorPlayer1;
    private Color m_colorPlayer2;

    private Color m_colorTie = Color.gray;

    private PlayerIndex m_playerInLead;
    private int m_curDelta = 0;
    private string m_timeLeftString;



    private float m_maxScore = 5000f;
    private float m_timeToPlay = 60f;

    private float timeRemaining;
    private GameType m_gameType;
    bool m_shouldRun = true;

    #endregion

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
    }
    public void ActivateTimer(bool toActivate)
    {
        m_shouldRun = toActivate;
    }

    public void Init(GameType gameType)
    {
        if (!m_initialized)
        {
            m_initialized = true;
            m_gameType = gameType;
            m_playerInLead = PlayerIndex.First;
            if (m_gameType == GameType.TurnsGame)
            {
                RemoveNormalScore();
                InitSliders();
            }
            else if (m_gameType == GameType.TalTalGame || m_gameType == GameType.OnePlayer)
            {
                RemoveSliders();
                InitNormalScore();
            }
            else
            {
                InitSliders();
            }


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
        if ((!isGamePaused) && m_initialized && m_shouldRun)
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
            m_timeLeftString = string.Format("{0:00}", seconds);

            if (timeToDisplay <= 4f && m_timeLeftString != m_timeTextSeconds.text)
            {
                Color newColor = m_timeTextSeconds.color == Color.red ? Color.white : Color.red;
                m_timeTextSeconds.color = newColor;
                LeanTween.scale(m_timeTextSeconds.gameObject, new Vector3(1.5f, 1.5f, 1.5f), 0.2f).setLoopPingPong(1);
            }
            m_timeTextSeconds.text = m_timeLeftString;
        }
    }


    void InitSliders()
    {
        m_leftSlider.gameObject.SetActive(true);
        m_rightSlider.gameObject.SetActive(true);
        m_leftSlider.value = 0f;
        m_leftSlider.maxValue = 1.0f;
        m_rightSlider.value = 0f;
        m_rightSlider.maxValue = 1.0f;
    }
    void RemoveSliders()
    {
        m_leftSlider.gameObject.SetActive(false);
        m_rightSlider.gameObject.SetActive(false);
    }
    void InitNormalScore()
    {
        m_leftNormalScore.gameObject.SetActive(true);
        m_rightNormalScore.gameObject.SetActive(true);
        m_leftNormalScore.Init();
        m_rightNormalScore.Init();

    }
    void RemoveNormalScore()
    {
        m_leftNormalScore.gameObject.SetActive(false);
        m_rightNormalScore.gameObject.SetActive(false);
    }

    public void SetColors(Color colorPlayer1, Color colorPlayer2)
    {

        m_colorPlayer1 = colorPlayer1;
        m_colorPlayer2 = colorPlayer2;
    }

    public void SetTimeToPlay(int timeToPlay)
    {
        m_timeToPlay = timeToPlay;
    }

    public PlayerIndex GetCurPlayerInLead()
    {
        return m_playerInLead;
    }
    public int GetCurDelta()
    {
        return m_curDelta;
    }
    public void SetNormalScore(int scoreLeft, int scoreRight)
    {
        if (m_gameType == GameType.TurnsGame)
        {
            //print("TurnsGame, please call setScore");
            return;
        }
        m_leftNormalScore.SetScore(scoreLeft);
        m_rightNormalScore.SetScore(scoreRight);

    }


    public void SetScore(int scoreDelta, PlayerIndex playerInLead)
    {
        if (m_gameType == GameType.TalTalGame)
        {
            //print("TalTalGame, please call setNormalScore");
            return;
        }
        m_playerInLead = playerInLead;
        m_curDelta = scoreDelta;
        UpdateProgressBar();
    }


    private void UpdateProgressBar()
    {
        Slider curSlider = (m_playerInLead == PlayerIndex.First) ? m_leftSlider : m_rightSlider;
        curSlider.value = (float)((float)m_curDelta / m_maxScore);
        Slider curSliderToZero = (m_playerInLead == PlayerIndex.First) ? m_rightSlider : m_leftSlider;
        curSliderToZero.value = 0;


    }
}
