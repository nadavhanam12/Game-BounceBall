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
        m_args.TutorialUI.InitAndPlay();
        m_args.TutorialUI.OnTouchScreen = OnTouchScreen;

    }

    //player hit the ball on tutorial
    public void OnBallHit()
    {
        if (!m_enemyTurn)
        {
            m_args.TutorialUI.NextPanel();
            Pause(true);
        }
        else
        {

        }

    }

    private void OnTouchScreen()
    {
        //print("OnTouchScreen");
        m_curPanel = m_args.TutorialUI.GetCurIndex();
        switch (m_curPanel)
        {
            case 0:
                m_args.TutorialUI.HideCurPanel();
                break;



            default:
                break;

        }

    }
}
