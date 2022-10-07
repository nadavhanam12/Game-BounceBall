using System;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using static PlayerScript;

public class BallScript : MonoBehaviourPun
{
    #region events
    public delegate void onBallLost(int ballIndex);
    public onBallLost m_onBallLost;

    #endregion

    #region private

    private bool m_initialized = false;
    private Quaternion m_initialRotation;
    private Vector3 m_initialPosition;
    private Vector3 m_initialScale;
    protected BallArgs m_args;
    private bool isGamePaused;
    private int m_ballIndex = -1;
    [HideInInspector] public bool BallHasFallen = false;

    #endregion

    BallParticles m_ballParticles;
    protected BallPhysics m_ballPhysics;
    BallColor m_ballColor;

    public virtual void Init(BallArgs args, int ballIndex)
    {
        if (!m_initialized)
        {
            m_ballParticles = GetComponent<BallParticles>();
            m_ballParticles.Init();

            m_ballPhysics = GetComponent<BallPhysics>();
            m_ballPhysics.Init(this, args);

            m_ballColor = GetComponent<BallColor>();
            m_ballColor.Init(args);

            this.gameObject.SetActive(false);

            m_initialRotation = gameObject.transform.localRotation;
            m_initialPosition = gameObject.transform.localPosition;
            m_initialScale = gameObject.transform.localScale;

            m_args = args;
            m_initialized = true;
            m_ballIndex = ballIndex;
        }
    }



    public bool IsInScene()
    {
        if (!this) return false;
        return gameObject.activeInHierarchy;
    }

    public void RemoveBallFromScene(bool fadeOut = false)
    {
        //print("RemoveBallFrom: " + m_ballIndex);
        if (gameObject.activeInHierarchy)
        {
            if (fadeOut)
                FadeOut();
            else
                RemoveFromScene();
        }
    }


    public void OnNewBallInScene(Color color, float forceX, float forceY)
    {
        //print("OnNewBallInScene");
        this.gameObject.transform.localPosition = m_initialPosition;
        this.gameObject.SetActive(true);
        m_ballColor.UpdateColorToWhite();
        m_initialized = false;
        m_ballPhysics?.ToggleSimulate(false);
        m_ballParticles.EnableTrail(false);
        m_ballColor.FadeIn(
            () =>
        {
            m_ballParticles.EnableTrail(true);
            m_initialized = true;
            m_ballPhysics?.ApplyPhysics(new Vector2(forceX, forceY));
            GenerateNewBall(color);
        });
    }
    public virtual void GenerateNewBallInScene(Color color, Vector3 pos)
    {
        //print("GenerateNewBallInScene");
        this.gameObject.transform.position = pos;
        this.gameObject.SetActive(true);
        GenerateNewBall(color);
    }


    protected void GenerateNewBall(Color color)
    {
        if (m_ballColor.CurTweenId != -1)
            m_ballColor.CancelTween();

        if (this != null)
        {
            UpdateColor(color);
            this.gameObject.SetActive(true);
            BallHasFallen = false;
        }
    }

    public virtual void UpdateColor(Color color)
    {
        m_ballColor.UpdateColor(color);
        if (m_ballParticles != null)
            m_ballParticles.UpdateStartColor(color);
    }

    void FixedUpdate()
    {
        if (!isGamePaused && m_initialized)
            if (gameObject.activeInHierarchy)
                m_ballPhysics?.CheckBounds();
    }
    public async void BallFallen()
    {
        //print("Ball has fallen");
        if (this)
        {
            //m_ballPhysics?.AddForce(m_args.m_ballReflectPower);
            BallHasFallen = true;
            //ResetVelocity();
            await Task.Delay(50);
            if (this)
            {
                RemoveFromScene();
                m_onBallLost(m_ballIndex);
            }
        }
    }

    public void EmitBallTrail(bool toTrail)
    {
        m_ballParticles.EmitBallTrail(toTrail);
    }


    public void OnHitPlay(float multiplierX, bool burstParticles, float multiplierY)
    {
        m_ballPhysics?.ApplyPhysics(new Vector2(multiplierX, multiplierY));
    }

    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        m_ballPhysics?.SetGamePause(m_initialized, isGamePaused);
    }

    private void FadeOut()
    {
        m_ballParticles.EnableTrail(false);
        m_ballColor.FadeOut();
    }


    public void InitGravity()
    {
        m_ballPhysics?.InitGravity();
    }

    public void AddToGravity(float gravityAdded)
    {
        m_ballPhysics?.AddToGravity(gravityAdded);
    }

    public int GetIndex()
    {
        return m_ballIndex;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Color GetColor()
    {
        return m_ballColor.GetColor();
    }
    public void ResetVelocity()
    {
        m_ballPhysics?.ResetVelocity();
    }

    void RemoveFromScene()
    {
        m_ballParticles.EnableTrail(false);
        this.gameObject.SetActive(false);
    }

    internal bool IsNearLowerBound()
    {
        throw new NotImplementedException();
    }
}
