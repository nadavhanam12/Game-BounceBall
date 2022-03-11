
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static GameManagerScript;
using static UnityEngine.UI.Button;

public class GameCanvasScript : MonoBehaviour
{
    [SerializeField] ScoreDeltaUIClass ScoreUIDelta;
    [SerializeField] ScoreUIClass ScoreUIPlayer1;
    [SerializeField] ScoreUIClass ScoreUIPlayer2;
    [SerializeField] CheerScript CheerObject;

    [SerializeField] SequenceMenuUI SequenceMenuUI;

    // [SerializeField]
    // [Range(0, 9)]
    // int ScoreCountingSpeed;

    [SerializeField]
    [Range(1, 2)]
    float ScoreScaleUpFactor;

    [SerializeField]
    [Range(1, 10)]
    int ScoreScaleDownSpeed;

    private PlayerIndex m_curPlayerInLead;
    private int m_timeToPlay;

    private bool isGamePaused;

    public delegate void TimeIsOver();

    public TimeIsOver m_onTimeIsOver;
    private bool m_initialized;

    [SerializeField] RawImage Background;

    public delegate void OnTouchKickRegular();
    public OnTouchKickRegular m_OnTouchKickRegular;
    public delegate void OnTouchKickUp();
    public OnTouchKickUp m_OnTouchKickUp;
    public delegate void OnTouchKickPower();
    public OnTouchKickPower m_OnTouchKickPower;

    private Animator m_anim;

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        ScoreUIPlayer1.SetGamePause(isGamePaused);
        ScoreUIPlayer2.SetGamePause(isGamePaused);
        ScoreUIDelta.SetGamePause(isGamePaused);

    }
    public void OnKickPower()
    {
        m_OnTouchKickPower();
    }
    public void OnKickRegular()
    {
        //print("OnKickRegular");
        m_OnTouchKickRegular();
    }
    public void OnKickUp()
    {
        m_OnTouchKickUp();
    }

    public void OnRestart()
    {
        print("OnRestart");
        EventManager.Broadcast(EVENT.EventOnRestart);
    }

    public void Init(GameCanvasArgs args)
    {
        if (!m_initialized)
        {
            m_initialized = true;
            this.GetComponent<Canvas>().worldCamera = Camera.main;
            m_anim = GetComponent<Animator>();
            m_timeToPlay = args.MatchTime;

            ScoreUiArgs scoreUiArgs1 = new ScoreUiArgs();
            scoreUiArgs1.ScoreColor = args.PlayerColor1;
            scoreUiArgs1.ScoreScaleDownSpeed = ScoreScaleDownSpeed;
            scoreUiArgs1.ScoreScaleUpFactor = ScoreScaleUpFactor;
            ScoreUIPlayer1.Init(scoreUiArgs1);

            ScoreUiArgs scoreUiArgs2 = new ScoreUiArgs();
            scoreUiArgs2.ScoreColor = args.PlayerColor2;
            scoreUiArgs2.ScoreScaleDownSpeed = ScoreScaleDownSpeed;
            scoreUiArgs2.ScoreScaleUpFactor = ScoreScaleUpFactor;
            ScoreUIPlayer2.Init(scoreUiArgs2);

            //need to set also the delta

            ScoreUiArgs scoreUiDeltaArgs = new ScoreUiArgs();
            scoreUiDeltaArgs.ScoreScaleDownSpeed = ScoreScaleDownSpeed;
            scoreUiDeltaArgs.ScoreScaleUpFactor = ScoreScaleUpFactor;
            ScoreUIDelta.SetColorsArgs(args.PlayerColor1, args.PlayerColor2);
            scoreUiDeltaArgs.TimeToPlay = m_timeToPlay;
            ScoreUIDelta.Init(scoreUiDeltaArgs);
            if (args.Background != null)
            {
                Background.texture = args.Background;
            }


            CheerObject.Init();
            SequenceMenuUI.Init();

            EventManager.AddHandler(EVENT.EventOnTimeOver, () => m_onTimeIsOver());
            //m_anim.Play("FadeIn", -1, 0f);


        }


    }

    public void CheerActivate()
    {
        CheerObject.Activate();
    }


    public void setScore(int scorePlayer1, int scorePlayer2, int scoreDelta, PlayerIndex playerInLead)
    {
        if (ScoreUIPlayer1.GetScore() != scorePlayer1)
        {
            ScoreUIPlayer1.setScore(scorePlayer1);
        }
        if (ScoreUIPlayer2.GetScore() != scorePlayer2)
        {
            ScoreUIPlayer2.setScore(scorePlayer2);
        }
        m_curPlayerInLead = playerInLead;
        if ((ScoreUIDelta.GetCurDelta() != scoreDelta) || (ScoreUIDelta.GetCurPlayerInLead() != m_curPlayerInLead))
        {
            ScoreUIDelta.setScore(scoreDelta, playerInLead);
        }

        /*int addedScore = score - m_curScore;
        ActivateScoreTween(addedScore.ToString());
        m_curScore = score;
        //Score.text = m_curScore.ToString();
        if (!m_isCounting)
        {
            StartCoroutine(CountTo());
        }

        if (m_curScale.x <= m_initScoreScale.x * 3f)
        {
            m_curScale.x = m_curScale.x * ScoreScaleUpFactor;
            m_curScale.y = m_curScale.y * ScoreScaleUpFactor;
        }
        if (!m_isScoreScaledUp)
        {
            StartCoroutine(ScoreScaleUp());
        }*/



    }

    public void UpdateSequenceUI(List<Sequence> newSequenceList)
    {
        if (!SequenceMenuUI.isInitialized())
        {
            SequenceMenuUI.Init();
        }
        SequenceMenuUI.UpdateSequenceUI(newSequenceList);
    }

    public void StartCountdown()
    {
        m_anim.Play("Countdown", -1, 0f);
    }
    public void OnCountdownEnds()
    {
        EventManager.Broadcast(EVENT.EventOnCountdownEnds);
    }

}