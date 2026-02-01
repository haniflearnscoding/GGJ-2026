using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Volume")]
    [SerializeField] private float masterVolume = 0.5f;

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();

    private const int SAMPLE_RATE = 44100;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            GenerateAllSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void GenerateAllSounds()
    {
        cachedClips["hit"] = GenerateHitSound();
        cachedClips["cough"] = GenerateCoughSound();
        cachedClips["cure"] = GenerateCureSound();
        cachedClips["throw"] = GenerateThrowSound();
        cachedClips["win"] = GenerateWinSound();
        cachedClips["lose"] = GenerateLoseSound();
        cachedClips["pickup"] = GeneratePickupSound();
    }

    public void PlaySound(string soundName)
    {
        if (cachedClips.TryGetValue(soundName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip, masterVolume);
        }
    }

    // Player taking damage - short descending blip
    AudioClip GenerateHitSound()
    {
        float duration = 0.15f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(400f, 150f, t); // Descending pitch
            float wave = SquareWave(i, freq);
            float envelope = 1f - t; // Fade out
            data[i] = wave * envelope * 0.4f;
        }

        return CreateClip("hit", data);
    }

    // NPC cough - noisy burst
    AudioClip GenerateCoughSound()
    {
        float duration = 0.2f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = Random.Range(-1f, 1f);
            float freq = 80f + Mathf.Sin(t * 20f) * 30f; // Wobbly low freq
            float wave = SquareWave(i, freq) * 0.5f + noise * 0.5f;

            // Envelope: quick attack, medium decay
            float envelope = t < 0.1f ? t * 10f : (1f - t) * 1.1f;
            data[i] = wave * envelope * 0.3f;
        }

        return CreateClip("cough", data);
    }

    // Cure sound - ascending cheerful blip
    AudioClip GenerateCureSound()
    {
        float duration = 0.25f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(300f, 600f, t); // Ascending
            float wave = SquareWave(i, freq);

            // Two-stage envelope
            float envelope = t < 0.5f ? 1f : (1f - t) * 2f;
            data[i] = wave * envelope * 0.35f;
        }

        return CreateClip("cure", data);
    }

    // Throw mask - whoosh
    AudioClip GenerateThrowSound()
    {
        float duration = 0.12f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(200f, 400f, t);
            float wave = SawtoothWave(i, freq);
            float envelope = Mathf.Sin(t * Mathf.PI); // Bell curve
            data[i] = wave * envelope * 0.3f;
        }

        return CreateClip("throw", data);
    }

    // Win jingle - happy ascending arpeggio
    AudioClip GenerateWinSound()
    {
        float duration = 0.8f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        float[] notes = { 262f, 330f, 392f, 523f }; // C E G C (major chord)
        float noteLength = duration / notes.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t % noteLength) / noteLength;

            float freq = notes[noteIndex];
            float wave = SquareWave(i, freq) * 0.5f + SquareWave(i, freq * 2f) * 0.25f;
            float envelope = 1f - noteT * 0.5f;

            data[i] = wave * envelope * 0.35f;
        }

        return CreateClip("win", data);
    }

    // Lose sound - sad descending
    AudioClip GenerateLoseSound()
    {
        float duration = 0.6f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        float[] notes = { 294f, 262f, 220f, 196f }; // D C A G (descending)
        float noteLength = duration / notes.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            float noteT = (t % noteLength) / noteLength;

            float freq = notes[noteIndex];
            float wave = SquareWave(i, freq);
            float envelope = 1f - noteT * 0.3f;

            data[i] = wave * envelope * 0.35f;
        }

        return CreateClip("lose", data);
    }

    // Pickup item
    AudioClip GeneratePickupSound()
    {
        float duration = 0.1f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(400f, 800f, t);
            float wave = SquareWave(i, freq);
            float envelope = 1f - t;
            data[i] = wave * envelope * 0.3f;
        }

        return CreateClip("pickup", data);
    }

    // Waveform helpers
    float SquareWave(int sample, float freq)
    {
        float period = SAMPLE_RATE / freq;
        return (sample % period) < period / 2 ? 1f : -1f;
    }

    float SawtoothWave(int sample, float freq)
    {
        float period = SAMPLE_RATE / freq;
        return 2f * ((sample % period) / period) - 1f;
    }

    float SineWave(int sample, float freq)
    {
        return Mathf.Sin(2f * Mathf.PI * freq * sample / SAMPLE_RATE);
    }

    AudioClip CreateClip(string name, float[] data)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }
}
