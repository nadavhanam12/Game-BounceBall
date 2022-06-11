using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum StageInTutorial
{
    WelcomePlayerText,
    KickTheBallText,
    FirstKickGamePlay,
    BallSplitText1,
    BallSplitText2,
    PracticeKickGamePlay,
    SlideText,
}
public class TutorialManager : MonoBehaviour
{

    public delegate void onFinishTutorial();
    public onFinishTutorial OnFinishTutorial;

    public delegate void OnShowOpponent();
    public OnShowOpponent onShowOpponent;

    public delegate void onPause(bool isPause);
    public onPause Pause;

    private TutorialArgs m_args;

    private bool m_enemyTurn;

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
        m_enemyTurn = false;
        m_freePlayMode = false;
        m_args.TutorialUI.Init(m_args.GameCanvas);
        m_args.TutorialUI.OnTouchScreen = OnTouchScreen;
        TurnOff();
    }
    public bool IsEnemyTurn()
    {
        return m_enemyTurn;
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
    }
    private void NextPanel()
    {
        m_curStageTutorial++;
        m_args.TutorialUI.OpenPanel(m_curStageTutorial);
        StartCoroutine("StartCoolDown");
    }
    private void NextPanel(int index)
    {
        m_curStageTutorial = (StageInTutorial)index;
        m_args.TutorialUI.OpenPanel(m_curStageTutorial);
        StartCoroutine("StartCoolDown");
    }

    private IEnumerator StartCoolDown()
    {
        //print("m_onCoolDown = true;");
        m_onCoolDown = true;
        yield return new WaitForSeconds(m_cooldownLength);
        m_onCoolDown = false;
        //print("m_onCoolDown = false;");

    }

    //player hit the ball on tutorial
    public void OnBallHit()
    {
        curCombo++;
        if (!m_enemyTurn)
        {
            if (m_curStageTutorial == StageInTutorial.FirstKickGamePlay)
            {
                Invoke("PauseGame", 0.75f);
                NextPanel();
            }

            else if (m_curStageTutorial == StageInTutorial.PracticeKickGamePlay)
            {
                if (curCombo < m_firstComboLength)
                {
                    //m_args.TutorialUI.ShowComboAndNextBall(true);
                    //should update how much kick left


                }
                else
                {
                    if (m_args.GameType == GameType.OnePlayer)//means no need to continue in the tutorial pas this point
                    {
                        //NextPanel(m_panels["GoodluckPanel"]);
                        m_args.GameCanvas.CheerActivate();
                        return;
                    }
                    m_enemyTurn = true;
                    m_args.GameCanvas.CheerActivate();
                }

            }

        }
        /*else if ((m_curStageTutorial != m_panels["SplitBallPanel"]) && (curCombo == m_FreePlayComboLength))
        {
            Invoke("ShowScoreDeltaWithDelay", 2f);
        }*/

    }

    private void ShowScoreDeltaWithDelay()
    {
        NextPanel();
        m_args.TutorialUI.ShowScoreDelta(true);
        PauseGame();
    }

    private void PauseGame()
    {
        Pause(true);
    }
    private void ResumeGame()
    {
        Pause(false);
    }
    public void OnBallLost()
    {
        /*
                //print(m_curPanel);
                if ((m_curPanel == m_panels["SplitBallPanel"]) && m_enemyTurn)
                {
                    Invoke("PauseGame", 0f);
                    onShowOpponent();
                    if (m_args.GameType == GameType.TalTalGame)
                    {
                        NextPanel(m_panels["EnemyPanel-KickKick"]);
                    }
                    else
                    {
                        NextPanel(m_panels["EnemyPanel-Turns"]);
                    }
                    //m_args.TutorialUI.HideButton();
                    //Invoke("ResumeGame", 3f);
                    //Invoke("HideEnemyPanel", 3f);
                }
                else if (m_args.GameType == GameType.OnePlayer && curCombo >= m_firstComboLength)
                {
                    PauseGame();
                    NextPanel(m_panels["GoodluckPanel"]);
                    //FinishedTutorial();
                }
                curCombo = 0;*/
    }

    private void HideEnemyPanel()
    {
        m_args.TutorialUI.HidePanel(m_panels["EnemyPanel"]);
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
                break;
            case StageInTutorial.KickTheBallText:
                NextPanel(2);
                ResumeGame();
                break;
            case StageInTutorial.BallSplitText1:
                m_args.TutorialUI.ShowComboAndNextBall(true);
                NextPanel();
                break;
            case StageInTutorial.BallSplitText2:
                NextPanel();
                Invoke("ResumeGame", 1.5f);
                break;
        }




        /*
                if (m_curStageTutorial == StageInTutorial.WelcomePlayerText)
                {
                    //Welcome
                    //m_args.TutorialUI.ShowComboAndNextBall(true);
                    NextPanel(1);

                }
                else if (m_curStageTutorial == m_panels["ButtonsPanel"])
                {
                    //Buttons
                    m_args.TutorialUI.HidePanel(m_curPanel);
                    ResumeGame();
                    //NextPanel();
                }
                else if (m_curStageTutorial == m_panels["SplitBallPanel"])
                {
                    //SplitBall
                    Invoke("ResumeGame", 0.5f);
                }
                else if ((m_curStageTutorial == m_panels["EnemyPanel-Turns"]) || (m_curPanel == m_panels["EnemyPanel-KickKick"]))
                {
                    //say hello to opponent
                    m_freePlayMode = true;
                    Invoke("ResumeGame", 0.5f);
                }
                else if (m_curStageTutorial == m_panels["ScorePanel"])
                {
                    //how score works
                    NextPanel();
                }
                else if (m_curStageTutorial == m_panels["GoodluckPanel"])
                {
                    //good luck
                    FinishedTutorial();
                }*/
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
        OnFinishTutorial();
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
