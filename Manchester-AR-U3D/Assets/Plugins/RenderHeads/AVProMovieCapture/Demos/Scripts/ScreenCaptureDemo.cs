using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture.Demos
{
	public class ScreenCaptureDemo : MonoBehaviour
	{
		[SerializeField] AudioClip _audioBG = null;
		[SerializeField] AudioClip _audioHit = null;
		[SerializeField] float _speed = 1.0f;
		[SerializeField] CaptureBase _capture = null;
		[SerializeField] GUISkin _guiSkin = null;
		[SerializeField] bool _spinCamera = true;

		// State
		private float _timer;
		private List<FileWritingHandler> _fileWritingHandlers = new List<FileWritingHandler>(4);

		private IEnumerator Start()
		{
			// Play music track
			if (_audioBG != null)
			{
				AudioSource.PlayClipAtPoint(_audioBG, Vector3.zero);
			}
			if (_capture != null)
			{
				_capture.BeginFinalFileWritingAction += OnBeginFinalFileWriting;
			}

			// Make sure we're authorised for using the microphone. On iOS the OS will forcibly
			// close the application if authorisation has not been granted. Make sure the
			// "Microphone Usage Description" field has been filled in the player settings.
			if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
			{
				yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
			}
			if (Application.HasUserAuthorization(UserAuthorization.Microphone))
			{
				// On iOS modified the audio session to allow recording from the microphone.
				NativePlugin.SetMicrophoneRecordingHint(true);
			}
		}

		private void OnBeginFinalFileWriting(FileWritingHandler handler)
		{
			_fileWritingHandlers.Add(handler);
		}

		private void Update()
		{
			// Press the S key to trigger audio and background color change - useful for testing A/V sync
			if (Input.GetKeyDown(KeyCode.S))
			{
				if (_audioHit != null && _capture != null && _capture.IsCapturing())
				{
					AudioSource.PlayClipAtPoint(_audioHit, Vector3.zero);
					Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value, 0);
				}
			}

			// ESC to stop capture and quit
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (_capture != null && _capture.IsCapturing())
				{
					_capture.StopCapture();
				}
				else
				{
					Application.Quit();
				}
			}

			// Spin the camera around
			if (_spinCamera && Camera.main != null)
			{
				Camera.main.transform.RotateAround(Vector3.zero, Vector3.up, 20f * Time.deltaTime * _speed);
			}

			if (FileWritingHandler.Cleanup(_fileWritingHandlers))
			{
				if (_fileWritingHandlers.Count == 0)
				{
					Debug.Log("All pending file writes completed");
				}
			}
		}

		void OnDestroy()
		{
			foreach (FileWritingHandler handler in _fileWritingHandlers)
			{
				handler.Dispose();
			}
		}

		private void OnGUI()
		{
			GUI.skin = _guiSkin;
			Rect r = new Rect(Screen.width - 108, 64, 128, 28);
			GUI.Label(r, "Frame " + Time.frameCount);
		}
	}
}