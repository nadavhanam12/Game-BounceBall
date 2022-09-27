using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BallParticles : MonoBehaviour
{
    const float m_speedEmitTrail = 0;
    const int ParticlesToEmit = 500;
    private TrailRenderer m_curBallTrail;

    public void Init()
    {
        m_curBallTrail = GetComponentInChildren<TrailRenderer>();
    }

    public void EnableTrail(bool enable)
    {
        m_curBallTrail.enabled = enable;
    }
    public async void UpdateStartColor(Color color)
    {
        //await Task.Delay(100);
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
