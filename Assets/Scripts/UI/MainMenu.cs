using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
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
    [SerializeField] private GameObject m_gameName;
    [SerializeField] private GameObject m_gameOption;
    [SerializeField] private RawImage m_background;
    [SerializeField] private GameObject m_BallOnePlayer;
    [SerializeField] private GameObject m_BallTwoPlayer;
    [SerializeField] private GameObject m_BallTurns;
    [SerializeField] private GameObject m_Player1;
    [SerializeField] private GameObject m_Player2;
    [SerializeField] private TMP_InputField m_nameTMP;
    [SerializeField] private GameObject m_ballName;




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

        AnalyticsManager.CommitData(
                    "App_Launched");

        m_anim = GetComponent<Animator>();
        m_camera = Camera.main;
        m_eventSystem = EventSystem.current;
        backgroundsList = Resources.LoadAll<Texture2D>(backgroundsPath);

        m_background.texture = ChooseRandomBackground();

        m_posBallOnePlayer = m_BallOnePlayer.transform.localPosition;
        m_posBallTwoPlayer = m_BallTwoPlayer.transform.localPosition;
        m_posBallTurns = m_BallTurns.transform.localPosition;

        SetPlayerPrefs();

        EventManager.Broadcast(EVENT.EventStartApp);

        m_StartGame.SetActive(true);
        m_gameOption.SetActive(false);
        m_gameName.SetActive(false);

    }

    public void StartButtonPressed()
    {
        /*m_StartGame.SetActive(false);
        m_gameOption.SetActive(true);*/
        m_anim.SetTrigger("Start_Pressed");
        EventManager.Broadcast(EVENT.EventMainMenu);
    }

    public void NameEntered()
    {
        string newName = m_nameTMP.text;
        print("Name entered: " + newName);
        if (newName == "")
        {
            m_nameTMP.Select();
            return;
        }

        PlayerPrefs.SetString("PlayerName", newName);
        m_anim.SetTrigger("Name_Entered");
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
        //Debug.Log("StartGame");

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
            OnActivateBallTween();
        }
    }

    public void OnTwoPlayer()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallTwoPlayer;
            m_gameType = GameType.TwoPlayer;
            OnActivateBallTween();
        }
    }

    public void OnTurnsGame()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallTurns;
            m_gameType = GameType.TurnsGame;
            OnActivateBallTween();
        }
    }


    public void OnTalTalGame()
    {
        if (m_userBallChoosen == null)
        {
            m_userBallChoosen = m_BallTwoPlayer;
            m_gameType = GameType.TalTalGame;
            OnActivateBallTween();
        }
    }


    private void OnActivateBallTween()
    {
        m_anim.SetTrigger("Match_Start");
    }

    public void AdvanceMenuName()
    {
        m_gameName.SetActive(true);
        m_StartGame.SetActive(false);
        m_Player1.SetActive(true);
        m_Player2.SetActive(true);
    }
    public void OpenMenuGameOptions()
    {
        m_gameOption.SetActive(true);
        m_gameName.SetActive(false);
    }


    private void SetPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("CompletedTutorial"))
        {
            PlayerPrefs.SetInt("CompletedTutorial", 0);
        }
        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerPrefs.SetString("PlayerName", "Player");
        }

    }

}
