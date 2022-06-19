using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerScript;

public enum StageInTutorial
{
    WelcomePlayerText,
    KickTheBallText,
    FirstKickGamePlay,
    BallSplitText1,
    BallSplitText2,
    PracticeKickGamePlay,
    PracticeKickFinishText,
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
    public delegate void OnGenerateNewBall(PlayerIndex playerIndex, PlayerIndex nextPlayerIndex);
    public OnGenerateNewBall onGenerateNewBall;

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
        Invoke("PauseGame", 0.2f);
        AnalyticsManager.CommitData("Tutorial_Started");

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
        Debug.Log("Stage: " + m_curStageTutorial);
        StartCoroutine("StartCoolDown");
    }
    private void NextPanel(int index)
    {
        m_curStageTutorial = (StageInTutorial)index;
        m_args.TutorialUI.OpenPanel(m_curStageTutorial);
        Debug.Log("Stage: " + m_curStageTutorial);
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


    private IEnumerator GenerateBallWithDelay(float delay)
    {
        print("WaitForSeconds 1");
        yield return new WaitForSeconds(delay);
        print("WaitForSeconds 2");
        onGenerateNewBall(PlayerIndex.First, PlayerIndex.First);
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
                        PauseGame();
                        m_args.GameCanvas.CheerActivate();
                        NextPanel();

                    }
                    break;
                case StageInTutorial.PracticeSlideGamePlay:
                    if (curCombo == 1)
                    {
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

            case StageInTutorial.PracticeSlideGamePlay:
                onInitPlayers();
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
            print("CANT PRESS NOW");
            return;
        }
        switch (m_curStageTutorial)
        {
            case StageInTutorial.WelcomePlayerText:
                NextPanel(1);
                onRemoveAllBalls();
                break;
            case StageInTutorial.KickTheBallText:
                NextPanel(2);
                Invoke("ResumeGame", 0.5f);
                StartCoroutine(GenerateBallWithDelay(0.75f));
                break;
            case StageInTutorial.BallSplitText1:
                m_args.TutorialUI.ShowComboAndNextBall(true);
                NextPanel();
                break;
            case StageInTutorial.BallSplitText2:
                NextPanel();
                Invoke("ResumeGame", 1.5f);

                break;
            case StageInTutorial.PracticeKickFinishText:
                NextPanel();

                break;
            case StageInTutorial.SlideIntroductionText:
                NextPanel();
                onRemoveAllBalls();
                onInitPlayers();
                break;
            case StageInTutorial.SlideExplanationText:
                NextPanel();
                ResumeGame();
                StartCoroutine(GenerateBallWithDelay(1f));
                m_args.GameCanvas.ActiveOnlySlideButton();
                break;
            case StageInTutorial.PracticeSlideFinishText:
                NextPanel();
                onRemoveAllBalls();
                onInitPlayers();
                onShowOpponent();
                m_args.GameCanvas.ActiveButtons();
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
                StartCoroutine(GenerateBallWithDelay(1.5f));
                m_freePlayMode = true;
                NextPanel();
                break;
            case StageInTutorial.PointsMechanismText:
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
        AnalyticsManager.CommitData("Tutorial_Completed");
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
