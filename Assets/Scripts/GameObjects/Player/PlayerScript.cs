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
    [SerializeField] private SpriteRenderer playerSprite;

    private CircleCollider2D m_hitZone;
    [SerializeField] private GameObject m_pickableSpot;

    #endregion
    #region private

    private bool m_initialized = false;
    private bool m_inParalyze;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;
    private bool isGamePaused = true;
    private KickType m_curKickType;
    private bool m_currentlyInTurn = true;

    private PlayerArgs m_args;
    private bool isJumping = false;
    private bool isJumpingUp = false;
    private bool isJumpingDown = false;

    private bool m_inKickCooldown = false;
    private bool m_inInitCooldown = false;
    private bool m_onWinLoseAnim = false;
    bool m_onSlide = false;
    private float m_halfFieldDistance;
    private Bomb m_curBomb;

    #endregion


    PlayerAnimatorController m_playerAnimatorController;
    public void Init(PlayerArgs args)
    {
        if (!m_initialized)
        {
            m_args = args;
            InitScripts();

            m_hitZone = gameObject.GetComponent<CircleCollider2D>();
            m_hitZone.radius = m_args.playerStats.m_hitZoneRadius;
            m_inParalyze = false;

            m_initialRotation = gameObject.transform.rotation;
            m_initialPosition = gameObject.transform.position;
            m_initialScale = gameObject.transform.localScale;

            //m_hitZone.gameObject.SetActive(false);


            m_halfFieldDistance = (m_args.Bounds.GameRightBound - m_args.Bounds.GameLeftBound) / 2f;

            m_initialized = true;
            InitPlayer();
            if (m_args.PlayerIndex == PlayerIndex.Second && m_args.AutoPlay == false) //means we on one player mood, no need second player
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(true);
            }


        }

    }

    void InitScripts()
    {
        m_playerAnimatorController = GetComponent<PlayerAnimatorController>();
        m_playerAnimatorController.Init();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isGamePaused)
        {
            if (!m_inParalyze)
            {
                if (!m_args.AutoPlay)
                {
                    GetJump();
                    List<BallScript> ballsHit = CheckBallInHitZone();
                    if (ballsHit.Count > 0)
                        OnKickPlay(KickType.Regular);
                }
                else
                {
                    AutoPlayGeneral();
                    GetJump();
                }
            }
        }

    }

    private void GetJump()
    {
        if (isJumping)
        {

            if (isJumpingUp)
            {
                if (transform.position.y < m_args.playerStats.m_maxHeight)
                {//apply move up
                    //print("jump up");
                    /*transform.position.y += m_jumpPower;
                    transform.position = transform.position;*/
                    transform.Translate(Vector3.up * m_args.playerStats.m_jumpSpeed, Space.World);

                }
                else
                {
                    isJumpingUp = false;
                    isJumpingDown = true;
                }
            }
            else if (isJumpingDown)
            {
                if (transform.position.y > m_initialPosition.y)
                {//apply move down
                 // print("jump down");
                    transform.Translate(Vector3.down * m_args.playerStats.m_jumpSpeed, Space.World);
                    if (transform.position.y < m_initialPosition.y)
                    {
                        Vector3 curPos = transform.position;
                        curPos.y = m_initialPosition.y;
                        transform.position = curPos;
                    }
                }
                else
                {
                    isJumpingDown = false;
                }
            }
            else
            {//stop jumping
                //print("jump stop");
                isJumping = false;
            }
        }

    }




    public void OnMoveX(Vector2 direction, bool withAnimation = true)
    {
        if (isGamePaused)
            return;
        if (CheckPlayerInBounds(direction))
        {
            if (withAnimation)
                m_playerAnimatorController.RunAnimation();

            SpinPlayerToDirection(direction);
            transform.Translate(direction * m_args.playerStats.m_movingSpeed, Space.World);
        }
        else if (m_onSlide)
        {
            m_inParalyze = false;
            ToggleSlide(false);
        }
    }



    void SpinPlayerToDirection(Vector3 direction)
    {
        if ((direction == Vector3.left) && (transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

        }
        else if ((direction == Vector3.right) && (transform.localScale.x < 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private bool CheckPlayerInBounds(Vector3 direction)
    {
        Vector3 playerPos = transform.position;
        if (direction.x <= 0)//he is moving left
        {
            if (playerPos.x - m_args.playerStats.PlayerBoundDistanceTrigger < m_args.Bounds.GameLeftBound)
            {
                /*playerPos.x = m_args.Bounds.GameRightBound;
                transform.position = playerPos;*/
                return false;
            }
        }
        else //he is moving right
        {
            if (playerPos.x + m_args.playerStats.PlayerBoundDistanceTrigger > m_args.Bounds.GameRightBound)
            {
                /*playerPos.x = m_args.Bounds.GameLeftBound;
                transform.position = playerPos;*/
                return false;
            }

        }

        return true;
    }

    void AutoPlayGeneral()
    {
        if (m_currentlyInTurn)
        {
            AutoPlayKick();
            AutoPlayMovement();
        }
        else
        {
            //shit not on turn
        }
    }

    void AutoPlayKick()
    {
        if (!m_inKickCooldown)
        {
            int rnd = UnityEngine.Random.Range(0, 100);
            if (rnd <= m_args.playerStats.m_autoPlayDifficult)
            {
                List<BallScript> ballsHit = CheckBallInHitZone();
                if (ballsHit.Count > 0)
                {
                    OnKickPlay(KickType.Regular);
                }
            }
        }

    }

    IEnumerator KickCooldown()
    {
        //print("m_inKickCooldown = true");
        m_inKickCooldown = true;
        yield return new WaitForSeconds(m_args.playerStats.KickCooldown);
        m_inKickCooldown = false;
        //print("m_inKickCooldown = false");
    }
    IEnumerator InitCooldown()
    {
        //print("m_inKickCooldown = true");
        m_inInitCooldown = true;
        yield return new WaitForSeconds(m_args.playerStats.InitCooldown);
        m_inInitCooldown = false;
        //print("m_inKickCooldown = false");
    }

    void AutoPlayMovement()
    {
        Vector3 ballTransform = m_args.BallsManager.GetCorrectBallPosition();
        Vector3 playerTransform = gameObject.transform.position;
        float deltaX = ballTransform.x - playerTransform.x;
        if (Mathf.Abs(deltaX) > m_args.playerStats.AutoPlayBallDistance)
        {
            if (deltaX > 0)
            {
                OnMoveX(Vector3.right);
                /*if (Mathf.Abs(deltaX) < m_halfFieldDistance)//if the distance is less then half field then its better to move the other side
                {
                    OnMoveX(Vector3.right);
                }
                else
                {
                    OnMoveX(Vector3.left);
                }*/

            }
            else
            {
                OnMoveX(Vector3.left);
                /*if (Mathf.Abs(deltaX) <= m_halfFieldDistance)//if the distance is less then half field then its better to move the other side
                 {
                     OnMoveX(Vector3.left);
                 }
                 else
                 {
                     OnMoveX(Vector3.right);
                 }*/
            }
        }


    }


    public void Win()
    {
        m_onWinLoseAnim = true;
        m_playerAnimatorController.WinAnimation();
    }

    public void Lose()
    {
        m_onWinLoseAnim = true;
        m_playerAnimatorController.LoseAnimation();
    }



    public void FinishAnimation()
    {
        m_inParalyze = false;
        if (m_onSlide)
            ToggleSlide(false);
        OnPlayIdle();
    }

    public void InitPlayer(bool initPos = true)
    {
        //print("InitPlayer");
        if (initPos && !m_inInitCooldown)
        {
            gameObject.transform.rotation = m_initialRotation;
            Vector3 positionUpper = m_initialPosition;
            positionUpper.y += m_args.playerStats.m_startHeight;
            gameObject.transform.position = positionUpper;
            gameObject.transform.localScale = m_initialScale;

            isJumping = true;
            isJumpingDown = true;
            if (gameObject.activeInHierarchy)
                StartCoroutine(InitCooldown());
        }


        FinishAnimation();
        m_onWinLoseAnim = false;
        ShowPlayer();
    }


    public void ReachHitPosition()
    {
        //print("ReachHitPosition");
        if (m_currentlyInTurn && !m_inKickCooldown)
        {

            List<BallScript> ballsHit = CheckBallInHitZone();
            if (ballsHit.Count > 0)
            {
                //print("ballsHit.Count " + ballsHit.Count);
                if (m_args.BallsManager.ContainsCorrectBall(ballsHit))
                    StartCoroutine(KickCooldown());
                m_args.BallsManager.ApplyKick(m_args.PlayerIndex, m_curKickType, ballsHit);

            }
        }
        //else
        //print("in kick cool down");


    }

    private List<BallScript> CheckBallInHitZone()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(m_hitZone.bounds.center, m_hitZone.radius);
        List<BallScript> ballsHit = new List<BallScript>();
        BallScript curBallScript;
        foreach (Collider2D hitCollider in hitColliders)
        {
            //print("hitCollider: " + hitCollider.name);
            curBallScript = hitCollider.GetComponent<BallScript>();
            if (curBallScript != null && !curBallScript.BallHasFallen)
                ballsHit.Add(curBallScript);

        }
        return ballsHit;
    }







    public void OnKickPlay(KickType kickType)
    {
        if ((!isGamePaused) && (!m_inParalyze))
        {
            if (!m_args.AutoPlay && kickType == KickType.Special)
            {
                AnalyticsManager.AnalyticsEvents kickDataEvent = kickType ==
                KickType.Regular ?
                AnalyticsManager.AnalyticsEvents.Event_Kick_Regular :
                AnalyticsManager.AnalyticsEvents.Event_Kick_Special;

                AnalyticsManager.Instance().CommitData(kickDataEvent);
            }


            //if ((!m_args.AutoPlay) || (!m_inKickCooldown))

            m_curKickType = kickType;
            string triggerName;
            switch (kickType)
            {
                case (KickType.Special):
                    triggerName = "KickSpecial Trigger";
                    m_inParalyze = true;
                    ToggleSlide(true);
                    break;

                default:
                    triggerName = "KickReg Trigger";
                    ReachHitPosition();
                    break;
            }
            if (!isJumping)
                m_playerAnimatorController.SetAndResetTriger(triggerName);
        }
    }

    void ToggleSlide(bool shouldSlide)
    {
        m_onSlide = shouldSlide;
        if (m_onSlide)
        {
            Vector3 dir = gameObject.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            dir *= m_args.playerStats.SlideSpeed;
            StartCoroutine(Slide(dir));
        }
        else
        {
            //print("stop slide");
            StopCoroutine(Slide(Vector3.zero));
        }
    }

    private void OnJump()
    {

        //print("jump");
        if (m_curBomb != null)
        {
            ActivateBomb();
            return;
        }
        if (!isJumping)
        {
            if (!m_args.AutoPlay)
                AnalyticsManager.Instance().CommitData(AnalyticsManager.AnalyticsEvents.Event_Jump);
            isJumping = true;
            isJumpingUp = true;

            m_playerAnimatorController.AnimSetTrigger("Jump Trigger");
        }
    }




    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        m_onSlide = false;
        m_playerAnimatorController.SetGamePause(isPause);
    }

    public void OnTouchKickRegular()
    {
        //OnKickPlay(KickType.Regular);
    }
    public void MovePlayerToPosition(Vector2 position)
    {
        Vector2 playerPosition = transform.position;
        //print(playerPosition + "   " + position);
        if ((Math.Abs(playerPosition.x - position.x) > 0.2f) && (!m_onSlide))
            if (playerPosition.x < position.x)
                OnMoveX(Vector2.right);
            else
                OnMoveX(Vector2.left);
        else
            OnPlayIdle();
    }

    public void OnTouchJump()
    {
        OnJump();
    }

    public void OnPlayIdle()
    {
        if (!m_inParalyze && !m_onSlide)
            m_playerAnimatorController.PlayIdle();
    }
    public void OnTouchKickSpecial()
    {
        OnKickPlay(KickType.Special);
    }

    public void LostTurn()
    {
        m_currentlyInTurn = false;
    }
    public void StartTurn(bool throwNewBall = true)
    {
        m_currentlyInTurn = true;
        if (throwNewBall)
            m_args.BallsManager.OnNewBallInScene();

        if (m_args.AutoPlay)
            StartCoroutine(KickCooldown());

    }
    public void ShowPlayer()
    {
        playerSprite.gameObject.SetActive(true);
    }
    public void HidePlayer()
    {
        playerSprite.gameObject.SetActive(false);
    }

    IEnumerator Slide(Vector3 slideDirection)
    {
        while (m_onSlide)
        {
            OnMoveX(slideDirection, false);
            yield return new WaitForSeconds(.01f);
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        //print("OnTriggerEnter2D");
        /*BallScript ball = collider.GetComponent<BallScript>();
        if (ball)
            print("OnTriggerEnter2D " + ball.GetIndex());*/
        Bomb curCollider = collider.GetComponent<Bomb>();
        if (curCollider != null)
        {
            Status bombStatus = curCollider.GetStatus();
            if ((bombStatus == Status.FreeInScene) || (bombStatus == Status.ReachedLowerBound))
            {
                if (m_curBomb == null)
                {
                    m_curBomb = curCollider;
                    m_curBomb.gameObject.transform.parent = m_pickableSpot.transform;
                    m_curBomb.PickUp();
                    if (m_args.ToggleBombUI != null)
                        m_args.ToggleBombUI(true);
                }

            }
            /* else if (bombStatus == Status.Activated)
             {
                 curCollider.Explode();
                 PlayerHitByBomb();
             }*/


        }
    }

    public void PlayerHitByBomb()
    {
        //print("PlayerHitByBomb");
        AnalyticsManager.Instance().CommitData(
          AnalyticsManager.AnalyticsEvents.Event_Player_Hit_By_Bomb,
          new Dictionary<string, object> { { "PlayerIndex", m_args.PlayerIndex } });
        ToggleSlide(false);
        m_inParalyze = true;
        m_playerAnimatorController.AnimSetTrigger("Die Trigger");
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

    private void ActivateBomb()
    {
        if (m_curBomb != null)
        {
            AnalyticsManager.Instance().CommitData(
  AnalyticsManager.AnalyticsEvents.Event_Bomb_Throw,
  new Dictionary<string, object> {
                 { "PlayerIndex", m_args.PlayerIndex }

});
            //print("Player ActivateBomb");
            bool throwRight = transform.localScale.x > 0;
            m_curBomb.SetActivateDirections(throwRight);
            m_curBomb.Activate();
            m_curBomb = null;
            if (m_args.ToggleBombUI != null)
                m_args.ToggleBombUI(false);
        }
    }

    public bool IsOnSlide()
    {
        return m_onSlide;
    }

    public bool IsOnJumpKick()
    {
        return isJumping;
    }
}
