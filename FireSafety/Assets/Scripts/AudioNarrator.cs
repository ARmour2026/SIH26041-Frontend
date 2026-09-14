using UnityEngine;
using System.Collections.Generic;

// Plays short voice clips by key (e.g. "ask_fire_cause", "feedback_correct").
// Supports switching language so the same key plays a different clip per language,
// which is what your Icon + Voice-First twist and Hindi/Santali localisation need.
//
// Owner: Riddhi/Aashi (whoever wires up training + localisation)

public enum TrainingLanguage { English, Hindi, Santali }

[System.Serializable]
public class VoiceLineEntry
{
    public string key;              // e.g. "ask_fire_cause"
    public AudioClip englishClip;
    public AudioClip hindiClip;
    public AudioClip santaliClip;
}

public class AudioNarrator : MonoBehaviour
{
    public static AudioNarrator Instance { get; private set; }

    [Tooltip("Set this from your language-selection screen before training starts.")]
    public TrainingLanguage currentLanguage = TrainingLanguage.English;

    [Tooltip("All voice lines used across every module — add one entry per key you reference in MultipleChoiceStep.")]
    public List<VoiceLineEntry> voiceLines = new List<VoiceLineEntry>();

    private AudioSource audioSource;
    private Dictionary<string, VoiceLineEntry> lookup;

    void Awake()
    {
        // Simple singleton — persists across scene loads if you need it to
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        BuildLookup();
    }

    void BuildLookup()
    {
        lookup = new Dictionary<string, VoiceLineEntry>();
        foreach (var entry in voiceLines)
        {
            if (!string.IsNullOrEmpty(entry.key) && !lookup.ContainsKey(entry.key))
            {
                lookup.Add(entry.key, entry);
            }
        }
    }

    public void PlayLine(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (lookup == null) BuildLookup();

        if (!lookup.TryGetValue(key, out var entry))
        {
            Debug.LogWarning($"AudioNarrator: no voice line found for key '{key}'");
            return;
        }

        AudioClip clip = currentLanguage switch
        {
            TrainingLanguage.Hindi => entry.hindiClip,
            TrainingLanguage.Santali => entry.santaliClip,
            _ => entry.englishClip,
        };

        if (clip == null)
        {
            Debug.LogWarning($"AudioNarrator: no clip assigned for key '{key}' in language {currentLanguage}");
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void SetLanguage(TrainingLanguage language)
    {
        currentLanguage = language;
    }
}