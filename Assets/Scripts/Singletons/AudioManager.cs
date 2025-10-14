using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized audio management system for playing sound effects and background music.
/// Supports multiple AudioSource components for different sound categories.
/// </summary>
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float uiVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.5f;

    [Header("Sound Clips")]
    [SerializeField] private List<SoundMapping> soundMappings = new List<SoundMapping>();

    [Header("Music Clips")]
    [SerializeField] private List<MusicMapping> musicMappings = new List<MusicMapping>();
    [SerializeField] private float musicFadeDuration = 2f;

    private Dictionary<SoundType, AudioClip> soundDictionary;
    private Dictionary<MusicType, AudioClip> musicDictionary;
    private MusicType? currentMusicTrack = null;
    private Coroutine fadeCoroutine = null;

    private void Awake() {
        // Singleton pattern
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize sound dictionary
        InitializeSoundDictionary();

        // Initialize music dictionary
        InitializeMusicDictionary();

        // Create default AudioSources if not assigned
        EnsureAudioSources();
    }

    /// <summary>
    /// Initialize the sound dictionary from the serialized mappings list.
    /// </summary>
    private void InitializeSoundDictionary() {
        soundDictionary = new Dictionary<SoundType, AudioClip>();

        foreach (SoundMapping mapping in soundMappings) {
            if (mapping.clip != null && !soundDictionary.ContainsKey(mapping.soundType)) {
                soundDictionary.Add(mapping.soundType, mapping.clip);
            }
        }

        Debug.Log($"AudioManager initialized with {soundDictionary.Count} sound mappings");
    }

    /// <summary>
    /// Ensure AudioSources exist, create them if missing.
    /// </summary>
    private void EnsureAudioSources() {
        if (sfxSource == null) {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            Debug.Log("Created default SFX AudioSource");
        }

        if (uiSource == null) {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
            Debug.Log("Created default UI AudioSource");
        }

        if (ambientSource == null) {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
            Debug.Log("Created default Ambient AudioSource");
        }

        // Apply volume settings
        ApplyVolumeSettings();
    }

    /// <summary>
    /// Apply volume settings to all AudioSources.
    /// </summary>
    private void ApplyVolumeSettings() {
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (uiSource != null) uiSource.volume = uiVolume;
        if (ambientSource != null) ambientSource.volume = ambientVolume;
    }

    /// <summary>
    /// Play a sound effect by type.
    /// </summary>
    /// <param name="type">The type of sound to play</param>
    /// <param name="volumeMultiplier">Optional volume multiplier (0-1)</param>
    public void PlaySound(SoundType type, float volumeMultiplier = 1f) {
        // Get the appropriate AudioSource for this sound type
        AudioSource source = GetSourceForType(type);
        if (source == null) {
            Debug.LogWarning($"No AudioSource available for sound type: {type}");
            return;
        }

        // Get the clip for this sound type
        AudioClip clip = GetClipForType(type);
        if (clip == null) {
            Debug.LogWarning($"No AudioClip assigned for sound type: {type}");
            return;
        }

        // Play the sound
        source.PlayOneShot(clip, volumeMultiplier);
    }

    /// <summary>
    /// Get the appropriate AudioSource for a given sound type.
    /// </summary>
    private AudioSource GetSourceForType(SoundType type) {
        // Route sounds to appropriate AudioSource based on category
        switch (type) {
            case SoundType.UIClick:
            case SoundType.UIConfirm:
            case SoundType.UICancel:
            case SoundType.UIError:
                return uiSource;

            case SoundType.CashRegister:
            case SoundType.DoorBell:
            case SoundType.BoxOpen:
            case SoundType.ShelfRestock:
                return sfxSource;

            default:
                return sfxSource; // Default to SFX source
        }
    }

    /// <summary>
    /// Get the AudioClip for a given sound type.
    /// </summary>
    private AudioClip GetClipForType(SoundType type) {
        if (soundDictionary != null && soundDictionary.TryGetValue(type, out AudioClip clip)) {
            return clip;
        }

        return null;
    }

    /// <summary>
    /// Set master volume for SFX sounds.
    /// </summary>
    public void SetSFXVolume(float volume) {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Set master volume for UI sounds.
    /// </summary>
    public void SetUIVolume(float volume) {
        uiVolume = Mathf.Clamp01(volume);
        if (uiSource != null) {
            uiSource.volume = uiVolume;
        }
    }

    /// <summary>
    /// Set master volume for ambient sounds.
    /// </summary>
    public void SetAmbientVolume(float volume) {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null) {
            ambientSource.volume = ambientVolume;
        }
    }

    /// <summary>
    /// Initialize the music dictionary from the serialized mappings list.
    /// </summary>
    private void InitializeMusicDictionary() {
        musicDictionary = new Dictionary<MusicType, AudioClip>();

        foreach (MusicMapping mapping in musicMappings) {
            if (mapping.clip != null && !musicDictionary.ContainsKey(mapping.musicType)) {
                musicDictionary.Add(mapping.musicType, mapping.clip);
            }
        }

        Debug.Log($"AudioManager initialized with {musicDictionary.Count} music tracks");
    }

    /// <summary>
    /// Play background music by type with optional fade-in.
    /// </summary>
    /// <param name="type">The type of music to play</param>
    /// <param name="fadeIn">Whether to fade in the music</param>
    public void PlayMusic(MusicType type, bool fadeIn = true) {
        if (ambientSource == null) {
            Debug.LogWarning("No ambientSource available for music playback!");
            return;
        }

        // Get the clip for this music type
        AudioClip musicClip = GetMusicClipForType(type);
        if (musicClip == null) {
            Debug.LogWarning($"No AudioClip assigned for music type: {type}");
            return;
        }

        // Stop any ongoing fade
        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Set the new clip
        ambientSource.clip = musicClip;
        currentMusicTrack = type;

        if (fadeIn) {
            // Start at zero volume and fade in
            ambientSource.volume = 0f;
            ambientSource.Play();
            fadeCoroutine = StartCoroutine(FadeMusicVolume(ambientVolume, musicFadeDuration));
            Debug.Log($"Playing music: {type} (fade in)");
        } else {
            // Start at full volume immediately
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
            Debug.Log($"Playing music: {type} (no fade)");
        }
    }

    /// <summary>
    /// Stop the current background music with optional fade-out.
    /// </summary>
    /// <param name="fadeOut">Whether to fade out the music</param>
    public void StopMusic(bool fadeOut = true) {
        if (ambientSource == null || !ambientSource.isPlaying) {
            return;
        }

        // Stop any ongoing fade
        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (fadeOut) {
            fadeCoroutine = StartCoroutine(FadeOutAndStop());
            Debug.Log("Stopping music (fade out)");
        } else {
            ambientSource.Stop();
            currentMusicTrack = null;
            Debug.Log("Stopping music (no fade)");
        }
    }

    /// <summary>
    /// Smoothly transition from current music to a new track.
    /// </summary>
    /// <param name="newType">The new music type to play</param>
    public void CrossfadeMusic(MusicType newType) {
        if (currentMusicTrack == newType) {
            Debug.Log($"Music track {newType} is already playing");
            return;
        }

        if (ambientSource == null || !ambientSource.isPlaying) {
            // No music currently playing, just start the new one
            PlayMusic(newType, fadeIn: true);
            return;
        }

        // Crossfade: fade out current, then fade in new
        StartCoroutine(CrossfadeMusicCoroutine(newType));
    }

    /// <summary>
    /// Coroutine to handle crossfading between music tracks.
    /// </summary>
    private IEnumerator CrossfadeMusicCoroutine(MusicType newType) {
        Debug.Log($"Crossfading music: {currentMusicTrack} → {newType}");

        // Stop any ongoing fade
        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Fade out current track
        float halfDuration = musicFadeDuration / 2f;
        yield return FadeMusicVolume(0f, halfDuration);

        // Switch to new track and fade in
        PlayMusic(newType, fadeIn: true);
    }

    /// <summary>
    /// Coroutine to fade music volume over time.
    /// </summary>
    private IEnumerator FadeMusicVolume(float targetVolume, float duration) {
        if (ambientSource == null) yield break;

        float startVolume = ambientSource.volume;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            ambientSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        ambientSource.volume = targetVolume;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Coroutine to fade out music and stop it.
    /// </summary>
    private IEnumerator FadeOutAndStop() {
        yield return FadeMusicVolume(0f, musicFadeDuration);

        if (ambientSource != null) {
            ambientSource.Stop();
            currentMusicTrack = null;
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// Get the AudioClip for a given music type.
    /// </summary>
    private AudioClip GetMusicClipForType(MusicType type) {
        if (musicDictionary != null && musicDictionary.TryGetValue(type, out AudioClip clip)) {
            return clip;
        }

        return null;
    }

    /// <summary>
    /// Set music volume (applies to ambientSource).
    /// </summary>
    public void SetMusicVolume(float volume) {
        SetAmbientVolume(volume);
    }

    /// <summary>
    /// Serializable mapping for Inspector configuration.
    /// </summary>
    [System.Serializable]
    public class SoundMapping {
        public SoundType soundType;
        public AudioClip clip;
    }

    /// <summary>
    /// Serializable mapping for music tracks.
    /// </summary>
    [System.Serializable]
    public class MusicMapping {
        public MusicType musicType;
        public AudioClip clip;
    }
}
