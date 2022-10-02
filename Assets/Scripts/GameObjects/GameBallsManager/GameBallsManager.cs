using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static GameManagerAbstract;
using static PlayerScript;

public class GameBallsManager : MonoBehaviourPun
{

    #region Events

    public delegate void onTurnLost();
    public onTurnLost GameManagerOnTurnLost;

    public delegate void onBallHit(PlayerIndex index);
    public onBallHit GameManagerOnBallHit;

    #endregion

    #region public
    public BallHitVisual m_ballHitVisualPrefab;
    public BallScript m_ballPrefab;
    public Transform m_ballsContainer;
    public List<Color> ballColors;
    [Range(0, 1)]
    public float m_opponentBallAlpha = 0.5f;
    public int m_ballsPoolSize;
    public GameObject m_ballsHitVFX;

    [Range(0.2f, 1)] public float m_gravityAdded;
    public int m_gravityChangeRate;

    public float m_highKickHight = 0;
    public float m_kickCooldown = 0.1f;


    #endregion


    #region private

    public Queue<Color> ColorsQueue { get; protected set; }
    protected Color[] m_nextColorArray;

    protected GameBallsManagerArgs m_args;
    private bool m_initialized = false;
    protected Queue<BallHitVisual> m_hitVisualsQueue = new Queue<BallHitVisual>();
    protected Color m_curRequiredColor;

    protected bool isGamePaused = false;
    protected BallScript[] m_ballsArray;
    protected int m_nextBallIndex;
    private int m_correctBallIndex;

    private int m_curCombo;

    bool m_allowOnlyJumpKick = false;

    private bool m_inKickCooldown = false;
    #endregion


    public virtual void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        foreach (BallScript ball in m_ballsArray)
            ball.SetGamePause(isGamePaused);
    }

    public void Init(GameBallsManagerArgs args)
    {
        if (!m_initialized)
        {
            m_args = args;
            m_curCombo = 0;
            InitBalls();
            InitHitBallVisuals();
            InitColorQueue();
            m_initialized = true;
        }
    }

    protected virtual void InitColorQueue()
    {
        ColorsQueue = new Queue<Color>();
        //m_colorQueue.Enqueue(Color.white);
        for (int i = 0; i < 4; i++)
            ColorsQueue.Enqueue(GenerateRandomColor(Color.black));
    }
    void InitHitBallVisuals()
    {
        BallHitVisual curBallVisuals;
        Vector3 ballScale = m_ballPrefab.transform.localScale;
        for (int i = 0; i < m_ballsPoolSize; i++)
        {
            curBallVisuals = Instantiate(m_ballHitVisualPrefab, m_ballsHitVFX.transform);
            curBallVisuals.Init(ballScale);
            m_hitVisualsQueue.Enqueue(curBallVisuals);
        }
    }

    protected virtual void InitBalls()
    {
        m_nextBallIndex = 0;
        m_ballsArray = new BallScript[m_ballsPoolSize];
        for (int i = 0; i < m_ballsArray.Length; i++)
            m_ballsArray[i] = Instantiate(m_ballPrefab, m_ballsContainer);

        BallScript curBall;
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            curBall = m_ballsArray[i];
            curBall.Init(m_args.BallArgs, i);
            curBall.m_onBallLost = OnBallLost;
            curBall.RemoveBallFromScene();
            RemoveBallFromScene(i);
        }
    }


    protected virtual void OnBallLost(int ballIndex)
    {
        RemoveBallFromScene(ballIndex, false);
        //if ((m_correctBallIndex == ballIndex) || (!IsBallsInPLay()))
        if (m_correctBallIndex == ballIndex)
        {
            GameManagerOnTurnLost();
            m_curCombo = 0;
            foreach (BallScript ball in m_ballsArray)
                ball.InitGravity();
        }
    }
    private bool IsBallsInPLay()
    {
        for (int i = 0; i < m_ballsArray.Length; i++)
            if (m_ballsArray[i].IsInScene())
                return true;
        return false;
    }


    public virtual void RemoveAllBalls()
    {
        for (int i = 0; i < m_ballsArray.Length; i++)
            RemoveBallFromScene(i);
    }

    public void Destroy()
    {
        foreach (BallScript ball in m_ballsArray)
            Destroy(ball);
        //Destroy(this);
    }

    protected virtual void RemoveBallFromScene(int ballIndex, bool fadeOut = false)
    {
        m_ballsArray[ballIndex].RemoveBallFromScene(fadeOut);
    }

    BallScript GetNextBall()
    {
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            BallScript nextBall = m_ballsArray[m_nextBallIndex];
            m_nextBallIndex++;
            m_nextBallIndex = m_nextBallIndex % m_ballsArray.Length;
            if (!nextBall.IsInScene())
                return nextBall;
        }
        return null;
    }

    //Generate another ball with different color and direction
    //update the game manager for score and combo
    protected virtual void OnHitPlay(PlayerIndex playerIndex, int ballIndex, KickType kickType)
    {
        //print("OnHitPlay- playerIndex: " + playerIndex);
        BallScript ball = m_ballsArray[ballIndex];
        if (ball.BallHasFallen)
        {
            print("OnHitPlay- BallHasFallen");
            return;
        }
        if (m_allowOnlyJumpKick)
        {
            //print(ball.transform.position);
            if (ball.transform.position.y < m_highKickHight)
            {
                print("OnHitPlay- ball hight is lower then high kick height value");
                return;
            }
        }

        if (m_curRequiredColor != ball.GetColor())
        {//not correct color
            //print("OnHitPlay- not correct color " + ball.GetIndex());
            RemoveBallFromScene(ballIndex, true);
            return;
        }
        if (m_inKickCooldown)
        {
            //print("OnHitPlay- inKickCooldown");
            return;
        }

        StartCoroutine(KickCooldown());
        GameManagerOnBallHit(playerIndex);

        BallScript otherBall = GetNextBall();
        if (otherBall == null)
            return;

        Vector2 ballPos = ball.GetPosition();
        ActivateBallHitVisual(m_curRequiredColor, ballPos);

        Color color1 = PullColorFromQueue();
        Color color2 = GenerateRandomColor(color1);

        UpdateNextBallColor(color1, true);

        float multiplierX = RandomDirectionX();
        float multiplierY = m_args.BallArgs.BallHitPowerY;

        Vector2 otherBallPos = ballPos;
        if (multiplierX > 0)
            otherBallPos.x -= 1f;
        else
            otherBallPos.x += 1f;

        GenerateNewBallInScene(otherBall.GetIndex(), color2, otherBallPos);

        ball.ResetVelocity();
        otherBall.ResetVelocity();
        float multiplierY1 = multiplierY;
        float multiplierY2 = multiplierY;
        if (kickType == KickType.Special)
        {
            multiplierY1 = RandomMultiplierY();
            multiplierY2 = RandomMultiplierY();
            multiplierX = RandomMultiplierX();
            CameraShake();
        }
        ball.OnHitPlay(multiplierX, true, multiplierY1);
        ball.UpdateColor(color1);

        otherBall.OnHitPlay((-1) * multiplierX, false, multiplierY2);
        otherBall.UpdateColor(color2);


        UpdateCurCombo();
    }
    protected virtual void CameraShake()
    {
        CameraVFX cameraVFX = FindObjectOfType<CameraVFX>();
        cameraVFX.Shake();
    }

    //is called when a ball is split
    protected virtual void GenerateNewBallInScene(int ballIndex, Color color2, Vector2 otherBallPos)
    {
        BallScript ball = m_ballsArray[ballIndex];
        ball.GenerateNewBallInScene(color2, otherBallPos);
    }

    Color PullColorFromQueue()
    {
        Color color1 = ColorsQueue.Dequeue();
        Color lastColorInQueue = ColorsQueue.ToArray()[ColorsQueue.Count - 1];
        ColorsQueue.Enqueue(GenerateRandomColor(lastColorInQueue));
        return color1;
    }
    float RandomDirectionX()
    {
        float rnd = m_args.BallArgs.BallHitPowerX;
        if (FlipDistance())
            rnd *= (-1);
        return rnd;
    }

    float RandomMultiplierX()
    {
        float rnd = Random.Range(m_args.BallArgs.BallSpecialKickPowerX, m_args.BallArgs.BallSpecialKickPowerX * 1.5f);
        if (FlipDistance())
            rnd *= (-1);
        return rnd;
    }
    float RandomMultiplierY()
    {
        float rnd = Random.Range(m_args.BallArgs.BallSpecialKickPowerY, m_args.BallArgs.BallSpecialKickPowerY);
        return rnd;
    }

    float IgnoreDisX(float distanceX)
    {
        if (distanceX >= 0)
            return 1;
        else
            return -1;
    }
    public virtual void UpdateNextBallColor(Color color, bool shouldEmitParticles)
    {
        m_curRequiredColor = color;
        m_nextColorArray = ColorsQueue.ToArray();
        m_args.GameCanvas.UpdateNextBallColor(color, m_nextColorArray, shouldEmitParticles);
    }

    protected virtual void ActivateBallHitVisual(Color color, Vector3 position)
    {
        BallHitVisual hitVisual = m_hitVisualsQueue.Dequeue();
        hitVisual.Activate(color, position);
        m_hitVisualsQueue.Enqueue(hitVisual);
    }

    bool FlipDistance()
    {
        int rnd = Random.Range(0, 2);
        return rnd == 0;
    }

    protected Color GenerateRandomColor(Color forbiddenColor)
    {
        int rnd;
        int i = 0;
        while (true)
        {
            i++;
            rnd = Random.Range(0, ballColors.Count);
            if (ballColors[rnd] != forbiddenColor)
                return ballColors[rnd];
            if (i == 200)
            {
                print("GenerateRandomColor exit after 200 loops");
                return Color.white;
            }
        }

    }


    //should turn off the balls
    public void TimeIsOver()
    {
        Destroy();
    }

    public void OnNewBallInScene()
    {
        OnNewBallInScene(true, Vector2Int.zero);
    }

    //should start new turn
    public virtual void OnNewBallInScene(bool randomDirection, Vector2Int directionVector)
    {
        if (m_ballsArray[m_correctBallIndex].IsInScene())
        {
            //print("correct ball is in scene");
            return;
        }
        UpdateCorrectBallIndex(m_nextBallIndex);
        //print("m_correctBallIndex " + m_correctBallIndex);
        BallScript ball = GetNextBall();
        if (ball == null) { return; }
        Color color = Color.white;
        float disXMultiplier;
        if (randomDirection)
            disXMultiplier = Random.Range(-m_args.BallArgs.m_startForceX, m_args.BallArgs.m_startForceX);
        else
            disXMultiplier = directionVector.x;
        int ballIndex = ball.GetIndex();
        GenerateFirstBall(ballIndex, color, disXMultiplier, m_args.BallArgs.m_startForceY);
        UpdateNextBallColor(color, false);
    }

    protected virtual void UpdateCorrectBallIndex(int nextBallIndex)
    {
        m_correctBallIndex = nextBallIndex;
    }

    protected virtual void GenerateFirstBall(int ballIndex, Color color, float disXMultiplier, float startForceY)
    {
        BallScript ball = m_ballsArray[ballIndex];
        ball.OnNewBallInScene(color, disXMultiplier, startForceY);
    }

    public virtual void ApplyKick(PlayerIndex playerIndex, List<BallScript> ballsHit, KickType kickType)
    {
        BallScript correctBall = m_ballsArray[m_correctBallIndex];
        int ballIndex;

        if (ballsHit.Contains(correctBall))
            //kick correct ball if in range
            ballIndex = m_correctBallIndex;
        else
            //kick closest ball if in range
            ballIndex = ballsHit[0].GetIndex();

        OnHitPlay(playerIndex, ballIndex, kickType);
    }

    public bool ContainsCorrectBall(List<BallScript> ballsHit)
    {
        BallScript correctBall = m_ballsArray[m_correctBallIndex];
        return ballsHit.Contains(correctBall);
    }

    public Vector3 GetCorrectBallPosition()
    {
        return m_ballsArray[m_correctBallIndex].gameObject.transform.position;
    }

    void UpdateCurCombo()
    {
        m_curCombo++;
        if (m_curCombo % m_gravityChangeRate == 0)
        {
            //print("Adding gravity: " + m_gravityAdded);
            m_args.GameCanvas.GravityIncrease();
            foreach (BallScript ball in m_ballsArray)
                ball.AddToGravity(m_gravityAdded);
        }
    }

    public void onAllowOnlyJumpKick(bool isOn)
    {
        m_allowOnlyJumpKick = isOn;
    }

    IEnumerator KickCooldown()
    {
        //print("m_inKickCooldown = true");
        m_inKickCooldown = true;
        yield return new WaitForSeconds(m_kickCooldown);
        m_inKickCooldown = false;
        //print("m_inKickCooldown = false");
    }
    public bool IsInKickCooldown()
    {
        return m_inKickCooldown;
    }

}

