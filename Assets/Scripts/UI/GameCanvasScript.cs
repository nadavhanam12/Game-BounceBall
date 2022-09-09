
using UnityEngine;
using UnityEngine.UI;
using static GameManagerScript;

public class GameCanvasScript : MonoBehaviour
{
    #region events

    public delegate void TimeIsOver();

    public TimeIsOver m_onTimeIsOver;
    public delegate void OnMovePlayerToPosition(Vector2 position);
    public OnMovePlayerToPosition m_MovePlayerToPosition;
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
            EndGameScreen.gameObject.SetActive(false);
            //m_anim.Play("FadeIn", -1, 0f);


        }


    }

    private void OnTimeIsOver()
    {
        m_onTimeIsOver();
        //TurnsUI.ActivateTimeEnd();
        //CheerActivate(false);
    }

    void OnDestroy()
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
        EndGameScreen = GetComponentInChildren<EndGameScreen>(true);
    }

    public void CheerActivate(bool withTextAndInit = true)
    {
        CheerObject.Activate(withTextAndInit);
        m_args.ConfettiManager.Activate();
        EventManager.Broadcast(EVENT.EventCombo);
    }


    public void SetScore(int scorePlayer1, int scorePlayer2, int scoreDelta, PlayerIndex playerInLead)
    {
        m_curPlayerInLead = playerInLead;
        if ((ScoreUIDelta.GetCurDelta() != scoreDelta) || (ScoreUIDelta.GetCurPlayerInLead() != m_curPlayerInLead))
        {
            ScoreUIDelta.SetScore(scoreDelta, playerInLead);
        }
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
    public void SetCombo(int curCombo)
    {
        if (ComboConterUI != null)
        {
            ComboConterUI.SetCombo(curCombo);
            ScoreUIDelta.UpdateCombo(ComboConterUI.GetCurCombo());
        }
    }
    public void IncrementCombo()
    {
        if (ComboConterUI != null)
        {
            ComboConterUI.IncrementCombo();
            ScoreUIDelta.UpdateCombo(ComboConterUI.GetCurCombo());
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
    public void SetNormalScore(int scoreLeft, int scoreRight)
    {
        ScoreUIDelta.SetNormalScore(scoreLeft, scoreRight);
    }
    public void ShowBombUI(bool toShow)
    {
        BombUI.gameObject.SetActive(toShow);
    }
    public void GravityIncrease()
    {
        TurnsUI.GravityIncrease();
    }
    private void DisableSlaide()
    {
        ToggleSingleInput("Slide", false);
    }
    public void ToggleAllowOneSlide(bool isOn)
    {
        if (isOn)
            m_OnTouchKickSpecial += DisableSlaide;
        else
            m_OnTouchKickSpecial -= DisableSlaide;
    }
    public void ActiveOnlySlideButton()
    {
        CanMove = false;
        CanJump = false;
        CanKick = false;
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
    internal void OnTalTalGameEnd(int playerOneScore, int playerTwoScore)
    {
        if (playerOneScore > playerTwoScore)
            CheerActivate(false);
        EndGameScreen.OnTalTalGameEnd(playerOneScore, playerTwoScore);
    }

}