#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.GameServices
{
    /// <summary>
    /// Manages the Steam Voice feature.
    /// <a href="https://partner.steamgames.com/doc/features/voice">https://partner.steamgames.com/doc/features/voice</a>
    /// </summary>
    public class SteamworksVoiceManager : MonoBehaviour
    {
        public enum SampleRateMethod
        {
            Optimal,
            Native,
            Custom
        }

        /// <summary>
        /// The audio source to output recieved and decoded voice messages to.
        /// </summary>
        public AudioSource OutputSource;
        public SampleRateMethod sampleRateMethod = SampleRateMethod.Optimal;
        [Range(11025, 48000)]
        public uint customSampleRate = 28000;
        public bool useAudioStreaming = true;
        [Range(0f, 1f)]
        public float bufferLength = 0.25f;

        [ReadOnly(true)]
        [SerializeField]
        private bool isRecording = false;
        /// <summary>
        /// Is the system currently recording audio data.
        /// </summary>
        public bool IsRecording
        {
            get { return isRecording; }
        }
        /// <summary>
        /// Occures when the Voice Result Restricted EVoiceResult is recieved from the Steam API.
        /// </summary>
        public UnityEvent StopedOnChatRestricted;
        /// <summary>
        /// Occures every frame when the Steam API has a voice stream payload from the user.
        /// </summary>
        public ByteArrayEvent VoiceStream;
        private int sampleRate;
        private Queue<float> audioBuffer = new Queue<float>(48000);
        private Queue<AudioClip> clipBuffer = new Queue<AudioClip>();
        private float packetCounter = 0;

        public double encodingTime = 0;

        private void Start()
        {
            OutputSource.loop = true;
            packetCounter = bufferLength;
        }

        private void Update()
        {
            var nSample = sampleRateMethod == SampleRateMethod.Optimal ? (int)SteamUser.GetVoiceOptimalSampleRate() : sampleRateMethod == SampleRateMethod.Native ? AudioSettings.outputSampleRate : (int)customSampleRate;

            if(nSample != sampleRate)
            {
                sampleRate = nSample;
                OutputSource.Stop();

                if (OutputSource.clip != null)
                    Destroy(OutputSource.clip);

                if (useAudioStreaming)
                {
                    OutputSource.clip = AudioClip.Create("VOICE", sampleRate * 2, 1, (int)sampleRate, true, OnAudioRead);
                    OutputSource.Play();
                }
                else
                {
                    OutputSource.clip = AudioClip.Create("VOICE", sampleRate * 2, 1, (int)sampleRate, false);
                }
            }

            if(!useAudioStreaming && OutputSource.loop)
            {
                OutputSource.loop = false;
                OutputSource.clip = AudioClip.Create("VOICE", sampleRate * 2, 1, (int)sampleRate, false);
            }
            else if (useAudioStreaming && !OutputSource.loop)
            {
                OutputSource.loop = true;
                OutputSource.clip = AudioClip.Create("VOICE", sampleRate * 2, 1, (int)sampleRate, true, OnAudioRead);
                OutputSource.Play();
            }

            if(!useAudioStreaming && clipBuffer.Count > 0 && !OutputSource.isPlaying)
            {
                OutputSource.clip = clipBuffer.Dequeue();
                OutputSource.Play();
            }

            packetCounter -= Time.unscaledDeltaTime;

            if (packetCounter <= 0)
            {
                packetCounter = bufferLength;

                if (isRecording)
                {
                    var result = SteamUser.GetAvailableVoice(out uint pcbCompressed);
                    switch (result)
                    {
                        case EVoiceResult.k_EVoiceResultOK:
                            //All is well check the compressed size to see if we have data and if so package it
                            byte[] buffer = new byte[pcbCompressed];
                            SteamUser.GetVoice(true, buffer, pcbCompressed, out uint bytesWriten);
                            if (bytesWriten > 0)
                                VoiceStream.Invoke(buffer);
                            break;
                        case EVoiceResult.k_EVoiceResultNoData:
                            //No data so do nothing
                            break;
                        case EVoiceResult.k_EVoiceResultNotInitialized:
                            //Not initalized ... report the error
                            Debug.LogError("The Steam Voice systemis not initalized and will be stoped.");
                            SteamUser.StopVoiceRecording();
                            break;
                        case EVoiceResult.k_EVoiceResultNotRecording:
                            //We are not recording but think we are
                            SteamUser.StartVoiceRecording();
                            break;
                        case EVoiceResult.k_EVoiceResultRestricted:
                            //User is chat restricted ... report this out and turn off recording.
                            StopedOnChatRestricted.Invoke();
                            SteamUser.StopVoiceRecording();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the Steam API recording audio from the user's configured mic
        /// </summary>
        public void StartRecording()
        {
            isRecording = true;
            SteamUser.StartVoiceRecording();
        }

        /// <summary>
        /// Stops the Steam API from recording audio for the user's configured mic
        /// </summary>
        public void StopRecording()
        {
            isRecording = false;
            SteamUser.StopVoiceRecording();
        }

        /// <summary>
        /// Players a recieved Steam Voice package through the <see cref="OutputSource"/> <see cref="AudioSource"/>.
        /// </summary>
        /// <param name="buffer"></param>
        public void PlayVoiceData(byte[] buffer)
        {
            uint bytesWritten;
            byte[] destBuffer = new byte[20000];
            var result = SteamUser.DecompressVoice(buffer, (uint)buffer.Length, destBuffer, (uint)destBuffer.Length, out bytesWritten, (uint)sampleRate);
            var timeStamp = DateTime.Now;

            if (result == EVoiceResult.k_EVoiceResultBufferTooSmall)
            {
                destBuffer = new byte[bytesWritten];
                result = SteamUser.DecompressVoice(buffer, (uint)buffer.Length, destBuffer, (uint)destBuffer.Length, out bytesWritten, (uint)sampleRate);
            }

            //Handle audio encoding result == EVoiceResult.k_EVoiceResultOK && 
            if (bytesWritten > 0)
            {
                if (useAudioStreaming)
                {
                    //We are currently playing so enqueue this data and let the reader handle it
                    for (int i = 0; i < bytesWritten; i += 2)
                    {
                        audioBuffer.Enqueue((short)(destBuffer[i] | destBuffer[i + 1] << 8) / 32768f);
                    }
                }
                else
                {
                    float[] clipData = new float[ 2 + (bytesWritten / 2)];
                    var clipSample = 1;
                    for (int i = 0; i < bytesWritten; i += 2)
                    {
                        clipData[clipSample] = (short)(destBuffer[i] | destBuffer[i + 1] << 8) / 32768f;
                        clipSample++;
                    }
                    clipSample += 1;

                    if (!OutputSource.isPlaying && OutputSource.clip != null)
                    {
                        Destroy(OutputSource.clip);
                        OutputSource.clip = AudioClip.Create("VOICE", clipSample, 1, (int)sampleRate, false);
                        OutputSource.clip.SetData(clipData, 0);
                        OutputSource.Play();
                    }
                    else
                    {
                        var nClip = AudioClip.Create("VOICE " + timeStamp.ToBinary().ToString(), clipSample, 1, (int)sampleRate, false);
                        nClip.SetData(clipData, 0);
                        clipBuffer.Enqueue(nClip);
                    }
                }

                var clip = (DateTime.Now - timeStamp).TotalMilliseconds;
                if (clip > encodingTime)
                    encodingTime = clip;
            }
            else
            {
                Debug.LogWarning("Unknown result message: " + result.ToString());
            }
        }

        private void OnAudioRead(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (audioBuffer.Count > 0)
                {
                    //If we have data write it
                    data[i] = audioBuffer.Dequeue();
                }
                else
                {
                    //If we dont write silence
                    data[i] = 0;
                }
            }
        }
    }
}
#endif