using System;
using UnityEngine;

// Dedicated sustained voices: bass never changes the upper chord's voicing.
[RequireComponent(typeof(AudioSource))]
public sealed class BassExplorerAudio : MonoBehaviour
{
    private static readonly int[] Midi = { 40, 41, 43, 45, 60, 64, 67, 59, 62 };
    private readonly double[] phase = new double[9];
    private readonly float[] gain = new float[9];
    private volatile int voiceMask;
    private volatile int sampleRate = 48000;
    private AudioClip carrier;

    private void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        var source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0;
        source.volume = 1;
        carrier = AudioClip.Create("Bass explorer carrier", 1024, 1, sampleRate, false);
        source.clip = carrier;
    }

    private void OnEnable()
    {
        AudioSettings.OnAudioConfigurationChanged += AudioConfigurationChanged;
        GetComponent<AudioSource>().Play();
    }

    private void AudioConfigurationChanged(bool deviceChanged)
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    public void Play(int bassIndex, bool upperG)
    {
        if (bassIndex < 0 || bassIndex > 3) return;
        voiceMask = (1 << bassIndex) | (1 << 6) |
            (upperG ? (1 << 7) | (1 << 8) : (1 << 4) | (1 << 5));
    }

    public void Stop() { voiceMask = 0; }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        int mask = voiceMask;
        double step = 2 * Math.PI / Math.Max(1, sampleRate);
        float smoothing = (float)(1 - Math.Exp(-1.0 / (sampleRate * 0.025)));
        for (int frame = 0; frame < data.Length; frame += channels)
        {
            double value = 0;
            for (int voice = 0; voice < Midi.Length; voice++)
            {
                float target = (mask & (1 << voice)) == 0 ? 0 : (voice < 4 ? 0.17f : 0.085f);
                gain[voice] += (target - gain[voice]) * smoothing;
                double p = phase[voice];
                value += gain[voice] * (Math.Sin(p) + 0.18 * Math.Sin(2 * p) + 0.06 * Math.Sin(3 * p));
                phase[voice] = (p + step * (440 * Math.Pow(2, (Midi[voice] - 69) / 12.0))) % (2 * Math.PI);
            }
            for (int channel = 0; channel < channels; channel++) data[frame + channel] = (float)value;
        }
    }

    private void OnDisable()
    {
        Stop();
        AudioSettings.OnAudioConfigurationChanged -= AudioConfigurationChanged;
        GetComponent<AudioSource>().Stop();
        Array.Clear(gain, 0, gain.Length);
    }

    private void OnDestroy() { if (carrier != null) Destroy(carrier); }
}
