using System.Threading.Tasks;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using static PlayerScript;

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
        m_gameCanvas.m_onTimeIsOver = TimeEnded;

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
        bool isMaster = PhotonNetwork.IsMasterClient;
        PlayerArgs playerData;
        if (playerIndex == PlayerIndex.First.ToString())
        {
            playerData = m_playerData1;
            playerData.Darker = !isMaster;
        }
        else
        {
            playerData = m_playerData2;
            playerData.Darker = isMaster;
        }

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

    protected override void InitData()
    {
        base.InitData();
        float[] bounds = new float[5];
        bounds[0] = m_gameBounds.GameUpperBound;
        bounds[1] = m_gameBounds.GameLowerBound;
        bounds[2] = m_gameBounds.GameLeftBound;
        bounds[3] = m_gameBounds.GameRightBound;
        bounds[4] = m_gameBounds.GamePlayGroundLowerBound;

        this.photonView.RPC("UpdateBoundsPvP", RpcTarget.AllBuffered, bounds);
    }

    [PunRPC]
    void UpdateBoundsPvP(float[] bounds)
    {
        //print("UpdateBoundsPvP");
        if (bounds[0] <= m_gameBounds.GameUpperBound)
            m_gameBounds.GameUpperBound = bounds[0];

        if (bounds[1] >= m_gameBounds.GameLowerBound)
            m_gameBounds.GameLowerBound = bounds[1];

        if (bounds[2] >= m_gameBounds.GameLeftBound)
            m_gameBounds.GameLeftBound = bounds[2];

        if (bounds[3] <= m_gameBounds.GameRightBound)
            m_gameBounds.GameRightBound = bounds[3];

        if (bounds[4] <= m_gameBounds.GamePlayGroundLowerBound)
            m_gameBounds.GamePlayGroundLowerBound = bounds[4];

        /*print(m_gameBounds.GameUpperBound);
        print(m_gameBounds.GameLowerBound);
        print(m_gameBounds.GameLeftBound);
        print(m_gameBounds.GameRightBound);
        print(m_gameBounds.GamePlayGroundLowerBound);*/

        m_gameBoundsData.ChangeBoundsPvP(m_gameBounds);
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
        if (!PhotonNetwork.IsMasterClient)
            return;
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

    protected override void InitPlayersStatus()
    {
        m_playerData1.PlayerScript?.InitPlayer(false);
        m_playerData2.PlayerScript?.InitPlayer(false);
    }

    public override void onTurnLost()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PlayerArgs playerData;
        EventManager.Broadcast(EVENT.EventOnBallLost);
        if (m_curPlayerTurn == PlayerIndex.First)
            playerData = m_playerData1;
        else
            playerData = m_playerData2;


        playerData.ComboSinceSpecialKick = 0;
        m_gameCanvas.SetSliderValue(0, m_gameArgs.ComboKicksAmount, m_curPlayerTurn);
        playerData.CurCombo = 0;
        playerData.PlayerScript.SetAllowedSpecialKick(false, true);
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

    public override void OnBallHit(PlayerIndex playerIndex, KickType kickType)
    {
        BroadcastKickType();
        m_gameCanvas.IncrementCombo();
        SwitchPlayerTurnAfterWait(false);
        CheckPlayerCombo(playerIndex);
    }


    public override void GameIsOver()
    {
        //print("Time is over");
        if (!m_isGamePause)
        {
            SetGamePause(true);
            m_ballsManager.TimeIsOver();//should turn off the balls
            m_gameCanvas.OnPvPEnd(m_playerData1.CurScore, m_playerData2.CurScore);
            this.photonView.RPC("EndGameRPC", RpcTarget.Others);
        }
    }

    [PunRPC]
    void EndGameRPC()
    {
        SetGamePause(true);
        m_playerData1.PlayerScript?.HidePlayer();
        m_playerData2.PlayerScript?.HidePlayer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Invoke("GameIsOver", 1f);
    }


    protected override void UpdatePlayerPrefsCompletedTutorial() { }

}
