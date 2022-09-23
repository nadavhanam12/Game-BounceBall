using System.Threading.Tasks;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameManagerPvPMode : GameManagerAbstract
{
    protected override void InitBallsManager()
    {
        GameBallsManagerArgs ballsManagerArgs = new GameBallsManagerArgs();
        ballsManagerArgs.BallArgs = m_playersDataContainer.ballArgs;
        ballsManagerArgs.GameType = m_gameArgs.GameType;
        ballsManagerArgs.GameCanvas = m_gameCanvas;

        GameBallsManagerPvP gameBallsManagerPvP = m_ballsManager.gameObject.AddComponent<GameBallsManagerPvP>();
        gameBallsManagerPvP.CopyParameters(m_ballsManager);
        Destroy(m_ballsManager);
        m_ballsManager = gameBallsManagerPvP;

        m_ballsManager.GameManagerOnBallHit = OnBallHit;
        m_ballsManager.GameManagerOnTurnLost = onTurnLost;
        m_ballsManager.Init(ballsManagerArgs);
    }

    protected override void InitGameCanvas()
    {
        GameCanvasArgs canvasArgs = new GameCanvasArgs();
        canvasArgs.GameType = m_gameArgs.GameType;
        canvasArgs.MatchTime = m_gameArgs.MatchTime;
        canvasArgs.PlayerColor1 = m_playerData1.Color;
        canvasArgs.PlayerColor2 = m_playerData2.Color;
        canvasArgs.PlayerImage1 = m_playerData1.Image;
        canvasArgs.PlayerImage2 = m_playerData2.Image;
        canvasArgs.ConfettiManager = m_confettiManager;
        m_gameCanvas.Init(canvasArgs);
        m_gameCanvas.m_onTimeIsOver = () => m_timeIsOver = true;

        PlayerArgs playerData;
        if (PhotonNetwork.IsMasterClient)
            playerData = m_playerData1;
        else
            playerData = m_playerData2;
        InitGameCanvasDelegates(playerData);
    }

    protected override void InitPlayers()
    {
        m_playerContainer = FindObjectOfType<PlayerContainer>(true);
        PlayerIndex playerIndex;
        if (PhotonNetwork.IsMasterClient)
            playerIndex = PlayerIndex.First;
        else
            playerIndex = PlayerIndex.Second;

        PlayerScript playerScript = m_playerContainer.SpawnPlayerPvP(playerIndex);
        int playerViewId = playerScript.GetComponent<PhotonView>().ViewID;

        this.photonView.RPC("InitPlayer", RpcTarget.All, playerIndex.ToString(), playerViewId);
    }

    [PunRPC]
    void InitPlayer(string playerIndex, int playerViewId)
    {
        //print("PunRPC: InitPlayer " + playerIndex);
        GameObject playerGameObject = PhotonView.Find(playerViewId).gameObject;
        Destroy(playerGameObject.GetComponent<PlayerScript>());

        PlayerScript playerScript = playerGameObject.AddComponent<PlayerScriptPvP>();

        playerGameObject.gameObject.SetActive(false);
        PlayerArgs playerData;
        if (playerIndex == PlayerIndex.First.ToString())
            playerData = m_playerData1;
        else
            playerData = m_playerData2;

        playerData.PlayerScript = playerScript;
        m_playerContainer.SetPlayerParent(playerIndex, playerScript);
        playerScript.Init(playerData);
    }


    void HandleNewPlayerCreated(EventData photonEvent)
    {
        print("HandleNewPlayerCreated");
        int viewId = (int)photonEvent.CustomData;
        PlayerScript playerScript = PhotonView.Find(viewId).GetComponent<PlayerScript>();
        if (!playerScript)
            Debug.LogError("HandleNewPlayerCreated: got null");

        if (PhotonNetwork.IsMasterClient)
            m_playerData2.PlayerScript = playerScript;
        else
            m_playerData1.PlayerScript = playerScript;
    }


    public override void StartGameScene()
    {
        if (m_gameArgs == null)
        {
            print("cant Start Game Scene without game args");
            return;
        }
        this.gameObject.SetActive(true);
        SendDataMatchStarted();
        EventManager.Broadcast(EVENT.EventStartGameScene);
        m_shouldRestart = false;
        AfterTutorial();
    }

    protected override void AfterTutorial()
    {
        m_gameCanvas.ActivateTimer(true);
        //Invoke("StartCountdown", m_gameArgs.CountDownDelay);
        FinishGameCountdown();
    }
    protected override void InitGameMood(bool throwNewBall = true)
    {
        m_ballsManager.UpdateNextBallColor(Color.white, false);
        if (!PhotonNetwork.IsMasterClient)
            return;
        m_curPlayerTurn = PlayerIndex.First;
        m_playerData1.PlayerScript?.StartTurn(throwNewBall);
        m_playerData2.PlayerScript?.LostTurn();
    }


    protected override async void SwitchPlayerTurn(bool shouldSwitchTurn = true)
    {
        m_gameCanvas.SwitchTurn(m_curPlayerTurn != PlayerIndex.First);

        if (m_curPlayerTurn != PlayerIndex.First)
        {//win player1
            m_playerData1.PlayerScript.Win();
            m_playerData2.PlayerScript?.Lose();
        }
        else
        {//win player2
            m_playerData1.PlayerScript.Lose();
            m_playerData2.PlayerScript?.Win();
        }

        await Task.Delay(2000);
        InitPlayersStatus();

        await Task.Delay(1000);
        SwitchPlayerTurnAfterWait(true, shouldSwitchTurn);
    }

    public override void onTurnLost()
    {
        PlayerArgs playerData;
        EventManager.Broadcast(EVENT.EventOnBallLost);
        if (m_curPlayerTurn == PlayerIndex.First)
            playerData = m_playerData1;
        else
            playerData = m_playerData2;

        playerData.CurComboIndex = -1;
        playerData.CurCombo = 0;
        m_gameCanvas.SetCombo(0);

        playerData = playerData == m_playerData1 ? m_playerData2 : m_playerData1;
        playerData.CurScore++;
        if (playerData == m_playerData1)
            m_gameCanvas.CheerActivate();
        else
            m_gameCanvas.CheerActivateSecondPlayer();

        m_gameCanvas.SetNormalScore(m_playerData1.CurScore, m_playerData2.CurScore);
        if (m_timeIsOver)
            MatchEnd();
        else
            SwitchPlayerTurn(false);

    }

    public override void OnBallHit(PlayerIndex playerIndex)
    {
        BroadcastKickType();
        m_gameCanvas.IncrementCombo();
        SwitchPlayerTurnAfterWait(false);
    }


    public override void GameIsOver()
    {
        //print("Time is over");
        /* if (!m_isGamePause)
         {
             SetGamePause(true);
             m_ballsManager.TimeIsOver();//should turn off the balls
             m_gameCanvas.OnPvPEnd(m_playerData1.CurScore, m_playerData2.CurScore);
         }*/
    }

    protected override void UpdatePlayerPrefsCompletedTutorial() { }

}
