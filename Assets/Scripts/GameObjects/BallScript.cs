using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;
// using System;
using static PlayerScript;
using static UnityEngine.ParticleSystem;

public class BallScript : MonoBehaviour
{
    #region events


    public delegate void onBallLost(PlayerIndex index, int ballIndex);
    public onBallLost m_onBallLost;

    #endregion

    #region const

    const float m_speedEmitTrail = 0;
    const int ParticlesToEmit = 500;
    const float m_startVelocityY = 0.2f;
    const float m_startVelocityX = -0.2f;
    const float m_maxVelocityY = 100;
    const float m_velocityMultiplier = 0.001f;
    const float m_hitMultiplierY = 0.1f;
    const float m_hitMultiplierX = 0.2f;

    #endregion

    #region serialized private

    #endregion


    #region private

    private bool m_initialized = false;

    private bool m_isBallInPlay;
    private Animator m_anim;
    private ParticleSystem m_particles;
    private bool m_inAnimation;
    private float m_curVelocityY;
    private float m_curVelocityX;

    private float curY;
    private float curX;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;

    private BallArgs m_args;

    private bool isGamePaused;
    private TrailRenderer m_curBallTrail;
    private bool m_isTrailEmmiting = false;

    private string animName = "BallRegularHitSide1";

    private SpriteRenderer m_spriteRenderer;
    private PlayerIndex m_playerIndex;
    private int m_ballIndex = -1;
    private Color m_curColor = Color.white;


    #endregion



    public void Init(BallArgs args, PlayerIndex playerIndex, int ballIndex)
    {
        if (!m_initialized)
        {

            //print("init ballscript");
            m_anim = gameObject.GetComponent<Animator>();
            m_particles = gameObject.GetComponentInChildren<ParticleSystem>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_curBallTrail = GetComponentInChildren<TrailRenderer>();
            //m_rigidBody = GetComponent<Rigidbody>();

            this.gameObject.SetActive(false);
            //m_anim.enabled = false;
            m_inAnimation = false;

            m_initialRotation = gameObject.transform.localRotation;
            m_initialPosition = gameObject.transform.localPosition;
            m_initialScale = gameObject.transform.localScale;

            m_curVelocityY = 0;
            m_curVelocityX = 0;
            m_args = args;

            m_curBallTrail.emitting = false;
            m_initialized = true;

            m_playerIndex = playerIndex;
            m_ballIndex = ballIndex;

            // this.gameObject.SetActive(true);
            // m_isBallInPlay = true;



            //OnNewBallInScene();
        }

    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public float GetVelocityX()
    {
        return m_curVelocityX;
    }
    public float GetVelocityY()
    {
        return m_curVelocityY;
    }

    public Color GetColor()
    {
        return m_curColor;
    }

    public bool IsInScene()
    {
        return m_isBallInPlay;
    }

    public void RemoveBallFromScene()
    {

        m_isBallInPlay = false;
        this.gameObject.SetActive(false);

    }


    public void OnNewBallInScene(Color color)
    {

        this.gameObject.transform.localPosition = m_initialPosition;
        m_curVelocityY = m_startVelocityY;
        m_curVelocityX = m_startVelocityX;
        GenerateNewBall(color);

    }
    public void GenerateNewBallInScene(Color color, Vector3 pos, float velocityY, float velocityX)
    {

        this.gameObject.transform.position = pos;
        m_curVelocityY = velocityY;
        m_curVelocityX = velocityX;
        GenerateNewBall(color);

    }


    private void GenerateNewBall(Color color)
    {
        UpdateColor(color);
        m_isBallInPlay = true;
        m_curBallTrail.emitting = false;

        //SetBallSprite(0);
        ApplyHitVisuals(false, false);

        this.gameObject.SetActive(true);
    }

    private void UpdateColor(Color color)
    {
        if (color != m_curColor)
        {
            m_curColor = color;
            m_spriteRenderer.color = m_curColor;
            /*m_colorGradientParticles.color.gradient.colorKeys[0].color = m_curColor;
            m_colorGradientTrail.colorKeys[0].color = m_curColor;*/
            var particlesMain = m_particles.main;
            particlesMain.startColor = m_curColor;
        }

    }

    void FixedUpdate()
    {
        if (!isGamePaused)
        {
            if (m_isBallInPlay)
            {
                if (Math.Abs(m_curVelocityY) < m_maxVelocityY)
                {
                    m_curVelocityY -= m_args.m_gravity * m_velocityMultiplier;
                }

                curY = this.gameObject.transform.localPosition.y;
                curY += m_curVelocityY;

                curX = this.gameObject.transform.localPosition.x;
                curX += m_curVelocityX;

                this.gameObject.transform.localPosition = new Vector3(curX, curY, m_initialPosition.z);
                if ((m_curVelocityX >= 0) && (animName != "BallRegularHitSide1"))
                {
                    animName = "BallRegularHitSide1";
                }
                else if ((m_curVelocityX < 0) && (animName != "BallRegularHitSide2"))
                {
                    animName = "BallRegularHitSide2";
                }

                CheckBounds();
                CheckEmitTrail();
            }
        }
    }

    void CheckEmitTrail()
    {
        //print(m_curVelocityY);
        bool inSpeedToEmit = Math.Abs(m_curVelocityY) >= m_speedEmitTrail;
        if ((inSpeedToEmit) && (!m_isTrailEmmiting))
        {
            m_isTrailEmmiting = true;
        }
        else if ((!inSpeedToEmit) && (m_isTrailEmmiting))
        {
            m_isTrailEmmiting = false;
        }
        m_curBallTrail.emitting = m_isTrailEmmiting;

    }

    private void CheckBounds()
    {
        if (this.gameObject.transform.position.y < m_args.Bounds.GameLowerBound)
        {
            m_isBallInPlay = false;
            this.gameObject.SetActive(false);
            m_onBallLost(m_playerIndex, m_ballIndex);

        }
        else if (this.gameObject.transform.position.y > m_args.Bounds.GameUpperBound)
        {
            Vector3 curPos = this.gameObject.transform.localPosition;
            curPos.y -= 0.1f;
            this.gameObject.transform.localPosition = curPos;
            //this.gameObject.transform.localPosition = m_initialPosition;
            m_curVelocityY = m_curVelocityY * (-1f) * m_args.m_ballReflectPower;
        }
        else if (this.gameObject.transform.position.x - 2 < m_args.Bounds.GameLeftBound)
        {
            m_curVelocityX *= -1;
        }
        else if (this.gameObject.transform.position.x + 2 > m_args.Bounds.GameRightBound)
        {
            m_curVelocityX *= -1;
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


    public void OnHitPlay(KickType kickType, float distanceX, Color color, bool burstParticles)
    {
        UpdateColor(color);
        bool isSpecial = true;

        if (kickType == KickType.Regular)
        {
            isSpecial = false;
        }

        ApplyHitPhysics(isSpecial, distanceX);
        ApplyHitVisuals(isSpecial, burstParticles);

    }


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        this.gameObject.SetActive(!isGamePaused);

    }

    private void ApplyHitPhysics(bool isSpecial, float distanceX)
    {
        float kickPower = isSpecial ? m_args.BallSpecialHitPower : m_args.BallRegularHitPower;
        m_curVelocityY = kickPower * m_hitMultiplierY;
        //m_curVelocityX = distanceX * m_args.XAxisMultiplier * m_hitMultiplierX;
        float multiX = (distanceX >= 0) ? 1 : -1;
        m_curVelocityX = multiX * m_args.XAxisMultiplier * m_hitMultiplierX;


    }

    private void ApplyHitVisuals(bool isSpecial, bool withParticles)
    {
        if (this.isActiveAndEnabled)
        {
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




}
