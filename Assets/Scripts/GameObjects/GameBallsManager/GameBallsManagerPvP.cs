using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using System;
using static GameManagerAbstract;
using static PlayerScript;

public class GameBallsManagerPvP : GameBallsManager
{
    public void CopyParameters(GameBallsManager gameBallsManager)
    {
        m_ballHitVisualPrefab = gameBallsManager.m_ballHitVisualPrefab;
        m_ballPrefab = gameBallsManager.m_ballPrefab;
        m_ballsContainer = gameBallsManager.m_ballsContainer;
        ballColors = gameBallsManager.ballColors;
        m_opponentBallAlpha = gameBallsManager.m_opponentBallAlpha;
        m_ballsPoolSize = gameBallsManager.m_ballsPoolSize;
        m_ballsHitVFX = gameBallsManager.m_ballsHitVFX;
        m_gravityAdded = gameBallsManager.m_gravityAdded;
        m_gravityChangeRate = gameBallsManager.m_gravityChangeRate;
        m_highKickHight = gameBallsManager.m_highKickHight;
        m_kickCooldown = gameBallsManager.m_kickCooldown;

    }

    protected override void InitBalls()
    {
        m_ballsArray = new BallScript[m_ballsPoolSize];
        m_nextBallIndex = 0;
        if (!PhotonNetwork.IsMasterClient)
            return;
        for (int i = 0; i < m_ballsArray.Length; i++)
        {
            GameObject ballGameObject = PhotonNetwork.Instantiate("Ball/Ball", Vector3.zero, Quaternion.identity);
            int ballViewId = ballGameObject.GetComponent<PhotonView>().ViewID;
            this.photonView.RPC("InitBallsArray", RpcTarget.All, ballViewId, i);
        }
    }

    [PunRPC]
    void InitBallsArray(int ballViewId, int index)
    {
        GameObject ballGameObject = PhotonView.Find(ballViewId).gameObject;
        ballGameObject.transform.parent = m_ballsContainer;
        ballGameObject.transform.localPosition = Vector3.zero;

        Destroy(ballGameObject.GetComponent<BallScript>());
        BallScript ballScript = ballGameObject.AddComponent<BallPvP>();

        m_ballsArray[index] = ballScript;
        ballScript.Init(m_args.BallArgs, index);
        ballScript.m_onBallLost = OnBallLost;
        ballScript.RemoveBallFromScene();
    }

    protected override void InitColorQueue()
    {
        ColorsQueue = new Queue<Color>();
        if (!PhotonNetwork.IsMasterClient)
            return;
        for (int i = 0; i < 4; i++)
            ColorsQueue.Enqueue(GenerateRandomColor(Color.black));
    }

    public override void UpdateNextBallColor(Color color, bool shouldEmitParticles)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        object[] dataColor = ColorToArray(color);
        object[] dataColorsArray = ColorsArrayToData(ColorsQueue.ToArray());
        this.photonView.RPC("UpdateNextBallColorRPC", RpcTarget.All, dataColor, dataColorsArray, shouldEmitParticles);
    }

    [PunRPC]
    void UpdateNextBallColorRPC(object[] dataColor, object[] dataColorsArray, bool shouldEmitParticles)
    {
        //print("UpdateNextBallColorRPC");
        m_curRequiredColor = ArrayToColor(dataColor);
        m_nextColorArray = DataToColorsArray(dataColorsArray);
        m_args.GameCanvas.UpdateNextBallColor(m_curRequiredColor, m_nextColorArray, shouldEmitParticles);
    }

    public override void SetGamePause(bool isPause)
    {
        isGamePaused = isPause;
        if (PhotonNetwork.IsMasterClient)
            foreach (BallScript ball in m_ballsArray)
                ball.SetGamePause(isGamePaused);
    }

    public override void OnNewBallInScene(bool randomDirection, Vector2Int directionVector)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        base.OnNewBallInScene(randomDirection, directionVector);
    }
    protected override void GenerateFirstBall(int ballIndex, Color color, float disXMultiplier, float startForceY)
    {
        BallScript ball = m_ballsArray[ballIndex];
        int viewId = ball.GetComponent<PhotonView>().ViewID;
        this.photonView.RPC("GenerateFirstBallWithViewID", RpcTarget.All, viewId, ColorToArray(color), disXMultiplier, startForceY);
    }

    [PunRPC]
    void GenerateFirstBallWithViewID(int viewId, object[] colorArray, float disXMultiplier, float startForceY)
    {
        BallScript ball = PhotonView.Find(viewId).GetComponent<BallScript>();
        Color color = ArrayToColor(colorArray);
        ball.OnNewBallInScene(color, disXMultiplier, startForceY);
    }

    protected override void RemoveBallFromScene(int ballIndex, bool fadeOut = false)
    {
        BallScript ball = m_ballsArray[ballIndex];
        int viewId = ball.GetComponent<PhotonView>().ViewID;
        this.photonView.RPC("RemoveBallFromSceneWithViewId", RpcTarget.All, viewId, fadeOut);
    }

    [PunRPC]
    void RemoveBallFromSceneWithViewId(int viewId, bool fadeOut = false)
    {
        BallScript ball = PhotonView.Find(viewId).GetComponent<BallScript>();
        ball?.RemoveBallFromScene(fadeOut);
    }

    //is called when a ball is split
    protected override void GenerateNewBallInScene(int ballIndex, Color color2, Vector2 otherBallPos)
    {
        BallScript ball = m_ballsArray[ballIndex];
        int viewId = ball.GetComponent<PhotonView>().ViewID;
        object[] colorData = ColorToArray(color2);
        this.photonView.RPC("GenerateNewBallInSceneWithViewId", RpcTarget.All, viewId, colorData, otherBallPos);
    }

    [PunRPC]
    void GenerateNewBallInSceneWithViewId(int viewId, object[] colorData, Vector2 otherBallPos)
    {
        BallScript ball = PhotonView.Find(viewId).GetComponent<BallScript>();
        Color color = ArrayToColor(colorData);
        ball.GenerateNewBallInScene(color, otherBallPos);
    }

    protected override void UpdateCorrectBallIndex(int nextBallIndex)
    {
        this.photonView.RPC("UpdateCorrectBallIndexRPC", RpcTarget.All, nextBallIndex);
    }
    [PunRPC]
    protected void UpdateCorrectBallIndexRPC(int nextBallIndex)
    {
        base.UpdateCorrectBallIndex(nextBallIndex);
    }
    protected override void OnHitPlay(PlayerIndex playerIndex, int ballIndex, KickType kickType)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        base.OnHitPlay(playerIndex, ballIndex, kickType);
    }


    protected override void ActivateBallHitVisual(Color color, Vector3 position)
    {
        object[] colorData = ColorToArray(color);
        this.photonView.RPC("ActivateBallHitVisualRPC", RpcTarget.All, colorData, position);
    }

    [PunRPC]
    void ActivateBallHitVisualRPC(object[] colorData, Vector3 position)
    {
        Color color = ArrayToColor(colorData);
        BallHitVisual hitVisual = m_hitVisualsQueue.Dequeue();
        hitVisual.Activate(color, position);
        m_hitVisualsQueue.Enqueue(hitVisual);
    }
    protected override void CameraShake()
    {
        this.photonView.RPC("CameraShakeRPC", RpcTarget.All);
    }
    [PunRPC]
    void CameraShakeRPC()
    {
        CameraVFX cameraVFX = FindObjectOfType<CameraVFX>();
        cameraVFX.Shake();
    }


    #region DataConvertor
    object[] ColorsArrayToData(Color[] ColorsQueue)
    {
        object[] data = new object[ColorsQueue.Length];
        for (int i = 0; i < ColorsQueue.Length; i++)
            data[i] = ColorToArray(ColorsQueue[i]);
        return data;
    }
    Color[] DataToColorsArray(object[] data)
    {
        Color[] colorArray = new Color[data.Length];
        for (int i = 0; i < data.Length; i++)
            colorArray[i] = ArrayToColor((object[])data[i]);
        return colorArray;
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

    int[] BallScriptsToBallIndexes(List<BallScript> balls)
    {
        int[] ballsIndexes = new int[balls.Count];
        int index = 0;
        foreach (BallScript ballScript in balls)
        {
            ballsIndexes[index] = (ballScript.GetIndex());
            index++;
        }
        return ballsIndexes;
    }
    List<BallScript> BallIndexesToBallScripts(int[] balls)
    {
        List<BallScript> ballsScript = new List<BallScript>();
        foreach (int ballIndex in balls)
            ballsScript.Add(m_ballsArray[ballIndex]);
        return ballsScript;
    }

    #endregion
}
