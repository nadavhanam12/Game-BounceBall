using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManagerScript;
using static PlayerScript;

public class GameBallsManager : MonoBehaviour
{

    #region Events

    public delegate void onTurnLost();
    public onTurnLost GameManagerOnTurnLost;

    public delegate void onBallHit(PlayerIndex index);
    public onBallHit GameManagerOnBallHit;


    #endregion

    #region serialized
    [SerializeField] BallHitVisual m_ballHitVisualPrefab;
    [SerializeField] List<Color> ballColors;

    [Range(0, 1)]
    [SerializeField] private float m_opponentBallAlpha = 0.5f;
    [SerializeField] private int m_ballsPoolSize;
    [SerializeField] private GameObject m_ballsHitVFX;

    [SerializeField][Range(0.2f, 1)] private float m_gravityAdded;
    [SerializeField] private int m_gravityChangeRate;



    #endregion


    #region private

    private Queue<Color> m_colorQueue;
    private Color[] m_nextColorArray;

    private GameBallsManagerArgs m_args;
    private bool m_initialized = false;
    private Queue<BallHitVisual> m_hitVisualsQueue = new Queue<BallHitVisual>();
    private Color m_curRequiredColor;

    private bool isGamePaused = false;
    private BallScript[] m_ballsArray;
    private BallScript firstBall;
    private int m_nextBallIndex;
    private int m_correctBallIndex;

    private int m_curCombo;


    #endregion


    public void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        foreach (BallScript ball in m_ballsArray)
        {
            ball.SetGamePause(isGamePaused);
        }

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

    void InitColorQueue()
    {
        m_colorQueue = new Queue<Color>();
        for (int i = 0; i < 4; i++)
        {
            m_colorQueue.Enqueue(GenerateRandomColor());
        }
    }
    void InitHitBallVisuals()
    {
        BallHitVisual curBallVisuals;
        Vector3 ballScale = firstBall.transform.localScale;
        for (int i = 0; i < m_ballsPoolSize; i++)
        {
            curBallVisuals = Instantiate(m_ballHitVisualPrefab, m_ballsHitVFX.transform);
            curBallVisuals.Init(ballScale);
            m_hitVisualsQueue.Enqueue(curBallVisuals);
        }
    }

    private void InitBalls()
    {
        m_nextBallIndex = 0;
        firstBall = GetComponentInChildren<BallScript>();
        m_ballsArray = new BallScript[m_ballsPoolSize];
        m_ballsArray[0] = firstBall;
        for (int i = 1; i < m_ballsArray.Length; i++)
        {
            m_ballsArray[i] = (Instantiate(firstBall, firstBall.transform.parent));
        }
        BallScript curBall;
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            curBall = m_ballsArray[i];
            curBall.Init(m_args.BallArgs, i);
            curBall.m_onBallLost = OnBallLost;
            curBall.RemoveBallFromScene();
        }

    }


    public void OnBallLost(int ballIndex)
    {
        //bool fadeOut = m_correctBallIndex != ballIndex;
        m_ballsArray[ballIndex].RemoveBallFromScene(false);
        if (!IsBallsInPLay())
        {
            GameManagerOnTurnLost();
            m_curCombo = 0;
            foreach (BallScript ball in m_ballsArray)
            {
                ball.InitGravity();
            }
        }


    }
    private bool IsBallsInPLay()
    {
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            if (m_ballsArray[i].IsInScene())
            {
                return true;
            }
        }
        return false;
    }


    public void RemoveAllBalls()
    {
        foreach (BallScript ball in m_ballsArray)
        {
            ball.RemoveBallFromScene();
        }
    }

    public void Destroy()
    {
        foreach (BallScript ball in m_ballsArray)
        {
            Destroy(ball);
        }
        Destroy(this);
    }
    void WrongBallHit(int ballIndex)
    {
        m_ballsArray[ballIndex].RemoveBallFromScene(true);
    }

    private BallScript GetNextBall()
    {
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            BallScript nextBall = m_ballsArray[m_nextBallIndex];
            m_nextBallIndex++;
            m_nextBallIndex = m_nextBallIndex % m_ballsArray.Length;
            if (!nextBall.IsInScene())
            {
                return nextBall;
            }
        }
        return null;
    }

    //Generate another ball with different color and direction
    //update the game manager for score and combo
    private void OnHitPlay(PlayerIndex playerIndex, int ballIndex, KickType kickType)
    {
        BallScript ball = m_ballsArray[ballIndex];
        if (ball.BallHasFallen)
        {
            return;
        }
        if (m_curRequiredColor != ball.GetColor())
        {//not correct color
            WrongBallHit(ballIndex);
            return;
        }

        GameManagerOnBallHit(playerIndex);

        BallScript otherBall = GetNextBall();
        if (otherBall == null) { return; }

        Color color1 = m_colorQueue.Dequeue();
        Color color2 = GenerateRandomColor();
        m_colorQueue.Enqueue(GenerateRandomColor());

        if (m_args.GameType == GameType.TurnsGame)
        {
            if (playerIndex == PlayerIndex.Second)
            {
                color1.a = m_opponentBallAlpha;
                color2.a = m_opponentBallAlpha;
            }
        }
        UpdateNextBallColor(color1, true);

        Vector2 ballPos = ball.GetPosition();
        float distanceX = RandomDisX();
        //print("distanceX: " + distanceX);

        ActivateBallHitVisual(color1, ballPos);

        Vector2 otherBallPos = ballPos;
        if (distanceX > 0)
            otherBallPos.x -= 1f;
        else
            otherBallPos.x += 1f;

        otherBall.GenerateNewBallInScene(color2, otherBallPos);

        ball.ResetVelocity();
        otherBall.ResetVelocity();


        ball.OnHitPlay(kickType, distanceX, color1, true);
        otherBall.OnHitPlay(kickType, (-1) * distanceX, color2, false);

        UpdateCurCombo();
    }

    float RandomDisX()
    {
        float rnd = Random.Range(m_args.BallArgs.BallMinimumHitPowerX, m_args.BallArgs.BallMaximumHitPowerX);
        if (FlipDistance())
        {
            rnd *= (-1);
        }

        return rnd;
    }

    float IgnoreDisX(float distanceX)
    {
        if (distanceX >= 0)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
    void UpdateNextBallColor(Color color, bool shouldEmitParticles)
    {
        m_curRequiredColor = color;
        m_nextColorArray = m_colorQueue.ToArray();
        m_args.GameCanvas.UpdateNextBallColor(color, m_nextColorArray, shouldEmitParticles);

    }

    void ActivateBallHitVisual(Color color, Vector3 position)
    {
        BallHitVisual hitVisual = m_hitVisualsQueue.Dequeue();
        hitVisual.Activate(color, position);
        m_hitVisualsQueue.Enqueue(hitVisual);
    }

    bool FlipDistance()
    {
        int rnd = Random.Range(0, 10);
        return rnd <= 4;
    }

    private Color GenerateRandomColor()
    {
        int rnd = Random.Range(0, ballColors.Count);
        return ballColors[rnd];
    }


    //should turn off the balls
    public void TimeIsOver()
    {
        Destroy();
    }


    //should start new turn
    public void OnNewBallInScene(PlayerIndex playerIndex, PlayerIndex nextPlayerIndex = PlayerIndex.First)
    {
        if (m_ballsArray[m_correctBallIndex].IsInScene())
        {
            print("correct ball is in scene");
            return;
        }
        m_correctBallIndex = m_nextBallIndex;
        //print("m_correctBallIndex " + m_correctBallIndex);
        BallScript ball = GetNextBall();
        if (ball == null) { return; }
        Color color = Color.white;
        if (m_args.GameType == GameType.TurnsGame)
        {
            if (playerIndex == PlayerIndex.Second)
            {
                color.a = m_opponentBallAlpha;
            }
        }
        int disXMultiplier = nextPlayerIndex == PlayerIndex.First ? -1 : 1;
        ball.OnNewBallInScene(color, disXMultiplier);
        UpdateNextBallColor(color, false);
        /*if (isGamePaused)
        {
            SetGamePause(isGamePaused);
        }*/


    }

    public void ApplyKick(PlayerIndex playerIndex, KickType kickType, List<BallScript> ballsHit)
    {
        BallScript correctBall = m_ballsArray[m_correctBallIndex];
        if (ballsHit.Contains(correctBall)) //kick correct ball if in range
        {
            int ballIndex = m_correctBallIndex;
            OnHitPlay(playerIndex, ballIndex, kickType);
        }
        else //kick closest ball if in range
        {
            BallScript kickBall = ballsHit[0];
            int ballIndex = System.Array.IndexOf(m_ballsArray, kickBall);
            OnHitPlay(playerIndex, ballIndex, kickType);
        }

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
            {
                ball.AddToGravity(m_gravityAdded);

            }
        }


    }
}

