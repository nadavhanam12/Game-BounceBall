using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    #region events


    #endregion

    #region serialized

    [SerializeField] private GameObject m_StartGame;
    [SerializeField] private GameObject m_gameOption;
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
    private GameType m_gameType;
    private float m_inputDelay = 0.0f;

    private Camera m_camera;
    private EventSystem m_eventSystem;
    private string m_gameSceneName = "GameScene";


    #endregion






    void Awake()
    {
        Application.targetFrameRate = 60;

        m_anim = GetComponent<Animator>();
        m_camera = Camera.main;
        m_eventSystem = EventSystem.current;
        backgroundsList = Resources.LoadAll<Texture2D>(backgroundsPath);

        m_background.texture = ChooseRandomBackground();

        m_posBallOnePlayer = m_BallOnePlayer.transform.localPosition;
        m_posBallTwoPlayer = m_BallTwoPlayer.transform.localPosition;
        m_posBallTurns = m_BallTurns.transform.localPosition;

        SetPlayerPrefs();


        m_gameOption.SetActive(false);
        m_StartGame.SetActive(true);




    }

    void Start()
    {
        //Debug.Log("StartApp");
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

    }

    public Texture ChooseRandomBackground()
    {
        int rnd = Random.Range(0, backgroundsList.Length);
        return backgroundsList[rnd];
    }
    public async void OnRestart()
    {
        AsyncOperation operation = SceneManager.UnloadSceneAsync(m_gameSceneName);
        while (!operation.isDone)
        {
            await Task.Delay(30);
        }
        StartGameScene();
    }

    public async void StartGameScene()
    {
        Debug.Log("StartGame");

        AsyncOperation operation = SceneManager.LoadSceneAsync(m_gameSceneName, LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            await Task.Delay(30);
        }
        m_gameManager = FindObjectOfType<GameManagerScript>(true);
        ToggleMenu(false);
        if (m_gameManager != null)
        {
            GameArgs args = new GameArgs(m_gameType);
            m_gameManager.SetGameArgs(args);
        }
        else
        {
            BackToMenu();
        }
    }
    public async void BackToMenu()
    {
        print("BackToMenu");
        AsyncOperation operation = SceneManager.UnloadSceneAsync(m_gameSceneName);
        while (!operation.isDone)
        {
            await Task.Delay(30);
        }

        m_BallOnePlayer.transform.localPosition = m_posBallOnePlayer;
        m_BallOnePlayer.gameObject.SetActive(true);
        m_BallTwoPlayer.transform.localPosition = m_posBallTwoPlayer;
        m_BallTwoPlayer.gameObject.SetActive(true);
        m_BallTurns.transform.localPosition = m_posBallTurns;
        m_BallTurns.gameObject.SetActive(true);

        ChooseRandomBackground();
        m_userBallChoosen = null;
        m_StartGame.SetActive(true);
        m_gameOption.SetActive(false);
        ToggleMenu(true);
    }

    private void ToggleMenu(bool activateMenu)
    {
        m_camera.gameObject.SetActive(activateMenu);
        m_eventSystem.gameObject.SetActive(activateMenu);
        this.gameObject.SetActive(activateMenu);
    }


    public void OnOnePlayer()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallOnePlayer;
            m_gameType = GameType.OnePlayer;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }

    public void OnTwoPlayer()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallTwoPlayer;
            m_gameType = GameType.TwoPlayer;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }

    public void OnTurnsGame()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallTurns;
            m_gameType = GameType.TurnsGame;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }


    public void OnTalTalGame()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallTwoPlayer;
            m_gameType = GameType.TalTalGame;
            Invoke("OnActivateBallTween", m_inputDelay);
        }
    }

    public void OnHighlights()
    {


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

    private void SetPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("CompletedTutorialSinglePlayer"))
        {
            PlayerPrefs.SetInt("CompletedTutorialSinglePlayer", 0);
        }
        if (!PlayerPrefs.HasKey("CompletedTutorialKickKick"))
        {
            PlayerPrefs.SetInt("CompletedTutorialKickKick", 0);
        }
        if (!PlayerPrefs.HasKey("CompletedTutorialTurns"))
        {
            PlayerPrefs.SetInt("CompletedTutorialTurns", 0);
        }
    }

}
