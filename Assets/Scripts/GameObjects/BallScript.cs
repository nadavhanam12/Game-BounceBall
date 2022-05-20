using System;
using System.Threading.Tasks;
using UnityEngine;
using static PlayerScript;

public class BallScript : MonoBehaviour
{
    #region events


    public delegate void onBallLost(int ballIndex);
    public onBallLost m_onBallLost;

    #endregion

    #region const

    const float m_speedEmitTrail = 0;
    const int ParticlesToEmit = 500;
    //const float m_startVelocityY = 0.2f;
    //const float m_startVelocityX = -0.2f;
    //const float m_startVelocityX = 0f;

    #endregion

    #region serialized private

    #endregion


    #region private

    private bool m_initialized = false;

    private ParticleSystem m_particles;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;

    private BallArgs m_args;

    private bool isGamePaused;
    private ParticleSystem m_curBallTrail;
    private bool m_isTrailEmmiting = false;
    private SpriteRenderer m_spriteRenderer;
    private int m_ballIndex = -1;
    private Color m_curColor = Color.white;

    private int m_curTweenId = -1;
    private Rigidbody2D m_rigidBody;
    private CircleCollider2D m_collider;
    public bool BallHasFallen = false;

    private Vector2 waitingForce = Vector2.zero;

    #endregion


    public void Init(BallArgs args, int ballIndex)
    {
        if (!m_initialized)
        {

            //print("init ballscript");
            //m_particles = gameObject.GetComponentInChildren<ParticleSystem>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_curBallTrail = GetComponentInChildren<ParticleSystem>();
            m_rigidBody = GetComponent<Rigidbody2D>();
            m_collider = GetComponent<CircleCollider2D>();

            this.gameObject.SetActive(false);

            m_initialRotation = gameObject.transform.localRotation;
            m_initialPosition = gameObject.transform.localPosition;
            m_initialScale = gameObject.transform.localScale;

            m_args = args;

            m_initialized = true;

            m_ballIndex = ballIndex;

            m_rigidBody.gravityScale = m_args.m_gravity;
        }

    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public Vector2 GetVelocity()
    {
        return m_rigidBody.velocity;
    }

    public Color GetColor()
    {
        return m_curColor;
    }
    public SpriteRenderer GetSprite()
    {
        return m_spriteRenderer;
    }

    public bool IsInScene()
    {
        return gameObject.activeInHierarchy;
    }

    public void RemoveBallFromScene(bool fadeOut = false)
    {
        //print("RemoveBallFrom: " + m_ballIndex);
        if (gameObject.activeInHierarchy)
        {
            if (fadeOut)
            {
                FadeOut();
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }

    }


    public void OnNewBallInScene(Color color, int disXMultiplier = 1)
    {
        //print("OnNewBallInScene");
        this.gameObject.SetActive(true);
        this.gameObject.transform.localPosition = m_initialPosition;
        ApplyPhysics(new Vector2(m_args.m_startVelocityX * disXMultiplier, m_args.m_startVelocityY));
        GenerateNewBall(color);

    }
    public void GenerateNewBallInScene(Color color, Vector3 pos)
    {
        //print("GenerateNewBallInScene");
        this.gameObject.SetActive(true);
        this.gameObject.transform.position = pos;
        GenerateNewBall(color);

    }


    private void GenerateNewBall(Color color)
    {
        if (m_curTweenId != -1)
        {
            LeanTweenExt.LeanCancel(m_spriteRenderer.gameObject, m_curTweenId);
            m_curTweenId = -1;
        }
        UpdateColor(color);
        this.gameObject.SetActive(true);
        BallHasFallen = false;
    }

    private void UpdateColor(Color color)
    {
        m_curColor = color;
        m_spriteRenderer.color = m_curColor;
        /*m_colorGradientParticles.color.gradient.colorKeys[0].color = m_curColor;
        m_colorGradientTrail.colorKeys[0].color = m_curColor;*/
        if (m_curBallTrail != null)
        {
            var particlesMain = m_curBallTrail.main;
            particlesMain.startColor = m_curColor;
        }




    }

    void FixedUpdate()
    {
        if (!isGamePaused && m_initialized)
        {

            if (gameObject.activeInHierarchy)
            {
                CheckBounds();
                CheckEmitTrail();
            }
        }

    }

    void CheckEmitTrail()
    {
        bool inSpeedToEmit = Math.Abs(m_rigidBody.velocity.y) >= m_speedEmitTrail;
        if ((inSpeedToEmit) && (!m_isTrailEmmiting))
        {
            m_isTrailEmmiting = true;
        }
        else if ((!inSpeedToEmit) && (m_isTrailEmmiting))
        {
            m_isTrailEmmiting = false;
        }
    }
    private async void BallFallen()
    {
        //print("Ball has fallen");
        m_rigidBody.AddForce(new Vector2(0, m_args.m_ballReflectPower));
        BallHasFallen = true;
        await Task.Delay(1000);
        this.gameObject.SetActive(false);
        m_onBallLost(m_ballIndex);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        //print("Ball OnCollusion: " + col.collider.name);
        if (col.gameObject.tag == "GameLowerBound")
        {
            BallFallen();
        }
        else
        {
            Physics2D.IgnoreCollision(m_collider, col.collider);
        }
    }

    private void CheckBounds()
    {
        Vector3 ballPosition = this.gameObject.transform.position;
        if (ballPosition.x - 2 < m_args.Bounds.GameLeftBound)
        {
            //print("Reached left bound");
            //m_curVelocityX *= -1;
            ballPosition.x = m_args.Bounds.GameRightBound - 3;
            this.gameObject.transform.localPosition = ballPosition;
        }
        else if (ballPosition.x + 2 > m_args.Bounds.GameRightBound)
        {
            //print("Reached right bound");
            //m_curVelocityX *= -1;
            ballPosition.x = m_args.Bounds.GameLeftBound + 3;
            this.gameObject.transform.localPosition = ballPosition;
        }

    }


    public void OnHitPlay(KickType kickType, float distanceX, Color color, bool burstParticles)
    {
        UpdateColor(color);
        ApplyPhysics(new Vector2(distanceX, m_args.BallHitPowerY));
    }


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        m_rigidBody.simulated = !isGamePaused;
        if (!isGamePaused && waitingForce != Vector2.zero)
        {
            ResetVelocity();
            ApplyPhysics(waitingForce);
            waitingForce = Vector2.zero;
        }

    }

    public void ResetVelocity()
    {
        m_rigidBody.velocity = Vector2.zero;
        m_rigidBody.angularVelocity = 0;
    }
    private void ApplyPhysics(Vector2 force)
    {
        if (isGamePaused)
        {
            waitingForce = force;
        }
        else
        {
            m_rigidBody.AddForce(force);
            m_rigidBody.AddTorque(force.x * m_args.BallTorqueMultiplier * -1);
        }
    }


    private void FadeOut()
    {
        LTDescr curTween =
        LeanTweenExt.MyLeanAlphaSpriteRenderer
        (m_spriteRenderer, 0, m_args.m_ballTimeFadeOut)
        .setEase(LeanTweenType.easeOutSine)
        .setOnComplete(
            () => this.gameObject.SetActive(false));
        m_curTweenId = curTween.id;
    }

    public void InitGravity()
    {
        m_rigidBody.gravityScale = m_args.m_gravity;
    }

    public void AddToGravity(float gravityAdded)
    {
        if (m_rigidBody.gravityScale <= m_args.BallMaxGravity)
            m_rigidBody.gravityScale += gravityAdded;
    }


}
