using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManagerScript;

[System.Serializable]
public class BallArgs
{
    [HideInInspector] public GameBounds Bounds;
    //public Sprite BallTexture;
    public float m_gravity = 20f;
    public float BallHitPowerY = 4f;
    public float BallMinimumHitPowerX = 1f;
    public float BallMaximumHitPowerX = 1f;

    public float m_ballReflectPower = 1f;
    public float m_ballTimeFadeOut = 1f;
    public float m_ballTimeFadeIn = 1f;

    public float m_startForceY = 1f;
    public float m_startForceX = 3f;
    public float BallTorqueMultiplier = 1f;

    public float BallMaxGravity = 3f;

    public float BallBoundDistanceTrigger = 1f;
    public float BallBoundDistanceSpawn = 1f;





}

