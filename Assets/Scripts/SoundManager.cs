using System;
using UnityEngine;

namespace ReverseTicTacToe
{
    /// <summary>
    /// Procedural audio manager that generates sound effects at runtime without external audio assets.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource audioSource;
        private bool isMuted = false;

        private AudioClip clickClip;
        private AudioClip moveXClip;
        private AudioClip moveOClip;
        private AudioClip winClip;
        private AudioClip lossClip;
        private AudioClip drawClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound

            GenerateClips();
        }

        private void GenerateClips()
        {
            clickClip = CreateToneClip("Click", 0.05f, 1200f, 800f, ToneWave.Sine, 0.4f);
            moveXClip = CreateChimeClip("MoveX", new float[] { 587.33f, 880f }, 0.15f, 0.45f);
            moveOClip = CreateChimeClip("MoveO", new float[] { 440f, 659.25f }, 0.15f, 0.45f);
            winClip = CreateFanfareClip("Win", new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.14f, 0.5f);
            lossClip = CreateLossClip("Loss", new float[] { 415.30f, 370.0f, 311.13f, 246.94f }, 0.18f, 0.55f);
            drawClip = CreateChimeClip("Draw", new float[] { 440f, 523.25f, 659.25f }, 0.25f, 0.4f);
        }

        public void PlayClick() => PlaySound(clickClip);
        public void PlayMove(PlayerType player) => PlaySound(player == PlayerType.X ? moveXClip : moveOClip);
        public void PlayWin() => PlaySound(winClip);
        public void PlayLoss() => PlaySound(lossClip);
        public void PlayDraw() => PlaySound(drawClip);

        public void ToggleMute()
        {
            isMuted = !isMuted;
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }

        public bool IsMuted => isMuted;

        private void PlaySound(AudioClip clip)
        {
            if (isMuted || clip == null || audioSource == null) return;
            audioSource.PlayOneShot(clip);
        }

        private enum ToneWave { Sine, Triangle }

        private AudioClip CreateToneClip(string name, float duration, float startFreq, float endFreq, ToneWave wave, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float phase = 2f * Mathf.PI * currentFreq * (i / (float)sampleRate);

                float waveVal = wave == ToneWave.Sine ? Mathf.Sin(phase) : Mathf.PingPong(phase / Mathf.PI, 1f) * 2f - 1f;
                float envelope = 1f - t; // Exponential or linear fade out
                samples[i] = waveVal * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateChimeClip(string name, float[] notes, float noteDuration, float volume)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.CeilToInt(sampleRate * (notes.Length * noteDuration + 0.1f));
            float[] samples = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startIdx = Mathf.CeilToInt(sampleRate * n * noteDuration);
                float freq = notes[n];
                int noteSamples = Mathf.CeilToInt(sampleRate * (noteDuration + 0.1f));

                for (int i = 0; i < noteSamples; i++)
                {
                    int targetIdx = startIdx + i;
                    if (targetIdx >= totalSamples) break;

                    float t = (float)i / noteSamples;
                    float phase = 2f * Mathf.PI * freq * (i / (float)sampleRate);
                    float waveVal = Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 2f) * 0.3f;
                    float envelope = Mathf.Exp(-t * 4f); // Smooth decay
                    samples[targetIdx] += waveVal * envelope * volume;
                }
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateFanfareClip(string name, float[] notes, float noteDuration, float volume)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.CeilToInt(sampleRate * (notes.Length * noteDuration + 0.25f));
            float[] samples = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startIdx = Mathf.CeilToInt(sampleRate * n * noteDuration);
                float freq = notes[n];
                int noteSamples = Mathf.CeilToInt(sampleRate * (noteDuration + 0.25f));

                for (int i = 0; i < noteSamples; i++)
                {
                    int targetIdx = startIdx + i;
                    if (targetIdx >= totalSamples) break;

                    float t = (float)i / noteSamples;
                    float phase = 2f * Mathf.PI * freq * (i / (float)sampleRate);
                    float waveVal = Mathf.Sin(phase) * 0.6f + Mathf.Sin(phase * 2f) * 0.3f + Mathf.Sin(phase * 3f) * 0.1f;
                    float envelope = Mathf.Exp(-t * 3.5f);
                    samples[targetIdx] += waveVal * envelope * volume;
                }
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateLossClip(string name, float[] notes, float noteDuration, float volume)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.CeilToInt(sampleRate * (notes.Length * noteDuration + 0.3f));
            float[] samples = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startIdx = Mathf.CeilToInt(sampleRate * n * noteDuration);
                float freq = notes[n];
                int noteSamples = Mathf.CeilToInt(sampleRate * (noteDuration + 0.3f));

                for (int i = 0; i < noteSamples; i++)
                {
                    int targetIdx = startIdx + i;
                    if (targetIdx >= totalSamples) break;

                    float t = (float)i / noteSamples;
                    float phase = 2f * Mathf.PI * freq * (i / (float)sampleRate);
                    // Triangle wave with slight buzz for dramatic defeat
                    float waveVal = (Mathf.PingPong(phase / Mathf.PI, 1f) * 2f - 1f) * 0.7f + Mathf.Sin(phase) * 0.3f;
                    float envelope = Mathf.Exp(-t * 3f);
                    samples[targetIdx] += waveVal * envelope * volume;
                }
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
