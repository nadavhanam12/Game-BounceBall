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

public abstract class GameManagerAbstract : MonoBehaviour
{
    #region enum
    public enum PlayerIndex
    {
        First = 1,
        Second = 2
    }

    #endregion

    #region Refs
    GameBoundsData m_gameBoundsData;
    protected PlayerContainer m_playerContainer;
    protected GameCanvasScript m_gameCanvas;
    protected ComboDataContainer m_comboDataContainer;
    PlayersDataContainer m_playersDataContainer;
    protected GameBallsManager m_ballsManager;
    protected TutorialManager m_tutorialManager;
    PickablesManager m_pickablesManager;
    ConfettiManager m_confettiManager;
    IinputManager m_inputManager;

    #endregion

    #region private
    GameBounds m_gameBounds;
    protected PlayerArgs m_playerData1;
    protected PlayerArgs m_playerData2;
    int m_curPlayersScoreDelta;
    PlayerIndex m_curPlayerInLead;
    protected bool m_isGamePause;
    protected GameArgs m_gameArgs;
    protected PlayerIndex m_curPlayerTurn = PlayerIndex.First;
    protected bool m_inTutorial = false;
    bool m_shouldRestart;
    MainMenu m_mainMenu;
    protected bool m_timeIsOver = false;

    #endregion

    public void SetGameArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;
        Init();
    }

    public void StartGameScene()
    {
        if (m_gameArgs == null)
        {
            print("cant Start Game Scene without game args");
            return;
        }
        this.gameObject.SetActive(true);
        SendDataMatchStarted();
        EventManager.Broadcast(EVENT.EventStartGameScene);
        m_shouldRestart = false;
        if (m_gameArgs.ShouldPlayTutorial)
            StartTutorial();
        else
            AfterTutorial();
    }

    #region Inits
    void Init()
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
    }

    void InitRefs()
    {
        m_gameCanvas = GetComponentInChildren<GameCanvasScript>();
        m_ballsManager = GetComponentInChildren<GameBallsManager>();
        m_comboDataContainer = GetComponentInChildren<ComboDataContainer>();
        m_playersDataContainer = GetComponentInChildren<PlayersDataContainer>();
        m_tutorialManager = GetComponentInChildren<TutorialManager>();
        m_pickablesManager = GetComponentInChildren<PickablesManager>();
        m_confettiManager = GetComponentInChildren<ConfettiManager>();
    }
    void InitGameCanvas()
    {
        GameCanvasArgs canvasArgs = new GameCanvasArgs();
        canvasArgs.GameType = m_gameArgs.GameType;
        canvasArgs.MatchTime = m_gameArgs.MatchTime;
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
        if (m_gameCanvas != null)
            m_gameCanvas.SetCombo(0);
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
    }

    void InitData()
    {
        m_playerData1 = m_playersDataContainer.PlayerData1;
        m_playerData2 = m_playersDataContainer.PlayerData2;

        m_gameBoundsData = FindObjectOfType<GameBoundsData>(true);
        m_gameBounds = m_gameBoundsData.GenerateBounds(Camera.main);
        m_gameBoundsData.gameObject.SetActive(false);

    }

    protected abstract void InitGameMood(bool throwNewBall = true);

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
        m_tutorialManager.onShowOpponent = () => m_playerData2.PlayerScript?.ShowPlayer();
        m_tutorialManager.onRemoveAllBalls = m_ballsManager.RemoveAllBalls;
        m_tutorialManager.onGenerateNewBall = m_ballsManager.OnNewBallInScene;
        m_tutorialManager.onAllowOnlyJumpKick = m_ballsManager.onAllowOnlyJumpKick;
    }

    void InitListeners()
    {
        EventManager.Broadcast(EVENT.EventStartApp);
        EventManager.AddHandler(EVENT.EventOnRestart, OnRestartButtonPressed);
        EventManager.AddHandler(EVENT.EventOnCountdownEnds, FinishGameCountdown);
        EventManager.AddHandler(EVENT.EventMathEndRetryPressed, RestartGame);
        EventManager.AddHandler(EVENT.EventMathEndManuPressed, BackToMenu);
    }

    #endregion 

    void StartTutorial()
    {
        InitGameMood(false);
        m_playerData2.PlayerScript?.HidePlayer();
        m_inTutorial = true;
        Invoke("PlayTutorial", 1f);
        m_gameCanvas.ActivateTimer(false);
    }
    void PlayTutorial()
    {
        m_tutorialManager.Play();
    }
    protected virtual void FinishedTutorial()
    {
        m_inTutorial = false;
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
        if (m_gameArgs.ShouldPlayCountdown)
            Invoke("StartCountdown", m_gameArgs.CountDownDelay);
        else
            Invoke("FinishGameCountdown", m_gameArgs.CountDownDelay);
    }

    void StartCountdown()
    {
        SetGamePause(true);
        m_gameCanvas.StartCountdown();
    }

    public void FinishGameCountdown()
    {
        InitGameMood();
        /*if (m_gameArgs.GameType != GameType.SinglePlayer)
        {
            m_pickablesManager.FinishInitialize();
            m_pickablesManager.GeneratePickables();
        }*/
        SetGamePause(false);
    }

    protected abstract void SwitchPlayerTurn(bool shouldSwitchTurn = true);


    protected void InitPlayersStatus()
    {
        m_playerData1.PlayerScript.InitPlayer();
        m_playerData2.PlayerScript?.InitPlayer();
    }
    void InitPlayerOneStatus()
    {
        m_playerData1.PlayerScript.InitPlayer();
    }


    protected virtual void SwitchPlayerTurnAfterWait(bool throwNewBall = true, bool shouldSwitchTurn = true)
    {
        if (shouldSwitchTurn)
            m_curPlayerTurn = m_curPlayerTurn == PlayerIndex.First ? PlayerIndex.Second : PlayerIndex.First;

        if (m_curPlayerTurn == PlayerIndex.First)
        {
            m_playerData1.PlayerScript.StartTurn(throwNewBall);
            m_playerData2.PlayerScript?.LostTurn();
        }
        else
        {
            m_playerData1.PlayerScript.LostTurn();
            m_playerData2.PlayerScript?.StartTurn(throwNewBall);
        }

        m_gameCanvas.SetCurPlayerUI(m_curPlayerTurn == PlayerIndex.First);
    }

    protected void SetGamePause(bool isPause)
    {
        m_isGamePause = isPause;
        m_playerData1.PlayerScript.SetGamePause(isPause);
        m_playerData2.PlayerScript?.SetGamePause(isPause);
        m_ballsManager.SetGamePause(isPause);
        m_pickablesManager.SetGamePause(isPause);
        m_gameCanvas.SetGamePause(isPause);
    }

    protected abstract void InitPlayers();

    public abstract void onTurnLost();

    public abstract void OnBallHit(PlayerIndex playerIndex);

    protected void onTurnLostTutorial()
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
            SwitchPlayerTurn(false);

    }

    protected void UpdatePlayerCurCombo(PlayerArgs playerArgs)
    {
        playerArgs.CurCombo++;
        ComboData nextComboData = m_comboDataContainer.GetNextCombo(playerArgs.CurComboIndex);
        if (playerArgs.CurCombo == nextComboData.ComboRequired)
        {//New Combo-apply cheer and added score
            playerArgs.CurComboIndex++;
            if ((!m_inTutorial) && (m_curPlayerTurn == PlayerIndex.First))
                m_gameCanvas.CheerActivate();

            playerArgs.CurScore += (nextComboData.ScoreBonus);
        }

        m_gameCanvas.SetCombo(playerArgs.CurCombo);
    }


    IEnumerator UpdateScores()
    {
        while (!m_isGamePause)
        {
            m_curPlayersScoreDelta = m_playerData1.CurScore - m_playerData2.CurScore;
            if (m_curPlayersScoreDelta > 0)//player 1 lead
                m_curPlayerInLead = PlayerIndex.First;

            else if (m_curPlayersScoreDelta < 0)//player 2 lead
            {
                m_curPlayersScoreDelta *= (-1);
                m_curPlayerInLead = PlayerIndex.Second;
            }
            m_gameCanvas.SetScore(m_playerData1.CurScore, m_playerData2.CurScore,
             m_curPlayersScoreDelta, m_curPlayerInLead);

            yield return null;
        }

    }

    public abstract void GameIsOver();

    protected void MatchEnd()
    {
        PlayerIndex winner = m_playerData1.CurScore > m_playerData2.CurScore ? PlayerIndex.First : PlayerIndex.Second;
        SendDataMatchEnded(winner);
        GameIsOver();
    }

    void ExitScene()
    {
        if ((!m_shouldRestart) && (m_mainMenu != null))
            m_mainMenu.BackToMenu();
        else
        {
            if (!m_mainMenu)
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
    }

    public void BackToMenu()
    {
        if (!m_mainMenu)
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
        if (!m_mainMenu)
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

    void OnRestartButtonPressed()
    {
        if (m_inTutorial)
            FinishedTutorial();
        else
        {
            PlayerIndex winner = m_playerData1.CurScore > m_playerData2.CurScore ? PlayerIndex.First : PlayerIndex.Second;
            SendDataRetryButtonPressed(winner);
            GameIsOver();
        }
    }

    protected abstract void UpdatePlayerPrefsCompletedTutorial();

    void SendDataMatchStarted()
    {
        AnalyticsManager.Instance().CommitData(
                    AnalyticsManager.AnalyticsEvents.Event_Match_Started,
                    new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType }
                         });
    }
    void SendDataMatchEnded(PlayerIndex winner)
    {
        AnalyticsManager.Instance().CommitData(
                    AnalyticsManager.AnalyticsEvents.Event_Match_Ended,
                    new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType},
                  { "MatchWinner", winner }
                 });
    }
    void SendDataRetryButtonPressed(PlayerIndex winner)
    {
        AnalyticsManager.Instance().CommitData(
           AnalyticsManager.AnalyticsEvents.Event_Retry_Button_Pressed,
           new Dictionary<string, object> {
                 { "GameMode", m_gameArgs.GameType},
                  { "MatchWinner", winner }
        });
    }


    protected void BroadcastKickType()
    {
        KickType kickType = KickType.Regular;
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

    void RemoveListeners()
    {
        EventManager.RemoveHandler(EVENT.EventOnRestart, OnRestartButtonPressed);
        EventManager.RemoveHandler(EVENT.EventOnCountdownEnds, FinishGameCountdown);
        EventManager.RemoveHandler(EVENT.EventMathEndRetryPressed, RestartGame);
        EventManager.RemoveHandler(EVENT.EventMathEndManuPressed, BackToMenu);
    }
    void OnDestroy()
    {
        RemoveListeners();
    }

}
