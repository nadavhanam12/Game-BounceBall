using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{

    public delegate void onFinishTutorial();
    public onFinishTutorial OnFinishTutorial;

    public delegate void onPause(bool isPause);
    public onPause Pause;

    private TutorialArgs m_args;

    private bool m_enemyTurn;

    private int m_curPanel = -1;
    private int curCombo = 0;

    private bool m_onCoolDown = false;

    public void Init(TutorialArgs args)
    {
        m_args = args;
        m_enemyTurn = false;
    }
    public bool IsEnemyTurn()
    {
        return m_enemyTurn;
    }

    //play the tutorial 
    public void Play()
    {
        m_args.TutorialUI.InitAndPlay(m_args.GameCanvas);
        m_args.TutorialUI.OnTouchScreen = OnTouchScreen;
        NextPanel();
    }
    private void NextPanel()
    {
        m_curPanel++;
        m_args.TutorialUI.OpenPanel(m_curPanel);
        StartCoroutine("StartCoolDown");
    }

    private IEnumerator StartCoolDown()
    {
        print("m_onCoolDown = true;");
        m_onCoolDown = true;
        yield return new WaitForSeconds(.25f);
        m_onCoolDown = false;
        print("m_onCoolDown = false;");

    }

    //player hit the ball on tutorial
    public void OnBallHit()
    {
        curCombo++;
        if (!m_enemyTurn)
        {
            if ((m_curPanel == 2) && (curCombo == 3))
            {

                m_enemyTurn = true;
                Invoke("PauseGame", 0.75f);
                Invoke("NextPanel", 0.75f);

            }
            else if (m_curPanel == 0)
            {
                Invoke("PauseGame", 0.5f);
                m_args.TutorialUI.ShowComboAndNextBall(true);
                NextPanel();

            }

        }
        else if (curCombo == 1)
        {
            Invoke("ShowScoreDeltaWithDelay", 2f);
        }

    }

    private void ShowScoreDeltaWithDelay()
    {
        Invoke("PauseGame", 0.5f);
        NextPanel();
        m_args.TutorialUI.ShowScoreDelta(true);
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
        //print(m_curPanel);
        if ((m_curPanel == 2) && (!m_enemyTurn))
        {
            Invoke("PauseGame", 0f);

            //skip the nice combo screen
            m_curPanel++;
            m_enemyTurn = true;

            NextPanel();

        }
    }

    private void OnTouchScreen()
    {
        //print("OnTouchScreen");
        if (m_onCoolDown)
        {
            print("CANT PRESS NOW");
            return;
        }

        switch (m_curPanel)
        {
            case 0://Buttons
                m_args.TutorialUI.HidePanel(m_curPanel);
                break;
            case 1://SplitBall
                NextPanel();
                break;
            case 2://how to lose turn
                m_args.TutorialUI.HidePanel(m_curPanel);
                Invoke("ResumeGame", 0.75f);
                break;
            case 3://nice combo panel
                NextPanel();
                break;
            case 4://say hello to opponent
                curCombo = 0;
                m_args.TutorialUI.HidePanel(m_curPanel);
                Invoke("ResumeGame", 0.5f);
                break;
            case 5://how score works
                NextPanel();
                break;
            case 6://good luck
                FinishedTutorial();
                break;
            default:
                break;

        }

    }

    private void FinishedTutorial()
    {
        print("FinishedTutorial");
        m_args.TutorialUI.FinishTutorial();
        OnFinishTutorial();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            OnTouchScreen();
        }
    }


}
