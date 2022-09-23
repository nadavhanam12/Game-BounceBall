using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BallPvP : BallScript
{


    public override void Init(BallArgs args, int ballIndex)
    {
        base.Init(args, ballIndex);
        if (!PhotonNetwork.IsMasterClient)
        {
            m_ballPhysics = null;
            Destroy(GetComponent<Rigidbody2D>());
            //Destroy(GetComponent<CircleCollider2D>());
        }
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
