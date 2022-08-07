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
    private TrailRenderer m_curBallTrail;
    private SpriteRenderer m_spriteRenderer;
    private int m_ballIndex = -1;
    private Color m_curColor = Color.white;

    private int m_curTweenId = -1;
    private Rigidbody2D m_rigidBody;
    private CircleCollider2D m_collider;
    [HideInInspector] public bool BallHasFallen = false;

    private Vector2 waitingForce = Vector2.zero;

    #endregion


    public void Init(BallArgs args, int ballIndex)
    {
        if (!m_initialized)
        {

            //print("init ballscript");
            //m_particles = gameObject.GetComponentInChildren<ParticleSystem>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_curBallTrail = GetComponentInChildren<TrailRenderer>();
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


    public void OnNewBallInScene(Color color, float forceX, float forceY)
    {
        //print("OnNewBallInScene");
        this.gameObject.SetActive(true);
        m_spriteRenderer.color = Color.white;
        m_initialized = false;
        this.gameObject.transform.localPosition = m_initialPosition;
        m_rigidBody.simulated = false;
        m_curBallTrail.enabled = false;
        FadeIn(() =>
        {
            m_rigidBody.simulated = true;
            m_curBallTrail.enabled = true;
            m_initialized = true;
            ApplyPhysics(new Vector2(forceX, forceY));
            GenerateNewBall(color);
        }
        );
        /*m_rigidBody.simulated = true;
        m_initialized = true;
        ApplyPhysics(new Vector2(forceX, forceY));
        GenerateNewBall(color);*/


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

        if (m_curBallTrail != null)
        {
            m_curBallTrail.enabled = true;
            m_curBallTrail.startColor = m_curColor;
        }




    }

    void FixedUpdate()
    {
        if (!isGamePaused && m_initialized)
        {

            if (gameObject.activeInHierarchy)
            {
                CheckBounds();
            }
        }

    }
    private async void BallFallen()
    {
        //print("Ball has fallen");
        m_rigidBody.AddForce(new Vector2(0, m_args.m_ballReflectPower));
        BallHasFallen = true;
        //ResetVelocity();
        await Task.Delay(150);
        if (this)
        {
            this.gameObject.SetActive(false);
            m_onBallLost(m_ballIndex);
        }
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
        if ((ballPosition.x - m_args.BallBoundDistanceTrigger < m_args.Bounds.GameLeftBound) && (m_rigidBody.velocity.x < 0))
        {
            //print("Reached left bound");
            Vector2 tempVelocity = m_rigidBody.velocity;
            if (Math.Abs(tempVelocity.x) > 0.5)
                tempVelocity.x *= -1;
            else
                tempVelocity.x = 0.5f;
            m_rigidBody.velocity = tempVelocity;
            /*m_curBallTrail.emitting = false;
            ballPosition.x = m_args.Bounds.GameRightBound - m_args.BallBoundDistanceSpawn;
            this.gameObject.transform.localPosition = ballPosition;*/
        }
        else if ((ballPosition.x + m_args.BallBoundDistanceTrigger > m_args.Bounds.GameRightBound) && (m_rigidBody.velocity.x > 0))
        {
            //print("Reached right bound");
            Vector2 tempVelocity = m_rigidBody.velocity;
            if (Math.Abs(tempVelocity.x) > 0.5)
                tempVelocity.x *= -1;
            else
                tempVelocity.x = 0.5f;

            m_rigidBody.velocity = tempVelocity;
            /*m_curBallTrail.emitting = false;
            ballPosition.x = m_args.Bounds.GameLeftBound + m_args.BallBoundDistanceSpawn;
            this.gameObject.transform.localPosition = ballPosition;*/
        }
        else if (!m_curBallTrail.emitting)
        {
            m_curBallTrail.emitting = true;
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
        if (m_initialized)
        {
            m_rigidBody.simulated = !isGamePaused;
            if (!isGamePaused && waitingForce != Vector2.zero)
            {
                ResetVelocity();
                ApplyPhysics(waitingForce);
                waitingForce = Vector2.zero;
            }
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
        m_curBallTrail.enabled = false;
        LTDescr curTween =
        LeanTweenExt.MyLeanAlphaSpriteRenderer
        (m_spriteRenderer, 0, m_args.m_ballTimeFadeOut)
        .setEase(LeanTweenType.easeOutQuart)
        .setOnComplete(
            () => this.gameObject.SetActive(false));
        m_curTweenId = curTween.id;
    }
    private void FadeIn(Action actionOnComplete)
    {
        Color curColor = m_spriteRenderer.color;
        curColor.a = 0.2f;
        m_spriteRenderer.color = curColor;
        LTDescr curTween =
        LeanTweenExt.MyLeanAlphaSpriteRenderer
        (m_spriteRenderer, 1, m_args.m_ballTimeFadeIn)
        .setEase(LeanTweenType.easeInSine)
        .setOnComplete(actionOnComplete);
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
