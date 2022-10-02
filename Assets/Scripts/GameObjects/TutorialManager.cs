using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerAbstract;

public enum StageInTutorial
{
    WelcomePlayerText,
    KickTheBallText,
    FirstKickGamePlay,
    BallSplitText1,
    BallSplitText2,
    PracticeKickGamePlay,
    PracticeKickFinishText,
    JumpExplanationText,
    PracticeJumpGamePlay,
    PracticeJumpFinishText,
    SlideIntroductionText,
    SlideExplanationText,
    PracticeSlideGamePlay,
    PracticeSlideFinishText,
    OpponentAppears,
    TurnsExplanationText,
    TurnsUIExplanationText,
    PracticeOpponentGamePlay,
    PointsMechanismText,
    WinStateText,
    BounceThatBallText


}
public class TutorialManager : MonoBehaviour
{
    public delegate void OnFinishTutorial();
    public OnFinishTutorial onFinishTutorial;

    public delegate void OnShowOpponent();
    public OnShowOpponent onShowOpponent;

    public delegate void OnInitPlayers();
    public OnInitPlayers onInitPlayers;
    public delegate void OnRemoveAllBalls();
    public OnRemoveAllBalls onRemoveAllBalls;
    public delegate void OnGenerateNewBall(bool randomDirection, Vector2Int directionVector);
    public OnGenerateNewBall onGenerateNewBall;
    public delegate void OnAllowOnlyJumpKick(bool isOn);
    public OnAllowOnlyJumpKick onAllowOnlyJumpKick;

    public delegate void onPause(bool isPause);
    public onPause Pause;

    private TutorialArgs m_args;

    Dictionary<string, int> m_panels = new Dictionary<string, int>();

    private int curCombo = 0;

    private bool m_onCoolDown = false;
    private bool m_freePlayMode = false;
    [SerializeField] int m_firstComboLength = 2;
    [SerializeField] int m_FreePlayComboLength = 2;
    [SerializeField][Range(0, 3)] float m_cooldownLength = 0.5f;

    private StageInTutorial m_curStageTutorial;

    public void Init(TutorialArgs args)
    {
        m_args = args;
        m_freePlayMode = false;
        m_args.TutorialUI.Init(m_args.GameCanvas);
        m_args.TutorialUI.OnTouchScreen = OnTouchScreen;
        TurnOff();
    }
    public bool IsFreePlayMode()
    {
        return m_freePlayMode;
    }

    //play the tutorial 
    public void Play()
    {
        this.gameObject.SetActive(true);
        m_args.TutorialUI.Play();
        NextPanel(0);
        Invoke("PauseGame", 0.05f);
        AnalyticsManager.Instance().CommitData(AnalyticsManager.AnalyticsEvents.Event_Tutorial_Started,
           new Dictionary<string, object> {
                 { "GameMode", m_args.GameType }
        });

        /*NextPanel(14);
        m_args.TutorialUI.ShowComboAndNextBall(true);
        onShowOpponent();
        m_freePlayMode = true;
        ResumeGame();*/
    }
    private void NextPanel()
    {
        m_curStageTutorial++;
        m_args.TutorialUI.OpenPanel(m_curStageTutorial);
        //Debug.Log("Stage: " + m_curStageTutorial);
        StartCoroutine("StartCoolDown");
    }
    private void NextPanel(StageInTutorial stage)
    {
        m_curStageTutorial = stage;
        m_args.TutorialUI.OpenPanel(m_curStageTutorial);
        //Debug.Log("Stage: " + m_curStageTutorial);
        StartCoroutine("StartCoolDown");
    }
    public StageInTutorial GetCurStage()
    {
        return m_curStageTutorial;
    }

    private IEnumerator StartCoolDown()
    {
        //print("m_onCoolDown = true;");
        m_onCoolDown = true;
        yield return new WaitForSeconds(m_cooldownLength);
        m_onCoolDown = false;
        //print("m_onCoolDown = false;");

    }


    private IEnumerator GenerateBallWithDelay(float delay, bool shoudlThrowRandomDirection, Vector2Int directionToThrow)
    {
        yield return new WaitForSeconds(delay);
        onGenerateNewBall(shoudlThrowRandomDirection, directionToThrow);
    }

    //player hit the ball on tutorial
    public void OnBallHit()
    {
        curCombo++;
        if (m_curStageTutorial != StageInTutorial.PracticeOpponentGamePlay)
        {
            switch (m_curStageTutorial)
            {
                case StageInTutorial.FirstKickGamePlay:

                    Invoke("PauseGame", 0.75f);
                    NextPanel();
                    break;
                case StageInTutorial.PracticeKickGamePlay:
                    if (curCombo < m_firstComboLength)
                    {
                        //TODO: should update how much kick left
                    }
                    else if (curCombo == m_firstComboLength)
                    {
                        m_args.GameCanvas.CheerActivate();
                    }
                    break;
                case StageInTutorial.PracticeSlideGamePlay:
                    //if (m_args.PlayerScript.IsOnSlide())
                    if (true)
                    {
                        curCombo = 0;
                        m_args.GameCanvas.CheerActivate();
                        Invoke("NextPanel", 0.75f);
                        Invoke("PauseGame", 0.75f);
                    }
                    break;
                case StageInTutorial.PracticeJumpGamePlay:
                    if (m_args.PlayerScript.IsOnJumpKick())
                    {
                        curCombo = 0;
                        m_args.GameCanvas.CheerActivate();
                        Invoke("NextPanel", 0.75f);
                        Invoke("PauseGame", 0.75f);
                    }
                    break;
            }

        }
        else
        {
            if (curCombo < m_FreePlayComboLength)
            {
                //TODO: should update how much kick left
            }
        }

    }

    public void OnBallLost()
    {
        switch (m_curStageTutorial)
        {
            case StageInTutorial.PracticeKickGamePlay:
                if (curCombo >= m_firstComboLength)
                {
                    PauseGame();
                    NextPanel();
                }
                break;
            case StageInTutorial.PracticeSlideGamePlay:
                onInitPlayers();
                //m_args.GameCanvas.ToggleSingleInput("Slide", true);
                break;

            case StageInTutorial.PracticeOpponentGamePlay:
                //onInitPlayers();
                if (curCombo >= m_FreePlayComboLength)
                {
                    m_args.GameCanvas.CheerActivate();
                    PauseGame();
                    NextPanel();
                    //FinishedTutorial();
                }
                break;

        }
        curCombo = 0;
    }

    private void PauseGame()
    {
        Pause(true);
    }
    private void ResumeGame()
    {
        Pause(false);
    }


    private void OnTouchScreen()
    {
        //print("OnTouchScreen");
        if (m_onCoolDown)
        {
            //print("CANT PRESS NOW");
            return;
        }
        switch (m_curStageTutorial)
        {
            case StageInTutorial.WelcomePlayerText:
                onRemoveAllBalls();
                if (m_args.GameType == GameType.PvE)
                {
                    onInitPlayers();
                    onShowOpponent();
                    NextPanel(StageInTutorial.OpponentAppears);
                }
                else
                    NextPanel(StageInTutorial.KickTheBallText);
                break;
            case StageInTutorial.KickTheBallText:
                NextPanel(StageInTutorial.FirstKickGamePlay);
                Invoke("ResumeGame", 0.5f);
                StartCoroutine(GenerateBallWithDelay(0.75f, false, Vector2Int.zero));
                break;
            case StageInTutorial.BallSplitText1:
                AnalyticsManager.Instance().CommitData(
                       AnalyticsManager.AnalyticsEvents.Event_Tutorial_Step,
                       new Dictionary<string, object> {
                 { "TutorialStage", "kick_example" }
                    });
                m_args.TutorialUI.ShowComboAndNextBall(true);

                NextPanel();
                break;
            case StageInTutorial.BallSplitText2:
                NextPanel();
                Invoke("ResumeGame", 1.5f);

                break;
            case StageInTutorial.PracticeKickFinishText:
                NextPanel();
                AnalyticsManager.Instance().CommitData(
                       AnalyticsManager.AnalyticsEvents.Event_Tutorial_Step,
                       new Dictionary<string, object> {
                 { "TutorialStage", "kick_practice" }
                    });

                break;
            case StageInTutorial.JumpExplanationText:
                NextPanel();
                onRemoveAllBalls();
                onInitPlayers();
                ResumeGame();
                StartCoroutine(GenerateBallWithDelay(1f, false, Vector2Int.right));
                m_args.GameCanvas.ToggleAllInput(true);
                m_args.GameCanvas.ToggleSingleInput("Slide", false);
                break;

            case StageInTutorial.PracticeJumpFinishText:
                onRemoveAllBalls();
                AnalyticsManager.Instance().CommitData(
                       AnalyticsManager.AnalyticsEvents.Event_Tutorial_Step,
                       new Dictionary<string, object> {
                 { "TutorialStage", "jump_example" }
                    });
                NextPanel();
                m_args.GameCanvas.ToggleAllInput(false);
                break;
            case StageInTutorial.SlideIntroductionText:
                NextPanel();
                onRemoveAllBalls();
                onInitPlayers();
                break;
            case StageInTutorial.SlideExplanationText:
                NextPanel();
                ResumeGame();
                StartCoroutine(GenerateBallWithDelay(1f, false, Vector2Int.right));
                //m_args.GameCanvas.ActiveOnlySlideButton();
                m_args.GameCanvas.ToggleAllInput(true);
                break;
            case StageInTutorial.PracticeSlideFinishText:
                m_args.GameCanvas.ToggleAllInput(false);
                onRemoveAllBalls();
                m_args.GameCanvas.ActiveButtons();
                AnalyticsManager.Instance().CommitData(
                       AnalyticsManager.AnalyticsEvents.Event_Tutorial_Step,
                       new Dictionary<string, object> {
                 { "TutorialStage", "slide_example" }
                    });

                //check if its a solo player tutorial
                if (m_args.GameType == GameType.SinglePlayer)
                    NextPanel(StageInTutorial.BounceThatBallText);
                else
                {
                    NextPanel();
                    onInitPlayers();
                    onShowOpponent();
                }
                break;
            case StageInTutorial.OpponentAppears:
                NextPanel();
                m_args.TutorialUI.ShowScoreDelta(true);
                break;
            case StageInTutorial.TurnsExplanationText:
                NextPanel();
                break;
            case StageInTutorial.TurnsUIExplanationText:
                Invoke("ResumeGame", 1.5f);
                StartCoroutine(GenerateBallWithDelay(1.5f, true, Vector2Int.zero));
                m_freePlayMode = true;
                NextPanel();
                break;
            case StageInTutorial.PointsMechanismText:
                AnalyticsManager.Instance().CommitData(
                           AnalyticsManager.AnalyticsEvents.Event_Tutorial_Step,
                           new Dictionary<string, object> {
                 { "TutorialStage", "opponent_practice" }
                        });
                onRemoveAllBalls();
                NextPanel();
                break;
            case StageInTutorial.WinStateText:
                NextPanel();
                break;
            case StageInTutorial.BounceThatBallText:
                FinishedTutorial();
                //Invoke("onGenerateNewBall", 1.5f);
                break;
        }
    }

    public void TurnOff()
    {
        m_args.TutorialUI.FinishTutorial();
        gameObject.SetActive(false);
    }
    private void FinishedTutorial()
    {
        //print("FinishedTutorial");
        AnalyticsManager.Instance().CommitData(AnalyticsManager.AnalyticsEvents.Event_Tutorial_Completed,
           new Dictionary<string, object> {
                 { "GameMode", m_args.GameType }
        });

        onFinishTutorial();
        //TurnOff();
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            OnTouchScreen();
        }
    }


}
