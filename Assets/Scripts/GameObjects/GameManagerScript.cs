using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerScript;

public class GameManagerScript : MonoBehaviour
{

    #region enum
    public enum PlayerIndex
    {
        First = 1,
        Second = 2
    }

    #endregion

    #region events

    #endregion

    #region Refs

    [SerializeField] private GameBoundsData m_gameBoundsData;
    [SerializeField] private GameObject m_playerContainer1;
    [SerializeField] private GameObject m_playerContainer2;
    [SerializeField] private GameObject NoMenuStartPage;
    private GameCanvasScript m_gameCanvas;
    private ComboDataContainer m_comboDataContainer;

    private PlayersDataContainer m_playersDataContainer;
    private SequenceManager m_sequenceManager;
    private GameBallsManager m_ballsManager;





    #endregion

    #region serialized
    [SerializeField] private int m_matchTime;
    [SerializeField] private float m_countDownDelay = 1f;

    #endregion

    #region private

    private GameBounds m_gameBounds;
    private PlayerArgs m_playerData1;
    private PlayerArgs m_playerData2;
    private int m_curPlayersScoreDelta;
    private PlayerIndex m_curPlayerInLead;
    private bool m_isGamePause;
    private GameArgs m_gameArgs;
    private PlayerIndex m_curPlayerPlay = PlayerIndex.First;
    private bool m_onMobileDevice = false;





    #endregion

    public void SetGameArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;
    }



    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_onMobileDevice = true;
#endif

        if (m_gameArgs == null)
        {
            if (m_onMobileDevice)
            {
                // Code for Google Android Project here
                NoMenuStartPage.gameObject.SetActive(true);

            }
            else
            {
                StartGameScene();
            }
        }


    }

    public void StopGameScene()
    {
        this.gameObject.SetActive(false);
    }

    public void StartGameScene()
    {
        if (NoMenuStartPage.gameObject.activeInHierarchy) { NoMenuStartPage.gameObject.SetActive(false); }
        if (m_gameArgs == null)
        {
            //m_gameArgs = new GameArgs(GameType.OnePlayer, null);
            m_gameArgs = new GameArgs(GameType.TurnsGame, null);
            //print("No GameType Provided- GameType=TwoPlayer");
        }
        Application.targetFrameRate = 60;
        Init();
        this.gameObject.SetActive(true);
        //m_playerData1.PlayerScript.PlayIdle();
        //m_playerData2.PlayerScript.PlayIdle();

        if (m_onMobileDevice)
        {
            Invoke("StartCountdown", m_countDownDelay);
        }
        else
        {
            Invoke("FinishGameCountdown", m_countDownDelay);
        }


    }

    void InitListeners()
    {
        EventManager.Broadcast(EVENT.EventStartApp);
        EventManager.AddHandler(EVENT.EventOnRestart, TimeIsOver);
        //EventManager.AddHandler(EVENT.EventCombo, OnCombo);
        EventManager.AddHandler(EVENT.EventOnCountdownEnds, FinishGameCountdown);
    }

    void StartCountdown()
    {
        m_gameCanvas.StartCountdown();
    }

    public void FinishGameCountdown()
    {
        EventManager.Broadcast(EVENT.EventStartGameScene);
        m_gameBoundsData.gameObject.SetActive(false);
        InitGameMood();
        SetGamePause(false);
        StartCoroutine(UpdateScores());
    }


    private void Init()
    {
        InitRefs();
        InitData();
        InitBallsManager();
        InitPlayersData();
        InitPlayers();
        InitListeners();

        InitGameCanvas();
        //InitSequenceManager();

        SetGamePause(true);

        //Invoke("InitGameMood", 1f);
        //SetGamePause(false);




    }

    void InitRefs()
    {
        m_gameCanvas = GetComponentInChildren<GameCanvasScript>();
        m_ballsManager = GetComponentInChildren<GameBallsManager>();
        m_comboDataContainer = GetComponentInChildren<ComboDataContainer>();
        m_playersDataContainer = GetComponentInChildren<PlayersDataContainer>();
        m_sequenceManager = GetComponentInChildren<SequenceManager>();


    }

    private void InitBallsManager()
    {

        GameBallsManagerArgs ballsManagerArgs = new GameBallsManagerArgs();
        ballsManagerArgs.BallArgs = m_playersDataContainer.ballArgs;

        ballsManagerArgs.Player1Balls = m_playerContainer1.GetComponentsInChildren<BallScript>().ToList();
        if (ballsManagerArgs.Player1Balls.Count < 2)
        {
            BallScript curBall = ballsManagerArgs.Player1Balls[0];
            BallScript otherBall = Instantiate(curBall, curBall.transform.parent);
            ballsManagerArgs.Player1Balls.Add(otherBall);
        }
        ballsManagerArgs.Player2Balls = m_playerContainer2.GetComponentsInChildren<BallScript>().ToList();
        if (ballsManagerArgs.Player2Balls.Count < 2)
        {
            BallScript curBall = ballsManagerArgs.Player2Balls[0];
            BallScript otherBall = Instantiate(curBall, curBall.transform.parent);
            ballsManagerArgs.Player2Balls.Add(otherBall);
        }
        m_ballsManager.GameManagerOnBallHit = onBallHit;
        m_ballsManager.GameManagerOnTurnLost = onTurnLost;

        ballsManagerArgs.GameCanvas = m_gameCanvas;

        m_ballsManager.Init(ballsManagerArgs);

    }

    void InitPlayersData()
    {
        m_playerData1.CurScore = 0;
        m_playerData2.CurScore = 0;
        m_playerData1.CurComboIndex = -1;
        m_playerData2.CurComboIndex = -1;
        m_playerData2.CurCombo = 0;
        m_playerData2.CurCombo = 0;

        m_playerData1.Bounds = m_gameBounds;
        m_playerData2.Bounds = m_gameBounds;

        m_playerData1.playerStats = m_playersDataContainer.playerStats;
        m_playerData2.playerStats = m_playersDataContainer.playerStats;

        m_playersDataContainer.ballArgs.Bounds = m_gameBounds;

        m_playerData1.BallsManager = m_ballsManager;
        m_playerData2.BallsManager = m_ballsManager;

    }

    private void InitData()
    {

        m_playerData1 = m_playersDataContainer.PlayerData1;
        m_playerData2 = m_playersDataContainer.PlayerData2;


        m_gameBounds = m_gameBoundsData.GenerateBounds(Camera.main);



    }
    private void InitSequenceManager()
    {
        m_sequenceManager.Init(m_gameCanvas);
    }
    void InitGameMood()
    {
        if (m_gameArgs.GameType == GameType.TurnsGame)
        {
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.LostTurn();
        }
        else
        {
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.StartTurn();
        }
    }

    public void SwitchPlayerTurn()
    {
        if (m_curPlayerPlay == PlayerIndex.First)
        {
            m_curPlayerPlay = PlayerIndex.Second;
            m_playerData1.PlayerScript.LostTurn();
            m_playerData2.PlayerScript.StartTurn();
        }
        else
        {
            m_curPlayerPlay = PlayerIndex.First;
            m_playerData2.PlayerScript.LostTurn();
            m_playerData1.PlayerScript.StartTurn();
        }


    }

    private void SetGamePause(bool isPause)
    {
        m_isGamePause = isPause;
        m_playerData1.PlayerScript.SetGamePause(isPause);
        m_playerData2.PlayerScript.SetGamePause(isPause);
        m_gameCanvas.SetGamePause(isPause);
        if (!m_isGamePause)
        {
            StartCoroutine(UpdateScores());
        }

    }

    private void InitGameCanvas()
    {
        GameCanvasArgs canvasArgs = new GameCanvasArgs();
        canvasArgs.MatchTime = m_matchTime;
        canvasArgs.PlayerColor1 = m_playerData1.Color;
        canvasArgs.PlayerColor2 = m_playerData2.Color;

        if (m_gameArgs.MainMenu != null)
        {
            //canvasArgs.Background = m_gameArgs.Background;
            canvasArgs.Background = m_gameArgs.MainMenu.ChooseRandomBackground();
        }

        m_gameCanvas.Init(canvasArgs);
        m_gameCanvas.m_onTimeIsOver = TimeIsOver;

        m_gameCanvas.m_OnTouchKickRegular = m_playerData1.PlayerScript.OnTouchKickRegular;
        m_gameCanvas.m_OnTouchKickPower = m_playerData1.PlayerScript.OnTouchKickPower;
        m_gameCanvas.m_OnTouchJump = m_playerData1.PlayerScript.OnTouchJump;




    }
    private void InitPlayers()
    {
        m_playerData1.PlayerScript = m_playerContainer1.GetComponentInChildren<PlayerScript>(true);
        m_playerData1.PlayerScript.gameObject.SetActive(false);

        m_playerData2.PlayerScript = m_playerContainer2.GetComponentInChildren<PlayerScript>(true);
        m_playerData2.PlayerScript.gameObject.SetActive(false);


        m_playerData1.PlayerScript.Init(m_playerData1);

        if (m_gameArgs.GameType != GameType.TwoPlayer)
        {
            m_playerData2.AutoPlay = true;
        }
        m_playerData2.PlayerScript.Init(m_playerData2);
    }



    public void onTurnLost(PlayerIndex ballIndex)
    {
        PlayerArgs playerData;
        if (ballIndex == PlayerIndex.First)
        {
            playerData = m_playerData1;
            EventManager.Broadcast(EVENT.EventOnBallLost);
        }
        else
        {
            playerData = m_playerData2;
        }

        playerData.CurComboIndex = -1;
        playerData.CurCombo = 0;
        m_gameCanvas.SetCombo(playerData.CurCombo);

        if (m_gameArgs.GameType == GameType.TurnsGame)
        {
            SwitchPlayerTurn();
        }
        else
        {
            //playerData.Ball1.OnNewBallInScene();
            m_ballsManager.OnNewBallInScene(PlayerIndex.First);
        }

    }


    public void onBallHit(PlayerIndex playerIndex)
    {
        //print("onBallHit");
        KickType kickType = KickType.Regular;
        if (playerIndex == PlayerIndex.First)
        {
            switch (kickType)
            {
                case KickType.Power:
                    EventManager.Broadcast(EVENT.EventPowerKick);
                    break;
                default:
                    EventManager.Broadcast(EVENT.EventNormalKick);
                    break;
            }
        }

        int addScore = m_comboDataContainer.RegularHitScore;
        PlayerArgs playerData;
        if (playerIndex == PlayerIndex.First)
        {
            playerData = m_playerData1;
        }
        else
        {
            playerData = m_playerData2;

        }


        playerData.CurScore += (addScore);
        UpdatePlayerCurCombo(playerData);

    }
    void UpdatePlayerCurCombo(PlayerArgs playerArgs)
    {
        //print("playerArgs.CurCombo: " + playerArgs.CurCombo);
        playerArgs.CurCombo++;
        ComboData nextComboData = m_comboDataContainer.GetNextCombo(playerArgs.CurComboIndex);
        if (playerArgs.CurCombo == nextComboData.ComboRequired)
        {//New Combo-apply cheer and added score
            playerArgs.CurComboIndex++;
            m_gameCanvas.CheerActivate();
            playerArgs.CurScore += (nextComboData.ScoreBonus);
            //print("nextComboData.ScoreBonus: " + nextComboData.ScoreBonus);
        }
        //Apply visual effect for combo counter
        m_gameCanvas.SetCombo(playerArgs.CurCombo);
    }


    IEnumerator UpdateScores()
    {
        while (!m_isGamePause)
        {
            m_curPlayersScoreDelta = m_playerData1.CurScore - m_playerData2.CurScore;
            if (m_curPlayersScoreDelta > 0)//player 1 lead
            {
                m_curPlayerInLead = PlayerIndex.First;
            }
            else if (m_curPlayersScoreDelta < 0)//player 2 lead
            {
                m_curPlayersScoreDelta *= (-1);
                m_curPlayerInLead = PlayerIndex.Second;
            }
            //print("m_playerData1.CurScore: " + m_playerData1.CurScore);
            //print("m_playerData2.CurScore: " + m_playerData2.CurScore);
            m_gameCanvas.setScore(m_playerData1.CurScore, m_playerData2.CurScore,
             m_curPlayersScoreDelta, m_curPlayerInLead);

            yield return null;
        }

    }

    public void TimeIsOver()
    {
        //print("Time is over");
        if (!m_isGamePause)
        {
            SetGamePause(true);
            m_ballsManager.TimeIsOver();//should turn off the balls


            if (m_playerData1.CurScore > m_playerData2.CurScore)
            {
                m_playerData1.PlayerScript.Win();
                m_playerData2.PlayerScript.Lose();
            }
            else if (m_playerData1.CurScore < m_playerData2.CurScore)
            {
                m_playerData1.PlayerScript.Lose();
                m_playerData1.PlayerScript.Win();
            }
            else
            {
                m_playerData1.PlayerScript.Win();
                m_playerData1.PlayerScript.Win();
            }

            Invoke("EndGame", 3f);

        }



    }

    private void EndGame()
    {
        if (m_gameArgs.MainMenu == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            m_gameArgs.MainMenu.BackToMenu();
        }
    }


}
