using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerAbstract;

public class ScoreDeltaUIClass : MonoBehaviour
{
    #region serialized

    [SerializeField] private NormalScoreUI m_leftNormalScore;
    [SerializeField] private NormalScoreUI m_rightNormalScore;
    [SerializeField] private GameObject m_onePlayerBar;
    [SerializeField] private RectTransform m_timeGameObject;
    [SerializeField] private TMP_Text m_timeTextSeconds;
    [SerializeField] private TMP_Text m_bestComboText;

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
    bool m_shouldRunTimer = true;
    int m_curBestCombo = 0;
    int m_prevBestCombo = 0;

    Vector2 timePosOnSinglePlayerMode = new Vector2(315, 0);
    #endregion

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
    }
    public void ActivateTimer(bool toActivate)
    {
        m_shouldRunTimer = toActivate;
    }

    public void Init(GameType gameType)
    {
        if (!m_initialized)
        {
            m_initialized = true;
            m_gameType = gameType;
            m_playerInLead = PlayerIndex.First;
            InitTimer();
            if (m_gameType == GameType.PvE || m_gameType == GameType.PvP)
            {
                InitNormalScore();
                m_timeGameObject.anchoredPosition = new Vector2(55, -30);
            }
            else if (m_gameType == GameType.SinglePlayer)
            {
                InitSinglePlayerBar();
                m_timeGameObject.anchoredPosition = timePosOnSinglePlayerMode;
                InitBestCombo();
            }
            gameObject.SetActive(false);
        }
    }

    private void InitBestCombo()
    {
        if (!PlayerPrefs.HasKey("SinglePlayerBestCombo"))
            PlayerPrefs.SetInt("SinglePlayerBestCombo", 0);
        m_prevBestCombo = PlayerPrefs.GetInt("SinglePlayerBestCombo", 0);
        m_curBestCombo = m_prevBestCombo;
        m_bestComboText.text = m_curBestCombo.ToString();
    }

    void InitTimer()
    {
        m_timeTextSeconds.gameObject.SetActive(true);
        timeRemaining = m_timeToPlay;
        DisplayTime(timeRemaining);
    }
    void Update()
    {
        if ((!isGamePaused) && m_initialized && m_shouldRunTimer)
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
    void InitNormalScore()
    {
        m_onePlayerBar.gameObject.SetActive(false);
        m_leftNormalScore.gameObject.SetActive(true);
        m_rightNormalScore.gameObject.SetActive(true);
        m_leftNormalScore.Init();
        m_rightNormalScore.Init();
    }
    void InitSinglePlayerBar()
    {
        m_onePlayerBar.gameObject.SetActive(true);
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
        m_leftNormalScore.SetScore(scoreLeft);
        m_rightNormalScore.SetScore(scoreRight);

    }


    public void SetScore(int scoreDelta, PlayerIndex playerInLead)
    {
        if (m_gameType == GameType.PvE)
        {
            //print("PvE, please call setNormalScore");
            return;
        }
        m_playerInLead = playerInLead;
        m_curDelta = scoreDelta;
    }

    public void UpdateCombo(int newCombo)
    {
        if (m_curBestCombo < newCombo)
        {
            m_curBestCombo = newCombo;
            m_bestComboText.text = m_curBestCombo.ToString();
        }
    }

    public int GetPrevBestCombo()
    {
        return m_prevBestCombo;
    }
    public int GetCurBestCombo()
    {
        return m_curBestCombo;
    }
}
