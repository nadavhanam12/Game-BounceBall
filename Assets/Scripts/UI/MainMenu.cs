using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    #region events


    #endregion

    #region serialized

    [SerializeField] private GameObject m_StartGame;
    [SerializeField] private GameObject m_gameOption;
    [SerializeField] private GameObject m_gameScene;
    [SerializeField] private RawImage m_background;
    [SerializeField] private GameObject m_BallOnePlayer;
    [SerializeField] private GameObject m_BallTwoPlayer;
    [SerializeField] private GameObject m_BallTurns;




    #endregion

    #region private
    private GameManagerScript m_gameManager;
    private Texture2D[] backgroundsList;
    private Animator m_anim;
    private Vector3 m_posBallOnePlayer;
    private Vector3 m_posBallTwoPlayer;
    private Vector3 m_posBallTurns;
    const string backgroundsPath = "BackGround";
    private int m_highMove = 150;
    private float m_timeToTween = 1f;
    private GameObject m_userBallChoosen = null;
    private float m_inputDelay = 0.0f;


    #endregion






    void Awake()
    {
        Application.targetFrameRate = 60;

        m_anim = GetComponent<Animator>();
        backgroundsList = Resources.LoadAll<Texture2D>(backgroundsPath);

        m_background.texture = ChooseRandomBackground();

        m_posBallOnePlayer = m_BallOnePlayer.transform.localPosition;
        m_posBallTwoPlayer = m_BallTwoPlayer.transform.localPosition;
        m_posBallTurns = m_BallTurns.transform.localPosition;


        m_gameOption.SetActive(false);
        m_StartGame.SetActive(true);




    }

    void Start()
    {
        Debug.Log("StartApp");
        EventManager.Broadcast(EVENT.EventStartApp);

    }
    public void StartGamePressed()
    {
        m_StartGame.SetActive(false);
        m_gameOption.SetActive(true);

        EventManager.Broadcast(EVENT.EventMainMenu);

        ApplyRandomTweens();
    }


    private void ApplyRandomTweens()
    {
        //apply idle Tweens

        // int high = 500;
        // LeanTween.moveLocalY(m_BallImage, high, m_timeToTween)
        //              .setEase(LeanTweenType.easeOutSine)
        //              .setLoopPingPong(5)
        //              .setOnComplete(() => high = high - 100)
        //              .setOnCompleteOnRepeat(true);


    }

    public Texture ChooseRandomBackground()
    {
        int rnd = Random.Range(0, backgroundsList.Length);
        return backgroundsList[rnd];
    }

    public void StartGameScene()
    {
        Debug.Log("StartGame");
        SceneManager.LoadSceneAsync("GameScene");
        //SceneManager.SetActiveScene("GameScene");
    }
    public void BackToMenu()
    {
        m_BallOnePlayer.transform.localPosition = m_posBallOnePlayer;
        m_BallOnePlayer.gameObject.SetActive(true);
        m_BallTwoPlayer.transform.localPosition = m_posBallTwoPlayer;
        m_BallTwoPlayer.gameObject.SetActive(true);
        m_BallTurns.transform.localPosition = m_posBallTurns;
        m_BallTurns.gameObject.SetActive(true);

        ChooseRandomBackground();
        m_userBallChoosen = null;
        this.gameObject.SetActive(true);
        m_StartGame.SetActive(true);
        m_gameOption.SetActive(false);
    }


    public void OnOnePlayer()
    {
        if (m_userBallChoosen == null)
        {
            //m_anim.Play("OnePlayerChoose", -1, 0f);
            m_userBallChoosen = m_BallOnePlayer;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }

    public void OnTwoPlayer()
    {
        if (m_userBallChoosen == null)
        {
            //m_anim.Play("TwoPlayerChoose", -1, 0f);
            m_userBallChoosen = m_BallTwoPlayer;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }

    public void OnTurnsGame()
    {
        if (m_userBallChoosen == null)
        {
            //m_anim.Play("TurnsGameChoose", -1, 0f);
            m_userBallChoosen = m_BallTurns;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }


    void OnActivateBallTween()
    {
        EventManager.Broadcast(EVENT.EventButtonClick);

        GameObject ballObject = m_userBallChoosen;
        Vector3 ballObjectPosition = ballObject.transform.localPosition;
        LeanTween.rotateZ(ballObject, -1080, m_timeToTween);

        LeanTween.moveLocalY(ballObject, ballObjectPosition.y + m_highMove, m_timeToTween / 4f)
        .setEase(LeanTweenType.easeOutSine)
        .setLoopPingPong(1).
        setOnComplete(
         () =>
         {
             LeanTween.moveLocalY(ballObject, ballObjectPosition.y + (m_highMove / 3), m_timeToTween / 6f)
             .setEase(LeanTweenType.easeOutSine)
             .setLoopPingPong(1);
         });
        //LeanTween.moveLocalY(m_BallTwoPlayer, m_highMove, m_timeToTween).setEase(LeanTweenType.easeInBounce);
        LeanTween.moveLocalX(ballObject, 0, m_timeToTween / 2f)
        //.setEase(LeanTweenType.easeInOutSine)
        .setOnComplete(
         () =>
         {
             LeanTween.moveLocalX(ballObject, (-1) * ballObjectPosition.x, m_timeToTween / 2f)
             //.setEase(LeanTweenType.easeInOutSine)
             .setOnComplete(() =>
             {

                 ballObject.SetActive(false);
                 Invoke("StartGameScene", 0.05f);
             });
         });


        // BackToMenu();

    }

    public void OnFindPlayer()
    {


    }

    public void OnHighlights()
    {


    }

}
