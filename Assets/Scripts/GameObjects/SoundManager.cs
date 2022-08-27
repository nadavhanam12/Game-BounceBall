using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundManager : MonoBehaviour
{
    // Audio players components.
    [SerializeField] private List<AudioClip> m_soundNormalKick;
    [SerializeField] private List<AudioClip> m_soundCrowd;
    [SerializeField] private AudioClip m_soundSpecialKick;
    [SerializeField] private AudioClip m_soundMenuChoose;
    [SerializeField] private AudioClip m_soundMainMenuOpen;
    [SerializeField] private AudioClip m_soundStartGameScene;
    [SerializeField] private AudioClip m_menuBGMusic;
    [SerializeField] private AudioClip m_gameBGMusic;
    [SerializeField] private AudioSource m_effectsSource;
    [SerializeField] private AudioSource m_musicSource;


    private List<AudioClip> m_prioritySounds = new List<AudioClip>();

    // Random pitch adjustment range.
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    // Singleton instance.
    public static SoundManager Instance = null;
    private bool m_availableForUpdate = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    // Initialize the singleton instance.
    private void Awake()
    {
        // If there is not already an instance of SoundManager, set it to this.
        if (Instance == null)
        {
            Instance = this;
            InitEventsListeners();
            InitPrioretyList();
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }



    private void InitEventsListeners()
    {
        EventManager.AddHandler(EVENT.EventStartApp, EventPlayMenuBGMusic);
        EventManager.AddHandler(EVENT.EventMainMenu, () => { PlayAudioClip(m_soundMainMenuOpen); });
        EventManager.AddHandler(EVENT.EventButtonClick, () => { PlayAudioClip(m_soundMenuChoose); });

        EventManager.AddHandler(EVENT.EventStartGameScene, EventStartGameScene);
        EventManager.AddHandler(EVENT.EventCombo, () => { EventAddSoundCrowd(); });
        EventManager.AddHandler(EVENT.EventNormalKick, () => { PlayNormalKick(); });
        EventManager.AddHandler(EVENT.EventSpecialKick, () => { PlayAudioClip(m_soundSpecialKick); });
        EventManager.AddHandler(EVENT.EventUpKick, () => { PlayAudioClip(m_soundSpecialKick); });



    }
    void OnDestroy()
    {
        RemoveEventsListeners();
    }

    private void RemoveEventsListeners()
    {
        EventManager.RemoveHandler(EVENT.EventStartApp, EventPlayMenuBGMusic);
        EventManager.RemoveHandler(EVENT.EventMainMenu, EventAddSoundMainMenuOpen);
        EventManager.RemoveHandler(EVENT.EventButtonClick, EventAddSoundMainMenuChoose);
        EventManager.RemoveHandler(EVENT.EventStartGameScene, EventStartGameScene);
        EventManager.RemoveHandler(EVENT.EventCombo, EventAddSoundCrowd);
        EventManager.RemoveHandler(EVENT.EventNormalKick, PlayNormalKick);
        EventManager.RemoveHandler(EVENT.EventSpecialKick, EventAddSoundSpecialKick);
        EventManager.RemoveHandler(EVENT.EventUpKick, EventAddSoundSpecialKick);



    }

    void InitPrioretyList()
    {
        for (int i = 0; i < m_soundCrowd.Count; i++)
            m_prioritySounds.Add(m_soundCrowd[i]);
        m_prioritySounds.Add(m_soundStartGameScene);

    }





    // Play a single clip through the sound effects source.
    public void PlayNormalKick()
    {
        int rnd = Random.Range(0, m_soundNormalKick.Count);
        PlayAudioClip(m_soundNormalKick[rnd]);
    }

    public void PlayAudioClip(AudioClip clipToPlay)
    {
        //print(clipToPlay.name);
        if (m_availableForUpdate)
        {
            m_availableForUpdate = false;
            bool isNewSoundPriority = m_prioritySounds.Contains(clipToPlay);
            bool isCurSoundPriority = m_prioritySounds.Contains(m_effectsSource.clip);
            bool isPlayingNow = m_effectsSource.isPlaying;
            if ((!isNewSoundPriority) && (isCurSoundPriority) && (isPlayingNow))
            {
                //print("Skip SFX");
                m_availableForUpdate = true;
                return;
            }
            m_effectsSource.Stop();
            m_effectsSource.clip = clipToPlay;
            m_effectsSource.Play();
            m_availableForUpdate = true;
        }

    }

    // Play a single clip through the music source.
    public void PlayBGMusic(bool onMenu)
    {
        //print("PlayMusic");
        m_musicSource.Stop();
        m_musicSource.clip = onMenu ? m_menuBGMusic : m_gameBGMusic;
        m_musicSource.Play();


    }

    private void EventPlayMenuBGMusic() { PlayBGMusic(true); }
    private void EventPlayGameBGMusic() { PlayBGMusic(false); }

    private void EventAddSoundMainMenuOpen() { PlayAudioClip(m_soundMainMenuOpen); }
    private void EventAddSoundMainMenuChoose() { PlayAudioClip(m_soundMenuChoose); }
    private void EventStartGameScene()
    {
        EventPlayGameBGMusic();
        Invoke("PlayWhisle", 1f);
    }

    void PlayWhisle()
    {
        PlayAudioClip(m_soundStartGameScene);
    }
    private void EventAddSoundCrowd()
    {
        int rnd = Random.Range(0, m_soundCrowd.Count - 1);
        AudioClip clip = m_soundCrowd[rnd];
        PlayAudioClip(clip);
    }
    private void EventAddSoundSpecialKick() { PlayAudioClip(m_soundSpecialKick); }



}