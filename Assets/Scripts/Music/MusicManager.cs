using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    [SerializeField] private MusicLibrary musicLibrary;

    [SerializeField] private List<string> lobbyPlaylist = new List<string> { "Lobby Sound 1", "Lobby Sound 2" };
    private Coroutine playlistCoroutine;
    private Coroutine activeCrossfadeCoroutine;
    private string currentTrackName = "";
    
    // Menyimpan referensi track yang sedang aktif berputar saat ini
    private MusicTrack currentActiveTrack; 
    [SerializeField] private AudioMixer audioMixer; // Tarik AudioMixer kamu ke slot ini via Inspector prefab/scene
    
    public AudioMixer GetAudioMixer() => audioMixer;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (audioMixer == null && musicLibrary != null)
            {
                MusicTrack sample = musicLibrary.GetTrackFromName("Lobby Sound 1");
                if (sample.audioSource != null && sample.audioSource.outputAudioMixerGroup != null)
                {
                    audioMixer = sample.audioSource.outputAudioMixerGroup.audioMixer;
                }
            }
        }
    }

    private void Start()
    {
        if (playlistCoroutine == null && SceneManager.GetActiveScene().name == "MainMenu")
        {
            playlistCoroutine = StartCoroutine(PlayLobbyPlaylistAutomatically());
        }
        ApplySavedVolume();
    }

    private void ApplySavedVolume()
    {
        if (audioMixer != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
            audioMixer.SetFloat("MusicVolume", savedVolume);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            StopLobbyPlaylist();
            currentTrackName = "";
            playlistCoroutine = StartCoroutine(PlayLobbyPlaylistAutomatically());
        }
        else if (scene.name == "PlayScene" || scene.name == "Tutorial")
        {
            StopLobbyPlaylist();
            PlayMusic("Game");
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        if (trackName == currentTrackName && currentActiveTrack.audioSource != null && currentActiveTrack.audioSource.isPlaying) return;
        currentTrackName = trackName;

        if (musicLibrary == null) return;
        MusicTrack nextTrack = musicLibrary.GetTrackFromName(trackName);

        if (nextTrack.clip != null && nextTrack.audioSource != null)
        {
            nextTrack.audioSource.loop = true;
            if (activeCrossfadeCoroutine != null)
            {
                StopCoroutine(activeCrossfadeCoroutine);
            }
            activeCrossfadeCoroutine = StartCoroutine(AnimateMusicCrossfade(nextTrack, fadeDuration));
        }
    }

    IEnumerator AnimateMusicCrossfade(MusicTrack nextTrack, float fadeDuration = 0.5f)
    {
        AudioSource oldSource = currentActiveTrack.audioSource;
        if (oldSource != null && oldSource.isPlaying && oldSource != nextTrack.audioSource)
        {
            float startVolume = oldSource.volume;
            float percent = 0f;
            while (percent < 1f)
            {
                percent += (fadeDuration > 0 ? Time.unscaledDeltaTime / fadeDuration : 1f);
                oldSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                yield return null;
            }
            oldSource.volume = 0f;
            oldSource.Stop();
        }
        else if (oldSource != null && oldSource == nextTrack.audioSource && oldSource.isPlaying)
        {
            float startVolume = oldSource.volume;
            float percent = 0f;
            while (percent < 1f)
            {
                percent += (fadeDuration > 0 ? Time.unscaledDeltaTime / fadeDuration : 1f);
                oldSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                yield return null;
            }
            oldSource.Stop();
        }

        currentActiveTrack = nextTrack;
        AudioSource newSource = currentActiveTrack.audioSource;
        if (newSource != null)
        {
            newSource.clip = nextTrack.clip;
            newSource.volume = 0f;
            newSource.Play();

            float inPercent = 0f;
            while (inPercent < 1f)
            {
                inPercent += (fadeDuration > 0 ? Time.unscaledDeltaTime / fadeDuration : 1f);
                newSource.volume = Mathf.Lerp(0f, 1f, inPercent);
                yield return null;
            }
            newSource.volume = 1f;
        }

        activeCrossfadeCoroutine = null;
    }

    IEnumerator PlayLobbyPlaylistAutomatically()
    {
        int currentIndex = 0;

        while (true)
        {
            if (lobbyPlaylist == null || lobbyPlaylist.Count == 0) yield break;

            string trackToPlay = lobbyPlaylist[currentIndex];
            if (musicLibrary != null)
            {
                MusicTrack nextTrack = musicLibrary.GetTrackFromName(trackToPlay);
                if (nextTrack.clip != null && nextTrack.audioSource != null)
                {
                    currentTrackName = trackToPlay;
                    nextTrack.audioSource.loop = false;
                    yield return StartCoroutine(AnimateMusicCrossfade(nextTrack, 0.5f));
                }
            }

            yield return new WaitForSecondsRealtime(0.2f);

            while (currentActiveTrack.audioSource != null && currentActiveTrack.audioSource.isPlaying)
            {
                yield return null;
            }

            currentIndex++;
            if (currentIndex >= lobbyPlaylist.Count)
            {
                currentIndex = 0;
            }
        }
    }

    public void StopLobbyPlaylist()
    {
        if (playlistCoroutine != null)
        {
            StopCoroutine(playlistCoroutine);
            playlistCoroutine = null;
        }
    }
}
