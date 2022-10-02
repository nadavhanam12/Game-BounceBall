using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerAuraCircle : MonoBehaviourPun
{
    public enum AuraState
    { Disabled, Idle, Ready, Activated, }

    ParticleSystem m_idleParticleSystem;
    ParticleSystem m_readyParticleSystem;
    ParticleSystem m_activateParticleSystem;
    AuraState m_auraState;
    bool m_initialized = false;
    bool m_isPvPMode = false;

    public void Init(bool isPvPMode = false)
    {
        m_isPvPMode = isPvPMode;
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        m_idleParticleSystem = particles[0];
        m_readyParticleSystem = particles[1];
        m_activateParticleSystem = particles[2];
        m_initialized = true;
        Disable();
    }
    [PunRPC]
    public void Disable()
    {
        if (IsPvPAndMaster())
            this.photonView.RPC("Disable", RpcTarget.Others);
        //print("Disable");
        //gameObject.SetActive(false);
        if (!m_initialized)
            Init();
        m_auraState = AuraState.Disabled;
        CancelAura();

    }
    [PunRPC]
    public void IdleAura()
    {
        if (IsPvPAndMaster())
            this.photonView.RPC("IdleAura", RpcTarget.Others);
        //print("IdleAura");
        m_auraState = AuraState.Idle;
        m_idleParticleSystem.Play();
    }
    [PunRPC]
    public void ReadyAura()
    {
        if (IsPvPAndMaster())
            this.photonView.RPC("ReadyAura", RpcTarget.Others);
        //print("ReadyAura");
        if (m_auraState != AuraState.Idle)
            m_idleParticleSystem.Play();
        m_auraState = AuraState.Ready;
        m_readyParticleSystem.Play();
    }
    [PunRPC]
    public void Activate()
    {
        if (IsPvPAndMaster())
            this.photonView.RPC("Activate", RpcTarget.Others);
        //print("Activate");
        if (m_auraState != AuraState.Ready)
        {
            m_idleParticleSystem.Play();
            m_readyParticleSystem.Play();
        }
        m_auraState = AuraState.Activated;
        m_activateParticleSystem.Play();
        Invoke("Disable", 0.5f);
    }

    void CancelAura()
    {
        m_idleParticleSystem.Stop();
        m_readyParticleSystem.Stop();
        m_activateParticleSystem.Stop();
    }

    bool IsPvPAndMaster()
    {
        return m_isPvPMode && PhotonNetwork.IsMasterClient;
    }
}
