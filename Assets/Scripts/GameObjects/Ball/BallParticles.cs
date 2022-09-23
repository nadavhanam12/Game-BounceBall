using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallParticles : MonoBehaviour
{
    const float m_speedEmitTrail = 0;
    const int ParticlesToEmit = 500;
    private ParticleSystem m_particles;
    private TrailRenderer m_curBallTrail;

    public void Init()
    {
        m_curBallTrail = GetComponentInChildren<TrailRenderer>();


    }

    public void EnableTrail(bool enable)
    {
        m_curBallTrail.enabled = enable;
    }
    public void UpdateStartColor(Color color)
    {
        EnableTrail(true);
        m_curBallTrail.startColor = color;
    }

    public void EmitBallTrail(bool emit)
    {
        if (emit)
        {
            if (!m_curBallTrail.emitting)
                m_curBallTrail.emitting = true;
        }
    }
}
