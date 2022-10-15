using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BallPhysicsPvP : BallPhysics
{
    Photon.Pun.RpcTarget rpcTarget = RpcTarget.AllViaServer;
    int m_viewId;
    public void SetViewId(int viewId)
    {
        m_viewId = viewId;
    }
    /*public override void ToggleSimulate(bool simulate)
    {
        int viewId = this.photonView.ViewID;
        byte simulateByte = 0;
        if (simulate)
            simulateByte = 1;
        this.photonView.RPC("ToggleSimulateRPC", rpcTarget, viewId, simulateByte);
    }
    [PunRPC]
    void ToggleSimulateRPC(int viewId, byte simulateByte)
    {
        if (this.photonView.ViewID != viewId)
            return;
        bool simulate = false;
        if (simulateByte == 1)
            simulate = true;
        base.ToggleSimulate(simulate);
    }
    protected override void AddForce(Vector2 force)
    {
        int viewId = this.photonView.ViewID;
        float[] data = new float[2];
        data[0] = force.x;
        data[1] = force.y;

        this.photonView.RPC("AddForceRPC", rpcTarget, viewId, data);
    }
    [PunRPC]
    void AddForceRPC(int viewId, float[] data)
    {
        if (this.photonView.ViewID != viewId)
            return;
        Vector2 force = new Vector2(data[0], data[1]);
        base.AddForce(force);
    }
    protected override void AddTorque(float torque)
    {
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("AddTorqueRPC", rpcTarget, viewId, torque);
    }
    [PunRPC]
    void AddTorqueRPC(int viewId, float torque)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.AddTorque(torque);
    }
    public override void ResetVelocity()
    {
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("ResetVelocityRPC", rpcTarget, viewId);
    }
    [PunRPC]
    void ResetVelocityRPC(int viewId)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.ResetVelocity();
    }
    public override void InitGravity()
    {
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("InitGravityRPC", rpcTarget, viewId);
    }
    [PunRPC]
    void InitGravityRPC(int viewId)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.InitGravity();
    }
    public override void AddToGravity(float gravityAdded)
    {
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("AddToGravityRPC", rpcTarget, viewId, gravityAdded);
    }
    [PunRPC]
    void AddToGravityRPC(int viewId, float gravityAdded)
    {
        if (this.photonView.ViewID != viewId)
            return;
        base.AddToGravity(gravityAdded);
    }
*/
}
