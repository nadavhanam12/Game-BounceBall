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
    private bool m_freePlayMode = false;
    [SerializeField] int m_firstComboLength = 2;
    [SerializeField] int m_FreePlayComboLength = 2;

    public void Init(TutorialArgs args)
    {
        m_args = args;
        m_enemyTurn = false;
        m_freePlayMode = false;
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
    public bool IsFreePlayMode()
    {
        return m_freePlayMode;
    }

    //play the tutorial 
    public void Play()
    {
        this.gameObject.SetActive(true);
        m_args.TutorialUI.Play();
        NextPanel();
        Invoke("PauseGame", 0.2f);
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
            if ((m_curPanel == m_panels["SplitBallPanel"]) && (curCombo == m_firstComboLength))
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
            else if (m_curPanel == m_panels["ButtonsPanel"] && curCombo <= 2)
            {
                Invoke("PauseGame", 0.75f);
                Invoke("NextPanel", 1.5f);

                m_args.TutorialUI.ShowComboAndNextBall(true);

            }

        }
        else if ((m_curPanel != m_panels["SplitBallPanel"]) && (curCombo == m_FreePlayComboLength))
        {
            Invoke("ShowScoreDeltaWithDelay", 2f);
        }

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
            FinishedTutorial();
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
            //m_curPanel++;//to skip the buttons panel
            m_args.TutorialUI.ShowComboAndNextBall(true);

            NextPanel();
        }
        else if (m_curPanel == m_panels["ButtonsPanel"])
        {
            //Buttons
            m_args.TutorialUI.HidePanel(m_curPanel);
            ResumeGame();
            //NextPanel();
        }
        else if (m_curPanel == m_panels["SplitBallPanel"])
        {
            //SplitBall
            m_args.TutorialUI.HideButton();
            Invoke("ResumeGame", 0.5f);
        }
        else if ((m_curPanel == m_panels["EnemyPanel-Turns"]) || (m_curPanel == m_panels["EnemyPanel-KickKick"]))
        {
            //say hello to opponent
            m_args.TutorialUI.HideButton();
            m_freePlayMode = true;
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
