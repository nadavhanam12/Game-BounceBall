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
        Power = 1,
    }

    #endregion
    #region serialized 
    [SerializeField] private SpriteRenderer Body;
    [SerializeField] private GameObject m_hitZone;

    #endregion
    #region private

    private bool m_initialized = false;
    private Animator m_anim;
    private bool m_inAnimation;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;
    private bool isGamePaused = true;
    private KickType m_curKickType;
    private int m_autoPlayDifficult = 100;
    private bool m_currentlyInTurn = true;

    private PlayerArgs m_args;
    private bool isRunning = false;
    private bool isJumping = false;
    private bool isJumpingUp = false;
    private bool isJumpingDown = false;
    private bool m_runRightFromTouch = false;
    private bool m_runLeftFromTouch = false;



    #endregion

    /*void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_hitZone.transform.position, m_hitZoneRadius);
    }*/
    void Awake()
    {
        this.gameObject.SetActive(false);
    }

    public void Init(PlayerArgs args)
    {
        if (!m_initialized)
        {
            m_anim = gameObject.GetComponent<Animator>();
            //m_anim.enabled = false;
            m_inAnimation = false;

            m_initialRotation = gameObject.transform.rotation;
            m_initialPosition = gameObject.transform.position;
            m_initialScale = gameObject.transform.lossyScale;

            m_hitZone.gameObject.SetActive(false);

            m_args = args;
            SetPlayerIndexSettings();

            m_anim.speed = 1;

            InitListeners();

            m_initialized = true;
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
            //if (!m_inAnimation)
            //{
            if (!m_args.AutoPlay)
            {
                GetPlayerKickFromKeyboard();
                GetPlayerMovement();
                GetJump();
            }
            else
            {
                AutoPlay();
            }
            //}
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
        if (m_args.PlayerIndex == PlayerIndex.First)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                OnJump();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                OnKickPlay(KickType.Regular);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                OnKickPlay(KickType.Power);
            }
        }

    }
    void GetPlayerMovement()
    {
        if (m_args.PlayerIndex == PlayerIndex.First)
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.LeftArrow) || m_runLeftFromTouch)
            {
                OnMoveX(Vector3.left);
            }
            else if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.RightArrow) || m_runRightFromTouch)
            {
                OnMoveX(Vector3.right);
            }
            else
            {
                //m_anim.SetBool("Running", true);
                //print("Idle Trigger");
                AnimSetTrigger("Idle Trigger");
            }
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
            //m_anim.ResetTrigger(triggerName);
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
        if (direction == Vector3.left)
        {
            inBounds = transform.position.x - 2 > m_args.Bounds.GameLeftBound;
        }
        else
        {
            inBounds = transform.position.x + 2 < m_args.Bounds.GameRightBound;
        }
        //print("leftFromRightBounds: " + leftFromRightBounds);
        //print("rightFromLeftBounds: " + rightFromLeftBounds);
        //print("transform.position.x: " + transform.position.x);
        //print("m_args.Bounds.GameLeftBound: " + m_args.Bounds.GameLeftBound);

        return inBounds;
    }

    void AutoPlay()
    {
        if (m_currentlyInTurn)
        {
            int rnd = Random.Range(0, 100);
            KickType kickType;
            if (rnd <= m_autoPlayDifficult)
            {
                Vector3[] ballsPositions = m_args.BallsManager.GetBallsPosition(m_args.PlayerIndex);
                //print("AUTOPLAYER PLAY");
                if (BallInHitZone(ballsPositions[0]))//check lower hit bounds
                {
                    kickType = RandomKick();
                    OnKickPlay(kickType);

                }

                /*else if (m_Ball.BallInHitBounds(true))//check upper hit bounds
                {
                    kickType = RandomKick();
                    OnKickPlay(kickType);

                }*/

            }
        }
        else
        {
            //shit not on turn
        }




    }


    private KickType RandomKick()
    {
        int rndKick = Random.Range(0, 100);
        if (rndKick <= 33)
        { return KickType.Power; }
        else
        { return KickType.Regular; }

    }

    public void Win()
    {
        /*m_anim.speed = 1;
        m_anim.Play("Win", -1, 0f);*/

    }

    public void Lose()
    {
        /*m_anim.speed = 1;
        m_anim.Play("Lose", -1, 0f);*/

    }



    public void FinishAnimation()
    {
        /*print("finishAnimation");

        //gameObject.transform.localRotation = m_initialRotation;
        //gameObject.transform.localPosition = m_initialPosition;
        //gameObject.transform.localScale = m_initialScale;
        //m_anim.SetTrigger("Idle Trigger");
        m_inAnimation = false;
        isRunning = false;
        AnimSetTrigger("Idle Trigger");*/

    }


    public void ReachHitPosition()
    {
        if (m_currentlyInTurn)
        {
            int ballIndex = -1;
            Vector3[] ballsPositions = m_args.BallsManager.GetBallsPosition(m_args.PlayerIndex);
            if (BallInHitZone(ballsPositions[0]))
            {
                ballIndex = 0;
                float distanceX = ballsPositions[ballIndex].x - m_hitZone.transform.position.x;
                m_args.BallsManager.OnHitPlay(m_args.PlayerIndex, ballIndex, m_curKickType, distanceX);

            }
            else if (BallInHitZone(ballsPositions[1]))
            {
                ballIndex = 1;
                float distanceX = ballsPositions[ballIndex].x - m_hitZone.transform.position.x;
                m_args.BallsManager.OnHitPlay(m_args.PlayerIndex, ballIndex, m_curKickType, distanceX);

            }

        }

    }

    private bool BallInHitZone(Vector3 ballPosition)
    {
        /*if (m_args.PlayerIndex == PlayerIndex.Second)
        {
            return true;
        }*/
        Vector3 hitZoneCenter = m_hitZone.transform.position;
        ballPosition.z = 0;
        hitZoneCenter.z = 0;
        float distance = Vector3.Distance(hitZoneCenter, ballPosition);
        /*if (m_args.PlayerIndex == PlayerIndex.First)
        {
            print("distance: " + distance);
            print("m_hitZoneRadius: " + m_hitZoneRadius);
            print(distance <= m_hitZoneRadius);
        }*/

        return distance <= m_args.playerStats.m_hitZoneRadius;
    }




    public void OnKickPlay(KickType kickType)
    {
        if (!isGamePaused)
        {
            //if (!m_inAnimation)
            if (true)
            {
                m_inAnimation = true;

                m_curKickType = kickType;
                string triggerName;
                switch (kickType)
                {
                    case (KickType.Power):
                        triggerName = "KickReg Trigger";
                        break;

                    default:
                        triggerName = "KickPower Trigger";
                        break;
                }
                //print(animName);
                //AnimSetTrigger(triggerName);
                m_anim.ResetTrigger(triggerName);
                m_anim.SetTrigger(triggerName);
                ReachHitPosition();

                //anim.enabled = false;
            }
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
    }

    public void OnTouchKickRegular()
    {
        OnKickPlay(KickType.Regular);
    }
    public void OnTouchJump()
    {
        OnJump();
    }
    public void OnTouchKickPower()
    {
        OnKickPlay(KickType.Power);
    }

    public void LostTurn()
    {
        m_currentlyInTurn = false;
    }
    public void StartTurn()
    {
        m_currentlyInTurn = true;
        m_args.BallsManager.OnNewBallInScene(m_args.PlayerIndex);
    }


}
