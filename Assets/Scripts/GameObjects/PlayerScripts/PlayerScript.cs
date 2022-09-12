using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bomb;
using static GameManagerScript;

public class PlayerScript : MonoBehaviour
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
    private bool m_onWinLoseAnim = false;
    private float m_halfFieldDistance;

    protected PlayerAnimations m_playerAnimations;
    protected PlayerMovement m_playerMovement;
    protected PlayerKick m_playerKicksManager;
    protected PlayerBomb m_playerBombsManager;




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
            InitPlayer();
        }
    }


    void InitScripts()
    {
        m_playerAnimations = GetComponent<PlayerAnimations>();
        m_playerAnimations.Init();

        m_playerMovement = GetComponent<PlayerMovement>();
        m_playerMovement.Init(this, m_args);

        m_playerKicksManager = GetComponent<PlayerKick>();
        m_playerKicksManager.Init(this, m_args);

        m_playerBombsManager = GetComponent<PlayerBomb>();
        m_playerBombsManager?.Init(m_args);

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isGamePaused)
        {
            if (!m_playerMovement.InParalyze)
            {
                m_playerMovement.GetJump();
                List<BallScript> ballsHit = m_playerKicksManager.CheckBallInHitZone();
                if (ballsHit.Count > 0)
                    OnKickPlay(KickType.Regular);
            }
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
        m_onWinLoseAnim = true;
        m_playerAnimations.WinAnim();
    }

    public void Lose()
    {
        m_onWinLoseAnim = true;
        m_playerAnimations.LoseAnim();
    }


    public void FinishAnimation()
    {
        m_playerMovement.SetInParalyze(false);
        m_playerMovement.ToggleSlide(false);
        OnPlayIdle();
    }

    public void InitPlayer(bool initPos = true)
    {
        if (initPos && !m_inInitCooldown)
        {
            m_playerMovement.InitPlayer();
            if (gameObject.activeInHierarchy)
                StartCoroutine(InitCooldown());
        }
        FinishAnimation();
        m_onWinLoseAnim = false;
        ShowPlayer();
    }

    public void OnKickPlay(KickType kickType)
    {
        if ((!isGamePaused) && (!m_playerMovement.InParalyze))
        {
            if (kickType == KickType.Special)
                SendKickEventData(kickType);

            string triggerName;
            switch (kickType)
            {
                case (KickType.Special):
                    triggerName = "KickSpecial Trigger";
                    m_playerMovement.SetInParalyze(true);
                    m_playerMovement.ToggleSlide(true);
                    break;

                default:
                    triggerName = "KickReg Trigger";
                    m_playerKicksManager.ReachHitPosition(kickType);
                    break;
            }
            if (!m_playerMovement.IsJumping)
                m_playerAnimations.AnimSetAndResetTrigger(triggerName);

            //anim.enabled = false;

        }
    }



    private void OnJump()
    {
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




    public void SetGamePause(bool isPause)
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
        if (!m_playerMovement.InParalyze && !m_playerMovement.InSlide)
            m_playerAnimations.OnPlayIdle();

    }
    public void OnTouchKickSpecial()
    {
        OnKickPlay(KickType.Special);
    }

    public void LostTurn()
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
        playerSprite.gameObject.SetActive(true);
    }
    public void HidePlayer()
    {
        playerSprite.gameObject.SetActive(false);
    }



    public void PlayerHitByBomb()
    {
        AnalyticsManager.Instance().CommitData(
          AnalyticsManager.AnalyticsEvents.Event_Player_Hit_By_Bomb,
          new Dictionary<string, object> {
                 { "PlayerIndex", m_args.PlayerIndex }

        });
        m_playerMovement.ToggleSlide(false);
        m_playerMovement.SetInParalyze(true);
        m_playerAnimations.AnimSetTrigger("Die Trigger");
    }

    public void Revive()
    {
        //print("Revive");
        Invoke("ReviveAfterWait", 0.3f);
    }

    private void ReviveAfterWait()
    {
        print("ReviveAfterWait");
        InitPlayer(true);
    }



    public bool IsOnSlide()
    {
        return m_playerMovement.InSlide;
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

    protected virtual void SendKickEventData(KickType kickType)
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
}
