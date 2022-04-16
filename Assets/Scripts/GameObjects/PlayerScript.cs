using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;

public class PlayerScript : MonoBehaviour
{

    #region events

    #endregion
    #region enums
    public enum KickType
    {
        Regular = 0,
        Special = 1,
    }

    #endregion
    #region serialized 
    [SerializeField] private SpriteRenderer Body;
    [SerializeField] private GameObject m_hitZone;

    #endregion
    #region private

    private bool m_initialized = false;
    private Animator m_anim;
    private bool m_inParalyze;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;
    private bool isGamePaused = true;
    private KickType m_curKickType;
    private bool m_currentlyInTurn = true;

    private PlayerArgs m_args;
    private bool isRunning = false;
    private bool isJumping = false;
    private bool isJumpingUp = false;
    private bool isJumpingDown = false;
    private bool m_runRightFromTouch = false;
    private bool m_runLeftFromTouch = false;

    private bool m_inKickCooldown = false;
    private bool m_onWinLoseAnim = false;
    bool m_onSlide = false;
    private float m_halfFieldDistance;


    #endregion

    /*void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_hitZone.transform.position, m_hitZoneRadius);
    }*/
    void Awake()
    {
        //this.gameObject.SetActive(false);
    }

    public void Init(PlayerArgs args)
    {
        if (!m_initialized)
        {
            m_args = args;
            m_anim = gameObject.GetComponent<Animator>();
            //m_anim.enabled = false;
            m_inParalyze = false;

            m_initialRotation = gameObject.transform.rotation;
            m_initialPosition = gameObject.transform.position;
            m_initialScale = gameObject.transform.localScale;

            m_hitZone.gameObject.SetActive(false);


            m_halfFieldDistance = m_args.Bounds.GameRightBound - m_args.Bounds.GameRightBound;
            SetPlayerIndexSettings();

            m_anim.speed = 1;

            InitListeners();

            m_initialized = true;
            InitPlayer();
            this.gameObject.SetActive(true);

        }

    }
    void OnDestroy()
    {
        RemoveListeners();
    }

    void InitListeners()
    {
        EventManager.AddHandler(EVENT.EventOnRightPressed, ToogleRunRightOn);
        EventManager.AddHandler(EVENT.EventOnRightReleased, ToogleRunRightOff);
        EventManager.AddHandler(EVENT.EventOnLeftPressed, ToogleRunLeftOn);
        EventManager.AddHandler(EVENT.EventOnLeftReleased, ToogleRunLeftOff);

    }
    void RemoveListeners()
    {
        EventManager.RemoveHandler(EVENT.EventOnRightPressed, ToogleRunRightOn);
        EventManager.RemoveHandler(EVENT.EventOnRightReleased, ToogleRunRightOff);
        EventManager.RemoveHandler(EVENT.EventOnLeftPressed, ToogleRunLeftOn);
        EventManager.RemoveHandler(EVENT.EventOnLeftReleased, ToogleRunLeftOff);

    }

    private void ToogleRunLeftOn()
    {
        m_runLeftFromTouch = true;
    }
    private void ToogleRunLeftOff()
    {
        m_runLeftFromTouch = false;
    }
    private void ToogleRunRightOn()
    {
        m_runRightFromTouch = true;
    }
    private void ToogleRunRightOff()
    {
        m_runRightFromTouch = false;
    }

    void UpdateColor()
    {
        //update player colors
        Body.color = m_args.Color;

    }


    private void SetPlayerIndexSettings()
    {
        UpdateColor();
        //set player index specific settings


    }

    // Update is called once per frame
    void Update()
    {
        if ((!isGamePaused))
        {
            if (!m_inParalyze)
            {
                if (!m_args.AutoPlay)
                {
                    GetPlayerKickFromKeyboard();
                    GetPlayerMovement();
                    GetJump();
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

    void GetPlayerKickFromKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnJump();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            OnKickPlay(KickType.Regular);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnKickPlay(KickType.Special);
        }

    }
    void GetPlayerMovement()
    {
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.LeftArrow) || m_runLeftFromTouch)
        {
            OnMoveX(Vector3.left);
        }
        else if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.RightArrow) || m_runRightFromTouch)
        {
            OnMoveX(Vector3.right);
        }
        else if (!m_onWinLoseAnim)
        {
            //print("Idle Trigger");
            AnimSetTrigger("Idle Trigger");
        }
    }



    public void OnMoveX(Vector3 direction)
    {
        if (CheckPlayerInBounds(direction))
        {
            AnimSetTrigger("Running Trigger");
            SpinPlayerToDirection(direction);
            transform.Translate(direction * m_args.playerStats.m_movingSpeed, Space.World);
        }
    }

    void AnimSetTrigger(string triggerName)
    {
        //if (!m_inAnimation)
        if (true)
        {

            m_anim.SetTrigger(triggerName);
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
        bool inBounds;
        Vector3 playerPos = transform.position;
        if (direction.x <= 0)//he is moving left
        {
            //inBounds = transform.position.x - 2 > m_args.Bounds.GameLeftBound;
            if (playerPos.x - 2 < m_args.Bounds.GameLeftBound)
            {
                playerPos.x = m_args.Bounds.GameRightBound;
                transform.position = playerPos;
            }
        }
        else //he is moving right
        {
            //inBounds = playerPos.x + 2 < m_args.Bounds.GameRightBound;
            if (playerPos.x + 2 > m_args.Bounds.GameRightBound)
            {
                playerPos.x = m_args.Bounds.GameLeftBound;
                transform.position = playerPos;
            }

        }

        //print("leftFromRightBounds: " + leftFromRightBounds);
        //print("rightFromLeftBounds: " + rightFromLeftBounds);
        //print("transform.position.x: " + transform.position.x);
        //print("m_args.Bounds.GameLeftBound: " + m_args.Bounds.GameLeftBound);


        //return inBounds;
        return true;
    }

    void AutoPlayGeneral()
    {
        if (m_currentlyInTurn)
        {
            m_anim.enabled = true;
            AutoPlayKick();
            AutoPlayMovement();
        }
        else
        {
            //shit not on turn
            m_anim.enabled = false;
        }




    }

    void AutoPlayKick()
    {
        if (!m_inKickCooldown)
        {
            int rnd = Random.Range(0, 100);
            if (rnd <= m_args.playerStats.m_autoPlayDifficult)
            {
                m_args.BallsManager.ApplyKick(m_args.PlayerIndex, m_curKickType, m_hitZone.transform.position, m_args.playerStats.m_hitZoneRadius);
                StartCoroutine(KickCooldown());
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

    void AutoPlayMovement()
    {
        Vector3 ballsTransform = m_args.BallsManager.GetCorrectBallPosition();
        Vector3 playerTransform = gameObject.transform.position;
        float deltaX = ballsTransform.x - playerTransform.x;
        if (Mathf.Abs(deltaX) > m_args.playerStats.AutoPlayBallDistance)
        {
            if (deltaX > 0)
            {
                if (Mathf.Abs(deltaX) < m_halfFieldDistance)//if the distance is less then half field then its better to move the other side
                {
                    OnMoveX(Vector3.right);
                }
                else
                {
                    OnMoveX(Vector3.left);
                }

            }
            else
            {
                if (Mathf.Abs(deltaX) <= m_halfFieldDistance)//if the distance is less then half field then its better to move the other side
                {
                    OnMoveX(Vector3.left);
                }
                else
                {
                    OnMoveX(Vector3.right);
                }
            }
        }

    }


    public void Win()
    {
        /* m_onWinLoseAnim = true;
         m_anim.enabled = true;
         m_anim.ResetAllAnimatorTriggers();

         m_anim.Play("Win", -1, 0f);*/
    }

    public void Lose()
    {
        /*m_onWinLoseAnim = true;
        m_anim.enabled = true;
        //m_anim.Play("Lose", -1, 0f);
        m_anim.ResetAllAnimatorTriggers();
        AnimSetTrigger("Lose Trigger");*/
    }



    public void FinishAnimation()
    {
        //print("finishAnimation");

        //gameObject.transform.localRotation = m_initialRotation;
        //gameObject.transform.localPosition = m_initialPosition;
        //gameObject.transform.localScale = m_initialScale;
        //m_anim.SetTrigger("Idle Trigger");
        m_inParalyze = false;
        if (m_onSlide)
        {
            ToggleSlide(false);
        }
    }

    public void InitPlayer()
    {
        gameObject.transform.rotation = m_initialRotation;
        Vector3 positionUpper = m_initialPosition;
        positionUpper.y += 8f;
        gameObject.transform.position = positionUpper;
        gameObject.transform.localScale = m_initialScale;

        //m_anim.SetTrigger("Idle Trigger");
        FinishAnimation();
        m_onWinLoseAnim = false;
        isJumping = true;
        isJumpingDown = true;

    }


    public void ReachHitPosition()
    {
        //print("ReachHitPosition");
        if (m_currentlyInTurn)
        {
            m_args.BallsManager.ApplyKick(m_args.PlayerIndex, m_curKickType, m_hitZone.transform.position, m_args.playerStats.m_hitZoneRadius);
        }

    }







    public void OnKickPlay(KickType kickType)
    {
        if (!isGamePaused)
        {
            //if (!m_inAnimation)
            if ((!m_inKickCooldown) && (!m_inParalyze))
            {
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
                //print(animName);
                //AnimSetTrigger(triggerName);
                m_anim.ResetTrigger(triggerName);
                m_anim.SetTrigger(triggerName);
                //ReachHitPosition();
                StartCoroutine(KickCooldown());


                //anim.enabled = false;
            }
        }
    }

    void ToggleSlide(bool shouldSlide)
    {
        m_onSlide = shouldSlide;
        if (m_onSlide)
        {
            Vector3 dir = gameObject.transform.localScale.x == 1 ? Vector3.right : Vector3.left;
            dir *= m_args.playerStats.SlideSpeed;
            StartCoroutine(Slide(dir));
        }
        else
        {
            print("stop slide");
            StopCoroutine(Slide(Vector3.zero));
        }
    }

    private void OnJump()
    {
        //print("jump");
        if (!isJumping)
        {
            isJumping = true;
            isJumpingUp = true;
        }
    }


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        m_anim.enabled = !isPause;
    }

    public void OnTouchKickRegular()
    {
        OnKickPlay(KickType.Regular);
    }
    public void OnTouchJump()
    {
        OnJump();
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
        {
            m_args.BallsManager.OnNewBallInScene(m_args.PlayerIndex, m_args.PlayerIndex);
        }

    }
    public void ShowPlayer(bool toShow)
    {
        gameObject.SetActive(toShow);
    }

    IEnumerator Slide(Vector3 slideDirection)
    {
        while (m_onSlide)
        {
            OnMoveX(slideDirection);
            yield return new WaitForSeconds(.01f);
        }
    }


}
