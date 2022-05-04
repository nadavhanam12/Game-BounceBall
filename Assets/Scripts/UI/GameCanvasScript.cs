
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
    public delegate void OnTouchKickSpecial();
    public OnTouchKickSpecial m_OnTouchKickSpecial;



    #endregion

    #region public

    #endregion

    #region Refs

    private ScoreDeltaUIClass ScoreUIDelta;
    private CheerScript CheerObject;
    [SerializeField] RawImage Background;
    [SerializeField] GameObject CountdownUI;
    [SerializeField] GameObject BombUI;
    private RestartUI RestartButton;

    //[SerializeField] SequenceMenuUI SequenceMenuUI;
    private NextColorScript NextColorUI;
    private ComboConterUI ComboConterUI;
    private TurnsUI TurnsUI;
    private TutorialUI TutorialUI;
    private CurPlayerUI CurPlayerUI;
    #endregion

    #region private

    private PlayerIndex m_curPlayerInLead;
    private int m_timeToPlay;

    private bool isGamePaused;
    private bool m_initialized;
    private Animator m_anim;
    private Texture2D[] backgroundsList;
    const string backgroundsPath = "BackGround";

    private GameCanvasArgs m_args;


    #endregion


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        ScoreUIDelta.SetGamePause(isGamePaused);

    }
    public void OnKickSpecialInput() { m_OnTouchKickSpecial(); }
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
            m_args = args;
            InitRefs();
            m_initialized = true;
            this.GetComponent<Canvas>().worldCamera = Camera.main;
            m_anim = GetComponent<Animator>();
            m_timeToPlay = m_args.MatchTime;

            //need to set also the delta
            ScoreUIDelta.SetColors(m_args.PlayerColor1, m_args.PlayerColor2);
            ScoreUIDelta.SetTimeToPlay(m_timeToPlay);
            ScoreUIDelta.Init(m_args.GameType);

            backgroundsList = Resources.LoadAll<Texture2D>(backgroundsPath);
            Background.texture = ChooseRandomBackground();



            CheerObject.Init(m_args.GameType);
            //SequenceMenuUI.Init();
            NextColorUI.Init();
            ComboConterUI.Init();
            TurnsUI.Init(m_args.GameType);
            CurPlayerUI.Init(m_args.PlayerImage1, m_args.PlayerImage2);

            EventManager.AddHandler(EVENT.EventOnTimeOver, OnTimeIsOver);

            ShowBombUI(false);
            CountdownUI.gameObject.SetActive(false);
            //m_anim.Play("FadeIn", -1, 0f);


        }


    }

    private void OnTimeIsOver()
    {
        m_onTimeIsOver();
    }

    void Destroy()
    {
        EventManager.RemoveHandler(EVENT.EventOnTimeOver, OnTimeIsOver);
    }

    private Texture ChooseRandomBackground()
    {
        int rnd = Random.Range(0, backgroundsList.Length);
        return backgroundsList[rnd];
    }

    void InitRefs()
    {
        ScoreUIDelta = GetComponentInChildren<ScoreDeltaUIClass>(true);
        CheerObject = GetComponentInChildren<CheerScript>(true);
        NextColorUI = GetComponentInChildren<NextColorScript>(true);
        ComboConterUI = GetComponentInChildren<ComboConterUI>(true);
        TurnsUI = GetComponentInChildren<TurnsUI>(true);
        TutorialUI = GetComponentInChildren<TutorialUI>(true);
        RestartButton = GetComponentInChildren<RestartUI>(true);
        CurPlayerUI = GetComponentInChildren<CurPlayerUI>(true);
    }

    public void CheerActivate()
    {
        CheerObject.Activate();
    }


    public void SetScore(int scorePlayer1, int scorePlayer2, int scoreDelta, PlayerIndex playerInLead)
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
            ScoreUIDelta.SetScore(scoreDelta, playerInLead);
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
        CountdownUI.SetActive(true);
        m_anim.Play("Countdown", -1, 0f);
    }
    public void OnCountdownEnds()
    {
        CountdownUI.SetActive(false);
        EventManager.Broadcast(EVENT.EventOnCountdownEnds);
    }
    public void UpdateNextBallColor(Color color)
    {
        NextColorUI.SetColor(color);
    }
    public void SetCombo(int curCombo)
    {
        if (ComboConterUI != null)
        {
            ComboConterUI.SetCombo(curCombo);
        }
    }
    public void IncrementCombo()
    {
        if (ComboConterUI != null)
        {
            ComboConterUI.IncrementCombo();
        }
    }
    public void SwitchTurn(bool isPlayerTurn)
    {
        /*if ((m_args.GameType == GameType.TalTalGame) && (isPlayerTurn))
        {
            return;
        }*/

        TurnsUI.Activate(isPlayerTurn);


    }
    public void SetCurPlayerUI(bool isPlayerTurn)
    {
        //print(isPlayerTurn);
        CurPlayerUI.SetImage(isPlayerTurn);
    }
    public TutorialUI GetTutorialUI()
    {
        return TutorialUI;
    }
    public void ShowScoreDelta(bool shouldShow)
    {
        ScoreUIDelta.gameObject.SetActive(shouldShow);
    }
    public void ShowComboAndNextBall(bool shouldShow)
    {
        transform.Find("Combo&BallColor").gameObject.SetActive(shouldShow);
    }
    public void ShowSkipButton(bool toShow)
    {
        RestartButton.ShowSkipButton(toShow);
    }
    public void SetNormalScore(int scoreLeft, int scoreRight)
    {
        ScoreUIDelta.SetNormalScore(scoreLeft, scoreRight);
    }
    public void ShowBombUI(bool toShow)
    {
        BombUI.gameObject.SetActive(toShow);
    }


}