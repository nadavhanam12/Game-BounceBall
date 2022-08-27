using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


class BallUIComponent
{
    public int OriginalIndex;
    public int CurIndexInOrder;
    public RawImage Image;
    public Vector2 OriginalPosition;

    public BallUIComponent(int originalIndex, int curIndexInOrder, RawImage image)
    {
        OriginalIndex = originalIndex;
        CurIndexInOrder = curIndexInOrder;
        Image = image;
        OriginalPosition = Image.rectTransform.anchoredPosition;
    }
}

public class NextColorScript : MonoBehaviour
{
    private bool m_initialized;
    private bool m_inTween;

    private BallUIComponent[] m_ballsArray;
    private int m_curBallCount;

    [SerializeField] private Color initColor;
    [SerializeField][Range(0, 0.5f)] private float m_tweenTime;
    [SerializeField][Range(0, 1)] private float m_refreshRate;
    [Range(0, 1f)] public float m_completeBallAlpha;

    [SerializeField] private int initStartBall = 3;
    private Color m_nextColor;
    private ParticleSystem m_particleSystem;

    private Queue<Color> m_colorsQueue;
    private Color[] m_nextColors;

    private BallUIComponent m_nextBallComponent;
    private BallUIComponent m_ballComponentToReset;
    private int m_curIndex;

    public void Init()
    {
        if (!m_initialized)
        {
            m_inTween = false;
            m_initialized = true;
            m_curBallCount = initStartBall;
            m_colorsQueue = new Queue<Color>();
            m_particleSystem = GetComponentInChildren<ParticleSystem>();

            InitBallsArray();
            InitListeners();
            ResetBalls();
        }
    }

    private void InitBallsArray()
    {

        RawImage[] rawImagesArray = GetComponentsInChildren<RawImage>();
        m_ballsArray = new BallUIComponent[rawImagesArray.Length];

        for (int i = 0; i < rawImagesArray.Length; i++)
        {
            m_ballsArray[i] = new BallUIComponent(i, i, rawImagesArray[i]);
            m_ballsArray[i].Image.color = initColor;
        }
    }

    private void InitListeners()
    {
        EventManager.AddHandler(EVENT.EventOnBallLost, ResetBalls);
    }
    private void OnDestroy()
    {
        EventManager.RemoveHandler(EVENT.EventOnBallLost, ResetBalls);
    }

    private void ResetBalls()
    {
        //print("ResetBalls");
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            m_ballsArray[i].Image.color = initColor;
            ScaleDown(m_ballsArray[i].Image);
            m_ballsArray[i].Image.rectTransform.anchoredPosition = m_ballsArray[i].OriginalPosition;
            m_ballsArray[i].CurIndexInOrder = m_ballsArray[i].OriginalIndex;
        }
        m_curBallCount = initStartBall;
    }

    public void SetNextBallImage(Color curColor, Color[] nextColors, bool shouldEmitParticles)
    {
        if ((shouldEmitParticles) && (m_nextColor != initColor))
            EmitBallParticles(m_nextColor);
        m_nextColors = nextColors;
        m_colorsQueue.Enqueue(curColor);
        StartCoroutine(PullColors());

    }

    private IEnumerator PullColors()
    {
        while (m_colorsQueue.Count > 0)
        {
            if (m_inTween)
            {
                yield return new WaitForSeconds(m_refreshRate);
            }
            else
            {
                m_inTween = true;
                Color nextColor = m_colorsQueue.Dequeue();
                SetColor(nextColor);
                yield return new WaitForSeconds(m_refreshRate);
            }
        }
    }


    private void SetColor(Color color)
    {
        m_nextColor = color;
        m_curIndex = m_curBallCount % m_ballsArray.Length;
        m_nextBallComponent = m_ballsArray[m_curIndex];
        m_curBallCount++;

        MoveImagesLeftVFX();
        UpdateCurBallColor();
        UpdatePrevBallColor();
        Invoke("UpdateNextBallsColor", m_tweenTime);
    }



    private void UpdateNextBallsColor()
    {
        int startIndex = m_nextBallComponent.OriginalIndex;
        BallUIComponent curComponent;
        for (int i = 0; i < m_ballsArray.Length / 2; i++)
        {
            //print((startIndex + i + 1) % m_ballsArray.Length);
            curComponent = m_ballsArray[(startIndex + i + 1) % m_ballsArray.Length];
            if (curComponent.Image.color != m_nextColors[i])
            {
                curComponent.Image.color = m_nextColors[i];
            }

        }
    }

    private void UpdatePrevBallColor()
    {
        BallUIComponent prevBall;
        if (m_curIndex != 0)
            prevBall = m_ballsArray[m_curIndex - 1];
        else
            prevBall = m_ballsArray[m_ballsArray.Length - 1];

        Color temp = prevBall.Image.color;
        temp.a = m_completeBallAlpha;
        prevBall.Image.color = temp;

    }


    private void MoveImagesLeftVFX()
    {
        BallUIComponent curBallComponent;
        float targetX;
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            curBallComponent = m_ballsArray[i];

            if (curBallComponent.CurIndexInOrder == 0)
            {
                //print("reset move : " + indexToMove);
                targetX = m_ballsArray[0].OriginalPosition.x - 100;
                m_ballComponentToReset = curBallComponent;
                LeanTween.moveX(curBallComponent.Image.rectTransform, targetX, m_tweenTime).setOnComplete(ResetLastBall);
                //LeanTween.alpha(curBallComponent.Image.rectTransform, 0, m_tweenTime);
                Color temp = curBallComponent.Image.color;
                temp.a = 0.0f;
                curBallComponent.Image.color = temp;
            }
            else
            {
                //print("normal move : " + indexToMove);
                targetX = m_ballsArray[curBallComponent.CurIndexInOrder - 1].OriginalPosition.x;
                LeanTween.moveX(curBallComponent.Image.rectTransform, targetX, m_tweenTime);
                curBallComponent.CurIndexInOrder--;
            }

        }


    }

    private void UpdateCurBallColor()
    {
        m_nextBallComponent.Image.color = m_nextColor;
        LeanTween.alpha(m_nextBallComponent.Image.rectTransform, 255, m_tweenTime / 2);
        ScaleUp(m_nextBallComponent.Image);
    }


    public void ScaleUp(RawImage image)
    {
        //image.rectTransform.localScale = Vector3.one * 1.3f;
        LeanTween.scale(image.gameObject, Vector3.one * 2f, m_tweenTime)
               .setLoopPingPong(1)
               .setEase(LeanTweenType.linear)
                .setOnComplete(FinishedTween)
                ;
        image.rectTransform.localScale = Vector3.one * 1f;
    }
    public void ScaleDown(RawImage image)
    {
        LeanTween.scale(image.gameObject, Vector3.one * 0.85f, m_tweenTime)
                .setEase(LeanTweenType.easeInOutBack)
                //.setOnComplete(FinishedTween)
                ;

    }
    private void FinishedTween()
    {
        m_inTween = false;
    }


    private void ResetLastBall()
    {
        m_ballComponentToReset.Image.rectTransform.anchoredPosition = m_ballsArray[m_ballsArray.Length - 1].OriginalPosition;
        m_ballComponentToReset.CurIndexInOrder = m_ballsArray.Length - 1;
        //m_ballComponentToReset.Image.color = initColor;
    }

    private void EmitBallParticles(Color color)
    {
        ParticleSystem.MainModule main = m_particleSystem.main;
        main.startColor = color;
        m_particleSystem.Emit(120);
    }

}
