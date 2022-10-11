using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerAbstract;
using static PlayerScript;

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
    SpecialKickIntroductionText,
    SpecialKickExplanationText,
    PracticeSpecialKickGamePlay,
    SpecialKickActivationGamePlay,
    PracticeSpecialKickFinishText,
    OpponentAppears,
    TurnsExplanationText,
    TurnsUIExplanationText,
    PracticeOpponentGamePlayPhase1,
    PracticeOpponentGamePlayPhase2,
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

    private int curCombo = 0;

    private bool m_onCoolDown = false;
    private bool m_freePlayMode = false;
    [SerializeField] int m_firstComboLength = 2;
    [SerializeField] int m_specialKickComboLength = 3;

    [SerializeField] int m_FreePlayComboLength = 2;
    [SerializeField][Range(0, 3)] float m_cooldownLength = 0.5f;

    bool m_hitSpecialKick = false;

    private StageInTutorial m_curStageTutorial;

    public void Init(TutorialArgs args)
    {
        m_args = args;
        m_freePlayMode = false;
        m_args.TutorialUI.Init(m_args.GameCanvas);
        m_args.TutorialUI.OnTouchScreen = OnTouchScreen;
        m_hitSpecialKick = false;
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
        if (m_args.GameType == GameType.PvE)
        {
            onShowOpponent();
            m_args.PlayerScriptSecond.OnPlayIdle();
        }
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
    public void OnBallHit(KickType kickType)
    {
        curCombo++;
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
                    if (curCombo >= 2)
                        m_args.TutorialUI.StopHandGesture();
                }
                else if (curCombo == m_firstComboLength)
                {
                    m_args.GameCanvas.CheerActivate();
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
            case StageInTutorial.PracticeSpecialKickGamePlay:
                m_args.GameCanvas.SetSliderValue(curCombo, m_specialKickComboLength);
                if (curCombo == m_specialKickComboLength)
                {
                    curCombo = 0;
                    m_args.GameCanvas.CheerActivate();
                    Invoke("NextPanel", 0.25f);
                    Invoke("PauseGame", 0.25f);
                    m_args.PlayerScript.SetAllowedSpecialKick(true);
                }
                break;
            case StageInTutorial.SpecialKickActivationGamePlay:
                if (kickType != KickType.Special)
                    return;
                if (m_hitSpecialKick)
                    return;
                m_hitSpecialKick = true;
                m_args.GameCanvas.CheerActivate();
                m_args.GameCanvas.SetSliderValue(0, m_specialKickComboLength);
                float waitDuration = 3f;
                Invoke("NextPanel", waitDuration);
                Invoke("PauseGame", waitDuration);
                break;
            case StageInTutorial.PracticeOpponentGamePlayPhase1:
                //PauseGame();
                NextPanel();
                m_args.TutorialUI.SwitchPvETurnUI();
                break;
            case StageInTutorial.PracticeOpponentGamePlayPhase2:
                m_args.TutorialUI.SwitchPvETurnUI();
                break;

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
            case StageInTutorial.PracticeSpecialKickGamePlay:
                //onInitPlayers();
                print("PracticeSpecialKickGamePlay init slider");
                m_args.GameCanvas.SetSliderValue(0, m_specialKickComboLength);
                break;

            case StageInTutorial.PracticeOpponentGamePlayPhase2:
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
                    m_args.PlayerScriptSecond.OnPlayIdle();
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
                m_args.GameCanvas.ToggleSliderBarFilling(true);
                break;
            case StageInTutorial.SpecialKickIntroductionText:
                NextPanel();
                onRemoveAllBalls();
                //onInitPlayers();
                break;
            case StageInTutorial.SpecialKickExplanationText:
                NextPanel();
                ResumeGame();
                StartCoroutine(GenerateBallWithDelay(1f, false, Vector2Int.right));
                //m_args.GameCanvas.ActiveOnlySlideButton();
                m_args.GameCanvas.ToggleAllInput(true);
                m_args.GameCanvas.ToggleSliderBarFilling(false);
                break;
            case StageInTutorial.SpecialKickActivationGamePlay:
                Invoke("ResumeGame", 0.0f);
                m_args.TutorialUI.DisablePanels();
                m_args.TutorialUI.StopHandGesture();
                break;
            case StageInTutorial.PracticeSpecialKickFinishText:
                m_args.GameCanvas.ToggleAllInput(false);
                onRemoveAllBalls();
                m_args.GameCanvas.ActiveButtons();
                AnalyticsManager.Instance().CommitData(
                       AnalyticsManager.AnalyticsEvents.Event_Tutorial_Step,
                       new Dictionary<string, object> {
                 { "TutorialStage", "SpecialKick_example" }
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
                StartCoroutine(GenerateBallWithDelay(1f, true, Vector2Int.zero));
                m_freePlayMode = true;
                //m_args.GameCanvas.ToggleAllInput(false);
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
            OnTouchScreen();
    }

    void HideSpecialKickActivationPanel()
    {
        //print("Tutorial OnTouchKickSpecial");
        if (m_curStageTutorial != StageInTutorial.SpecialKickActivationGamePlay)
            return;
        Invoke("ResumeGame", 0.25f);
        m_args.TutorialUI.DisablePanels();
    }


}
