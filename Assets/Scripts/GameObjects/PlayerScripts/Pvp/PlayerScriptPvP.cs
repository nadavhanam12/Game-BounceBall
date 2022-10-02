using System.Collections.Generic;
using Photon.Pun;

public class PlayerScriptPvP : PlayerScript
{
    protected override void InitScripts()
    {
        m_playerAnimations = GetComponent<PlayerAnimations>();
        Destroy(m_playerAnimations);
        PlayerAnimationsPvP playerAnimationsPvP = gameObject.AddComponent<PlayerAnimationsPvP>();
        m_playerAnimations = playerAnimationsPvP;
        playerAnimationsPvP.Init();
        playerAnimationsPvP.SetViewId(photonView.ViewID);

        m_playerMovement = GetComponent<PlayerMovement>();
        m_playerMovement.Init(this, m_args);

        m_playerKicksManager = GetComponent<PlayerKick>();
        m_playerKicksManager.Init(this, m_args);

        m_playerBombsManager = GetComponent<PlayerBomb>();
        m_playerBombsManager?.Init(m_args);

        m_playerAuraCircle = GetComponentInChildren<PlayerAuraCircle>();
        m_playerAuraCircle.Init(true);

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
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("InitPlayerRPC", RpcTarget.All, viewId, initPos);
    }
    [PunRPC]
    void InitPlayerRPC(int viewId, bool initPos = true)
    {
        if (this.photonView.ViewID != viewId)
            return;

        if (this.photonView.IsMine)
            base.InitPlayer(initPos);

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
