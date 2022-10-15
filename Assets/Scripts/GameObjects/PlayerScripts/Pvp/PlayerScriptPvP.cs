using System.Collections.Generic;
using Photon.Pun;

public class PlayerScriptPvP : PlayerScript
{
    protected override void InitScripts()
    {
        SetPlayerAnimationsPvP();
        SetPlayerMovementPvP();

        m_playerKicksManager = GetComponent<PlayerKick>();
        m_playerKicksManager.Init(this, m_args);

        m_playerBombsManager = GetComponent<PlayerBomb>();
        m_playerBombsManager?.Init(m_args);

        m_playerAuraCircle = GetComponentInChildren<PlayerAuraCircle>();
        m_playerAuraCircle.Init(true);

    }
    void SetPlayerAnimationsPvP()
    {
        m_playerAnimations = GetComponent<PlayerAnimations>();
        Destroy(m_playerAnimations);
        PlayerAnimationsPvP playerAnimationsPvP = gameObject.AddComponent<PlayerAnimationsPvP>();
        m_playerAnimations = playerAnimationsPvP;
        playerAnimationsPvP.Init();
        playerAnimationsPvP.SetViewId(photonView.ViewID);
    }
    void SetPlayerMovementPvP()
    {
        m_playerMovement = GetComponent<PlayerMovement>();
        Destroy(m_playerMovement);
        PlayerMovementPvP playerMovementPvP = gameObject.AddComponent<PlayerMovementPvP>();
        m_playerMovement = playerMovementPvP;
        playerMovementPvP.Init(this, m_args);
        playerMovementPvP.SetViewId(photonView.ViewID);
    }
    protected override void Update()
    {
        if (this.photonView.IsMine)
            base.Update();
        DetectBalls();
    }
    protected override void DetectBalls()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        base.DetectBalls();
    }
    public override void FinishAnimation()
    {
        m_playerMovement.SetInParalyze(false);
        if (this.photonView.IsMine)
            base.FinishAnimation();
    }
    public override void InitPlayer(bool initPos = true)
    {
        if (!this) return;
        if (!this.photonView) return;
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("InitPlayerRPC", RpcTarget.AllBuffered, viewId, initPos);
    }
    [PunRPC]
    void InitPlayerRPC(int viewId, bool initPos = true)
    {
        if (this.photonView.ViewID != viewId)
            return;

        /*if (this.photonView.IsMine)
        {*/
        base.InitPlayer(initPos);
        base.InitPosY();
        //}

        ShowPlayer();
    }
    public override void SetGamePause(bool isPause)
    {
        /*if (this.photonView.IsMine)
        base.SetGamePause(isPause);*/
        isGamePaused = isPause;
        m_playerMovement.SetGamePause(isPause);
    }

    public override void StartTurn(bool throwNewBall = true)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("StartTurnRPC", RpcTarget.All, viewId, throwNewBall);
    }
    [PunRPC]
    void StartTurnRPC(int viewId, bool throwNewBall = true)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.StartTurn(throwNewBall);
    }
    public override void LostTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("LostTurnRPC", RpcTarget.All, viewId);

    }
    [PunRPC]
    void LostTurnRPC(int viewId)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.LostTurn();
    }
    public override void OnTouchKickSpecial()
    {
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("OnTouchKickSpecialRPC", RpcTarget.MasterClient, viewId);
    }
    [PunRPC]
    void OnTouchKickSpecialRPC(int viewId)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.OnTouchKickSpecial();
    }


}
