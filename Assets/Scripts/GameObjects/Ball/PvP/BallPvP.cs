using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BallPvP : BallScript
{


    public override void Init(BallArgs args, int ballIndex)
    {
        base.Init(args, ballIndex);
        //set theSetBallPhysicsPvP(args);
        if (!PhotonNetwork.IsMasterClient)
        {
            /*m_ballPhysics = null;
            Destroy(GetComponent<Rigidbody2D>());*/
            //Destroy(GetComponent<CircleCollider2D>());
        }
    }

    void SetBallPhysicsPvP(BallArgs args)
    {
        m_ballPhysics = GetComponent<BallPhysics>();
        Destroy(m_ballPhysics);
        BallPhysicsPvP ballPhysicsPvP = gameObject.AddComponent<BallPhysicsPvP>();
        m_ballPhysics = ballPhysicsPvP;
        ballPhysicsPvP.SetViewId(photonView.ViewID);
        m_ballPhysics.Init(this, args);
    }

    public override void UpdateColor(Color color)
    {
        int viewId = this.photonView.ViewID;
        object[] colorArray = ColorToArray(color);
        this.photonView.RPC("UpdateColorRPC", RpcTarget.All, viewId, colorArray);
    }
    [PunRPC]
    void UpdateColorRPC(int viewId, object[] colorArray)
    {
        if (this.photonView.ViewID != viewId)
            return;
        Color color = ArrayToColor(colorArray);
        base.UpdateColor(color);
    }

    public override void GenerateNewBallInScene(Color color, Vector3 pos)
    {
        //print("GenerateNewBallInScene");
        //if (PhotonNetwork.IsMasterClient)
        this.gameObject.transform.position = pos;
        this.gameObject.SetActive(true);
        base.GenerateNewBall(color);
    }

    object[] ColorToArray(Color color)
    {
        object[] objects = new object[4];
        objects[0] = color.r;
        objects[1] = color.g;
        objects[2] = color.b;
        objects[3] = color.a;
        return objects;
    }

    Color ArrayToColor(object[] objects)
    {
        Color color = new Color();
        color.r = (float)objects[0];
        color.g = (float)objects[1];
        color.b = (float)objects[2];
        color.a = (float)objects[3];
        return color;
    }
}
