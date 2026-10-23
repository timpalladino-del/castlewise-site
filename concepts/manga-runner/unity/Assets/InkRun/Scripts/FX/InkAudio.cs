using System.Collections.Generic;
using UnityEngine;
using InkRun.Meta;

namespace InkRun.FX
{
    /// <summary>
    /// Small synthesized sound effects generated at startup, so the gray-box build has
    /// sound without any audio assets. Replace clips with real ones later via the Override list.
    /// </summary>
    public class InkAudio : MonoBehaviour
    {
        public static InkAudio Instance { get; private set; }

        [System.Serializable] public class Override { public string Id; public AudioClip Clip; }
        public List<Override> Overrides = new List<Override>();

        AudioSource _src;
        readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        enum Wave { Sine, Square, Saw }

        void Awake()
        {
            Instance = this;
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _clips["jump"] = Synth("jump", 0.16f, 300f, 640f, Wave.Square, 0.5f);
            _clips["slide"] = Synth("slide", 0.14f, 220f, 110f, Wave.Saw, 0.35f);
            _clips["coin"] = Synth("coin", 0.09f, 1100f, 1700f, Wave.Sine, 0.5f);
            _clips["power"] = Synth("power", 0.28f, 400f, 1400f, Wave.Square, 0.45f);
            _clips["smash"] = Synth("smash", 0.22f, 180f, 60f, Wave.Saw, 0.7f);
            _clips["crash"] = Synth("crash", 0.5f, 140f, 35f, Wave.Saw, 0.9f);
            _clips["revive"] = Synth("revive", 0.35f, 330f, 990f, Wave.Square, 0.45f);
            _clips["clear"] = Arpeggio("clear", new[] { 660f, 880f, 1320f }, 0.13f, Wave.Square, 0.45f);
            _clips["ui"] = Synth("ui", 0.06f, 900f, 1200f, Wave.Sine, 0.3f);
            foreach (var o in Overrides) if (o.Clip != null) _clips[o.Id] = o.Clip;
        }

        public static void Play(string id, float volume = 1f)
        {
            if (Instance == null || !SaveData.Current.SoundOn) return;
            if (Instance._clips.TryGetValue(id, out var clip)) Instance._src.PlayOneShot(clip, volume);
        }

        static AudioClip Synth(string name, float seconds, float f0, float f1, Wave wave, float volume)
        {
            const int sr = 44100;
            int n = Mathf.Max(1, Mathf.RoundToInt(seconds * sr));
            var data = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float f = f0 * Mathf.Pow(f1 / f0, t);
                phase += f / sr;
                float p = (float)(phase - System.Math.Floor(phase));
                float v = wave == Wave.Sine ? Mathf.Sin(p * Mathf.PI * 2f) : wave == Wave.Square ? (p < 0.5f ? 1f : -1f) : (2f * p - 1f);
                float env = Mathf.Exp(-4.5f * t) * (1f - t);
                data[i] = v * env * volume * 0.25f;
            }
            var clip = AudioClip.Create(name, n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Arpeggio(string name, float[] notes, float noteSeconds, Wave wave, float volume)
        {
            const int sr = 44100;
            int per = (int)(noteSeconds * sr), n = per * notes.Length;
            var data = new float[n];
            for (int k = 0; k < notes.Length; k++)
            {
                double phase = 0;
                for (int i = 0; i < per; i++)
                {
                    float t = i / (float)per;
                    phase += notes[k] / sr;
                    float p = (float)(phase - System.Math.Floor(phase));
                    float v = wave == Wave.Sine ? Mathf.Sin(p * Mathf.PI * 2f) : wave == Wave.Square ? (p < 0.5f ? 1f : -1f) : (2f * p - 1f);
                    data[k * per + i] = v * Mathf.Exp(-3f * t) * volume * 0.25f;
                }
            }
            var clip = AudioClip.Create(name, n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
