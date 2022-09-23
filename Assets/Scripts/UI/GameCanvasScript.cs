
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using static GameManagerAbstract;

public class GameCanvasScript : MonoBehaviourPun
{
    #region events

    public delegate void TimeIsOver();

    public TimeIsOver m_onTimeIsOver;
    public delegate void OnMovePlayerToPosition(Vector2 position);
    public OnMovePlayerToPosition m_MovePlayerToPosition;
    public delegate void OnMoveLeft();
    public OnMoveLeft m_OnMoveLeft;
    public delegate void OnMoveRight();
    public OnMoveRight m_OnMoveRight;
    public delegate void OnTouchJump();
    public OnTouchJump m_OnTouchJump;
    public delegate void OnTouchKickSpecial();
    public OnTouchKickSpecial m_OnTouchKickSpecial;
    public delegate void OnTouchEnd();
    public OnTouchEnd m_OnTouchEnd;



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
    private EndGameScreen EndGameScreen;

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
    private bool CanMove = true;
    private bool CanKick = true;
    private bool CanSlide = true;
    private bool CanJump = true;

    #endregion


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        ScoreUIDelta.SetGamePause(isGamePaused);
    }
    public void ActivateTimer(bool activate)
    {
        ScoreUIDelta.ActivateTimer(activate);
    }
    public void OnEndInput() { m_OnTouchEnd(); }
    public void OnKickSpecialInput() { if (CanSlide) m_OnTouchKickSpecial(); }
    public void OnJumpInput() { if (CanJump) m_OnTouchJump(); }
    public void MovePlayerToPosition(Vector2 position) { if (CanMove) m_MovePlayerToPosition(position); }
    public void MoveRight() { if (CanMove) m_OnMoveRight(); }
    public void MoveLeft() { if (CanMove) m_OnMoveLeft(); }
    public void OnRestartButtonPressed() { EventManager.Broadcast(EVENT.EventOnRestart); }
    public void OnMathEndRetryPressed() { EventManager.Broadcast(EVENT.EventMathEndRetryPressed); }
    public void OnMathEndManuPressed() { EventManager.Broadcast(EVENT.EventMathEndManuPressed); }


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
            EndGameScreen.gameObject.SetActive(false);

        }
    }
    private void OnTimeIsOver()
    {
        m_onTimeIsOver();
    }

    void OnDestroy()
    {
        EventManager.RemoveHandler(EVENT.EventOnTimeOver, OnTimeIsOver);
    }

    private Texture ChooseRandomBackground()
    {
        if (m_args.GameType == GameType.PvP)
        {
            int max = backgroundsList.Length;
            return backgroundsList[max - 1];
        }
        else
        {
            int max = backgroundsList.Length;
            int min = 0;
            int rnd = Random.Range(min, max);
            return backgroundsList[rnd];
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
        RestartButton = GetComponentInChildren<RestartUI>(true);
        CurPlayerUI = GetComponentInChildren<CurPlayerUI>(true);
        EndGameScreen = GetComponentInChildren<EndGameScreen>(true);
    }
    [PunRPC]
    public void CheerActivate(bool withTextAndInit = true)
    {
        CheerObject.Activate(withTextAndInit);
        m_args.ConfettiManager.Activate();
        EventManager.Broadcast(EVENT.EventCombo);
    }
    public void CheerActivateSecondPlayer(bool withTextAndInit = true)
    {
        this.photonView.RPC("CheerActivate", RpcTarget.Others, withTextAndInit);
    }

    public void SetScore(int scorePlayer1, int scorePlayer2, int scoreDelta, PlayerIndex playerInLead)
    {
        m_curPlayerInLead = playerInLead;
        if ((ScoreUIDelta.GetCurDelta() != scoreDelta) || (ScoreUIDelta.GetCurPlayerInLead() != m_curPlayerInLead))
            ScoreUIDelta.SetScore(scoreDelta, playerInLead);
    }

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
    public void UpdateNextBallColor(Color curColor, Color[] nextColors, bool shouldEmitParticles)
    {
        NextColorUI.SetNextBallImage(curColor, nextColors, shouldEmitParticles);
    }

    [PunRPC]
    public void SetCombo(int curCombo)
    {
        if (ComboConterUI != null)
        {
            ComboConterUI.SetCombo(curCombo);
            ScoreUIDelta.UpdateCombo(ComboConterUI.GetCurCombo());
        }
        if (IsMasterPvP())
            this.photonView.RPC("SetCombo", RpcTarget.Others, curCombo);
    }

    [PunRPC]
    public void IncrementCombo()
    {
        if (ComboConterUI != null)
        {
            ComboConterUI.IncrementCombo();
            ScoreUIDelta.UpdateCombo(ComboConterUI.GetCurCombo());
        }
        if (IsMasterPvP())
            this.photonView.RPC("IncrementCombo", RpcTarget.Others);
    }

    [PunRPC]
    public void SwitchTurn(bool isFirstPlayerTurn)
    {
        TurnsUI.Activate(isFirstPlayerTurn);
        if (IsMasterPvP())
            this.photonView.RPC("SwitchTurn", RpcTarget.Others, !isFirstPlayerTurn);
    }

    [PunRPC]
    public void SetCurPlayerUI(bool isFirstPlayerTurn)
    {
        //print(isPlayerTurn);
        CurPlayerUI.SetImage(isFirstPlayerTurn);
        if (IsMasterPvP())
            this.photonView.RPC("SetCurPlayerUI", RpcTarget.Others, isFirstPlayerTurn);
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
        if (shouldShow)
        {
            transform.Find("Combo&BallColor").gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            transform.Find("Combo&BallColor").gameObject.transform.localScale = Vector3.zero;
        }

    }
    public void ShowSkipButton(bool toShow)
    {
        RestartButton.ShowSkipButton(toShow);
    }

    [PunRPC]
    public void SetNormalScore(int scoreLeft, int scoreRight)
    {
        ScoreUIDelta.SetNormalScore(scoreLeft, scoreRight);
        if (IsMasterPvP())
            this.photonView.RPC("SetNormalScore", RpcTarget.Others, scoreLeft, scoreRight);
    }
    public void ShowBombUI(bool toShow)
    {
        BombUI.gameObject.SetActive(toShow);
    }
    public void GravityIncrease()
    {
        TurnsUI.GravityIncrease();
    }

    public void ActiveButtons()
    {
        CanMove = true;
        CanJump = true;
        CanKick = true;
        CanSlide = true;
    }

    public void ToggleAllInput(bool shouldGetInput)
    {
        CanMove = shouldGetInput;
        CanKick = shouldGetInput;
        CanSlide = shouldGetInput;
        CanJump = shouldGetInput;
    }

    public void ToggleSingleInput(string actionString, bool shouldGetInput)
    {
        switch (actionString)
        {
            case "Move":
                CanMove = shouldGetInput;
                break;
            case "RegKick":
                CanKick = shouldGetInput;
                break;
            case "Slide":
                CanSlide = shouldGetInput;
                break;
            case "Jump":
                CanJump = shouldGetInput;
                break;
        }
    }

    public int GetPrevBestCombo()
    {
        return ScoreUIDelta.GetPrevBestCombo();
    }
    public int GetCurBestCombo()
    {
        return ScoreUIDelta.GetCurBestCombo();
    }

    internal void OnNewBestScore(int curBestScore)
    {
        CheerActivate(false);
        EndGameScreen.Activate(true, curBestScore);
    }

    internal void OnPrevBestScore(int prevBestScore)
    {
        EndGameScreen.Activate(false, prevBestScore);
    }

    internal void OnPvEEnd(int playerOneScore, int playerTwoScore)
    {
        bool victory = playerOneScore > playerTwoScore;
        if (victory)
            CheerActivate(false);
        EndGameScreen.OnPvEEnd(victory, playerOneScore, playerTwoScore);
    }
    internal void OnPvPEnd(int playerOneScore, int playerTwoScore)
    {
        bool victory = playerOneScore > playerTwoScore;
        OnPvPEndRPC(victory, playerOneScore, playerTwoScore);
        if (IsMasterPvP())
            this.photonView.RPC("OnPvPEndRPC", RpcTarget.Others, !victory, playerOneScore, playerTwoScore);
    }

    [PunRPC]
    void OnPvPEndRPC(bool victory, int playerOneScore, int playerTwoScore)
    {
        if (victory)
            CheerActivate(false);
        EndGameScreen.OnPvEEnd(victory, playerOneScore, playerTwoScore);
    }

    bool IsMasterPvP()
    {
        return (m_args?.GameType == GameType.PvP) && PhotonNetwork.IsMasterClient;
    }

}