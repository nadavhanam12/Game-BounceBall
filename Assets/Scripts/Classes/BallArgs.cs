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
    public float BallRegularHitPower = 4f;
    public float BallSpecialHitPower = 15f;
    public float XAxisMultiplier = 0.75f;
    public float m_ballReflectPower = 1f;
    public float m_ballTimeFadeOut = 1f;
    public float m_startVelocityY = 0.2f;
    [Range(0, 0.5f)] public float m_startVelocityX = 0.25f;



}
