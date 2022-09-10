using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
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
    [SerializeField] private GameObject m_gameCredits;
    [SerializeField] private RawImage m_background;
    [SerializeField] private GameObject m_Player1;
    [SerializeField] private GameObject m_Player2;
    [SerializeField] private TMP_InputField m_nameTMP;
    [SerializeField] private GameObject m_ballName;
    [SerializeField] private GameObject m_pleasePlaySinglePlayerObject;





    #endregion

    #region private
    private GameManagerScript m_gameManager;
    private Texture2D[] backgroundsList;
    private Animator m_anim;
    const string backgroundsPath = "BackGround";
    private bool m_userChoosen = false;
    private GameType m_gameType;
    private Camera m_camera;
    private EventSystem m_eventSystem;
    private string m_gameSceneName = "GameScene";
    LoadingPage m_loadingPanel;

    #endregion





    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    void Awake()
    {
        Application.targetFrameRate = 60;

        AnalyticsManager.Instance().CommitData(
                    AnalyticsManager.AnalyticsEvents.Event_App_Launched);

        m_anim = GetComponent<Animator>();
        m_camera = Camera.main;
        m_eventSystem = EventSystem.current;
        backgroundsList = Resources.LoadAll<Texture2D>(backgroundsPath);

        //m_background.texture = ChooseRandomBackground();

        SetPlayerPrefs();

        EventManager.Broadcast(EVENT.EventStartApp);

        m_StartGame.SetActive(true);
        m_gameOption.SetActive(false);
        m_gameName.SetActive(false);
        m_gameCredits.SetActive(false);
        InitMultiPlayerPanels();

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
        int rnd = UnityEngine.Random.Range(0, backgroundsList.Length);
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
        UnloadMenu();
    }

    public async void StartGameScene()
    {
        //Debug.Log("StartGame");
        m_eventSystem.gameObject.SetActive(false);

        if (m_gameType == GameType.PvP)
        {
            PhotonNetwork.LoadLevel(m_gameSceneName);
            while (PhotonNetwork.LevelLoadingProgress < 0.98f)
            {
                Debug.Log(PhotonNetwork.LevelLoadingProgress);
                await Task.Delay(30);
            }
        }
        else
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(m_gameSceneName, LoadSceneMode.Additive);
            //operation.allowSceneActivation = false;
            while (!operation.isDone)
                await Task.Delay(30);
        }
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("m_gameSceneName"));
        //await Task.Delay(500);
        m_gameManager = FindObjectOfType<GameManagerScript>(true);

        if (m_gameManager != null)
        {
            GameArgs args = new GameArgs(m_gameType);
            m_gameManager.SetGameArgs(args);
        }
        else
            BackToMenu();
    }

    public void UnloadMenu()
    {
        ToggleMenu(false);
    }
    public async void BackToMenu()
    {
        print("BackToMenu");
        AsyncOperation operation = SceneManager.UnloadSceneAsync(m_gameSceneName);
        while (!operation.isDone)
        {
            await Task.Delay(30);
        }

        ChooseRandomBackground();
        m_userChoosen = false;
        m_StartGame.SetActive(true);
        m_gameOption.SetActive(false);
        ToggleMenu(true);

    }

    private void ToggleMenu(bool activateMenu)
    {
        //m_camera.gameObject.SetActive(activateMenu);
        m_eventSystem.gameObject.SetActive(activateMenu);
        this.gameObject.SetActive(activateMenu);
    }


    public void OnSinglePlayer()
    {
        print("OnSinglePlayer");
        if (!m_userChoosen)
        {
            m_userChoosen = true;
            m_gameType = GameType.SinglePlayer;
            OnActivateBallTween();
        }
    }

    public void OnPvE()
    {
        if (!m_userChoosen)
        {
            if (!Convert.ToBoolean(PlayerPrefs.GetInt("CompletedSinglePlayerTutorial")))
            {
                m_pleasePlaySinglePlayerObject.SetActive(true);
                return;
            }
            m_userChoosen = true;
            m_gameType = GameType.PvE;
            OnActivateBallTween();
        }
    }
    public void OnPvP()
    {
        if (!m_userChoosen)
        {
            m_userChoosen = true;
            m_loadingPanel.Activate();
            m_gameOption.SetActive(false);
            m_Player1.SetActive(false);
            m_Player2.SetActive(false);
        }
    }

    public async void StartPvP()
    {
        m_gameType = GameType.PvP;
        StartGameScene();
        await Task.Delay(500);
        UnloadMenu();
    }


    private void OnActivateBallTween()
    {
        m_anim.SetTrigger("Match_Start");
    }

    public void AdvanceMenuName()
    {
        if (PlayerPrefs.GetString("PlayerName") == "Player")
            m_gameName.SetActive(true);
        else
            m_gameOption.SetActive(true);

        m_StartGame.SetActive(false);
        m_Player1.SetActive(true);
        m_Player2.SetActive(true);
    }
    public void OpenMenuGameOptions()
    {
        m_gameOption.SetActive(true);
        m_gameName.SetActive(false);
        m_Player1.SetActive(true);
        m_Player2.SetActive(true);
        m_userChoosen = false;
    }


    private void SetPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("CompletedSinglePlayerTutorial"))
            PlayerPrefs.SetInt("CompletedSinglePlayerTutorial", 0);
        if (!PlayerPrefs.HasKey("CompletedTalTalTutorial"))
            PlayerPrefs.SetInt("CompletedTalTalTutorial", 0);
        if (!PlayerPrefs.HasKey("PlayerName"))
            PlayerPrefs.SetString("PlayerName", "Player");
    }

    public void OnCreditPage(bool toShow)
    {
        m_gameCredits.SetActive(toShow);
        m_gameOption.SetActive(!toShow);
        m_Player1.SetActive(!toShow);
        m_Player2.SetActive(!toShow);
    }


    void InitMultiPlayerPanels()
    {
        m_loadingPanel = GetComponentInChildren<LoadingPage>(true);
        m_loadingPanel.gameObject.SetActive(false);

        GetComponentInChildren<MultiplayerLobby>(true)?.gameObject.SetActive(false);
        GetComponentInChildren<MultiPlayerRoom>(true)?.gameObject.SetActive(false);
        GetComponentInChildren<MultiPlayerErrorPage>(true)?.gameObject.SetActive(false);
    }

}
