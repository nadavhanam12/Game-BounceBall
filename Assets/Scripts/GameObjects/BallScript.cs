using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;
// using System;
using static PlayerScript;

public class BallScript : MonoBehaviour
{
    #region events


    public delegate void onBallLost(PlayerIndex index);
    public onBallLost m_onBallLost;

    public delegate void onBallHit(PlayerIndex index, KickType kickType);
    public onBallHit m_onBallHit;

    #endregion

    #region const

    const float m_speedEmitTrail = 0;
    const int ParticlesToEmit = 100;
    const float m_startVelocityY = 0.2f;
    const float m_startVelocityX = 0.3f;
    const float m_fixedPositionX = -12f;
    const float m_maxVelocityY = 100;
    const float m_velocityMultiplier = 0.001f;

    #endregion

    #region serialized private
    [SerializeField] private float BallRegularHitAnimSpeed;

    [SerializeField] private float m_gravity;
    [SerializeField] private float BallRegularHitPower;
    [SerializeField] private float BallSpecialHitPower;
    [SerializeField] private float m_ballReflectPower;

    #endregion


    #region private

    private bool m_initialized = false;

    private bool m_isBallInPlay;
    private Animator m_anim;
    private ParticleSystem m_particles;
    private bool m_inAnimation;
    private float m_curVelocity;
    private float curY;
    private float curX;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;

    private BallArgs m_args;

    private bool isGamePaused;

    private List<Sprite> m_ballSprites;

    private Sprite m_curBallSprite;
    private SpriteRenderer m_sprite;

    private TrailRenderer m_curBallTrail;
    private bool m_isTrailEmmiting = false;



    #endregion



    public void Init(BallArgs args)
    {
        if (!m_initialized)
        {

            //print("init ballscript");
            m_anim = gameObject.GetComponent<Animator>();
            m_particles = gameObject.GetComponentInChildren<ParticleSystem>();
            m_sprite = GetComponentInChildren<SpriteRenderer>();
            m_curBallTrail = GetComponentInChildren<TrailRenderer>();
            //m_rigidBody = GetComponent<Rigidbody>();

            this.gameObject.SetActive(false);
            //m_anim.enabled = false;
            m_inAnimation = false;

            m_initialRotation = gameObject.transform.localRotation;
            m_initialPosition = gameObject.transform.localPosition;
            m_initialScale = gameObject.transform.localScale;

            m_curVelocity = 0;
            m_args = args;


            m_ballSprites = args.BallTextures;

            SetBallSprite(0);
            m_curBallTrail.emitting = false;
            m_initialized = true;


            // this.gameObject.SetActive(true);
            // m_isBallInPlay = true;



            //OnNewBallInScene();
        }

    }

    public void OnNewBallInScene()
    {

        m_isBallInPlay = true;
        this.gameObject.transform.localPosition = m_initialPosition;
        m_curVelocity = m_startVelocityY;
        m_isBallInPlay = true;
        m_curBallTrail.emitting = false;
        //SetBallSprite(0);
        ApplyHitVisuals(false, false);

        this.gameObject.SetActive(true);

    }

    void FixedUpdate()
    {
        if (!isGamePaused)
        {
            if (m_isBallInPlay)
            {
                if (Math.Abs(m_curVelocity) < m_maxVelocityY)
                {
                    m_curVelocity -= m_gravity * m_velocityMultiplier;
                }

                curY = this.gameObject.transform.localPosition.y;
                curY += m_curVelocity;
                curX = this.gameObject.transform.localPosition.x;

                if (curX > m_fixedPositionX)
                {
                    curX -= m_startVelocityX;
                }
                this.gameObject.transform.localPosition = new Vector3(curX, curY, m_initialPosition.z);

                CheckBounds();
                CheckEmitTrail();
            }
        }
    }

    void CheckEmitTrail()
    {
        //print(m_curVelocity);
        bool inSpeedToEmit = Math.Abs(m_curVelocity) >= m_speedEmitTrail;
        if ((inSpeedToEmit) && (!m_isTrailEmmiting))
        {
            m_isTrailEmmiting = true;
            m_curBallTrail.emitting = m_isTrailEmmiting;
        }
        else if ((!inSpeedToEmit) && (m_isTrailEmmiting))
        {
            m_isTrailEmmiting = false;
            m_curBallTrail.emitting = m_isTrailEmmiting;
        }
    }

    private void CheckBounds()
    {
        if (this.gameObject.transform.position.y < m_args.GameLowerBound)
        {
            m_isBallInPlay = false;
            this.gameObject.SetActive(false);
            m_onBallLost(m_args.BallIndex);

        }
        else if (this.gameObject.transform.position.y > m_args.GameUpperBound)
        {
            Vector3 curPos = this.gameObject.transform.localPosition;
            curPos.y -= 0.1f;
            this.gameObject.transform.localPosition = curPos;
            //this.gameObject.transform.localPosition = m_initialPosition;
            m_curVelocity = m_curVelocity * (-1f) * m_ballReflectPower;
        }

    }


    public void FinishAnimation()
    {
        /*
        gameObject.transform.localRotation = m_initialRotation;
        gameObject.transform.localPosition = m_initialPosition;
        gameObject.transform.localScale = m_initialScale;
        */

        m_inAnimation = false;

    }


    public void OnHitPlay(KickType kickType)
    {

        bool isSpecial = true;
        bool isUpperHit = kickType == KickType.Up;
        if (BallInHitBounds(isUpperHit))
        {
            if (kickType == KickType.Regular)
            {
                isSpecial = false;
            }

            ApplyHitPhysics(isSpecial);
            ApplyHitVisuals(isSpecial, true);

            m_onBallHit(m_args.BallIndex, kickType);

        }
    }


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        this.gameObject.SetActive(!isGamePaused);

    }

    private void ApplyHitPhysics(bool isSpecial)
    {
        float kickPower = isSpecial ? BallSpecialHitPower : BallRegularHitPower;

        if (isSpecial)
        {
            m_curVelocity = kickPower * 0.1f;
        }
        else
        {
            if (m_curVelocity <= 0)
            {
                m_curVelocity = kickPower * 0.1f;
            }
            else
            {
                m_curVelocity = kickPower * 0.1f;
            }
        }

    }

    private void ApplyHitVisuals(bool isSpecial, bool withParticles)
    {
        if (this.isActiveAndEnabled)
        {
            string animName;
            float animSpeed;

            animName = ChooseRegularHitAnimation();
            animSpeed = BallRegularHitAnimSpeed;
            m_anim.speed = animSpeed;
            //m_anim.enabled = true;
            m_inAnimation = true;
            //print("animName: " + animName);
            m_anim.Play(animName, -1, 0f);

            if (withParticles)
            {
                m_particles.Emit(ParticlesToEmit);
                //m_particles.Emit(25);
            }
        }



    }

    public bool BallInHitBounds(bool isUpperHit)
    {
        float y = this.gameObject.transform.position.y;
        float x = this.gameObject.transform.position.x;
        bool UnderUpperBound;
        bool AboveLowerBound;
        bool InXBound = true;
        if (isUpperHit)
        {
            UnderUpperBound = (y <= m_args.HitUpperBound2);
            AboveLowerBound = (y >= m_args.HitUpperBound);
        }
        else
        {
            UnderUpperBound = (y <= m_args.HitUpperBound);
            AboveLowerBound = (y >= m_args.HitLowerBound);
        }
        //InXBound = x <= m_fixedPositionX + 2;

        return (UnderUpperBound && AboveLowerBound && InXBound);

    }


    private string ChooseRegularHitAnimation()
    {
        /*
        int randInt = Random.Range(1, 4);

        switch (randInt)
        {
            case (1):
                return "BallRegularHit1";

            case (2):
                return "BallRegularHit2";

            case (3):
                return "BallRegularHit3";
            default:
                return "BallRegularHit1";

        }
        */
        return "BallRegularHit1";
    }


    public void SetBallSprite(int indexSprite)
    {
        //print(indexSprite);
        if (indexSprite != 4)
        {
            if (m_curBallSprite != m_ballSprites[indexSprite])
            {
                m_curBallSprite = m_ballSprites[indexSprite];
                m_sprite.sprite = m_curBallSprite;
                var textureSheet = m_particles.textureSheetAnimation;
                textureSheet.SetSprite(0, m_curBallSprite);


            }
        }

    }

}
