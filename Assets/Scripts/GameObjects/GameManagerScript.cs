using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
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

    public GameType GameType;

    [SerializeField] private GameBoundsData m_gameBoundsData;
    [SerializeField] private GameObject m_playerContainer1;
    [SerializeField] private GameObject m_playerContainer2;
    [SerializeField] private GameObject NoMenuStartPage;

    private GameCanvasScript m_gameCanvas;
    private ComboDataContainer m_comboDataContainer;

    private PlayersDataContainer m_playersDataContainer;
    //private SequenceManager m_sequenceManager;
    private GameBallsManager m_ballsManager;
    private TutorialManager m_tutorialManager;
    private PickablesManager m_pickablesManager;
    private ConfettiManager m_confettiManager;


    #endregion

    #region serialized
    [SerializeField] private int m_matchTime;
    [SerializeField] private float m_countDownDelay = 1f;
    [SerializeField] private bool m_shouldPlayTutorial = false;
    [SerializeField] private bool m_shouldPlayCountdown = false;
    [SerializeField] private bool m_shouldStartWithMenu = false;
    [SerializeField] private int m_singlePlayerCheerFrequency = 5;

    #endregion

    #region private

    private GameBounds m_gameBounds;
    private PlayerArgs m_playerData1;
    private PlayerArgs m_playerData2;
    private int m_curPlayersScoreDelta;
    private PlayerIndex m_curPlayerInLead;
    private bool m_isGamePause;
    private GameArgs m_gameArgs;
    private PlayerIndex m_curPlayerTurn = PlayerIndex.First;
    private bool m_inTutorial = false;

    private bool m_shouldRestart;
    private MainMenu m_mainMenu;





    #endregion

    public void SetGameArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;

        StartGameScene();
    }




    void Awake()
    {
        Application.targetFrameRate = 60;
        m_mainMenu = FindObjectOfType<MainMenu>(true);
        if (m_mainMenu == null) //runs as solo scene
        {
            if (m_shouldStartWithMenu)
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

        if (NoMenuStartPage.gameObject.activeInHierarchy)
        {
            NoMenuStartPage.gameObject.SetActive(false);
        }
        if (m_gameArgs == null)
        {
            m_gameArgs = new GameArgs(GameType);
            //m_gameArgs = new GameArgs(GameType.TurnsGame);
        }

        AnalyticsManager.CommitData(
                    "Match_Started",
                    new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType }
                         });

        Init();
        m_gameBoundsData.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        m_shouldRestart = false;

        if (m_mainMenu != null)
            m_shouldPlayTutorial = !PlayerPrefsHasCompletedTutorial(m_gameArgs.GameType);

        if (m_shouldPlayTutorial)
        {
            StartTutorial();
        }
        else
        {
            AfterTutorial();
        }
    }
    void StartTutorial()
    {
        InitGameMood();
        m_playerData2.PlayerScript.HidePlayer();
        m_inTutorial = true;
        m_tutorialManager.Play();
    }
    void FinishedTutorial()
    {
        //should init after tutorial
        m_inTutorial = false;
        m_playerData2.PlayerScript.ShowPlayer();
        m_ballsManager.RemoveAllBalls();
        m_tutorialManager.TurnOff();
        InitScoreAndCombo();
        m_curPlayerTurn = PlayerIndex.First;
        m_gameCanvas.SetCurPlayerUI(m_curPlayerTurn == PlayerIndex.First);

        UpdatePlayerPrefsCompletedTutorial(m_gameArgs.GameType);

        AfterTutorial();
    }

    void AfterTutorial()
    {

        if (m_shouldPlayCountdown)
        {
            Invoke("StartCountdown", m_countDownDelay);
        }
        else
        {
            Invoke("FinishGameCountdown", m_countDownDelay);
        }
    }

    void InitTutorial()
    {
        TutorialArgs tutorialArgs = new TutorialArgs();
        tutorialArgs.GameCanvas = m_gameCanvas;
        tutorialArgs.GameType = m_gameArgs.GameType;
        tutorialArgs.TutorialUI = m_gameCanvas.GetTutorialUI();
        m_tutorialManager.Init(tutorialArgs);
        m_tutorialManager.Pause = SetGamePause;
        m_tutorialManager.OnFinishTutorial = FinishedTutorial;
        m_tutorialManager.onShowOpponent = () => m_playerData2.PlayerScript.ShowPlayer();

    }

    void InitListeners()
    {
        EventManager.Broadcast(EVENT.EventStartApp);
        EventManager.AddHandler(EVENT.EventOnRestart, OnRestart);
        EventManager.AddHandler(EVENT.EventOnCountdownEnds, FinishGameCountdown);
    }
    void RemoveListeners()
    {
        EventManager.RemoveHandler(EVENT.EventOnRestart, OnRestart);
        EventManager.RemoveHandler(EVENT.EventOnCountdownEnds, FinishGameCountdown);
    }
    void OnDestroy()
    {
        RemoveListeners();
    }

    void StartCountdown()
    {
        SetGamePause(true);
        m_gameCanvas.StartCountdown();
    }

    public void FinishGameCountdown()
    {
        EventManager.Broadcast(EVENT.EventStartGameScene);
        InitGameMood();

        if (m_gameArgs.GameType != GameType.OnePlayer)
        {
            m_pickablesManager.FinishInitialize();
            m_pickablesManager.GeneratePickables();
        }

        SetGamePause(false);
        if (m_gameArgs.GameType == GameType.TurnsGame)
        {
            StartCoroutine(UpdateScores());
        }
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
        InitTutorial();
        InitPickablesManager();
        InitConfettiManager();

        //InitSequenceManager();
        //Invoke("InitGameMood", 1f);
        //SetGamePause(false);
    }

    void InitRefs()
    {
        m_gameCanvas = GetComponentInChildren<GameCanvasScript>();
        m_ballsManager = GetComponentInChildren<GameBallsManager>();
        m_comboDataContainer = GetComponentInChildren<ComboDataContainer>();
        m_playersDataContainer = GetComponentInChildren<PlayersDataContainer>();
        //m_sequenceManager = GetComponentInChildren<SequenceManager>();
        m_tutorialManager = GetComponentInChildren<TutorialManager>();
        m_pickablesManager = GetComponentInChildren<PickablesManager>();
        m_confettiManager = GetComponentInChildren<ConfettiManager>();


    }
    private void InitConfettiManager()
    {
        m_confettiManager.Init(m_gameBounds);
    }

    private void InitPickablesManager()
    {
        PickablesManagerArgs args = new PickablesManagerArgs();
        args.GameBounds = m_gameBounds;
        m_pickablesManager.Init(args);
    }

    private void InitBallsManager()
    {

        GameBallsManagerArgs ballsManagerArgs = new GameBallsManagerArgs();
        ballsManagerArgs.BallArgs = m_playersDataContainer.ballArgs;
        ballsManagerArgs.GameType = m_gameArgs.GameType;

        m_ballsManager.GameManagerOnBallHit = OnBallHit;
        m_ballsManager.GameManagerOnTurnLost = onTurnLost;

        ballsManagerArgs.GameCanvas = m_gameCanvas;

        m_ballsManager.Init(ballsManagerArgs);

    }

    private void InitScoreAndCombo()
    {
        m_playerData1.CurScore = 0;
        m_playerData2.CurScore = 0;
        m_playerData1.CurComboIndex = -1;
        m_playerData2.CurComboIndex = -1;
        m_playerData2.CurCombo = 0;
        m_playerData2.CurCombo = 0;
        if (m_gameCanvas != null)
        {
            m_gameCanvas.SetCombo(0);
        }
    }
    void InitPlayersData()
    {
        InitScoreAndCombo();

        m_playerData1.Bounds = m_gameBounds;
        m_playerData2.Bounds = m_gameBounds;

        m_playerData1.playerStats = m_playersDataContainer.playerStats;
        m_playerData2.playerStats = m_playersDataContainer.playerStats;

        m_playersDataContainer.ballArgs.Bounds = m_gameBounds;

        m_playerData1.BallsManager = m_ballsManager;
        m_playerData2.BallsManager = m_ballsManager;
        m_playerData1.PickablesManager = m_pickablesManager;
        m_playerData2.PickablesManager = m_pickablesManager;

        m_playerData1.ToggleBombUI = m_gameCanvas.ShowBombUI;
        //m_playerData2.ToggleBombUI = m_gameCanvas.ShowBombUI;

    }

    private void InitData()
    {

        m_playerData1 = m_playersDataContainer.PlayerData1;
        m_playerData2 = m_playersDataContainer.PlayerData2;


        m_gameBounds = m_gameBoundsData.GenerateBounds(Camera.main);



    }
    /*
    private void InitSequenceManager()
    {
        m_sequenceManager.Init(m_gameCanvas);
    }*/
    void InitGameMood()
    {
        if (m_gameArgs.GameType == GameType.TurnsGame)
        {
            m_curPlayerTurn = PlayerIndex.First;
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.LostTurn();
        }
        else if (m_gameArgs.GameType == GameType.TalTalGame)
        {
            m_curPlayerTurn = PlayerIndex.First;
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.LostTurn();
        }
        else if (m_gameArgs.GameType == GameType.OnePlayer)
        {
            m_curPlayerTurn = PlayerIndex.First;
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.HidePlayer();
        }
        else
        {
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.StartTurn();
        }
    }

    private async void SwitchPlayerTurn(bool shouldSwitchTurn = true)
    {
        if (!m_inTutorial)
        {
            m_gameCanvas.SwitchTurn(m_curPlayerTurn != PlayerIndex.First);
        }
        if (m_gameArgs.GameType == GameType.TalTalGame)
        {
            if (m_curPlayerTurn != PlayerIndex.First)
            {//win player1
                m_playerData1.PlayerScript.Win();
                m_playerData2.PlayerScript.Lose();
            }
            else
            {//win player2
                m_playerData1.PlayerScript.Lose();
                m_playerData2.PlayerScript.Win();
            }
        }
        else if (m_gameArgs.GameType == GameType.OnePlayer)
        {
            shouldSwitchTurn = false;
        }

        await Task.Delay(1000);
        InitPlayersStatus();

        await Task.Delay(1000);
        SwitchPlayerTurnAfterWait(true, shouldSwitchTurn);

    }

    private void InitPlayersStatus()
    {
        m_playerData1.PlayerScript.InitPlayer();
        m_playerData2.PlayerScript.InitPlayer();
    }

    void SwitchPlayerTurnAfterWait(bool throwNewBall = true, bool shouldSwitchTurn = true)
    {
        //print("shouldSwitchTurn: " + shouldSwitchTurn);
        if (m_gameArgs.GameType == GameType.OnePlayer)
        {
            shouldSwitchTurn = false;
        }

        if (shouldSwitchTurn)
        {
            m_curPlayerTurn = m_curPlayerTurn == PlayerIndex.First ? PlayerIndex.Second : PlayerIndex.First;
        }

        if (m_curPlayerTurn == PlayerIndex.First)
        {
            m_playerData1.PlayerScript.StartTurn(throwNewBall);
            m_playerData2.PlayerScript.LostTurn();
        }
        else
        {
            m_playerData1.PlayerScript.LostTurn();
            m_playerData2.PlayerScript.StartTurn(throwNewBall);
        }
        m_gameCanvas.SetCurPlayerUI(m_curPlayerTurn == PlayerIndex.First);
    }


    private void SetGamePause(bool isPause)
    {
        m_isGamePause = isPause;
        m_playerData1.PlayerScript.SetGamePause(isPause);
        m_playerData2.PlayerScript.SetGamePause(isPause);
        m_ballsManager.SetGamePause(isPause);
        m_pickablesManager.SetGamePause(isPause);
        m_gameCanvas.SetGamePause(isPause);
        if (!m_isGamePause)
        {
            if (m_gameArgs.GameType == GameType.TurnsGame)
            {
                StartCoroutine(UpdateScores());
            }
        }

    }

    private void InitGameCanvas()
    {
        GameCanvasArgs canvasArgs = new GameCanvasArgs();
        canvasArgs.GameType = m_gameArgs.GameType;
        canvasArgs.MatchTime = m_matchTime;
        canvasArgs.PlayerColor1 = m_playerData1.Color;
        canvasArgs.PlayerColor2 = m_playerData2.Color;
        canvasArgs.PlayerImage1 = m_playerData1.Image;
        canvasArgs.PlayerImage2 = m_playerData2.Image;
        canvasArgs.ConfettiManager = m_confettiManager;
        m_gameCanvas.Init(canvasArgs);
        m_gameCanvas.m_onTimeIsOver = TimeIsOver;

        m_gameCanvas.m_OnTouchKickRegular = m_playerData1.PlayerScript.OnTouchKickRegular;
        m_gameCanvas.m_OnTouchKickSpecial = m_playerData1.PlayerScript.OnTouchKickSpecial;
        m_gameCanvas.m_OnTouchJump = m_playerData1.PlayerScript.OnTouchJump;




    }
    private void InitPlayers()
    {
        m_playerData1.PlayerScript = m_playerContainer1.GetComponentInChildren<PlayerScript>(true);
        m_playerData1.PlayerScript.gameObject.SetActive(false);

        m_playerData2.PlayerScript = m_playerContainer2.GetComponentInChildren<PlayerScript>(true);
        m_playerData2.PlayerScript.gameObject.SetActive(false);


        m_playerData1.PlayerScript.Init(m_playerData1);

        if (m_gameArgs.GameType == GameType.TurnsGame || m_gameArgs.GameType == GameType.TalTalGame)
        {
            m_playerData2.AutoPlay = true;
        }
        else if (m_gameArgs.GameType == GameType.OnePlayer)
        {
            m_playerData2.AutoPlay = false;
        }
        m_playerData2.PlayerScript.Init(m_playerData2);

    }



    public void onTurnLost()
    {
        PlayerArgs playerData;
        if (m_curPlayerTurn == PlayerIndex.First)
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

        if (m_inTutorial)
        {
            m_tutorialManager.OnBallLost();
            if (!m_tutorialManager.IsEnemyTurn())
            {
                m_ballsManager.OnNewBallInScene(PlayerIndex.First);
            }
            else
            {
                if (m_curPlayerTurn == PlayerIndex.First) //for the first turn change while tutorial
                {
                    m_playerData2.PlayerScript.ShowPlayer();
                    if (m_gameArgs.GameType == GameType.TalTalGame)
                    {
                        SwitchPlayerTurn(true);
                    }
                    else
                    {
                        SwitchPlayerTurn(false);
                    }

                }
                else
                {
                    m_ballsManager.OnNewBallInScene(PlayerIndex.Second);
                }
            }

        }
        else
        {


            if (m_gameArgs.GameType == GameType.TurnsGame)
            {
                SwitchPlayerTurn();
            }
            else if (m_gameArgs.GameType == GameType.TalTalGame)
            {
                playerData = playerData == m_playerData1 ? m_playerData2 : m_playerData1;
                playerData.CurScore++;
                if (playerData == m_playerData1)
                {
                    m_gameCanvas.CheerActivate();
                }
                m_gameCanvas.SetNormalScore(m_playerData1.CurScore, m_playerData2.CurScore);
                SwitchPlayerTurn(false);

            }
            else if (m_gameArgs.GameType == GameType.OnePlayer)
            {
                m_playerData1.CurScore = 0;
                m_ballsManager.OnNewBallInScene(PlayerIndex.First);
            }
            else
            {
                //playerData.Ball1.OnNewBallInScene();
                m_ballsManager.OnNewBallInScene(PlayerIndex.First);
            }
        }

    }

    public void OnBallHit(PlayerIndex playerIndex)
    {
        //print("onBallHit");
        if (m_inTutorial)
        {
            m_tutorialManager.OnBallHit();
            //return;
        }

        KickType kickType = KickType.Regular;
        if (playerIndex == PlayerIndex.First)
        {
            switch (kickType)
            {
                case KickType.Special:
                    EventManager.Broadcast(EVENT.EventSpecialKick);
                    break;
                default:
                    EventManager.Broadcast(EVENT.EventNormalKick);
                    break;
            }
        }

        if (m_gameArgs.GameType == GameType.TalTalGame)
        {
            m_gameCanvas.IncrementCombo();
            if (m_inTutorial && !m_tutorialManager.IsFreePlayMode())
            {
                return;
            }
            else
            {
                SwitchPlayerTurnAfterWait(false);
            }

            return;
        }

        if (m_gameArgs.GameType == GameType.OnePlayer)
        {
            m_playerData1.CurScore += 1;
            m_gameCanvas.IncrementCombo();
            if (m_inTutorial && !m_tutorialManager.IsFreePlayMode())
            {
                return;
            }
            else if (m_playerData1.CurScore % m_singlePlayerCheerFrequency == 0)
            {
                m_gameCanvas.CheerActivate();
                EventManager.Broadcast(EVENT.EventCombo);
            }

            return;
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
            if ((!m_inTutorial) && (m_curPlayerTurn == PlayerIndex.First))
            {
                m_gameCanvas.CheerActivate();
                EventManager.Broadcast(EVENT.EventCombo);
            }
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
            m_gameCanvas.SetScore(m_playerData1.CurScore, m_playerData2.CurScore,
             m_curPlayersScoreDelta, m_curPlayerInLead);

            yield return null;
        }

    }

    public void GameIsOver()
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
                m_playerData2.PlayerScript.Win();
            }
            else
            {
                m_playerData1.PlayerScript.Win();
                m_playerData2.PlayerScript.Win();
            }
        }



        Invoke("ExitScene", 5f);

    }

    private void TimeIsOver()
    {
        PlayerIndex winner = m_playerData1.CurScore > m_playerData2.CurScore ? PlayerIndex.First : PlayerIndex.Second;
        AnalyticsManager.CommitData(
                   "Match_Ended",
                   new Dictionary<string, object> {
                 { "Match Mode", m_gameArgs.GameType },
                  { "Match Winner", winner },
                  { "Match Score Delta", Math.Abs(m_playerData1.CurScore-m_playerData2.CurScore)}
                });

        GameIsOver();
    }

    private void ExitScene()
    {
        if ((!m_shouldRestart) && (m_mainMenu != null))
        {
            m_mainMenu.BackToMenu();
        }
        else
        {
            if (m_mainMenu == null)
            {
                SceneManager.LoadSceneAsync("Root");
            }
            else
            {
                m_mainMenu.OnRestart();

            }
        }
    }
    private void OnRestart()
    {
        PlayerIndex winner = m_playerData1.CurScore > m_playerData2.CurScore ? PlayerIndex.First : PlayerIndex.Second;
        AnalyticsManager.CommitData(
           "Retry_Button_Pressed",
           new Dictionary<string, object> {
                 { "Match Mode", m_gameArgs.GameType },
                  { "Match Winner", winner },
                  { "Match Score Delta", Math.Abs(m_playerData1.CurScore-m_playerData2.CurScore)}
        });
        if (m_inTutorial)
        {
            FinishedTutorial();
        }
        else
        {
            //m_shouldRestart = true;
            GameIsOver();
        }

    }

    private bool PlayerPrefsHasCompletedTutorial(GameType gameType)
    {
        if (!PlayerPrefs.HasKey("CompletedTutorialSinglePlayer"))
        {
            PlayerPrefs.SetInt("CompletedTutorialSinglePlayer", 0);
            return false;
        }
        if (!PlayerPrefs.HasKey("CompletedTutorialKickKick"))
        {
            PlayerPrefs.SetInt("CompletedTutorialKickKick", 0);
            return false;
        }
        if (!PlayerPrefs.HasKey("CompletedTutorialTurns"))
        {
            PlayerPrefs.SetInt("CompletedTutorialTurns", 0);
            return false;
        }

        switch (gameType)
        {
            case (GameType.OnePlayer):
                return Convert.ToBoolean(PlayerPrefs.GetInt("CompletedTutorialSinglePlayer"));
            case (GameType.TalTalGame):
                return Convert.ToBoolean(PlayerPrefs.GetInt("CompletedTutorialKickKick"));
            case (GameType.TurnsGame):
                return Convert.ToBoolean(PlayerPrefs.GetInt("CompletedTutorialTurns"));
            default:
                return false;

        }
    }

    private void UpdatePlayerPrefsCompletedTutorial(GameType gameType)
    {
        switch (gameType)
        {
            case (GameType.OnePlayer):
                PlayerPrefs.SetInt("CompletedTutorialSinglePlayer", 1);
                break;
            case (GameType.TalTalGame):
                PlayerPrefs.SetInt("CompletedTutorialKickKick", 1);
                break;
            case (GameType.TurnsGame):
                PlayerPrefs.SetInt("CompletedTutorialTurns", 1);
                break;


        }

    }


}
