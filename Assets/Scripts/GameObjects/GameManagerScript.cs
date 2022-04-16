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


    #endregion

    #region serialized
    [SerializeField] private int m_matchTime;
    [SerializeField] private float m_countDownDelay = 1f;
    [SerializeField] private bool m_shouldPlayTutorial = false;
    [SerializeField] private bool m_shouldPlayCountdown = false;
    [SerializeField] private bool m_shouldStartWithMenu = false;

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
    private bool m_onMobileDevice = false;
    private bool m_inTutorial = false;

    private bool m_shouldRestart;





    #endregion

    public void SetGameArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;
    }




    void Awake()
    {
        Application.targetFrameRate = 60;
#if UNITY_ANDROID && !UNITY_EDITOR
        m_onMobileDevice = true;
#endif
        //m_onMobileDevice = true;
        if (m_gameArgs == null)
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

        Init();
        m_gameBoundsData.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        m_shouldRestart = false;

        //m_playerData1.PlayerScript.PlayIdle();
        //m_playerData2.PlayerScript.PlayIdle();

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
        SetGamePause(false);
        m_playerData2.PlayerScript.ShowPlayer(false);
        m_inTutorial = true;
        m_tutorialManager.Play();
    }
    void FinishedTutorial()
    {
        //should init after tutorial
        m_inTutorial = false;
        m_playerData2.PlayerScript.ShowPlayer(true);
        m_ballsManager.RemoveAllBalls();
        m_tutorialManager.TurnOff();
        InitScoreAndCombo();

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
        tutorialArgs.TutorialUI = m_gameCanvas.GetTutorialUI();
        m_tutorialManager.Init(tutorialArgs);
        m_tutorialManager.Pause = SetGamePause;
        m_tutorialManager.OnFinishTutorial = FinishedTutorial;
        m_tutorialManager.onShowOpponent = () => m_playerData2.PlayerScript.ShowPlayer(true);

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
        m_gameCanvas.StartCountdown();
    }

    public void FinishGameCountdown()
    {
        EventManager.Broadcast(EVENT.EventStartGameScene);
        InitGameMood();
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
        //m_sequenceManager = GetComponentInChildren<SequenceManager>();
        m_tutorialManager = GetComponentInChildren<TutorialManager>();


    }

    private void InitBallsManager()
    {

        GameBallsManagerArgs ballsManagerArgs = new GameBallsManagerArgs();
        ballsManagerArgs.BallArgs = m_playersDataContainer.ballArgs;
        ballsManagerArgs.GameType = m_gameArgs.GameType;

        m_ballsManager.GameManagerOnBallHit = onBallHit;
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
        else
        {
            m_playerData1.PlayerScript.StartTurn();
            m_playerData2.PlayerScript.StartTurn();
        }
    }

    IEnumerator SwitchPlayerTurn(bool shouldSwitchTurn = true)
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

        yield return new WaitForSeconds(1);
        InitPlayersStatus();
        /*if (shouldSwitchTurn)
        {
            m_gameCanvas.SetCurPlayerUI(m_curPlayerTurn != PlayerIndex.First);
        }*/
        yield return new WaitForSeconds(1);
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

        if (m_gameArgs.GameType != GameType.TurnsGame)
        {
            m_playerData2.AutoPlay = true;
        }
        else if (m_gameArgs.GameType != GameType.TalTalGame)
        {
            m_playerData2.AutoPlay = true;
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
                    m_playerData2.PlayerScript.ShowPlayer(true);
                    StartCoroutine(SwitchPlayerTurn(false));
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
                StartCoroutine(SwitchPlayerTurn());
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
                StartCoroutine(SwitchPlayerTurn(false));

            }
            else
            {
                //playerData.Ball1.OnNewBallInScene();
                m_ballsManager.OnNewBallInScene(PlayerIndex.First);
            }
        }

    }


    public void onBallHit(PlayerIndex playerIndex)
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
            SwitchPlayerTurnAfterWait(false);
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
        if (m_shouldRestart)
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //SceneManager.UnloadSceneAsync("GameScene");
            SceneManager.LoadSceneAsync("GameScene");
        }
        else
        {
            SceneManager.LoadSceneAsync("Root");
        }
    }
    private void OnRestart()
    {
        if (m_inTutorial)
        {
            FinishedTutorial();
        }
        else
        {
            m_shouldRestart = true;
            TimeIsOver();
        }

    }


}
