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

    private IinputManager m_inputManager;


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

    bool m_timeIsOver = false;



    #endregion

    public void SetGameArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;
        StartGameScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    void Awake()
    {
        Application.targetFrameRate = 60;
        m_mainMenu = FindObjectOfType<MainMenu>(true);
        if (m_mainMenu == null) //runs as solo scene
        {
            if (m_shouldStartWithMenu)
                NoMenuStartPage.gameObject.SetActive(true);
            else
                StartGameScene();

        }
        else
        {
            m_shouldPlayCountdown = true;
            m_matchTime = 60;
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

        AnalyticsManager.Instance().CommitData(
                    AnalyticsManager.AnalyticsEvents.Event_Match_Started,
                    new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType }
                         });

        if (m_mainMenu != null)
            m_shouldPlayTutorial = !PlayerPrefsHasCompletedTutorial();// false;

        Init();
        m_gameBoundsData.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventManager.Broadcast(EVENT.EventStartGameScene);
        m_shouldRestart = false;


        if (m_shouldPlayTutorial)
        {
            StartTutorial();
            //AfterTutorial();
        }
        else
        {
            AfterTutorial();
        }
    }
    void StartTutorial()
    {
        InitGameMood(false);
        m_playerData2.PlayerScript.HidePlayer();
        m_inTutorial = true;
        Invoke("PlayTutorial", 1f);
        m_gameCanvas.ActivateTimer(false);
    }
    void PlayTutorial()
    {
        m_tutorialManager.Play();
    }
    void FinishedTutorial()
    {
        //should init after tutorial
        m_inTutorial = false;
        if (m_gameArgs.GameType == GameType.PvE)
            m_playerData2.PlayerScript.ShowPlayer();
        m_ballsManager.RemoveAllBalls();
        m_tutorialManager.TurnOff();
        InitScoreAndCombo();
        m_curPlayerTurn = PlayerIndex.First;
        m_gameCanvas.SetCurPlayerUI(m_curPlayerTurn == PlayerIndex.First);

        UpdatePlayerPrefsCompletedTutorial();

        AfterTutorial();
    }

    void AfterTutorial()
    {
        m_gameCanvas.ActivateTimer(true);
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
        m_tutorialManager.onFinishTutorial = FinishedTutorial;
        m_tutorialManager.onInitPlayers = InitPlayerOneStatus;
        m_tutorialManager.onShowOpponent = () => m_playerData2.PlayerScript.ShowPlayer();
        m_tutorialManager.onRemoveAllBalls = m_ballsManager.RemoveAllBalls;
        m_tutorialManager.onGenerateNewBall = m_ballsManager.OnNewBallInScene;
        m_tutorialManager.onAllowOnlyJumpKick = m_ballsManager.onAllowOnlyJumpKick;
        //(PlayerIndex playerIndex, PlayerIndex nextPlayerIndex) => { m_ballsManager.OnNewBallInScene(playerIndex, nextPlayerIndex); };

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

        InitGameMood();

        if (m_gameArgs.GameType != GameType.SinglePlayer)
        {
            m_pickablesManager.FinishInitialize();
            m_pickablesManager.GeneratePickables();
        }

        SetGamePause(false);
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
        InitInputManager();

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
    void InitInputManager()
    {
        m_inputManager = gameObject.AddComponent<MobileInputManager>();
        m_inputManager.Init(m_gameCanvas);
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
    void InitGameMood(bool throwNewBall = true)
    {
        if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
        {
            m_curPlayerTurn = PlayerIndex.First;
            m_playerData1.PlayerScript.StartTurn(throwNewBall);
            m_playerData2.PlayerScript.LostTurn();
        }
        else if (m_gameArgs.GameType == GameType.SinglePlayer)
        {
            m_curPlayerTurn = PlayerIndex.First;
            m_playerData1.PlayerScript.StartTurn(throwNewBall);
            m_playerData2.PlayerScript.HidePlayer();
        }

    }

    private async void SwitchPlayerTurn(bool shouldSwitchTurn = true)
    {
        if (!m_inTutorial)
        {
            m_gameCanvas.SwitchTurn(m_curPlayerTurn != PlayerIndex.First);
        }
        if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
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
        else if (m_gameArgs.GameType == GameType.SinglePlayer)
        {
            shouldSwitchTurn = false;
        }

        await Task.Delay(2000);
        InitPlayersStatus();

        await Task.Delay(1000);
        SwitchPlayerTurnAfterWait(true, shouldSwitchTurn);

    }

    private void InitPlayersStatus()
    {
        m_playerData1.PlayerScript.InitPlayer();
        m_playerData2.PlayerScript.InitPlayer();
    }
    private void InitPlayerOneStatus()
    {
        m_playerData1.PlayerScript.InitPlayer();
    }

    void SwitchPlayerTurnAfterWait(bool throwNewBall = true, bool shouldSwitchTurn = true)
    {
        //print("shouldSwitchTurn: " + shouldSwitchTurn);
        if (m_gameArgs.GameType == GameType.SinglePlayer)
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
        m_gameCanvas.m_onTimeIsOver = () => m_timeIsOver = true;

        m_gameCanvas.m_MovePlayerToPosition = m_playerData1.PlayerScript.MovePlayerToPosition;
        m_gameCanvas.m_OnTouchKickSpecial = m_playerData1.PlayerScript.OnTouchKickSpecial;
        m_gameCanvas.m_OnTouchJump = m_playerData1.PlayerScript.OnTouchJump;
        m_gameCanvas.m_OnTouchEnd = m_playerData1.PlayerScript.OnPlayIdle;




    }
    private void InitPlayers()
    {
        m_playerData1.PlayerScript = m_playerContainer1.GetComponentInChildren<PlayerScript>(true);
        m_playerData1.PlayerScript.gameObject.SetActive(false);

        m_playerData2.PlayerScript = m_playerContainer2.GetComponentInChildren<PlayerScript>(true);
        m_playerData2.PlayerScript.gameObject.SetActive(false);


        m_playerData1.PlayerScript.Init(m_playerData1);

        if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
        {
            m_playerData2.AutoPlay = true;
        }
        else if (m_gameArgs.GameType == GameType.SinglePlayer)
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

            if (m_tutorialManager.GetCurStage() == StageInTutorial.FirstKickGamePlay ||
                m_tutorialManager.GetCurStage() == StageInTutorial.PracticeSlideGamePlay)
            {
                m_ballsManager.OnNewBallInScene(false, Vector2Int.right);
                return;
            }
            if (m_tutorialManager.GetCurStage() == StageInTutorial.PracticeKickGamePlay ||
                m_tutorialManager.GetCurStage() == StageInTutorial.PracticeJumpGamePlay)
            {
                m_ballsManager.OnNewBallInScene();
                return;
            }
            if (m_tutorialManager.GetCurStage() == StageInTutorial.PracticeOpponentGamePlay)
            {
                if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
                {
                    SwitchPlayerTurn(true);
                }
                else
                {
                    SwitchPlayerTurn(false);
                }
            }
        }
        else if (m_timeIsOver)
        {
            MatchEnd();
        }
        else if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
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
        else
        {
            if (m_gameArgs.GameType == GameType.SinglePlayer)
            {
                m_playerData1.CurScore = 0;
            }
            m_ballsManager.OnNewBallInScene();
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
        /*if (playerIndex == PlayerIndex.First)
        {*/
        switch (kickType)
        {
            case KickType.Special:
                EventManager.Broadcast(EVENT.EventSpecialKick);
                break;
            default:
                EventManager.Broadcast(EVENT.EventNormalKick);
                break;
        }
        //}

        if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
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

        if (m_gameArgs.GameType == GameType.SinglePlayer)
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

            if (m_gameArgs.GameType == GameType.SinglePlayer)
            {
                int prevBestScore = m_gameCanvas.GetPrevBestCombo();
                int curBestScore = m_gameCanvas.GetCurBestCombo();
                if (prevBestScore < curBestScore)
                {
                    PlayerPrefs.SetInt("SinglePlayerBestCombo", curBestScore);
                    m_gameCanvas.OnNewBestScore(curBestScore);
                }
                else
                    m_gameCanvas.OnPrevBestScore(prevBestScore);
            }
            else if (m_gameArgs.GameType == GameType.PvE || m_gameArgs.GameType == GameType.PvP)
                m_gameCanvas.OnPvEEnd(m_playerData1.CurScore, m_playerData2.CurScore);

        }

    }

    void MatchEnd()
    {
        PlayerIndex winner = m_playerData1.CurScore > m_playerData2.CurScore ? PlayerIndex.First : PlayerIndex.Second;
        AnalyticsManager.Instance().CommitData(
                   AnalyticsManager.AnalyticsEvents.Event_Match_Ended,
                   new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType },
                  { "MatchWinner", winner }
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
                Scene mainMenuScene = SceneManager.GetSceneByName("Root");
                if (mainMenuScene.IsValid())
                    SceneManager.LoadSceneAsync("Root");
                else
                    SceneManager.LoadSceneAsync("GameScene");
            }
            else
            {
                m_mainMenu.OnRestart();
            }
        }
    }

    public void BackToMenu()
    {
        //print("BackToMenu");
        if (m_mainMenu == null)
        {
            Scene mainMenuScene = SceneManager.GetSceneByName("Root");
            if (mainMenuScene.IsValid())
                SceneManager.LoadSceneAsync("Root");
            else
                SceneManager.LoadSceneAsync("GameScene");
        }
        else
            m_mainMenu.BackToMenu();

    }

    public void RestartGame()
    {
        //print("RestartGame");
        if (m_mainMenu == null)
        {
            Scene mainMenuScene = SceneManager.GetSceneByName("Root");
            if (mainMenuScene.IsValid())
                SceneManager.LoadSceneAsync("Root");
            else
                SceneManager.LoadSceneAsync("GameScene");
        }
        else
            m_mainMenu.OnRestart();

    }



    private void OnRestart()
    {
        PlayerIndex winner = m_playerData1.CurScore > m_playerData2.CurScore ? PlayerIndex.First : PlayerIndex.Second;
        AnalyticsManager.Instance().CommitData(
           AnalyticsManager.AnalyticsEvents.Event_Retry_Button_Pressed,
           new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType },
                  { "MatchWinner", winner }
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

    private bool PlayerPrefsHasCompletedTutorial()
    {
        string playerPrefsGameTutorial;
        if (m_gameArgs.GameType == GameType.SinglePlayer)
            playerPrefsGameTutorial = "CompletedSinglePlayerTutorial";
        else if (m_gameArgs.GameType == GameType.PvE)
            playerPrefsGameTutorial = "CompletedTalTalTutorial";
        else
            return true;
        return Convert.ToBoolean(PlayerPrefs.GetInt(playerPrefsGameTutorial));
    }

    private void UpdatePlayerPrefsCompletedTutorial()
    {
        string playerPrefsGameTutorial = m_gameArgs.GameType == GameType.SinglePlayer ?
                    "CompletedSinglePlayerTutorial" : "CompletedTalTalTutorial";
        PlayerPrefs.SetInt(playerPrefsGameTutorial, 1);

    }


}
