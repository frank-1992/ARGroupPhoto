//#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5 || UNITY_5_4_OR_NEWER
//	#define UNITY_FEATURE_UGUI
//#endif

//using UnityEngine;
//#if UNITY_FEATURE_UGUI
//using UnityEngine.UI;
//using System.IO;
//using System.Collections;
//using RenderHeads.Media.AVProVideo;
//using System;
//using DataBank;
//using System.Runtime.InteropServices;
//using FFmpeg;

////-----------------------------------------------------------------------------
//// Copyright 2015-2020 RenderHeads Ltd.  All rights reserved.
////-----------------------------------------------------------------------------

//namespace RenderHeads.Media.AVProVideo.Demos
//{
//	/// <summary>
//	/// A demo of a simple video player using uGUI for display
//	/// Uses two MediaPlayer components, with one displaying the current video
//	/// while the other loads the next video.  MediaPlayers are then swapped
//	/// once the video is loaded and has a frame available for display.
//	/// This gives a more seamless display than simply using a single MediaPlayer
//	/// as its texture will be destroyed when it loads a new video
//	/// </summary>
//	public class VCR : MonoBehaviour 
//	{
//		public static VCR inst;

//		public MediaPlayer	_mediaPlayerA;
//		public MediaPlayer	_mediaPlayerB;
//		public DisplayUGUI	_mediaDisplay;
//		public RectTransform _bufferedSliderRect;

//		public Slider		_videoSeekSlider;
//		private float		_setVideoSeekSliderValue;
//		private bool		_wasPlayingOnScrub;

//		public Slider		_audioVolumeSlider;
//		private float		_setAudioVolumeSliderValue;

//		public Toggle		_AutoStartToggle;
//		public Toggle		_MuteToggle;

//		public Button		_playbackPlayBtn = null;
//		public Button		_playbackPauseBtn = null;
//		public Text			_currentSec = null;
//		public Text			_maxSec = null;

//		public MediaPlayer.FileLocation _location = MediaPlayer.FileLocation.RelativeToPersistentDataFolder;
//		//public string _folder = "AVProVideos/";
//		//public string[] _videoFiles = { "MVX.mp4" };
//		private string videoFile = "MVX.mp4";

//		string src_path = "" + Path.Combine(Application.persistentDataPath, "MVX.mp4");
//		string finalPath = "" + Path.Combine(Application.persistentDataPath, "Mantis-Append.mp4");

//		private int _VideoIndex = 0;
//		private Image _bufferedSliderImage;

//		private MediaPlayer _loadingPlayer;

//		//user manager
//		public GameObject videoPlaybackManager;
//		public Canvas playbackCanvas;
//		//public ARSceneUIManager arsceneUIManager;
//		public GameObject uploadPanel;
//		private bool isStart = false;

//		AppendData config = new AppendData();

//		public MediaPlayer PlayingPlayer
//		{
//			get
//			{
//				if (LoadingPlayer == _mediaPlayerA)
//				{
//					return _mediaPlayerB;
//				}
//				return _mediaPlayerA;
//			}
//		}

//		public MediaPlayer LoadingPlayer
//		{
//			get
//			{
//				return _loadingPlayer;
//			}
//		}

//		private void SwapPlayers()
//		{
//			Debug.Log("VCR: SwapPlayers");
//			// Pause the previously playing video
//			PlayingPlayer.Control.Pause();

//			// Swap the videos
//			if (LoadingPlayer == _mediaPlayerA)
//			{
//				_loadingPlayer = _mediaPlayerB;
//			}
//			else
//			{
//				_loadingPlayer = _mediaPlayerA;
//			}

//			// Change the displaying video
//			_mediaDisplay.CurrentMediaPlayer = PlayingPlayer;
//		}

//        public void OnEnable()
//        {
//			Debug.Log("VCR: OnEnable");
//			_loadingPlayer = _mediaPlayerB;
//			Execute();
//			if (_playbackPlayBtn != null)
//            {
//				_playbackPlayBtn.gameObject.SetActive(false);
//			}
//			if (_playbackPauseBtn != null)
//            {
//				_playbackPauseBtn.gameObject.SetActive(true);
//			}
//			if (uploadPanel != null)
//            {
//				uploadPanel.SetActive(false);
//			}
//			if (_currentSec != null)
//			{
//				_currentSec.text = "00:00";
//			}
//			if (_maxSec != null)
//            {
//				_maxSec.text = "00:00";
//			}
//		}

//		public void OnDisable()
//		{

//		}

//		public void ShowUploadDialogPanel()
//		{
//			if (uploadPanel!=null)
//            {
//				uploadPanel.SetActive(true);
//			}
//		}

//		public void OnBackupFile()
//        {
//			string userid = PlayerPrefs.GetString("userid");

//			FileInfo info = new FileInfo(finalPath);
//			if (info.Exists == true)
//			{
//                Debug.Log("have source " + finalPath + " Length=" + info.Length + "  Userid=" + userid);
//			string src_path = "" + Path.Combine(Application.persistentDataPath, videoFile);
//            string dest_path = "" + Path.Combine(Application.persistentDataPath, "" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + videoFile);
//			Debug.Log(src_path);
//			Debug.Log(dest_path);

//				FileStream writeS = File.Create(dest_path);
//				if (writeS != null)
//				{
//					int total = 0;

//					using (var fs = File.OpenRead(finalPath))
//					{
//						byte[] buffer = new byte[4096];
//						//worker.ReportProgress(0);
//						for (; ; )
//						{
//							int read = fs.Read(buffer, 0, buffer.Length);
//							if (read == 0) break;
//							writeS.Write(buffer, 0, read);
//							total += read;
//							//worker.ReportProgress(total * 100 / fs.Length);
//						}

//						Debug.Log("Finish write " + dest_path + "  total=" + total);
//					}

//					int isPort = 0;// 横屏：0，竖屏：1
//					if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
//					{
//						isPort = 1;
//					}

//					//save to db
//					UploadVideoDB m_UpVideoDB = new UploadVideoDB();
//                    m_UpVideoDB.addData(new UploadVideoEntity(dest_path, "" + total, userid, "0", "" + isPort, "" + Screen.width, ""+Screen.height));
//					m_UpVideoDB.close();
//					Debug.Log("Save to db: " + dest_path + "  total=" + total);
//				}
//			}
//			else
//            {
//				Debug.Log("No file =" + src_path);
//			}

//			GameObject gobj = GameObject.Find("UICamera");
//			if (gobj!=null)
//            {
//				DemoScript dscript = (DemoScript)gobj.GetComponent( typeof(DemoScript) );
//				dscript.GoCoverScene();
//			}
//		}

//		public void OnOpenVideoFile()
// 		{
//			LoadingPlayer.m_VideoPath = System.IO.Path.Combine(Application.persistentDataPath, videoFile);
//			//_VideoIndex = (_VideoIndex + 1) % (_videoFiles.Length);
//			//if (string.IsNullOrEmpty(LoadingPlayer.m_VideoPath))
//			//{
//			//	LoadingPlayer.CloseVideo();
//			//	_VideoIndex = 0;
//			//}
//			//else
//			//{
//				LoadingPlayer.OpenVideoFromFile(_location, LoadingPlayer.m_VideoPath, _AutoStartToggle.isOn);
//			_mediaPlayerA.m_AutoStart = true;
////				SetButtonEnabled( "PlayButton", !_mediaPlayer.m_AutoStart );
////				SetButtonEnabled( "PauseButton", _mediaPlayer.m_AutoStart );
////}

//			if (_bufferedSliderRect != null)
//			{
//				_bufferedSliderImage = _bufferedSliderRect.GetComponent<Image>();
//			}
//		}

//		public void OnAutoStartChange()
//		{
//			if(PlayingPlayer && 
//				_AutoStartToggle && _AutoStartToggle.enabled &&
//				PlayingPlayer.m_AutoStart != _AutoStartToggle.isOn )
//			{
//				PlayingPlayer.m_AutoStart = _AutoStartToggle.isOn;
//			}
//			if (LoadingPlayer &&
//				_AutoStartToggle && _AutoStartToggle.enabled &&
//				LoadingPlayer.m_AutoStart != _AutoStartToggle.isOn)
//			{
//				LoadingPlayer.m_AutoStart = _AutoStartToggle.isOn;
//			}
//		}

//		public void OnMuteChange()
//		{
//			if (PlayingPlayer)
//			{
//				PlayingPlayer.Control.MuteAudio(_MuteToggle.isOn);
//			}
//			if (LoadingPlayer)
//			{
//				LoadingPlayer.Control.MuteAudio(_MuteToggle.isOn);
//			}
//		}

//		public void OnPlayButton()
//		{
//			if(PlayingPlayer)
//			{
//				PlayingPlayer.Control.Play();
//				_playbackPlayBtn.gameObject.SetActive(false);
//				_playbackPauseBtn.gameObject.SetActive(true);
//				//				SetButtonEnabled( "PlayButton", false );
//				//				SetButtonEnabled( "PauseButton", true );
//			}
//		}
//		public void OnPauseButton()
//		{
//			if(PlayingPlayer)
//			{
//				PlayingPlayer.Control.Pause();
//				_playbackPlayBtn.gameObject.SetActive(true);
//				_playbackPauseBtn.gameObject.SetActive(false);
//				//				SetButtonEnabled( "PauseButton", false );
//				//				SetButtonEnabled( "PlayButton", true );
//			}
//		}

//		public void OnVideoSeekSlider()
//		{
//			if (PlayingPlayer && _videoSeekSlider && _videoSeekSlider.value != _setVideoSeekSliderValue)
//			{
//				PlayingPlayer.Control.Seek(_videoSeekSlider.value * PlayingPlayer.Info.GetDurationMs());
//			}
//		}

//		public void OnVideoSliderDown()
//		{
//			if(PlayingPlayer)
//			{
//				_wasPlayingOnScrub = PlayingPlayer.Control.IsPlaying();
//				if( _wasPlayingOnScrub )
//				{
//					PlayingPlayer.Control.Pause();
////					SetButtonEnabled( "PauseButton", false );
////					SetButtonEnabled( "PlayButton", true );
//				}
//				OnVideoSeekSlider();
//			}
//		}
//		public void OnVideoSliderUp()
//		{
//			if(PlayingPlayer && _wasPlayingOnScrub )
//			{
//				PlayingPlayer.Control.Play();
//				_wasPlayingOnScrub = false;

////				SetButtonEnabled( "PlayButton", false );
////				SetButtonEnabled( "PauseButton", true );
//			}			
//		}

//		public void OnAudioVolumeSlider()
//		{
//			if (PlayingPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
//			{
//				PlayingPlayer.Control.SetVolume(_audioVolumeSlider.value);
//			}
//			if (LoadingPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
//			{
//				LoadingPlayer.Control.SetVolume(_audioVolumeSlider.value);
//			}
//		}
//		//		public void OnMuteAudioButton()
//		//		{
//		//			if( _mediaPlayer )
//		//			{
//		//				_mediaPlayer.Control.MuteAudio( true );
//		//				SetButtonEnabled( "MuteButton", false );
//		//				SetButtonEnabled( "UnmuteButton", true );
//		//			}
//		//		}
//		//		public void OnUnmuteAudioButton()
//		//		{
//		//			if( _mediaPlayer )
//		//			{
//		//				_mediaPlayer.Control.MuteAudio( false );
//		//				SetButtonEnabled( "UnmuteButton", false );
//		//				SetButtonEnabled( "MuteButton", true );
//		//			}
//		//		}

//		public void OnRewindButton()
//		{
//			if(PlayingPlayer)
//			{
//				PlayingPlayer.Control.Rewind();
//			}
//		}

//		private void Awake()
//		{
//			inst = this;
//			_loadingPlayer = _mediaPlayerB;

//			Debug.Log("VCR: Awake");
//		}

//		void Start()
//		{
//			Debug.Log("VCR: Start");
//			//Execute();
//			isStart = true;

//			int isPort = 0;// 横屏：0，竖屏：1
//			if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
//			{
//				isPort = 1;
//			}

//			//FFMPEG进行视频拼接
//			config.outputPath = finalPath;
//			config.inputPaths.Clear();
//			string inputVideoPath = src_path;
//			string appendVideoPath = "";
//			if (isPort == 0 && Screen.width == 2388)
//            {
//				appendVideoPath = Application.streamingAssetsPath + "/mantis_horizontal23881688.mp4";
//			}
//            else if(isPort == 1 && Screen.width == 1688)
//            {
//				appendVideoPath = Application.streamingAssetsPath + "/mantis_vertical16882388.mp4";
//            }
//            else if(isPort == 0 && Screen.width == 2732)
//            {
//				appendVideoPath = Application.streamingAssetsPath + "/mantis_horizontal.mp4";
//			}
//			config.inputPaths.Add(inputVideoPath);
//			config.inputPaths.Add(appendVideoPath);
//			FFmpegCommands.AppendFull(config);
//			isStart = true;
//		}

//		public void Execute()
//        {
//			Debug.Log("VCR: Execute");
//			if (PlayingPlayer)
//			{
//				PlayingPlayer.Events.AddListener(OnVideoEvent);

//				if (LoadingPlayer)
//				{
//					LoadingPlayer.Events.AddListener(OnVideoEvent);
//				}

//				if (_audioVolumeSlider)
//				{
//					// Volume
//					if (PlayingPlayer.Control != null)
//					{
//						float volume = PlayingPlayer.Control.GetVolume();
//						_setAudioVolumeSliderValue = volume;
//						_audioVolumeSlider.value = volume;
//					}
//				}

//				// Auto start toggle
//				_AutoStartToggle.isOn = PlayingPlayer.m_AutoStart;

//				if (PlayingPlayer.m_AutoOpen)
//				{
//					//					RemoveOpenVideoButton();

//					//					SetButtonEnabled( "PlayButton", !_mediaPlayer.m_AutoStart );
//					//					SetButtonEnabled( "PauseButton", _mediaPlayer.m_AutoStart );
//				}
//				else
//				{
//					//					SetButtonEnabled( "PlayButton", false );
//					//					SetButtonEnabled( "PauseButton", false );
//				}

//				//				SetButtonEnabled( "MuteButton", !_mediaPlayer.m_Muted );
//				//				SetButtonEnabled( "UnmuteButton", _mediaPlayer.m_Muted );

//				OnOpenVideoFile();
//			}
//		}

//		private void OnDestroy()
//		{
//			Debug.Log("VCR: OnDestroy");
//			if (LoadingPlayer)
//			{
//				LoadingPlayer.Events.RemoveListener(OnVideoEvent);
//			}
//			if (PlayingPlayer)
//			{
//				PlayingPlayer.Events.RemoveListener(OnVideoEvent);
//			}
//		}

//		void Update()
//		{
//			//Debug.Log("VCR: Update");
//			if (PlayingPlayer && PlayingPlayer.Info != null && PlayingPlayer.Info.GetDurationMs() > 0f)
//			{
//				float time = PlayingPlayer.Control.GetCurrentTimeMs();
//				float duration = PlayingPlayer.Info.GetDurationMs();
//				float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);

//				if (_maxSec != null)
//                {
//					int maxsecint = (int)(duration / 1000.0f);
//					if (maxsecint < 10)
//						_maxSec.text = "00:0" + maxsecint;
//					else
//						_maxSec.text = "00:" + maxsecint;
//				}
//				if (_currentSec != null)
//                {
//					int secint = (int)(time/1000.0f);
//					if (secint < 10)
//						_currentSec.text = "00:0" + secint;
//					else
//						_currentSec.text = "00:" + secint;
//					//_currentSec.text = "" + time;

//				}

//				// Debug.Log(string.Format("time: {0}, duration: {1}, d: {2}", time, duration, d));

//				_setVideoSeekSliderValue = d;
//				_videoSeekSlider.value = d;

//				if (_bufferedSliderRect != null)
//				{
//					if (PlayingPlayer.Control.IsBuffering())
//					{
//						float t1 = 0f;
//						float t2 = PlayingPlayer.Control.GetBufferingProgress();
//						if (t2 <= 0f)
//						{
//							if (PlayingPlayer.Control.GetBufferedTimeRangeCount() > 0)
//							{
//								PlayingPlayer.Control.GetBufferedTimeRange(0, ref t1, ref t2);
//								t1 /= PlayingPlayer.Info.GetDurationMs();
//								t2 /= PlayingPlayer.Info.GetDurationMs();
//							}
//						}

//						Vector2 anchorMin = Vector2.zero;
//						Vector2 anchorMax = Vector2.one;

//						if (_bufferedSliderImage != null &&
//							_bufferedSliderImage.type == Image.Type.Filled)
//						{
//							_bufferedSliderImage.fillAmount = d;
//						}
//						else
//						{   
//							anchorMin[0] = t1;   
//							anchorMax[0] = t2;
//						}

//						_bufferedSliderRect.anchorMin = anchorMin;
//						_bufferedSliderRect.anchorMax = anchorMax;
//					}
//				}
//			}			
//		}

//		// Callback function to handle events
//		public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
//		{
//			switch (et)
//			{
//				case MediaPlayerEvent.EventType.ReadyToPlay:
//				break;
//				case MediaPlayerEvent.EventType.Started:
//				break;
//				case MediaPlayerEvent.EventType.FirstFrameReady:
//					SwapPlayers();
//				break;
//				case MediaPlayerEvent.EventType.FinishedSeeking:
//                    {
//						if (_playbackPlayBtn != null)
//						{
//							_playbackPlayBtn.gameObject.SetActive(false);
//						}
//						if (_playbackPauseBtn != null)
//						{
//							_playbackPauseBtn.gameObject.SetActive(true);
//						}
//					}
//					break;
//				case MediaPlayerEvent.EventType.FinishedPlaying:
//					{
//						if (_playbackPlayBtn != null)
//						{
//							_playbackPlayBtn.gameObject.SetActive(true);
//						}
//						if (_playbackPauseBtn != null)
//						{
//							_playbackPauseBtn.gameObject.SetActive(false);
//						}
//						//videoPlaybackManager.SetActive(false);//manager
//						//playbackCanvas.gameObject.SetActive(false);//UI object
//						//CameraManager._inst.CloseUICamera();
//						//arsceneUIManager.ShowARGamingPanel();
//					}
//					break;
//			}

//			Debug.Log("MediaPlayerEvent Event: " + et.ToString());
//		}

////		private void SetButtonEnabled( string objectName, bool bEnabled )
////		{
////			Button button = GameObject.Find( objectName ).GetComponent<Button>();
////			if( button )
////			{
////				button.enabled = bEnabled;
////				button.GetComponentInChildren<CanvasRenderer>().SetAlpha( bEnabled ? 1.0f : 0.4f );
////				button.GetComponentInChildren<Text>().color = Color.clear;
////			}
////		}

////		private void RemoveOpenVideoButton()
////		{
////			Button openVideoButton = GameObject.Find( "OpenVideoButton" ).GetComponent<Button>();
////			if( openVideoButton )
////			{
////				openVideoButton.enabled = false;
////				openVideoButton.GetComponentInChildren<CanvasRenderer>().SetAlpha( 0.0f );
////				openVideoButton.GetComponentInChildren<Text>().color = Color.clear;
////			}
////
////			if( _AutoStartToggle )
////			{
////				_AutoStartToggle.enabled = false;
////				_AutoStartToggle.isOn = false;
////				_AutoStartToggle.GetComponentInChildren<CanvasRenderer>().SetAlpha( 0.0f );
////				_AutoStartToggle.GetComponentInChildren<Text>().color = Color.clear;
////				_AutoStartToggle.GetComponentInChildren<Image>().enabled = false;
////				_AutoStartToggle.GetComponentInChildren<Image>().color = Color.clear;
////			}
////		}
//	}
//}
//#endif

#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5 || UNITY_5_4_OR_NEWER
#define UNITY_FEATURE_UGUI
#endif

using UnityEngine;
#if UNITY_FEATURE_UGUI
using UnityEngine.UI;
using System.IO;
using System.Collections;
using RenderHeads.Media.AVProVideo;
using System;
using DataBank;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2015-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Demos
{
	/// <summary>
	/// A demo of a simple video player using uGUI for display
	/// Uses two MediaPlayer components, with one displaying the current video
	/// while the other loads the next video.  MediaPlayers are then swapped
	/// once the video is loaded and has a frame available for display.
	/// This gives a more seamless display than simply using a single MediaPlayer
	/// as its texture will be destroyed when it loads a new video
	/// </summary>
	public class VCR : MonoBehaviour
	{
		public static VCR inst;

		public MediaPlayer _mediaPlayerA;
		public MediaPlayer _mediaPlayerB;
		public DisplayUGUI _mediaDisplay;
		public RectTransform _bufferedSliderRect;

		public Slider _videoSeekSlider;
		private float _setVideoSeekSliderValue;
		private bool _wasPlayingOnScrub;

		public Slider _audioVolumeSlider;
		private float _setAudioVolumeSliderValue;

		public Toggle _AutoStartToggle;
		public Toggle _MuteToggle;

		public Button _playbackPlayBtn = null;
		public Button _playbackPauseBtn = null;
		public Text _currentSec = null;
		public Text _maxSec = null;

		public MediaPlayer.FileLocation _location = MediaPlayer.FileLocation.RelativeToPersistentDataFolder;
		//public string _folder = "AVProVideos/";
		//public string[] _videoFiles = { "MVX.mp4" };
		private string videoFile = "MVX.mp4";

		private int _VideoIndex = 0;
		private Image _bufferedSliderImage;

		private MediaPlayer _loadingPlayer;

		//user manager
		public GameObject videoPlaybackManager;
		public Canvas playbackCanvas;
		//public ARSceneUIManager arsceneUIManager;
		public GameObject uploadPanel;
		private bool isStart = false;

		public MediaPlayer PlayingPlayer
		{
			get
			{
				if (LoadingPlayer == _mediaPlayerA)
				{
					return _mediaPlayerB;
				}
				return _mediaPlayerA;
			}
		}

		public MediaPlayer LoadingPlayer
		{
			get
			{
				return _loadingPlayer;
			}
		}

		private void SwapPlayers()
		{
			Debug.Log("VCR: SwapPlayers");
			// Pause the previously playing video
			PlayingPlayer.Control.Pause();

			// Swap the videos
			if (LoadingPlayer == _mediaPlayerA)
			{
				_loadingPlayer = _mediaPlayerB;
			}
			else
			{
				_loadingPlayer = _mediaPlayerA;
			}

			// Change the displaying video
			_mediaDisplay.CurrentMediaPlayer = PlayingPlayer;
		}

		public void OnEnable()
		{
			Debug.Log("VCR: OnEnable");
			_loadingPlayer = _mediaPlayerB;
			Execute();
			if (_playbackPlayBtn != null)
			{
				_playbackPlayBtn.gameObject.SetActive(false);
			}
			if (_playbackPauseBtn != null)
			{
				_playbackPauseBtn.gameObject.SetActive(true);
			}
			if (uploadPanel != null)
			{
				uploadPanel.SetActive(false);
			}
			if (_currentSec != null)
			{
				_currentSec.text = "00:00";
			}
			if (_maxSec != null)
			{
				_maxSec.text = "00:00";
			}
		}

		public void OnDisable()
		{

		}

		public void ShowUploadDialogPanel()
		{
			if (uploadPanel != null)
			{
				uploadPanel.SetActive(true);
			}
		}

		public void OnBackupFile()
		{
			string userid = PlayerPrefs.GetString("userid");

			string src_path = "" + Path.Combine(Application.persistentDataPath, videoFile);
			string dest_path = "" + Path.Combine(Application.persistentDataPath, "" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + videoFile);
			Debug.Log(src_path);
			Debug.Log(dest_path);

			FileInfo info = new FileInfo(src_path);
			if (info.Exists == true)
			{
				Debug.Log("have source " + src_path + " Length=" + info.Length + "  Userid=" + userid);

				FileStream writeS = File.Create(dest_path);
				if (writeS != null)
				{
					int total = 0;
					using (var fs = File.OpenRead(src_path))
					{
						byte[] buffer = new byte[4096];
						//worker.ReportProgress(0);
						for (; ; )
						{
							int read = fs.Read(buffer, 0, buffer.Length);
							if (read == 0) break;
							writeS.Write(buffer, 0, read);
							total += read;
							//worker.ReportProgress(total * 100 / fs.Length);
						}

						Debug.Log("Finish write " + dest_path + "  total=" + total);
					}

					int isPort = 0;// 横屏：0，竖屏：1
					if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
					{
						isPort = 1;
					}

					//save to db
					UploadVideoDB m_UpVideoDB = new UploadVideoDB();
					m_UpVideoDB.addData(new UploadVideoEntity(dest_path, "" + total, userid, "0", "" + isPort, "" + Screen.width, "" + Screen.height));
					m_UpVideoDB.close();
					Debug.Log("Save to db: " + dest_path + "  total=" + total);
				}
			}
			else
			{
				Debug.Log("No file =" + src_path);
			}

			GameObject gobj = GameObject.Find("UICamera");
			if (gobj != null)
			{
				DemoScript dscript = (DemoScript)gobj.GetComponent(typeof(DemoScript));
				dscript.GoARScene();//GoCoverScene();
			}
		}

		public void OnOpenVideoFile()
		{
			LoadingPlayer.m_VideoPath = System.IO.Path.Combine(Application.persistentDataPath, videoFile);
			//_VideoIndex = (_VideoIndex + 1) % (_videoFiles.Length);
			//if (string.IsNullOrEmpty(LoadingPlayer.m_VideoPath))
			//{
			//	LoadingPlayer.CloseVideo();
			//	_VideoIndex = 0;
			//}
			//else
			//{
			LoadingPlayer.OpenVideoFromFile(_location, LoadingPlayer.m_VideoPath, _AutoStartToggle.isOn);
			_mediaPlayerA.m_AutoStart = true;
			//				SetButtonEnabled( "PlayButton", !_mediaPlayer.m_AutoStart );
			//				SetButtonEnabled( "PauseButton", _mediaPlayer.m_AutoStart );
			//}

			if (_bufferedSliderRect != null)
			{
				_bufferedSliderImage = _bufferedSliderRect.GetComponent<Image>();
			}
		}

		public void OnAutoStartChange()
		{
			if (PlayingPlayer &&
				_AutoStartToggle && _AutoStartToggle.enabled &&
				PlayingPlayer.m_AutoStart != _AutoStartToggle.isOn)
			{
				PlayingPlayer.m_AutoStart = _AutoStartToggle.isOn;
			}
			if (LoadingPlayer &&
				_AutoStartToggle && _AutoStartToggle.enabled &&
				LoadingPlayer.m_AutoStart != _AutoStartToggle.isOn)
			{
				LoadingPlayer.m_AutoStart = _AutoStartToggle.isOn;
			}
		}

		public void OnMuteChange()
		{
			if (PlayingPlayer)
			{
				PlayingPlayer.Control.MuteAudio(_MuteToggle.isOn);
			}
			if (LoadingPlayer)
			{
				LoadingPlayer.Control.MuteAudio(_MuteToggle.isOn);
			}
		}

		public void OnPlayButton()
		{
			if (PlayingPlayer)
			{
				PlayingPlayer.Control.Play();
				_playbackPlayBtn.gameObject.SetActive(false);
				_playbackPauseBtn.gameObject.SetActive(true);
				//				SetButtonEnabled( "PlayButton", false );
				//				SetButtonEnabled( "PauseButton", true );
			}
		}
		public void OnPauseButton()
		{
			if (PlayingPlayer)
			{
				PlayingPlayer.Control.Pause();
				_playbackPlayBtn.gameObject.SetActive(true);
				_playbackPauseBtn.gameObject.SetActive(false);
				//				SetButtonEnabled( "PauseButton", false );
				//				SetButtonEnabled( "PlayButton", true );
			}
		}

		public void OnVideoSeekSlider()
		{
			if (PlayingPlayer && _videoSeekSlider && _videoSeekSlider.value != _setVideoSeekSliderValue)
			{
				PlayingPlayer.Control.Seek(_videoSeekSlider.value * PlayingPlayer.Info.GetDurationMs());
			}
		}

		public void OnVideoSliderDown()
		{
			if (PlayingPlayer)
			{
				_wasPlayingOnScrub = PlayingPlayer.Control.IsPlaying();
				if (_wasPlayingOnScrub)
				{
					PlayingPlayer.Control.Pause();
					//					SetButtonEnabled( "PauseButton", false );
					//					SetButtonEnabled( "PlayButton", true );
				}
				OnVideoSeekSlider();
			}
		}
		public void OnVideoSliderUp()
		{
			if (PlayingPlayer && _wasPlayingOnScrub)
			{
				PlayingPlayer.Control.Play();
				_wasPlayingOnScrub = false;

				//				SetButtonEnabled( "PlayButton", false );
				//				SetButtonEnabled( "PauseButton", true );
			}
		}

		public void OnAudioVolumeSlider()
		{
			if (PlayingPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
			{
				PlayingPlayer.Control.SetVolume(_audioVolumeSlider.value);
			}
			if (LoadingPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
			{
				LoadingPlayer.Control.SetVolume(_audioVolumeSlider.value);
			}
		}
		//		public void OnMuteAudioButton()
		//		{
		//			if( _mediaPlayer )
		//			{
		//				_mediaPlayer.Control.MuteAudio( true );
		//				SetButtonEnabled( "MuteButton", false );
		//				SetButtonEnabled( "UnmuteButton", true );
		//			}
		//		}
		//		public void OnUnmuteAudioButton()
		//		{
		//			if( _mediaPlayer )
		//			{
		//				_mediaPlayer.Control.MuteAudio( false );
		//				SetButtonEnabled( "UnmuteButton", false );
		//				SetButtonEnabled( "MuteButton", true );
		//			}
		//		}

		public void OnRewindButton()
		{
			if (PlayingPlayer)
			{
				PlayingPlayer.Control.Rewind();
			}
		}

		private void Awake()
		{
			inst = this;
			_loadingPlayer = _mediaPlayerB;

			Debug.Log("VCR: Awake");
		}

		void Start()
		{
			Debug.Log("VCR: Start");
			//Execute();
			isStart = true;
		}

		public void Execute()
		{
			Debug.Log("VCR: Execute");
			if (PlayingPlayer)
			{
				PlayingPlayer.Events.AddListener(OnVideoEvent);

				if (LoadingPlayer)
				{
					LoadingPlayer.Events.AddListener(OnVideoEvent);
				}

				if (_audioVolumeSlider)
				{
					// Volume
					if (PlayingPlayer.Control != null)
					{
						float volume = PlayingPlayer.Control.GetVolume();
						_setAudioVolumeSliderValue = volume;
						_audioVolumeSlider.value = volume;
					}
				}

				// Auto start toggle
				_AutoStartToggle.isOn = PlayingPlayer.m_AutoStart;

				if (PlayingPlayer.m_AutoOpen)
				{
					//					RemoveOpenVideoButton();

					//					SetButtonEnabled( "PlayButton", !_mediaPlayer.m_AutoStart );
					//					SetButtonEnabled( "PauseButton", _mediaPlayer.m_AutoStart );
				}
				else
				{
					//					SetButtonEnabled( "PlayButton", false );
					//					SetButtonEnabled( "PauseButton", false );
				}

				//				SetButtonEnabled( "MuteButton", !_mediaPlayer.m_Muted );
				//				SetButtonEnabled( "UnmuteButton", _mediaPlayer.m_Muted );

				OnOpenVideoFile();
			}
		}

		private void OnDestroy()
		{
			Debug.Log("VCR: OnDestroy");
			if (LoadingPlayer)
			{
				LoadingPlayer.Events.RemoveListener(OnVideoEvent);
			}
			if (PlayingPlayer)
			{
				PlayingPlayer.Events.RemoveListener(OnVideoEvent);
			}
		}

		void Update()
		{
			//Debug.Log("VCR: Update");
			if (PlayingPlayer && PlayingPlayer.Info != null && PlayingPlayer.Info.GetDurationMs() > 0f)
			{
				float time = PlayingPlayer.Control.GetCurrentTimeMs();
				float duration = PlayingPlayer.Info.GetDurationMs();
				float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);

				if (_maxSec != null)
				{
					int maxsecint = (int)(duration / 1000.0f);
					if (maxsecint < 10)
						_maxSec.text = "00:0" + maxsecint;
					else
						_maxSec.text = "00:" + maxsecint;
				}
				if (_currentSec != null)
				{
					int secint = (int)(time / 1000.0f);
					if (secint < 10)
						_currentSec.text = "00:0" + secint;
					else
						_currentSec.text = "00:" + secint;
					//_currentSec.text = "" + time;

				}

				// Debug.Log(string.Format("time: {0}, duration: {1}, d: {2}", time, duration, d));

				_setVideoSeekSliderValue = d;
				_videoSeekSlider.value = d;

				if (_bufferedSliderRect != null)
				{
					if (PlayingPlayer.Control.IsBuffering())
					{
						float t1 = 0f;
						float t2 = PlayingPlayer.Control.GetBufferingProgress();
						if (t2 <= 0f)
						{
							if (PlayingPlayer.Control.GetBufferedTimeRangeCount() > 0)
							{
								PlayingPlayer.Control.GetBufferedTimeRange(0, ref t1, ref t2);
								t1 /= PlayingPlayer.Info.GetDurationMs();
								t2 /= PlayingPlayer.Info.GetDurationMs();
							}
						}

						Vector2 anchorMin = Vector2.zero;
						Vector2 anchorMax = Vector2.one;

						if (_bufferedSliderImage != null &&
							_bufferedSliderImage.type == Image.Type.Filled)
						{
							_bufferedSliderImage.fillAmount = d;
						}
						else
						{
							anchorMin[0] = t1;
							anchorMax[0] = t2;
						}

						_bufferedSliderRect.anchorMin = anchorMin;
						_bufferedSliderRect.anchorMax = anchorMax;
					}
				}
			}
		}

		// Callback function to handle events
		public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			switch (et)
			{
				case MediaPlayerEvent.EventType.ReadyToPlay:
					break;
				case MediaPlayerEvent.EventType.Started:
					break;
				case MediaPlayerEvent.EventType.FirstFrameReady:
					SwapPlayers();
					break;
				case MediaPlayerEvent.EventType.FinishedSeeking:
					{
						if (_playbackPlayBtn != null)
						{
							_playbackPlayBtn.gameObject.SetActive(false);
						}
						if (_playbackPauseBtn != null)
						{
							_playbackPauseBtn.gameObject.SetActive(true);
						}
					}
					break;
				case MediaPlayerEvent.EventType.FinishedPlaying:
					{
						if (_playbackPlayBtn != null)
						{
							_playbackPlayBtn.gameObject.SetActive(true);
						}
						if (_playbackPauseBtn != null)
						{
							_playbackPauseBtn.gameObject.SetActive(false);
						}
						//videoPlaybackManager.SetActive(false);//manager
						//playbackCanvas.gameObject.SetActive(false);//UI object
						//CameraManager._inst.CloseUICamera();
						//arsceneUIManager.ShowARGamingPanel();
					}
					break;
			}

			Debug.Log("MediaPlayerEvent Event: " + et.ToString());
		}

		//		private void SetButtonEnabled( string objectName, bool bEnabled )
		//		{
		//			Button button = GameObject.Find( objectName ).GetComponent<Button>();
		//			if( button )
		//			{
		//				button.enabled = bEnabled;
		//				button.GetComponentInChildren<CanvasRenderer>().SetAlpha( bEnabled ? 1.0f : 0.4f );
		//				button.GetComponentInChildren<Text>().color = Color.clear;
		//			}
		//		}

		//		private void RemoveOpenVideoButton()
		//		{
		//			Button openVideoButton = GameObject.Find( "OpenVideoButton" ).GetComponent<Button>();
		//			if( openVideoButton )
		//			{
		//				openVideoButton.enabled = false;
		//				openVideoButton.GetComponentInChildren<CanvasRenderer>().SetAlpha( 0.0f );
		//				openVideoButton.GetComponentInChildren<Text>().color = Color.clear;
		//			}
		//
		//			if( _AutoStartToggle )
		//			{
		//				_AutoStartToggle.enabled = false;
		//				_AutoStartToggle.isOn = false;
		//				_AutoStartToggle.GetComponentInChildren<CanvasRenderer>().SetAlpha( 0.0f );
		//				_AutoStartToggle.GetComponentInChildren<Text>().color = Color.clear;
		//				_AutoStartToggle.GetComponentInChildren<Image>().enabled = false;
		//				_AutoStartToggle.GetComponentInChildren<Image>().color = Color.clear;
		//			}
		//		}
	}
}
#endif