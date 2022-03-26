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

    public delegate void onBallHit(PlayerIndex index, Color ballColor);
    public onBallHit GameManagerOnBallHit;


    #endregion

    #region serialized

    [SerializeField] List<Color> ballColors;




    #endregion


    #region private

    private GameBallsManagerArgs m_args;
    private bool m_initialized = false;



    #endregion

    public void Init(GameBallsManagerArgs args)
    {
        if (!m_initialized)
        {

            m_args = args;

            InitBalls();
            m_initialized = true;
        }
    }


    private void InitBalls()
    {
        int i = 0;
        foreach (BallScript ball in m_args.player1Balls)
        {
            ball.Init(m_args.ballArgs, PlayerIndex.First, i);
            ball.m_onBallLost = OnBallLost;
            i++;
        }

        i = 0;
        foreach (BallScript ball in m_args.player2Balls)
        {
            ball.Init(m_args.ballArgs, PlayerIndex.Second, i);
            ball.m_onBallLost = OnBallLost;
            i++;

            ball.RemoveBallFromScene();
        }

    }


    public void OnBallLost(PlayerIndex playerIndex, int ballIndex)
    {
        List<BallScript> balls = m_args.player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.player1Balls;
        }
        BallScript ball = balls[ballIndex];
        ball.RemoveBallFromScene();

        BallScript otherBall = balls[1 - ballIndex];
        if (!otherBall.IsInScene())
        {
            GameManagerOnTurnLost(playerIndex);
        }


    }

    //Generate another ball with different color and direction
    //update the game manager for score and combo
    public void OnHitPlay(PlayerIndex playerIndex, int ballIndex, KickType kickType, float distanceX)
    {
        List<BallScript> balls = m_args.player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.player1Balls;
        }
        BallScript ball = balls[ballIndex];
        BallScript otherBall = balls[1 - ballIndex];
        otherBall.RemoveBallFromScene();

        Color color1 = ball.GetColor();
        Color color2 = GenerateRandomColor();

        if (FlipDistance())
        {
            distanceX *= (-1);
        }
        otherBall.GenerateNewBallInScene(color2, ball.GetPosition(), ball.GetVelocityY(), ball.GetVelocityX());
        ball.OnHitPlay(kickType, distanceX, color1, true);
        otherBall.OnHitPlay(kickType, (-1) * distanceX, color2, false);
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
        //return Color.yellow;
    }


    //should turn off the balls
    public void TimeIsOver()
    {
        foreach (BallScript ball in m_args.player1Balls)
        {
            ball.RemoveBallFromScene();

        }
        foreach (BallScript ball in m_args.player2Balls)
        {
            ball.RemoveBallFromScene();
        }
    }


    //should start new turn
    public void OnNewBallInScene(PlayerIndex playerIndex)
    {
        List<BallScript> balls = m_args.player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.player1Balls;
        }
        BallScript ball = balls[0];
        Color color = GenerateRandomColor();
        ball.OnNewBallInScene(color);

    }


    //return the balls position
    public Vector3[] GetBallsPosition(PlayerIndex playerIndex)
    {
        List<BallScript> balls = m_args.player2Balls;
        if (playerIndex == PlayerIndex.First)
        {
            balls = m_args.player1Balls;
        }

        Vector3[] ballsPositions = new Vector3[2];
        ballsPositions[0] = balls[0].gameObject.transform.position;
        ballsPositions[1] = balls[1].gameObject.transform.position;
        return ballsPositions;
    }



}
