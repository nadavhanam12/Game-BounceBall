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
        Up = 1,
        Power = 2
    }

    #endregion
    #region serialized 
    [SerializeField] private float RegularKickSpeed = 2;
    [SerializeField] private float PowerKickSpeed = 2;
    [SerializeField] private float UpKickSpeed = 2;
    [SerializeField] private float AutoPlaySequenceSpeed = 2;
    [SerializeField] private SpriteRenderer Body;
    [SerializeField] private GameObject m_hitZone;
    [SerializeField] private float m_hitZoneRadius = 2;
    [SerializeField] private float m_movingSpeed;


    #endregion
    #region private

    private bool m_initialized = false;
    private Animator m_anim;
    private bool m_inAnimation;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;
    private bool isGamePaused;
    private KickType m_curKickType;
    private int m_autoPlayDifficult = 100;
    private bool m_currentlyInTurn = true;

    private PlayerArgs m_args;
    private bool isRunning = false;
    private bool m_runRightFromTouch = false;
    private bool m_runLeftFromTouch = false;

    #endregion

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_hitZone.transform.position, m_hitZoneRadius);
    }


    public void Init(PlayerArgs args)
    {
        if (!m_initialized)
        {
            m_anim = gameObject.GetComponent<Animator>();
            //m_anim.enabled = false;
            m_inAnimation = false;

            m_initialRotation = gameObject.transform.localRotation;
            m_initialPosition = gameObject.transform.localPosition;
            m_initialScale = gameObject.transform.localScale;

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
        EventManager.AddHandler(EVENT.EventOnRightPressed, ToogleRunRightFromTouch);
        EventManager.AddHandler(EVENT.EventOnRightReleased, ToogleRunRightFromTouch);
        EventManager.AddHandler(EVENT.EventOnLeftPressed, ToogleRunLeftFromTouch);
        EventManager.AddHandler(EVENT.EventOnLeftReleased, ToogleRunLeftFromTouch);

    }
    void RemoveListeners()
    {
        EventManager.RemoveHandler(EVENT.EventOnRightPressed, ToogleRunRightFromTouch);
        EventManager.RemoveHandler(EVENT.EventOnRightReleased, ToogleRunRightFromTouch);
        EventManager.RemoveHandler(EVENT.EventOnLeftPressed, ToogleRunLeftFromTouch);
        EventManager.RemoveHandler(EVENT.EventOnLeftReleased, ToogleRunLeftFromTouch);

    }

    private void ToogleRunLeftFromTouch()
    {
        m_runLeftFromTouch = !m_runLeftFromTouch;
    }
    private void ToogleRunRightFromTouch()
    {
        m_runRightFromTouch = !m_runRightFromTouch;
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
            }
            else
            {
                AutoPlay();
            }
            //}
        }

    }

    void GetPlayerKickFromKeyboard()
    {
        if (m_args.PlayerIndex == PlayerIndex.First)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                OnKickPlay(KickType.Up);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                OnKickPlay(KickType.Regular);
            }
            else if (Input.GetKeyDown(KeyCode.D))
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
            else if (isRunning)
            {
                //m_anim.SetBool("Running", true);
                isRunning = false;
                m_anim.SetTrigger("Idle Triger");
            }
        }
    }


    public void OnMoveX(Vector3 direction)
    {
        if (CheckPlayerInBounds(direction))
        {
            if (!isRunning)
            {
                isRunning = true;
                m_anim.SetTrigger("Running Triger");
            }
            SpinPlayerToDirection(direction);
            transform.Translate(direction * m_movingSpeed, Space.World);
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




    public void PlayIdle()
    {
        if (!m_inAnimation)
        {
            m_anim.speed = 1;
            m_anim.Play("Idle", -1, 0f);
        }

    }
    void AutoPlay()
    {
        if (m_currentlyInTurn)
        {
            int rnd = Random.Range(0, 100);
            KickType kickType;
            if (rnd <= m_autoPlayDifficult)
            {
                //print("AUTOPLAYER PLAY");
                if (BallInHitZone())//check lower hit bounds
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
        m_anim.speed = 1;
        m_anim.Play("Win", -1, 0f);

    }

    public void Lose()
    {
        m_anim.speed = 1;
        m_anim.Play("Lose", -1, 0f);

    }



    public void FinishAnimation()
    {
        //print("finishAnimation");
        m_anim.speed = 1;

        gameObject.transform.localRotation = m_initialRotation;
        gameObject.transform.localPosition = m_initialPosition;
        //gameObject.transform.localScale = m_initialScale;

        m_inAnimation = false;
        isRunning = false;
        PlayIdle();
    }


    public void ReachHitPosition()
    {
        if (m_currentlyInTurn)
        {
            if (BallInHitZone())
            {
                float distanceX = m_args.Ball.transform.position.x - m_hitZone.transform.position.x;
                //print(distanceX);
                m_args.Ball.OnHitPlay(m_curKickType, distanceX);

            }

        }

    }

    private bool BallInHitZone()
    {
        /*if (m_args.PlayerIndex == PlayerIndex.Second)
        {
            return true;
        }*/
        Vector3 ballPosition = m_args.Ball.transform.position;
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

        return distance <= m_hitZoneRadius;
    }


    public void OnKickPlay(KickType kickType)
    {
        if (!isGamePaused)
        {
            if (!m_inAnimation)
            {
                m_curKickType = kickType;
                string animName;
                float animSpeed;
                switch (kickType)
                {
                    case (KickType.Up):
                        animName = "KickUp";
                        animSpeed = UpKickSpeed;
                        break;

                    case (KickType.Power):
                        animName = "KickPower";
                        animSpeed = PowerKickSpeed;
                        break;

                    default:
                        animName = "KickRegular";
                        animSpeed = RegularKickSpeed;
                        break;
                }
                //print(animName);
                m_anim.speed = animSpeed;
                //m_anim.enabled = true;
                m_inAnimation = true;
                m_anim.Play(animName, -1, 0f);
                ReachHitPosition();

                //anim.enabled = false;
            }
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
    public void OnTouchKickUp()
    {
        OnKickPlay(KickType.Up);
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
        m_args.Ball.OnNewBallInScene();
    }
}
