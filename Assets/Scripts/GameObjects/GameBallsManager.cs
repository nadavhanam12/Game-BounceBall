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


    #endregion


    #region private

    private GameBallsManagerArgs m_args;
    private bool m_initialized = false;
    private Queue<BallHitVisual> m_hitVisualsQueue = new Queue<BallHitVisual>();
    private Color m_curRequiredColor;
    private GameCanvasScript m_gameCanvas;

    private bool isGamePaused = false;
    private BallScript[] m_ballsArray;
    private BallScript firstBall;
    private int m_nextBallIndex;
    private int m_correctBallIndex;


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

            InitBalls();
            InitHitBallVisuals();
            m_initialized = true;
        }
    }
    void InitHitBallVisuals()
    {
        BallHitVisual curBallVisuals;
        Vector3 ballScale = firstBall.transform.localScale;
        for (int i = 0; i < 6; i++)
        {
            curBallVisuals = Instantiate(m_ballHitVisualPrefab, gameObject.transform);
            curBallVisuals.Init(ballScale);
            m_hitVisualsQueue.Enqueue(curBallVisuals);
        }
    }

    private void InitBalls()
    {
        m_nextBallIndex = 0;
        firstBall = GetComponentInChildren<BallScript>();
        m_ballsArray = new BallScript[6];
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

        m_ballsArray[ballIndex].RemoveBallFromScene();
        if (!IsBallsInPLay())
        {
            GameManagerOnTurnLost();
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
        BallScript nextBall = m_ballsArray[m_nextBallIndex];
        m_nextBallIndex++;
        m_nextBallIndex = m_nextBallIndex % m_ballsArray.Length;
        while (nextBall.IsInScene())
        {
            print("nextBall.IsInScene()");
            nextBall = m_ballsArray[m_nextBallIndex];
            m_nextBallIndex++;
            m_nextBallIndex = m_nextBallIndex % m_ballsArray.Length;
        }
        return nextBall;
    }

    //Generate another ball with different color and direction
    //update the game manager for score and combo
    private void OnHitPlay(PlayerIndex playerIndex, int ballIndex, KickType kickType)
    {
        BallScript ball = m_ballsArray[ballIndex];
        if (m_curRequiredColor != ball.GetColor())
        {//not correct color
            WrongBallHit(ballIndex);
            return;
        }

        GameManagerOnBallHit(playerIndex);

        BallScript otherBall = GetNextBall();
        Color color1 = GenerateRandomColor(Color.black);
        Color color2 = GenerateRandomColor(color1);
        if (m_args.GameType == GameType.TurnsGame)
        {
            if (playerIndex == PlayerIndex.Second)
            {
                color1.a = m_opponentBallAlpha;
                color2.a = m_opponentBallAlpha;
            }
        }
        UpdateNextBallColor(color1);


        ActivateBallHitVisual(color1, ball.GetPosition());
        otherBall.GenerateNewBallInScene(color2, ball.GetPosition(), ball.GetVelocityY(), ball.GetVelocityX());

        float distanceX = 1f;
        if (FlipDistance())
        {
            distanceX *= (-1);
        }
        //maybe add here lower and upper bound for disX, can make it random or like now that deppend on the kik itself


        distanceX = RandomDisX(distanceX);

        //print("distanceX: " + distanceX);
        ball.OnHitPlay(kickType, distanceX, color1, true);
        otherBall.OnHitPlay(kickType, (-1) * distanceX, color2, false);
    }

    float RandomDisX(float distanceX)
    {
        float rnd = Random.Range(0.75f, 1.75f);
        if (distanceX >= 0)
        {
            return rnd;
        }
        else
        {
            return -rnd;
        }
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
        Destroy();
    }


    //should start new turn
    public void OnNewBallInScene(PlayerIndex playerIndex, PlayerIndex nextPlayerIndex = PlayerIndex.First)
    {
        m_correctBallIndex = m_nextBallIndex;
        BallScript ball = GetNextBall();
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
        UpdateNextBallColor(color);
        if (isGamePaused)
        {
            SetGamePause(isGamePaused);
        }


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
}
