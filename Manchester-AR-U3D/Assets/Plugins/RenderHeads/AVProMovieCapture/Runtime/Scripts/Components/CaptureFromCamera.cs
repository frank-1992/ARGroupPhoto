#if UNITY_EDITOR && !UNITY_2018_3_OR_NEWER
	#define SUPPORT_SCENE_VIEW_GIZMOS_CAPTURE
#endif
using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{
	/// <summary>
	/// Capture from a specific Unity camera.  This component re-renders the camera manually, so it does add extra draw calls.
	/// </summary>
	//[RequireComponent(typeof(Camera))]
	[AddComponentMenu("AVPro Movie Capture/Capture From Camera", 1)]
	public class CaptureFromCamera : CaptureBase
	{
		[SerializeField] CameraSelector _cameraSelector = null;
		[SerializeField] Camera _lastCamera = null;
		[SerializeField] Camera[] _contribCameras = null;
		[SerializeField] bool _useContributingCameras = true;
#if SUPPORT_SCENE_VIEW_GIZMOS_CAPTURE
		[Tooltip("When capturing from the Scene View camera, whether to include rendering of Gizmos or not.  If Gizmos are included then resolution and AA options cannot be overridde and will use the current Scene View window settings")]
		[SerializeField] bool _includeSceneViewGizmos = true;
#endif

		private bool _supportTransparency;
		private RenderTexture _target;
		private RenderTexture _resolveTexture;
		private System.IntPtr _targetNativePointer = System.IntPtr.Zero;
		private Texture _targetNativeTexture;
		private Texture _previewTexture;

		public CameraSelector CameraSelector
		{
			get { return _cameraSelector; }
			set { _cameraSelector = value; }
		}

		public bool UseContributingCameras
		{
			get { return _useContributingCameras; }
			set { _useContributingCameras = value; }
		}

		public void SetCamera(Camera topCamera, bool useContributingCameras = true)
		{
			_lastCamera = topCamera;
			_contribCameras = null;
			_useContributingCameras = useContributingCameras;

			if (_useContributingCameras && _lastCamera != null)
			{
				if (Utils.HasContributingCameras(_lastCamera))
				{
					_contribCameras = Utils.FindContributingCameras(topCamera);
				}
			}
		}

		public void SetCamera(Camera topCamera, Camera[] contributingCameras)
		{
			_lastCamera = topCamera;
			_contribCameras = contributingCameras;
		}

		private bool RequiresResolve(Texture texture)
		{
			bool result = false;

			if (texture is RenderTexture)
			{
				RenderTexture rt = texture as RenderTexture;

				// Linear textures require resolving to sRGB
				result = !rt.sRGB;

				if (!result &&
					(rt.format != RenderTextureFormat.ARGB32) &&
					(rt.format != RenderTextureFormat.Default) &&
					(rt.format != RenderTextureFormat.BGRA32)
					)
				{
					// Exotic texture formats require resolving
					result = true;
				}
			}
			else
			{
				// Any other texture type needs to be resolve to RenderTexture
				result = true;
			}

			return result;
		}

		private bool HasCamera()
		{
			return (_lastCamera != null);
		}

		private bool HasContributingCameras()
		{
			return (_useContributingCameras && _contribCameras != null && _contribCameras.Length > 0);
		}

		public override void UpdateFrame()
		{
			// Detect changes from camera selector
			if (_cameraSelector != null)
			{
				if (_lastCamera != _cameraSelector.Camera)
				{
					SetCamera(_cameraSelector.Camera, _useContributingCameras);
				}
			}

			if (_useWaitForEndOfFrame)
			{
				if (_capturing && !_paused && HasCamera())
				{
					StartCoroutine(FinalRenderCapture());
				}

				base.UpdateFrame();
			}
			else
			{
				base.UpdateFrame();
				Capture();
			}
		}

#if EXPERIMENTAL_FRAME_PAUSE
		private bool _stillBusy = false;

		private IEnumerator FinalRenderCapture()
		{
			if (_stillBusy)
			{
				yield return null;
			}
			else
			{
				_stillBusy = true;

				Time.timeScale = 0.0f;
				yield return new WaitUntil(CanContinue);
				Time.timeScale = 1.0f;

				yield return _waitForEndOfFrame;
				yield return Capture();

				_stillBusy = false;
			}
		}
#else
		private IEnumerator FinalRenderCapture()
		{
			yield return _waitForEndOfFrame;
			yield return Capture();
		}
#endif

		// If we're forcing a resolution or AA change then we have to render the camera again to the new target
		// If we try to just set the targetTexture of the camera and grab it in OnRenderImage we can't render it to the screen as before :(
		public IEnumerator Capture()
		{
			TickFrameTimer();

			if (_capturing && !_paused && HasCamera())
			{
				bool canGrab = true;

				if (IsUsingMotionBlur())
				{
					// If the motion blur is still accumulating, don't grab this frame
					canGrab = _motionBlur.IsFrameAccumulated;
				}

				if (canGrab)
				{
					/*while (_handle >= 0 && !AVProMovieCapturePlugin.IsNewFrameDue(_handle))
					{
						System.Threading.Thread.Sleep(1);
					}*/
					if (_handle >= 0 && CanOutputFrame())
					{
						RenderTexture sourceTexture = _target;

						// In 2018.3 and above a different method is used to render the gizmos etc, so we don't yet support capturing these
#if SUPPORT_SCENE_VIEW_GIZMOS_CAPTURE
						// Support capturing the Scene View target texture directly so that we get all gizmo rendering
						if (_lastCamera != null && _includeSceneViewGizmos && (_lastCamera.hideFlags & HideFlags.NotEditable) != 0 && _lastCamera.targetTexture != null)
						{
							sourceTexture = _lastCamera.targetTexture;
						}
#endif
						if (sourceTexture == _target)
						{
							// Render the camera(s)
							if (!IsUsingMotionBlur())
							{
								UpdateTexture();
							}
							else
							{
								// Just grab the last result of the motion blur
								_target.DiscardContents();
								Graphics.Blit(_motionBlur.FinalTexture, _target);
							}
						}

						// If the texture isn't suitable then blit it to the Rendertexture so the native plugin can grab it
						if (RequiresResolve(sourceTexture))
						{
							CreateResolveTexture(sourceTexture.width, sourceTexture.height);
							_resolveTexture.DiscardContents();

							// Between Unity 2018.1.0 and 2018.3.0 Unity doesn't seem to set the correct sRGBWrite state and keeps it as false
							#if (UNITY_2018_1_OR_NEWER && !UNITY_2018_3_OR_NEWER)
							bool sRGBWritePrev = GL.sRGBWrite;
							GL.sRGBWrite = true;
							#endif

							Graphics.Blit(sourceTexture, _resolveTexture);
							sourceTexture = _resolveTexture;

							#if (UNITY_2018_1_OR_NEWER && !UNITY_2018_3_OR_NEWER)
							GL.sRGBWrite = sRGBWritePrev;
							#endif
						}

						if (_supportTextureRecreate || _targetNativeTexture != sourceTexture)
						{
							// NOTE: If support for captures to survive through alt-tab events, or window resizes where the GPU resources are recreated
							// is required, then this line is needed.  It is very expensive though as it does a sync with the rendering thread.
							_targetNativePointer = sourceTexture.GetNativeTexturePtr();
							_targetNativeTexture = sourceTexture;
						}

						_previewTexture = sourceTexture;

						NativePlugin.SetTexturePointer(_handle, _targetNativePointer);

						RenderThreadEvent(NativePlugin.PluginEvent.CaptureFrameBuffer);
						EncodeUnityAudio();

						UpdateFPS();
					}
				}
			}
			base.UpdateFrame();

			RenormTimer();

			yield break;
		}

		private bool RequiresHDR()
		{
			// Check if any of the cameras in the chain are set to allow HDR
			bool result = _lastCamera.allowHDR;

			if (!result && HasContributingCameras())
			{
				for (int cameraIndex = 0; cameraIndex < _contribCameras.Length; cameraIndex++)
				{
					Camera camera = _contribCameras[cameraIndex];
					if (camera != null && camera.isActiveAndEnabled)
					{
						result = camera.allowHDR;
						if (result)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		private void UpdateTexture()
		{
			// Between Unity 2018.1.0 and 2018.3.0 Unity doesn't seem to set the correct sRGBWrite state and keeps it as false
			#if (UNITY_2018_1_OR_NEWER && !UNITY_2018_3_OR_NEWER)
			bool sRGBWritePrev = GL.sRGBWrite;
			GL.sRGBWrite = _target.sRGB;
			#endif

			// Render a single camera
			if (!HasContributingCameras())
			{
				RenderTexture prev = _lastCamera.targetTexture;
				// Reset the viewport rect as we're rendering to a texture captures the full viewport
				Rect prevRect = _lastCamera.rect;
				CameraClearFlags prevClear = _lastCamera.clearFlags;
				Color prevColor = _lastCamera.backgroundColor;
				bool clearChanged = false;
				if (_lastCamera.clearFlags == CameraClearFlags.Nothing || _lastCamera.clearFlags == CameraClearFlags.Depth)
				{
					clearChanged = true;
					_lastCamera.clearFlags = CameraClearFlags.SolidColor;
					if (!_supportTransparency)
					{
						_lastCamera.backgroundColor = Color.black;
					}
					else
					{
						_lastCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
					}
				}

				// Render
				_lastCamera.rect = new Rect(0f, 0f, 1f, 1f);
				_lastCamera.targetTexture = _target;
				_lastCamera.Render();

				// Restore camera
				{
					_lastCamera.rect = prevRect;
					if (clearChanged)
					{
						_lastCamera.clearFlags = prevClear;
						_lastCamera.backgroundColor = prevColor;
					}
					_lastCamera.targetTexture = prev;
				}
			}
			// Render the camera chain
			else
			{
				// First render contributing cameras
				for (int cameraIndex = 0; cameraIndex < _contribCameras.Length; cameraIndex++)
				{
					Camera camera = _contribCameras[cameraIndex];
					if (camera != null && camera.isActiveAndEnabled)
					{
						RenderTexture prev = camera.targetTexture;
						camera.targetTexture = _target;
						camera.Render();
						camera.targetTexture = prev;
					}
				}
				// Finally render the last camera
				if (_lastCamera != null)
				{
					RenderTexture prev = _lastCamera.targetTexture;
					_lastCamera.targetTexture = _target;
					_lastCamera.Render();
					_lastCamera.targetTexture = prev;
				}
			}

			//UnityEditor.Selection.activeObject = _lastCamera.targetTexture;

			#if (UNITY_2018_1_OR_NEWER && !UNITY_2018_3_OR_NEWER)
			GL.sRGBWrite = sRGBWritePrev;
			#endif
		}

	#if false
		// NOTE: This is old code based on OnRenderImage...may be revived at some point
		private void OnRenderImage(RenderTexture source, RenderTexture dest)
		{
			if (_capturing && !_paused)
			{
	#if true
				while (_handle >= 0 && !NativePlugin.IsNewFrameDue(_handle))
				{
					System.Threading.Thread.Sleep(1);
				}
				if (_handle >= 0)
				{
					if (_audioCapture && _audioDeviceIndex < 0 && !_noAudio && _isRealTime)
					{
						int audioDataLength = 0;
						System.IntPtr audioDataPtr = _audioCapture.ReadData(out audioDataLength);
						if (audioDataLength > 0)
						{
							NativePlugin.EncodeAudio(_handle, audioDataPtr, (uint)audioDataLength);
						}
					}

					// In Direct3D the RT can be flipped vertically
					/*if (source.texelSize.y < 0)
					{

					}*/

					Graphics.Blit(source, dest);

					_lastSource = source;
					_lastDest = dest;

					if (dest != _originalTarget)
					{
						Graphics.Blit(dest, _originalTarget);
					}

	#if AVPRO_MOVIECAPTURE_GLISSUEEVENT_52
					GL.IssuePluginEvent(NativePlugin.GetRenderEventFunc(), NativePlugin.PluginID | (int)NativePlugin.PluginEvent.CaptureFrameBuffer | _handle);
	#else
					GL.IssuePluginEvent(NativePlugin.PluginID | (int)NativePlugin.PluginEvent.CaptureFrameBuffer | _handle);
	#endif
					GL.InvalidateState();

					UpdateFPS();

					return;
				}
	#endif
			}

			// Pass-through
			Graphics.Blit(source, dest);

			_lastSource = source;
			_lastDest = dest;
		}
	#endif

		public override void UnprepareCapture()
		{
			NativePlugin.SetTexturePointer(_handle, System.IntPtr.Zero);

			if (_target != null)
			{
				_target.DiscardContents();
			}

			_targetNativePointer = System.IntPtr.Zero;
			_targetNativeTexture = null;
			_previewTexture = null;

			base.UnprepareCapture();
		}

		private void CreateResolveTexture(int width, int height)
		{
			if (_resolveTexture != null)
			{
				if (_resolveTexture.width != width ||
					_resolveTexture.height != height)
				{
					RenderTexture.ReleaseTemporary(_resolveTexture);
					_resolveTexture = null;
				}
			}
			if (_resolveTexture == null)
			{
				_resolveTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
				_resolveTexture.Create();
			}

			if (_resolveTexture != null)
			{
				_targetNativePointer = _resolveTexture.GetNativeTexturePtr();
				_targetNativeTexture = _resolveTexture;
			}
		}

		public override Texture GetPreviewTexture()
		{
			return _previewTexture;
		}

		public override bool PrepareCapture()
		{
			if (_capturing)
			{
				return false;
			}
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			if (SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 9"))
			{
				Debug.LogError("[AVProMovieCapture] Direct3D9 not yet supported, please use Direct3D11 instead.");
				return false;
			}
			else if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") && !SystemInfo.graphicsDeviceVersion.Contains("emulated"))
			{
				Debug.LogError("[AVProMovieCapture] OpenGL not yet supported for CaptureFromCamera component, please use Direct3D11 instead. You may need to switch your build platform to Windows.");
				return false;
			}
#endif
			// Setup material
			_pixelFormat = NativePlugin.PixelFormat.RGBA32;
			_isTopDown = true;

			_supportTransparency = ((_outputTarget == OutputTarget.VideoFile || _outputTarget == OutputTarget.NamedPipe) && GetEncoderHints().videoHints.supportTransparency) ||
									(_outputTarget == OutputTarget.ImageSequence && GetEncoderHints().imageHints.supportTransparency);

			if (!HasCamera())
			{
				if (_cameraSelector != null)
				{
					if (_lastCamera != _cameraSelector.Camera)
					{
						SetCamera(_cameraSelector.Camera, _useContributingCameras);
					}
				}

				if (!HasCamera())
				{
					SetCamera(this.GetComponent<Camera>(), _useContributingCameras);
				}
				if (!HasCamera())
				{
					SetCamera(Camera.main, _useContributingCameras);
				}
				if (!HasCamera())
				{
					Debug.LogError("[AVProMovieCapture] No camera assigned to CaptureFromCamera");
					return false;
				}
			}

			if (!HasContributingCameras() && (_lastCamera.clearFlags == CameraClearFlags.Depth || _lastCamera.clearFlags == CameraClearFlags.Nothing))
			{
				Debug.LogWarning("[AVProMovieCapture] This camera doesn't clear, consider setting contributing cameras");
			}

			int width = Mathf.FloorToInt(_lastCamera.pixelRect.width);
			int height = Mathf.FloorToInt(_lastCamera.pixelRect.height);

			// Setup rendering a different render target if we're overriding resolution or anti-aliasing
			{
				if (_renderResolution == Resolution.Custom)
				{
					width = (int)_renderSize.x;
					height = (int)_renderSize.y;
				}
				else if (_renderResolution != Resolution.Original)
				{
					GetResolution(_renderResolution, ref width, ref height);
				}

				int aaLevel = GetCameraAntiAliasingLevel(_lastCamera);

				// Create the render target
				if (_target != null)
				{
					_target.DiscardContents();
					if (_target.width != width || _target.height != height || _target.antiAliasing != aaLevel)
					{
						_targetNativePointer = System.IntPtr.Zero;
						_targetNativeTexture = null;
						RenderTexture.ReleaseTemporary(_target);
						_target = null;
					}
				}

				if (_target == null)
				{
					RenderTextureFormat textureFormat = Utils.GetBestRenderTextureFormat(RequiresHDR(), _supportTransparency, IsRealTime);
					_target = RenderTexture.GetTemporary(width, height, 24, textureFormat, RenderTextureReadWrite.Default, aaLevel);
					_target.name = "[AVProMovieCapture] Camera Target";
					_target.Create();
				}

				if (_target != null)
				{
					_targetNativePointer = _target.GetNativeTexturePtr();
					_targetNativeTexture = _target;
				}

				// Adjust size for camera rectangle
				/*if (camera.rect.width < 1f || camera.rect.height < 1f)
				{
					float rectWidth = Mathf.Clamp01(camera.rect.width + camera.rect.x) - Mathf.Clamp01(camera.rect.x);
					float rectHeight = Mathf.Clamp01(camera.rect.height + camera.rect.y) - Mathf.Clamp01(camera.rect.y);
					width = Mathf.FloorToInt(width * rectWidth);
					height = Mathf.FloorToInt(height * rectHeight);
				}*/

				if (_useMotionBlur)
				{
					_motionBlurCameras = new Camera[1];
					_motionBlurCameras[0] = _lastCamera;
				}
			}

			SelectRecordingResolution(width, height);

			GenerateFilename();

			return base.PrepareCapture();
		}

		public override void OnDestroy()
		{
			if (_resolveTexture != null)
			{
				RenderTexture.ReleaseTemporary(_resolveTexture);
				_resolveTexture = null;
			}

			if (_target != null)
			{
				_targetNativePointer = System.IntPtr.Zero;
				RenderTexture.ReleaseTemporary(_target);
				_target = null;
			}

			base.OnDestroy();
		}
	}
}