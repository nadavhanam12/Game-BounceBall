using System;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviourPunCallbacks
{
    #region serialized
    [SerializeField] bool m_testingPvP;
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
    private GameSceneSetUp m_gameSceneSetUpScript;
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
        if (m_testingPvP)
        {
            float waitTime = 3f;
#if UNITY_EDITOR
            waitTime = 0;
#endif
            Invoke("TestPvPMode", waitTime);
        }

    }

    public void StartButtonPressed()
    {
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
    }

    public void StartGameScene()
    {
        //Debug.Log("StartGameScene");
        m_eventSystem.gameObject.SetActive(false);

        if (m_gameType == GameType.PvP)
            this.photonView.RPC("LoadScenePvP", RpcTarget.All);
        else
            LoadSceneSinglePlayerAndPvE();
    }

    GameArgs CreateGameArgs(GameType m_gameType)
    {
        GameArgs args = new GameArgs(m_gameType);
        args.ShouldPlayTutorial = !PlayerPrefsHasCompletedTutorial(m_gameType);
        args.ShouldPlayCountdown = true;
        return args;
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
            await Task.Delay(30);

        //ChooseRandomBackground();
        m_userChoosen = false;
        m_StartGame.SetActive(true);
        m_gameOption.SetActive(false);
        ToggleMenu(true);
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
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

    public void StartPvP()
    {
        m_gameType = GameType.PvP;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        StartGameScene();
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


    bool PlayerPrefsHasCompletedTutorial(GameType gameType)
    {
        string playerPrefsGameTutorial;
        if (gameType == GameType.SinglePlayer)
            playerPrefsGameTutorial = "CompletedSinglePlayerTutorial";
        else if (gameType == GameType.PvE)
            playerPrefsGameTutorial = "CompletedTalTalTutorial";
        else
            return true;
        return Convert.ToBoolean(PlayerPrefs.GetInt(playerPrefsGameTutorial));
    }

    [PunRPC]
    async Task LoadScenePvP()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        m_gameType = GameType.PvP;
        AsyncOperation operation = SceneManager.LoadSceneAsync(m_gameSceneName, LoadSceneMode.Additive);
        //operation.allowSceneActivation = false;
        while (!operation.isDone)
            await Task.Delay(30);

        ApplyArgs();
    }

    async Task LoadSceneSinglePlayerAndPvE()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(m_gameSceneName, LoadSceneMode.Additive);
        //operation.allowSceneActivation = false;
        while (!operation.isDone)
            await Task.Delay(30);

        ApplyArgs();
    }

    void ApplyArgs()
    {
        m_gameSceneSetUpScript = FindObjectOfType<GameSceneSetUp>(true);

        if (m_gameSceneSetUpScript != null)
        {
            GameArgs gameArgs = CreateGameArgs(m_gameType);
            m_gameSceneSetUpScript.SetGameSceneArgs(gameArgs);
        }
        else
            BackToMenu();
    }

    void TestPvPMode()
    {
        print("TestPvPMode");
        PhotonNetwork.ConnectUsingSettings();
    }


    public override void OnConnectedToMaster()
    {
        //Debug.Log("Connection made to " + PhotonNetwork.CloudRegion + " server.");
        if (m_testingPvP)
            PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public override void OnJoinedRoom()
    {
        //Debug.Log("OnJoinedRoom");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (m_testingPvP)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2)
                StartPvP();
    }
}
