using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private int m_curPanel = -1;
    private int curCombo = 0;

    private bool m_onCoolDown = false;

    public void Init(TutorialArgs args)
    {
        m_args = args;
        m_enemyTurn = false;
        m_args.TutorialUI.Init(m_args.GameCanvas);
        m_args.TutorialUI.OnTouchScreen = OnTouchScreen;
        InitDictionary();
        TurnOff();
    }


    private void InitDictionary()
    {
        List<Image> panels = m_args.TutorialUI.GetPanels();
        GameObject curPanel;
        for (int i = 0; i < panels.Count; i++)
        {
            curPanel = panels[i].gameObject;
            m_panels.Add(curPanel.name, i);
        }
    }
    public bool IsEnemyTurn()
    {
        return m_enemyTurn;
    }

    //play the tutorial 
    public void Play()
    {
        this.gameObject.SetActive(true);

        m_args.TutorialUI.Play();
        NextPanel();
    }
    private void NextPanel()
    {
        m_curPanel++;
        m_args.TutorialUI.OpenPanel(m_curPanel);
        StartCoroutine("StartCoolDown");
    }
    private void NextPanel(int index)
    {
        m_curPanel = index;
        m_args.TutorialUI.OpenPanel(m_curPanel);
        StartCoroutine("StartCoolDown");
    }

    private IEnumerator StartCoolDown()
    {
        //print("m_onCoolDown = true;");
        m_onCoolDown = true;
        yield return new WaitForSeconds(.25f);
        m_onCoolDown = false;
        //print("m_onCoolDown = false;");

    }

    //player hit the ball on tutorial
    public void OnBallHit()
    {
        curCombo++;
        if (!m_enemyTurn)
        {
            if ((m_curPanel == m_panels["SplitBallPanel"]) && (curCombo == 4))
            {

                m_enemyTurn = true;
                //onShowOpponent();
                /*Invoke("PauseGame", 0.75f);
                Invoke("NextPanel", 0.75f);*/

            }
            else
             if (m_curPanel == m_panels["ButtonsPanel"])
            {
                Invoke("PauseGame", 0.75f);
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
        if ((m_curPanel == m_panels["SplitBallPanel"]) && m_enemyTurn)
        {
            Invoke("PauseGame", 0f);
            onShowOpponent();
            NextPanel(m_panels["EnemyPanel"]);
            m_args.TutorialUI.HideButton();
            Invoke("ResumeGame", 3f);
            Invoke("HideEnemyPanel", 3f);
        }
        curCombo = 0;
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
            //print("CANT PRESS NOW");
            return;
        }

        if (m_curPanel == m_panels["WelcomePanel"])
        {
            //Welcome
            m_curPanel++;//to skip the buttons panel
            m_args.TutorialUI.ShowComboAndNextBall(true);

            NextPanel();
            m_args.TutorialUI.HideButton();
        }
        else if (m_curPanel == m_panels["ButtonsPanel"])
        {
            //Buttons
            m_args.TutorialUI.HidePanel(m_curPanel);
        }
        else if (m_curPanel == m_panels["SplitBallPanel"])
        {
            //SplitBall
            m_args.TutorialUI.HideButton();
            Invoke("ResumeGame", 0.0f);
        }
        else if (m_curPanel == m_panels["NiceComboPanel"])
        {
            //nice combo panel
            NextPanel();
        }
        else if (m_curPanel == m_panels["LosePanel"])
        {
            //how to lose turn
            m_args.TutorialUI.HidePanel(m_curPanel);
            Invoke("ResumeGame", 0.75f);


        }
        else if (m_curPanel == m_panels["EnemyPanel"])
        {
            //say hello to opponent
            m_args.TutorialUI.HidePanel(m_curPanel);
            Invoke("ResumeGame", 0.5f);
        }
        else if (m_curPanel == m_panels["ScorePanel"])
        {
            //how score works
            NextPanel();
        }
        else if (m_curPanel == m_panels["GoodluckPanel"])
        {
            //good luck
            FinishedTutorial();
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
