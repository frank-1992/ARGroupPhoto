#if UNITY_2017_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
#endif

#if AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
using UnityEngine;
#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
#else
using UnityEngine.Collections;
#endif

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{
	/// <summary>
	/// Encodes audio directly from AudioRenderer (https://docs.unity3d.com/ScriptReference/AudioRenderer.html)
	/// While capturing, audio playback in Unity becomes muted
	/// </summary>
	[AddComponentMenu("AVPro Movie Capture/Audio/Capture Audio (From AudioRenderer)", 500)]
	public class CaptureAudioFromAudioRenderer : UnityAudioCapture
	{
		[SerializeField] CaptureBase _capture = null;

		private int _unityAudioChannelCount;
		private bool _isRendererRecording;

		public CaptureBase Capture { get { return _capture; } set { _capture = value; } }
		public override int ChannelCount { get { return _unityAudioChannelCount; } }

		public override void PrepareCapture()
		{
			_unityAudioChannelCount = GetUnityAudioChannelCount();
		}

		public override void StartCapture()
		{
			if (!_isRendererRecording)
			{
				AudioRenderer.Start();
				_isRendererRecording = true;
			}
			FlushBuffer();
		}

		public override void StopCapture()
		{
			if (_isRendererRecording)
			{
				_isRendererRecording = false;
				AudioRenderer.Stop();
			}
		}

		public override void FlushBuffer()
		{
			int sampleFrameCount = AudioRenderer.GetSampleCountForCaptureFrame();
			int sampleCount = sampleFrameCount * _unityAudioChannelCount;
			NativeArray<float> audioSamples = new NativeArray<float>(sampleCount, Allocator.Temp);
			AudioRenderer.Render(audioSamples);
			audioSamples.Dispose();
		}

		void Update()
		{
			if (_isRendererRecording && _capture != null && _capture.IsCapturing() && !_capture.IsPaused())
			{
				int sampleFrameCount = AudioRenderer.GetSampleCountForCaptureFrame();
				int sampleCount = sampleFrameCount * _unityAudioChannelCount;
				// TODO: reuse NativeArray for less GC (but not super important in offline mode)
				NativeArray<float> audioSamples = new NativeArray<float>(sampleCount, Allocator.TempJob);
				if (AudioRenderer.Render(audioSamples))
				{
					// TODO: use NativeArray instead of converting to array for less GC (but not super important in offline mode)
					_capture.EncodeAudio(audioSamples.ToArray());
				}
				audioSamples.Dispose();
			}
		}
	}
}

#endif // AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE