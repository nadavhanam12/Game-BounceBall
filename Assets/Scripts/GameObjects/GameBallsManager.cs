using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;
using static PlayerScript;

public class GameBallsManager : MonoBehaviour
{

    #region Events

    public delegate void onTurnLost(PlayerIndex index);
    public onTurnLost GameManagerOnTurnLost;

    public delegate void onBallHit(PlayerIndex index);
    public onBallHit GameManagerOnBallHit;


    #endregion

    #region serialized
    [SerializeField] BallHitVisual m_ballHitVisualPrefab;
    [SerializeField] List<Color> ballColors;




    #endregion


    #region private

    private GameBallsManagerArgs m_args;
    private bool m_initialized = false;
    private Queue<BallHitVisual> m_hitVisualsQueue = new Queue<BallHitVisual>();
    private Color m_curRequiredColor;
    private GameCanvasScript m_gameCanvas;



    #endregion

    public void Init(GameBallsManagerArgs args)
    {
        if (!m_initialized)
        {

            m_args = args;

            InitBalls();
            InitHitBallVisuals();
            m_initialized = true;
        }
    }
    void InitHitBallVisuals()
    {
        BallHitVisual curBallVisuals;
        Vector3 ballScale = m_args.Player1Balls[0].transform.localScale;
        for (int i = 0; i < 6; i++)
        {
            curBallVisuals = Instantiate(m_ballHitVisualPrefab, gameObject.transform);
            curBallVisuals.Init(ballScale);
            m_hitVisualsQueue.Enqueue(curBallVisuals);
        }
    }
    private void InitBalls()
    {
        int i = 0;
        foreach (BallScript ball in m_args.Player1Balls)
        {
            ball.Init(m_args.BallArgs, PlayerIndex.First, i);
            ball.m_onBallLost = OnBallLost;
            i++;
        }

        i = 0;
        foreach (BallScript ball in m_args.Player2Balls)
        {
            ball.Init(m_args.BallArgs, PlayerIndex.Second, i);
            ball.m_onBallLost = OnBallLost;
            i++;

            ball.RemoveBallFromScene();
        }

    }


    public void OnBallLost(PlayerIndex playerIndex, int ballIndex)
    {
        List<BallScript> balls = m_args.Player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.Player1Balls;
        }
        BallScript ball = balls[ballIndex];
        ball.RemoveBallFromScene();

        BallScript otherBall = balls[1 - ballIndex];
        if (!otherBall.IsInScene())
        {
            GameManagerOnTurnLost(playerIndex);
        }


    }

    private void EndPlayerTurn(PlayerIndex playerIndex)
    {
        List<BallScript> balls = m_args.Player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.Player1Balls;
        }
        BallScript ball = balls[0];
        ball.RemoveBallFromScene();
        BallScript otherBall = balls[1 - 1];
        otherBall.RemoveBallFromScene();
        GameManagerOnTurnLost(playerIndex);
    }

    //Generate another ball with different color and direction
    //update the game manager for score and combo
    public void OnHitPlay(PlayerIndex playerIndex, int ballIndex, KickType kickType, float distanceX)
    {
        List<BallScript> balls = m_args.Player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.Player1Balls;
        }
        BallScript ball = balls[ballIndex];

        if (m_curRequiredColor != ball.GetColor())
        {//not correct color
            OnBallLost(playerIndex, ballIndex);
        }

        GameManagerOnBallHit(playerIndex);

        BallScript otherBall = balls[1 - ballIndex];
        otherBall.RemoveBallFromScene();

        Color color1 = GenerateRandomColor(Color.black);
        Color color2 = GenerateRandomColor(color1);
        UpdateNextBallColor(color1);

        if (FlipDistance())
        {
            distanceX *= (-1);
        }
        ActivateBallHitVisual(color1, ball.GetPosition());
        otherBall.GenerateNewBallInScene(color2, ball.GetPosition(), ball.GetVelocityY(), ball.GetVelocityX());
        ball.OnHitPlay(kickType, distanceX, color1, true);
        otherBall.OnHitPlay(kickType, (-1) * distanceX, color2, false);
    }
    void UpdateNextBallColor(Color color)
    {
        m_curRequiredColor = color;
        m_args.GameCanvas.UpdateNextBallColor(color);

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

    private Color GenerateRandomColor(Color curHitColor)
    {
        Color choosenColor = curHitColor;
        while (choosenColor == curHitColor)
        {
            int rnd = Random.Range(0, ballColors.Count);
            choosenColor = ballColors[rnd];
        }
        return choosenColor;
        //return Color.yellow;
    }


    //should turn off the balls
    public void TimeIsOver()
    {
        foreach (BallScript ball in m_args.Player1Balls)
        {
            ball.RemoveBallFromScene();

        }
        foreach (BallScript ball in m_args.Player2Balls)
        {
            ball.RemoveBallFromScene();
        }
    }


    //should start new turn
    public void OnNewBallInScene(PlayerIndex playerIndex)
    {
        List<BallScript> balls = m_args.Player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.Player1Balls;
        }
        BallScript ball = balls[0];
        //Color color = GenerateRandomColor(Color.black);
        Color color = Color.white;
        ball.OnNewBallInScene(color);
        UpdateNextBallColor(color);


    }


    //return the balls position
    public Vector3[] GetBallsPosition(PlayerIndex playerIndex)
    {
        List<BallScript> balls = m_args.Player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.Player1Balls;
        }

        Vector3[] ballsPositions = new Vector3[2];
        ballsPositions[0] = balls[0].gameObject.transform.position;
        ballsPositions[1] = balls[1].gameObject.transform.position;
        return ballsPositions;
    }



}
