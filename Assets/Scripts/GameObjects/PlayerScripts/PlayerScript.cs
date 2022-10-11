using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static GameManagerAbstract;

public class PlayerScript : MonoBehaviourPun
{
    #region enums
    public enum KickType
    {
        Regular = 0,
        Special = 1,
    }

    #endregion
    #region serialized 
    private SpriteRenderer playerSprite;
    #endregion

    #region private
    private bool m_initialized = false;
    protected bool isGamePaused = true;
    protected bool m_currentlyInTurn = true;
    protected PlayerArgs m_args;
    private bool m_inInitCooldown = false;
    private float m_halfFieldDistance;

    protected PlayerAnimations m_playerAnimations;
    protected PlayerMovement m_playerMovement;
    protected PlayerKick m_playerKicksManager;
    protected PlayerBomb m_playerBombsManager;
    protected PlayerAuraCircle m_playerAuraCircle;


    public bool IsAllowedSpecialKick { get; protected set; }

    #endregion

    public void Init(PlayerArgs args)
    {
        if (!m_initialized)
        {
            m_args = args;
            playerSprite = GetComponent<SpriteRenderer>();
            InitScripts();

            m_halfFieldDistance = (m_args.Bounds.GameRightBound - m_args.Bounds.GameLeftBound) / 2f;

            m_initialized = true;
            SetAllowedSpecialKick(false);
            InitPlayer();
            if (m_args.Darker)
                DarkerPlayer();
        }
    }

    public PlayerIndex GetIndex()
    {
        return m_args.PlayerIndex;
    }
    protected virtual void InitScripts()
    {
        m_playerAnimations = GetComponent<PlayerAnimations>();
        m_playerAnimations.Init();

        m_playerMovement = GetComponent<PlayerMovement>();
        m_playerMovement.Init(this, m_args);

        m_playerKicksManager = GetComponent<PlayerKick>();
        m_playerKicksManager.Init(this, m_args);

        m_playerBombsManager = GetComponent<PlayerBomb>();
        m_playerBombsManager?.Init(m_args);

        m_playerAuraCircle = GetComponentInChildren<PlayerAuraCircle>();
        m_playerAuraCircle.Init();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isGamePaused)
            if (!m_playerMovement.InParalyze)
            {
                m_playerMovement.GetJump();
                DetectBalls();
            }
    }

    protected virtual void DetectBalls()
    {
        if (IsCurrentlyInTurn() && !m_playerKicksManager.InKickCooldown)
        {
            List<BallScript> ballsHit = m_playerKicksManager.CheckBallInHitZone();
            if (ballsHit.Count > 0)
                OnKickPlay(ballsHit);
        }
    }


    IEnumerator InitCooldown()
    {
        //print("m_inKickCooldown = true");
        m_inInitCooldown = true;
        yield return new WaitForSeconds(m_args.playerStats.InitCooldown);
        m_inInitCooldown = false;
        //print("m_inKickCooldown = false");
    }

    public void Win()
    {
        /*m_playerMovement.SetInParalyze(true);
        m_playerAnimations.WinAnim();*/
    }

    public void Lose()
    {
        m_playerAuraCircle.Disable();
        /*m_playerMovement.SetInParalyze(true);
        m_playerAnimations.LoseAnim();*/
    }


    public virtual void FinishAnimation()
    {
        m_playerMovement.SetInParalyze(false);
        m_playerMovement.SetIsJumping(false);
        OnPlayIdle();
    }

    public virtual void InitPlayer(bool initPos = true)
    {
        if (!this) return;
        if (initPos && !m_inInitCooldown)
        {
            m_playerMovement.InitPlayer();
            if (gameObject.activeInHierarchy)
                StartCoroutine(InitCooldown());
        }
        FinishAnimation();
        ShowPlayer();
        //SetAllowedSpecialKick(false);
    }
    protected void InitPosY()
    {
        m_playerMovement.InitPosY();
        FinishAnimation();
    }

    public void OnKickPlay(List<BallScript> ballsHit)
    {
        KickType kickType = KickType.Regular;
        //kickType = KickType.Special;
        string triggerName = "KickReg Trigger";
        if (isGamePaused || m_playerMovement.InParalyze || m_args.BallsManager.IsInKickCooldown())
            return;

        if (m_playerKicksManager.IsNextKickIsSpecial)
        {
            kickType = KickType.Special;
            //triggerName = "KickSpecial Trigger";
        }
        m_playerKicksManager.ReachHitPosition(kickType, ballsHit);
        if (!m_playerMovement.IsJumping)
            m_playerAnimations.AnimSetTrigger(triggerName);

    }



    protected void OnJump()
    {
        if (m_playerMovement.InParalyze)
            return;
        if (m_playerBombsManager != null && m_playerBombsManager.HasBomb())
        {
            m_playerBombsManager.ActivateBomb();
            return;
        }
        if (!m_playerMovement.IsJumping)
        {
            if (!m_args.AutoPlay)
                AnalyticsManager.Instance().CommitData(AnalyticsManager.AnalyticsEvents.Event_Jump);
            m_playerMovement.SetIsJumping(true);
            m_playerMovement.SetIsJumpingUp(true);

            m_playerAnimations.AnimSetTrigger("Jump Trigger");
        }
    }

    public virtual void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        m_playerAnimations.SetGamePause(isPause);
        m_playerMovement.SetGamePause(isPause);
    }

    public void OnTouchJump()
    {
        OnJump();
    }

    public void OnPlayIdle()
    {
        if (!m_playerMovement.InParalyze)
            m_playerAnimations.OnPlayIdle();
    }

    public void MoveRight()
    {
        m_playerMovement.MoveX(Vector2.right);
    }
    public void MoveLeft()
    {
        m_playerMovement.MoveX(Vector2.left);
    }

    public virtual void OnTouchKickSpecial()
    {
        //print("onTouchKickSpecial");
        /*
        if (!IsAllowedSpecialKick)
            return;
        SetAllowedSpecialKick(false);
        m_playerKicksManager.SetNextKickIsSpecial(true);
        m_playerAuraCircle.ReadyAura();*/
    }

    public void SetAllowedSpecialKick(bool isAllowed, bool initNextKickIsSpecial = false)
    {
        //print("SetAllowedSpecialKick " + isAllowed);
        IsAllowedSpecialKick = isAllowed;
        if (IsAllowedSpecialKick)
        {
            SetAllowedSpecialKick(false);
            m_playerKicksManager.SetNextKickIsSpecial(true);
            m_playerAuraCircle.ReadyAura();
            //m_playerAuraCircle?.IdleAura();
        }
        else
            m_playerAuraCircle?.Disable();

        if (initNextKickIsSpecial)
            m_playerKicksManager.SetNextKickIsSpecial(false);
    }
    public void ActivateAura()
    {
        m_playerAuraCircle.Activate();
    }


    public virtual void LostTurn()
    {
        m_currentlyInTurn = false;
    }
    public virtual void StartTurn(bool throwNewBall = true)
    {
        m_currentlyInTurn = true;
        if (throwNewBall)
            m_args.BallsManager.OnNewBallInScene();
    }
    public void ShowPlayer()
    {
        playerSprite?.gameObject.SetActive(true);
    }
    public void HidePlayer()
    {
        playerSprite?.gameObject.SetActive(false);
    }

    public void PlayerHitByBomb()
    {
        AnalyticsManager.Instance().CommitData(
          AnalyticsManager.AnalyticsEvents.Event_Player_Hit_By_Bomb,
          new Dictionary<string, object> {
                 { "PlayerIndex", m_args.PlayerIndex }

        });
        m_playerMovement.SetInParalyze(true);
        m_playerAnimations.AnimSetTrigger("Die Trigger");
    }


    public bool IsOnJumpKick()
    {
        return m_playerMovement.IsJumping;
    }

    public void PlayRunAnim()
    {
        m_playerAnimations.RunAnim();
    }
    public void MovePlayerToPosition(Vector2 position)
    {
        m_playerMovement.MovePlayerToPosition(position);
    }

    public virtual void SendKickEventData(KickType kickType)
    {
        AnalyticsManager.AnalyticsEvents kickDataEvent = kickType ==
                KickType.Regular ?
                AnalyticsManager.AnalyticsEvents.Event_Kick_Regular :
                AnalyticsManager.AnalyticsEvents.Event_Kick_Special;

        AnalyticsManager.Instance().CommitData(kickDataEvent);
    }

    internal bool IsCurrentlyInTurn()
    {
        return m_currentlyInTurn;
    }
    void DarkerPlayer()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float valueColor = 175f / 255f;
        sprite.color = new Color(valueColor, valueColor, valueColor);
        sprite.sortingOrder = -1;
    }
}
