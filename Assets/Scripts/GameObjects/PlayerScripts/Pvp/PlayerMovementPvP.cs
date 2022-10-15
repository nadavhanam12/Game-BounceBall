using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovementPvP : PlayerMovement
{
    //0 = move left
    //1 = move right
    //2 = move down
    //3 = move up
    //4 - initialPosAfterJump

    int framesCountInLastUpdate = 0;
    Queue<byte> MoveCommandsQueue = new Queue<byte>();

    int m_viewId;
    public void SetViewId(int viewId)
    {
        m_viewId = viewId;
    }

    protected override void MoveToDirection(Vector2Int direction)
    {
        if (!this.photonView) return;
        int viewId = this.photonView.ViewID;
        byte dir = 0;
        if (direction == Vector2.right)
            dir = 1;
        this.photonView.RPC("MoveToDirectionRPC", Photon.Pun.RpcTarget.All, viewId, dir);
    }
    [PunRPC]
    void MoveToDirectionRPC(int viewId, byte dir)
    {
        if (this.photonView.ViewID != viewId)
            return;
        MoveCommandsQueue.Enqueue(dir);
    }

    protected override void ApplyJump(int movY)
    {
        if (!this.photonView) return;
        int viewId = this.photonView.ViewID;
        byte movByte = 2;
        if (movY == 1)
            movByte = 3;
        this.photonView.RPC("ApplyJumpRPC", Photon.Pun.RpcTarget.All, viewId, movByte);
    }
    [PunRPC]
    public void ApplyJumpRPC(int viewId, byte movByte)
    {
        if (this.photonView.ViewID != viewId)
            return;
        MoveCommandsQueue.Enqueue(movByte);

        /*MoveCommandsQueue.Clear();

        if (movByte == 2)
            base.ApplyJump(-1);
        else
            base.ApplyJump(1);*/
    }
    protected override void InitialAfterJump()
    {
        //print("InitialAfterJump");
        if (!this.photonView) return;
        int viewId = this.photonView.ViewID;
        this.photonView.RPC("InitialAfterJumpRPC", Photon.Pun.RpcTarget.All, viewId);
    }

    [PunRPC]
    public void InitialAfterJumpRPC(int viewId)
    {
        if (this.photonView.ViewID != viewId)
            return;
        //byte init = 4;
        //MoveCommandsQueue.Enqueue(init);
        MoveCommandsQueue.Clear();
        //print("InitialAfterJumpRPC " + viewId);
        base.InitialAfterJump();
    }

    void FixedUpdate()
    {
        if (MoveCommandsQueue.Count == 0) return;
        ExecuteMoveCommand(MoveCommandsQueue.Dequeue());
    }

    void ExecuteMoveCommand(byte dir)
    {
        //print("framesSinceLastMove " + Time.frameCount);
        switch (dir)
        {
            case (0):
                base.MoveToDirection(Vector2Int.left);
                break;
            case (1):
                base.MoveToDirection(Vector2Int.right);
                break;
            case (2):
                base.ApplyJump(-1);
                break;
            case (3):
                base.ApplyJump(1);
                break;
            case (4):
                base.InitialAfterJump();
                break;
        }
    }


}
