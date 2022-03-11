using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;

public class PlayerScript : MonoBehaviour
{
    private PlayerIndex m_playerIndex;

    private Color m_playerColor;
    private bool m_initialized = false;

    private Animator m_anim;
    private bool m_inAnimation;

    private BallScript m_Ball;

    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;

    private Vector3 m_initialScale;

    private bool isGamePaused;


    [SerializeField] private float RegularKickSpeed = 2;
    [SerializeField] private float PowerKickSpeed = 2;
    [SerializeField] private float UpKickSpeed = 2;
    [SerializeField] private float AutoPlaySequenceSpeed = 2;


    [SerializeField] private SpriteRenderer Body;

    //private float m_touchSensetivity = 15f;


    private Touch theTouch;
    private Vector2 touchStartPosition, touchEndPosition;

    private bool m_autoPlay = false;


    public enum KickType
    {
        Regular = 0,
        Up = 1,
        Power = 2
    }
    private KickType m_curKickType;

    private int m_autoPlayDifficult = 100;

    private bool m_currentlyInTurn = true;


    // Start is called before the first frame update

    // bool AnimatorIsPlaying()
    // {
    //     return m_anim.GetCurrentAnimatorStateInfo(0).length >
    //            m_anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    // }



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

            m_Ball = args.Ball;
            m_playerIndex = args.PlayerIndex;
            m_playerColor = args.Color;
            SetPlayerIndexSettings();

            m_anim.speed = 1;

            m_autoPlay = args.AutoPlay;
            m_initialized = true;
            this.gameObject.SetActive(true);

        }

    }

    void UpdateColor()
    {
        //update player colors
        Body.color = m_playerColor;

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
            if (!m_inAnimation)
            {
                if (!m_autoPlay)
                {
                    GetPlayerInputFromKeyboard();
                    //GetPlayerInputFromTouch();
                }
                else
                {
                    AutoPlay();
                }
            }
        }

    }

    void GetPlayerInputFromKeyboard()
    {
        if (m_playerIndex == PlayerIndex.First)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                OnKickPlay(KickType.Up);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                OnKickPlay(KickType.Regular);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                OnKickPlay(KickType.Power);
            }
            else
            {
                //print("Idle PlayerIndex.First");
                //PlayIdle();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnKickPlay(KickType.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                OnKickPlay(KickType.Regular);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnKickPlay(KickType.Power);
            }
            else
            {
                //PlayIdle();

            }
        }

    }

    public void PlayIdle()
    {
        if (!m_inAnimation)
        {
            //print("Idle");
            m_anim.speed = 1;
            //m_anim.enabled = true;
            //m_inAnimation = true;
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
                if (m_Ball.BallInHitBounds(false))//check lower hit bounds
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
        gameObject.transform.localScale = m_initialScale;

        m_inAnimation = false;
        PlayIdle();
    }


    public void ReachHitPosition()
    {
        if (m_currentlyInTurn)
        {
            m_Ball.OnHitPlay(m_curKickType);

        }

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
        m_Ball.OnNewBallInScene();
    }
}
