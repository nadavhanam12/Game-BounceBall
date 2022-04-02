
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
    #region events

    public delegate void TimeIsOver();

    public TimeIsOver m_onTimeIsOver;
    public delegate void OnTouchKickRegular();
    public OnTouchKickRegular m_OnTouchKickRegular;
    public delegate void OnTouchJump();
    public OnTouchJump m_OnTouchJump;
    public delegate void OnTouchKickPower();
    public OnTouchKickPower m_OnTouchKickPower;



    #endregion

    #region public

    #endregion

    #region Refs

    private ScoreDeltaUIClass ScoreUIDelta;
    private CheerScript CheerObject;
    [SerializeField] RawImage Background;

    //[SerializeField] SequenceMenuUI SequenceMenuUI;
    private NextColorScript NextColorUI;
    private ComboConterUI ComboConterUI;
    private TurnsUI TurnsUI;
    private TutorialUI TutorialUI;
    #endregion

    #region private

    private PlayerIndex m_curPlayerInLead;
    private int m_timeToPlay;

    private bool isGamePaused;
    private bool m_initialized;
    private Animator m_anim;


    #endregion


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        ScoreUIDelta.SetGamePause(isGamePaused);

    }
    public void OnKickPowerInput() { m_OnTouchKickPower(); }
    public void OnKickRegularInput() { m_OnTouchKickRegular(); }
    public void OnJumpInput() { m_OnTouchJump(); }
    public void OnMoveRightInputPressed() { EventManager.Broadcast(EVENT.EventOnRightPressed); }
    public void OnMoveLeftInputPressed() { EventManager.Broadcast(EVENT.EventOnLeftPressed); }
    public void OnMoveRightInputReleased() { EventManager.Broadcast(EVENT.EventOnRightReleased); }
    public void OnMoveLeftInputReleased() { EventManager.Broadcast(EVENT.EventOnLeftReleased); }
    public void OnRestart() { EventManager.Broadcast(EVENT.EventOnRestart); }


    public void Init(GameCanvasArgs args)
    {
        if (!m_initialized)
        {
            InitRefs();
            m_initialized = true;
            this.GetComponent<Canvas>().worldCamera = Camera.main;
            m_anim = GetComponent<Animator>();
            m_timeToPlay = args.MatchTime;

            //need to set also the delta
            ScoreUIDelta.SetColors(args.PlayerColor1, args.PlayerColor2);
            ScoreUIDelta.SetTimeToPlay(m_timeToPlay);
            ScoreUIDelta.Init();
            if (args.Background != null)
            {
                Background.texture = args.Background;
            }


            CheerObject.Init();
            //SequenceMenuUI.Init();
            NextColorUI.Init();
            ComboConterUI.Init();
            TurnsUI.Init();

            EventManager.AddHandler(EVENT.EventOnTimeOver, () => m_onTimeIsOver());
            //m_anim.Play("FadeIn", -1, 0f);


        }


    }

    void InitRefs()
    {
        ScoreUIDelta = GetComponentInChildren<ScoreDeltaUIClass>(true);
        CheerObject = GetComponentInChildren<CheerScript>(true);
        NextColorUI = GetComponentInChildren<NextColorScript>(true);
        ComboConterUI = GetComponentInChildren<ComboConterUI>(true);
        TurnsUI = GetComponentInChildren<TurnsUI>(true);
        TutorialUI = GetComponentInChildren<TutorialUI>(true);
    }

    public void CheerActivate()
    {
        CheerObject.Activate();
    }


    public void setScore(int scorePlayer1, int scorePlayer2, int scoreDelta, PlayerIndex playerInLead)
    {
        /*if (ScoreUIPlayer1.GetScore() != scorePlayer1)
        {
            ScoreUIPlayer1.setScore(scorePlayer1);
        }
        if (ScoreUIPlayer2.GetScore() != scorePlayer2)
        {
            ScoreUIPlayer2.setScore(scorePlayer2);
        }*/
        m_curPlayerInLead = playerInLead;
        if ((ScoreUIDelta.GetCurDelta() != scoreDelta) || (ScoreUIDelta.GetCurPlayerInLead() != m_curPlayerInLead))
        {
            ScoreUIDelta.setScore(scoreDelta, playerInLead);
        }
    }

    /*
        public void UpdateSequenceUI(List<Sequence> newSequenceList)
        {
            if (!SequenceMenuUI.isInitialized())
            {
                SequenceMenuUI.Init();
            }
            SequenceMenuUI.UpdateSequenceUI(newSequenceList);
        }*/

    public void StartCountdown()
    {
        m_anim.Play("Countdown", -1, 0f);
    }
    public void OnCountdownEnds()
    {
        EventManager.Broadcast(EVENT.EventOnCountdownEnds);
    }
    public void UpdateNextBallColor(Color color)
    {
        NextColorUI.SetColor(color);
    }
    public void SetCombo(int curCombo)
    {
        ComboConterUI.SetCombo(curCombo);
    }
    public void SwitchTurn(bool isPlayerTurn)
    {
        TurnsUI.Activate(isPlayerTurn);
    }
    public TutorialUI GetTutorialUI()
    {
        return TutorialUI;
    }


}