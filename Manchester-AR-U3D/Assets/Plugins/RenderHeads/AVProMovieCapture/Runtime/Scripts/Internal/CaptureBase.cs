#if UNITY_2017_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
#endif
#if UNITY_5_6_OR_NEWER && UNITY_2018_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
#endif
#if UNITY_2017_1_OR_NEWER
	#define AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
#endif
#if UNITY_2019_2_OR_NEWER
	#define AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
#endif
#if ENABLE_IL2CPP
using AOT;
#endif
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{
	/// <summary>
	/// Live stats about an active capture session
	/// </summary>
	public class CaptureStats
	{
		public float FPS { get { return _fps; } }
		public float FramesTotal { get { return _frameTotal; } }

		public uint NumDroppedFrames { get { return _numDroppedFrames; } internal set { _numDroppedFrames = value; } }
		public uint NumDroppedEncoderFrames { get { return _numDroppedEncoderFrames; } internal set { _numDroppedEncoderFrames = value; } }
		public uint NumEncodedFrames { get { return _numEncodedFrames; } internal set { _numEncodedFrames = value; } }
		public uint TotalEncodedSeconds { get { return _totalEncodedSeconds; } internal set { _totalEncodedSeconds = value; } }

		public AudioCaptureSource AudioCaptureSource { get { return _audioCaptureSource; } internal set { _audioCaptureSource = value; } }
		public int UnityAudioSampleRate { get { return _unityAudioSampleRate; } internal set { _unityAudioSampleRate = value; } }
		public int UnityAudioChannelCount { get { return _unityAudioChannelCount; } internal set { _unityAudioChannelCount = value; } }

		// Frame stats
		private uint _numDroppedFrames = 0;
		private uint _numDroppedEncoderFrames = 0;
		private uint _numEncodedFrames = 0;
		private uint _totalEncodedSeconds = 0;

		// Audio
		private AudioCaptureSource _audioCaptureSource = AudioCaptureSource.None;
		private int _unityAudioSampleRate = -1;
		private int _unityAudioChannelCount = -1;

		// Capture rate
		private float _fps = 0f;
		private int _frameTotal = 0;
		private int _frameCount = 0;
		private float _startFrameTime = 0f;

		internal void ResetFPS()
		{
			_frameCount = 0;
			_frameTotal = 0;
			_fps = 0.0f;
			_startFrameTime = 0.0f;
		}

		internal void UpdateFPS()
		{
			_frameCount++;
			_frameTotal++;

			float timeNow = Time.realtimeSinceStartup;
			float timeDelta = timeNow - _startFrameTime;
			if (timeDelta >= 1.0f)
			{
				_fps = (float)_frameCount / timeDelta;
				_frameCount = 0;
				_startFrameTime = timeNow;
			}
		}
	}

	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class VideoEncoderHints
	{
		public VideoEncoderHints()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			averageBitrate = 0;
			maximumBitrate = 0;
			quality = 1.0f;
			keyframeInterval = 0;
			allowFastStartStreamingPostProcess = true;
			supportTransparency = false;
			useHardwareEncoding = true;
		}

		internal void Validate()
		{
			quality = Mathf.Clamp(quality, 0f, 1f);
		}

		[Tooltip("Average number of bits per second for the resulting video. Zero uses the codec defaults.")]
		public uint averageBitrate;
		[Tooltip("Maximum number of bits per second for the resulting video. Zero uses the codec defaults.")]
		public uint maximumBitrate;
		[Range(0f, 1f)] public float quality;
		[Tooltip("How often a keyframe is inserted.  Zero uses the codec defaults.")]
		public uint keyframeInterval;

		// Only for MP4 files on Windows
		[Tooltip("Move the 'moov' atom in the video file from the end to the start of the file to make streaming start fast.  Also known as 'Fast Start' in some encoders")]
		[MarshalAs(UnmanagedType.U1)]
		public bool allowFastStartStreamingPostProcess;

		// Currently only for HEVC and ProRes 4444 on macOS/iOS, and supported DirectShow codecs (eg Lagarith/Uncompressed) on Windows
		[Tooltip("Hints to the encoder to use the alpha channel for transparency if possible")]
		[MarshalAs(UnmanagedType.U1)]
		public bool supportTransparency;

		// Windows only
		[MarshalAs(UnmanagedType.U1)]
		public bool useHardwareEncoding;
	}

	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class ImageEncoderHints
	{
		public ImageEncoderHints()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			quality = 0.85f;
			supportTransparency = false;
		}

		internal void Validate()
		{
			quality = Mathf.Clamp(quality, 0f, 1f);
		}

		// Currently only affects JPG and HEIF formats (macOS only)
		[Range(0f, 1f)] public float quality;

		// Currently only for PNG
		[Tooltip("Hints to the encoder to use the alpha channel for transparency if possible")]
		[MarshalAs(UnmanagedType.U1)]
		public bool supportTransparency;
	}

	[System.Serializable]
	public class EncoderHints
	{
		public EncoderHints()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			videoHints = new VideoEncoderHints();
			imageHints = new ImageEncoderHints();
		}

		public VideoEncoderHints videoHints;
		public ImageEncoderHints imageHints;
	}

	/// <summary>
	/// Base class wrapping common capture functionality
	/// </summary>
	public partial class CaptureBase : MonoBehaviour
	{
		public enum Resolution
		{
			POW2_8192x8192,
			POW2_8192x4096,
			POW2_4096x4096,
			POW2_4096x2048,
			POW2_2048x4096,
			UHD_3840x2160,
			UHD_3840x2048,
			UHD_3840x1920,
			POW2_2048x2048,
			POW2_2048x1024,
			HD_1920x1080,
			HD_1280x720,
			SD_1024x768,
			SD_800x600,
			SD_800x450,
			SD_640x480,
			SD_640x360,
			SD_320x240,
			Original,
			Custom,
		}

		public enum CubemapDepth
		{
			Depth_24 = 24,
			Depth_16 = 16,
			Depth_Zero = 0,
		}

		public enum CubemapResolution
		{
			POW2_8192 = 8192,
			POW2_4096 = 4096,
			POW2_2048 = 2048,
			POW2_1024 = 1024,
			POW2_512 = 512,
			POW2_256 = 256,
		}

		public enum AntiAliasingLevel
		{
			UseCurrent,
			ForceNone,
			ForceSample2,
			ForceSample4,
			ForceSample8,
		}

		public enum DownScale
		{
			Original = 1,
			Half = 2,
			Quarter = 4,
			Eighth = 8,
			Sixteenth = 16,
			Custom = 100,
		}

		public enum OutputPath
		{
			RelativeToProject,
			RelativeToPeristentData,
			Absolute,
			RelativeToDesktop,
		}

		public enum FrameUpdateMode
		{
			Automatic,
			Manual,
		}

		/*public enum OutputExtension
		{
			AVI,
			MP4,
			PNG,
			Custom = 100,
		}*/

#if false
		[System.Serializable]
		public class WindowsPostCaptureSettings
		{
			[SerializeField]
			[Tooltip("Move the 'moov' atom in the MP4 file from the end to the start of the file to make streaming start fast.  Also called 'Fast Start' in some encoders")]
			public bool writeFastStartStreamingForMp4 = true;
		}

		[System.Serializable]
		public class PostCaptureSettings
		{
			[SerializeField]
			[Tooltip("Move the 'moov' atom in the MP4 file from the end to the start of the file to make streaming start fast.  Also called 'Fast Start' in some encoders")]
			public WindowsPostCaptureSettings windows = new WindowsPostCaptureSettings();
		}

		[SerializeField] PostCaptureSettings _postCaptureSettings = new PostCaptureSettings();
#endif

		[SerializeField] EncoderHints _encoderHintsWindows = new EncoderHints();
		[SerializeField] EncoderHints _encoderHintsMacOS = new EncoderHints();
		[SerializeField] EncoderHints _encoderHintsIOS = new EncoderHints();

		// General options

		[SerializeField] KeyCode _captureKey = KeyCode.None;
		[SerializeField] bool _isRealTime = true;
		[SerializeField] bool _persistAcrossSceneLoads = false;

		// Start options

		[SerializeField] StartTriggerMode _startTrigger = StartTriggerMode.Manual;
		[SerializeField] StartDelayMode _startDelay = StartDelayMode.None;
		[SerializeField] float _startDelaySeconds = 0f;

		// Stop options

		[SerializeField] StopMode _stopMode = StopMode.None;
		// TODO: add option to pause instead of stop?
		[SerializeField] int _stopFrames = 0;
		[SerializeField] float _stopSeconds = 0f;

		// Video options

		public static readonly string[] DefaultVideoCodecPriorityWindows = { 	"H264",
																				"HEVC",
																				"Lagarith Lossless Codec",
																				"Uncompressed",
																				"x264vfw - H.264/MPEG-4 AVC codec",
																				"Xvid MPEG-4 Codec" };

		public static readonly string[] DefaultVideoCodecPriorityMacOS =  {		"H264",
																				"HEVC",
																				"Apple ProRes 422",
																				"Apple ProRes 4444" };


		public static readonly string[] DefaultAudioCodecPriorityWindows = {	"AAC",
																				"Uncompressed" };

		public static readonly string[] DefaultAudioCodecPriorityMacOS =  {	"AAC",
																			"FLAC",
																			"Apple Lossless",
																			"Linear PCM",
																			"Uncompresssed" };

		public static readonly string[] DefaultAudioCodecPriorityIOS =  {	"AAC",
																			"FLAC",
																			"Apple Lossless",
																			"Linear PCM",
																			"Uncompresssed" };

		public static readonly string[] DefaultAudioCaptureDevicePriorityWindow = { "Microphone (Realtek Audio)", "Stereo Mix", "What U Hear", "What You Hear", "Waveout Mix", "Mixed Output" };
		public static readonly string[] DefaultAudioCaptureDevicePriorityMacOS = { };
		public static readonly string[] DefaultAudioCaptureDevicePriorityIOS = { };

		[SerializeField] string[] _videoCodecPriorityWindows = DefaultVideoCodecPriorityWindows;
		[SerializeField] string[] _videoCodecPriorityMacOS = DefaultVideoCodecPriorityMacOS;

		[SerializeField] string[] _audioCodecPriorityWindows = DefaultAudioCodecPriorityWindows;
		[SerializeField] string[] _audioCodecPriorityMacOS = DefaultAudioCodecPriorityMacOS;

		[SerializeField] float _frameRate = 30f;

		[Tooltip("Timelapse scale makes the frame capture run at a fraction of the target frame rate.  Default value is 1")]
		[SerializeField] int _timelapseScale = 1;
		[Tooltip("Manual update mode requires user to call FrameUpdate() each time a frame is ready")]
		[SerializeField] FrameUpdateMode _frameUpdateMode = FrameUpdateMode.Automatic;

		[SerializeField] DownScale _downScale = DownScale.Original;
		[SerializeField] Vector2 _maxVideoSize = Vector2.zero;

		#pragma warning disable 414
		[SerializeField, Range(-1, 128)] int _forceVideoCodecIndexWindows = -1;
		[SerializeField, Range(-1, 128)] int _forceVideoCodecIndexMacOS = 0;
		[SerializeField, Range(0, 128)] int _forceVideoCodecIndexIOS = 0;
		[SerializeField, Range(-1, 128)] int _forceAudioCodecIndexWindows = -1;
		[SerializeField, Range(-1, 128)] int _forceAudioCodecIndexMacOS = 0;
		[SerializeField, Range(0, 128)] int _forceAudioCodecIndexIOS = 0;
		#pragma warning restore 414
		[SerializeField] bool _flipVertically = false;

		[Tooltip("Flushing the GPU during each capture results in less latency, but can slow down rendering performance for complex scenes.")]
		[SerializeField] bool _forceGpuFlush = false;

		[Tooltip("This option can help issues where skinning is used, or other animation/rendering effects that only complete later in the frame.")]
		[SerializeField] protected bool _useWaitForEndOfFrame = true;

		[Tooltip("Log the start and stop of the capture.  Disable this for less garbage generation.")]
		[SerializeField] bool _logCaptureStartStop = true;

		// Audio options

		[SerializeField] AudioCaptureSource _audioCaptureSource = AudioCaptureSource.None;
		[SerializeField] UnityAudioCapture _unityAudioCapture = null;
		[SerializeField, Range(0, 32)] int _forceAudioInputDeviceIndex = 0;
		[SerializeField, Range(8000, 96000)] int _manualAudioSampleRate = 48000;
		[SerializeField, Range(1, 8)] int _manualAudioChannelCount = 2;

		// Output options

		[SerializeField] protected OutputTarget _outputTarget = OutputTarget.VideoFile;

		public OutputTarget OutputTarget
		{
			get { return _outputTarget; }
			set { _outputTarget = value; }
		}

#if (!UNITY_EDITOR && UNITY_IOS)
		// Can only write to persistent data path (Documents) on iOS
		private const OutputPath DefaultOutputFolderType = OutputPath.RelativeToPeristentData;
		// Subfolders prevent direct access via iTunes so avoid
		private const string DefaultOutputFolderPath = "";
#else
		private const OutputPath DefaultOutputFolderType = OutputPath.RelativeToProject;
		private const string DefaultOutputFolderPath = "Captures";
#endif

		[SerializeField] OutputPath _outputFolderType = DefaultOutputFolderType;
		[SerializeField] string _outputFolderPath = DefaultOutputFolderPath;
		[SerializeField] string _filenamePrefix = "MovieCapture";
		[SerializeField] bool _appendFilenameTimestamp = true;
		[SerializeField] bool _allowManualFileExtension = false;
		[SerializeField] string _filenameExtension = "mp4";
		[SerializeField] string _namedPipePath = @"\\.\pipe\test_pipe";

		public OutputPath OutputFolder
		{
			get { return _outputFolderType; }
			set { _outputFolderType = value; }
		}
		public string OutputFolderPath
		{
			get { return _outputFolderPath; }
			set { _outputFolderPath = value; }
		}
		public string FilenamePrefix
		{
			get { return _filenamePrefix; }
			set { _filenamePrefix = value; }
		}
		public bool AppendFilenameTimestamp
		{
			get { return _appendFilenameTimestamp; }
			set { _appendFilenameTimestamp = value; }
		}
		public bool AllowManualFileExtension
		{
			get { return _allowManualFileExtension; }
			set { _allowManualFileExtension = value; }
		}
		public string FilenameExtension
		{
			get { return _filenameExtension; }
			set { _filenameExtension = value; }
		}
		public string NamedPipePath
		{
			get { return _namedPipePath; }
			set { _namedPipePath = value; }
		}

		[SerializeField] int _imageSequenceStartFrame = 0;
		[SerializeField, Range(2, 12)] int _imageSequenceZeroDigits = 6;
		#pragma warning disable 414
		[SerializeField] ImageSequenceFormat _imageSequenceFormatWindows = ImageSequenceFormat.PNG;
		[SerializeField] ImageSequenceFormat _imageSequenceFormatMacOS = ImageSequenceFormat.PNG;
		[SerializeField] ImageSequenceFormat _imageSequenceFormatIOS = ImageSequenceFormat.PNG;
		#pragma warning restore 414

		public int ImageSequenceStartFrame
		{
			get { return _imageSequenceStartFrame; }
			set { _imageSequenceStartFrame = value; }
		}
		public int ImageSequenceZeroDigits
		{
			get { return _imageSequenceZeroDigits; }
			set { _imageSequenceZeroDigits = Mathf.Clamp(_imageSequenceZeroDigits, 2, 12); }
		}

		// Camera specific options

		[SerializeField] protected Resolution _renderResolution = Resolution.Original;
		[SerializeField] protected Vector2 _renderSize = Vector2.one;
		[SerializeField] protected int _renderAntiAliasing = -1;

		// Motion blur options

		[SerializeField] protected bool _useMotionBlur = false;
		[SerializeField, Range(0, 64)] protected int _motionBlurSamples = 16;
		[SerializeField] protected Camera[] _motionBlurCameras = null;
		[SerializeField] protected MotionBlur _motionBlur;

		public bool UseMotionBlur
		{
			get { return _useMotionBlur; }
			set { _useMotionBlur = value; }
		}
		public int MotionBlurSamples
		{
			get { return _motionBlurSamples; }
			set { _motionBlurSamples = (int)Mathf.Clamp((float)value, 0f, 64f); }
		}
		public Camera[] MotionBlurCameras
		{
			get { return _motionBlurCameras; }
			set { _motionBlurCameras = value; }
		}
		public MotionBlur MotionBlur
		{
			get { return _motionBlur; }
			set { _motionBlur = value; }
		}

		// Performance options

		[SerializeField] bool _allowVSyncDisable = true;
		[SerializeField] protected bool _supportTextureRecreate = false;

		// Other options

		[SerializeField] int _minimumDiskSpaceMB = -1;

#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
		[SerializeField] TimelineController _timelineController = null;
#endif
#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
		[SerializeField] VideoPlayerController _videoPlayerController = null;
#endif

		//public bool _allowFrameRateChange = true;

		protected Texture2D _texture;
		protected int _handle = -1;
		protected int _targetWidth, _targetHeight;
		protected bool _capturing = false;
		protected bool _paused = false;
		protected string _filePath;
		protected FileInfo _fileInfo;
		protected NativePlugin.PixelFormat _pixelFormat = NativePlugin.PixelFormat.YCbCr422_YUY2;
		private Codec _selectedVideoCodec = null;
		private Codec _selectedAudioCodec = null;
		private Device _selectedAudioInputDevice = null;
		private int _oldVSyncCount = 0;
		//private int _oldTargetFrameRate = -1;
		private float _oldFixedDeltaTime = 0f;
		protected bool _isTopDown = true;
		protected bool _isDirectX11 = false;
		private bool _queuedStartCapture = false;
		private bool _queuedStopCapture = false;
		private float _captureStartTime = 0f;
		private float _capturePrePauseTotalTime = 0f;
		private float _timeSinceLastFrame = 0f;
		protected YieldInstruction _waitForEndOfFrame;
		private long _freeDiskSpaceMB;

		private float _startDelayTimer;
		private bool _startPaused;
		private System.Action<FileWritingHandler> _beginFinalFileWritingAction;
		private List<FileWritingHandler> _pendingFileWrites = new List<FileWritingHandler>(4);

		public string LastFilePath
		{
			get { return _filePath; }
		}

		// Register for notification of when the final file writing begins
		public System.Action<FileWritingHandler> BeginFinalFileWritingAction
		{
			get { return _beginFinalFileWritingAction; }
			set { _beginFinalFileWritingAction = value; }
		}

		// Stats
		private CaptureStats _stats = new CaptureStats();

		private static bool _isInitialised = false;
		private static bool _isApplicationQuiting = false;

		public Resolution CameraRenderResolution
		{
			get { return _renderResolution; }
			set { _renderResolution = value; }
		}
		public Vector2 CameraRenderCustomResolution
		{
			get { return _renderSize; }
			set { _renderSize = value; }
		}

		public int CameraRenderAntiAliasing
		{
			get { return _renderAntiAliasing; }
			set { _renderAntiAliasing = value; }
		}

		public bool IsRealTime
		{
			get { return _isRealTime; }
			set { _isRealTime = value; }
		}

		public bool PersistAcrossSceneLoads
		{
			get { return _persistAcrossSceneLoads; }
			set { _persistAcrossSceneLoads = value; }
		}

		public AudioCaptureSource AudioCaptureSource
		{
			get { return _audioCaptureSource; }
			set { _audioCaptureSource = value; }
		}

		public int ManualAudioSampleRate
		{
			get { return _manualAudioSampleRate; }
			set { _manualAudioSampleRate = value; }
		}

		public int ManualAudioChannelCount
		{
			get { return _manualAudioChannelCount; }
			set { _manualAudioChannelCount = value; }
		}

		public UnityAudioCapture UnityAudioCapture
		{
			get { return _unityAudioCapture; }
			set { _unityAudioCapture = value; }
		}

		public int ForceAudioInputDeviceIndex
		{
			get { return _forceAudioInputDeviceIndex; }
			set { _forceAudioInputDeviceIndex = value; SelectAudioInputDevice(); }
		}

		public float FrameRate
		{
			get { return _frameRate; }
			set { _frameRate = Mathf.Clamp(value, 0.01f, 240f); }
		}

		public StartTriggerMode StartTrigger
		{
			get { return _startTrigger; }
			set { _startTrigger = value; }
		}

		public StartDelayMode StartDelay
		{
			get { return _startDelay; }
			set { _startDelay = value; }
		}

		public float StartDelaySeconds
		{
			get { return _startDelaySeconds; }
			set { _startDelaySeconds = value; }
		}

		public StopMode StopMode
		{
			get { return _stopMode; }
			set { _stopMode = value; }
		}

		public int StopAfterFramesElapsed
		{
			get { return _stopFrames; }
			set { _stopFrames = value; }
		}

		public float StopAfterSecondsElapsed
		{
			get { return _stopSeconds; }
			set { _stopSeconds = value; }
		}

		public CaptureStats CaptureStats
		{
			get { return _stats; }
		}

		public string[] VideoCodecPriorityWindows
		{
			get { return _videoCodecPriorityWindows; }
			set { _videoCodecPriorityWindows = value; SelectVideoCodec(false); }
		}

		public string[] VideoCodecPriorityMacOS
		{
			get { return _videoCodecPriorityMacOS; }
			set { _videoCodecPriorityMacOS = value; SelectVideoCodec(false); }
		}

		public string[] AudioCodecPriorityWindows
		{
			get { return _audioCodecPriorityWindows; }
			set { _audioCodecPriorityWindows = value; SelectAudioCodec(); }
		}

		public string[] AudioCodecPriorityMacOS
		{
			get { return _audioCodecPriorityMacOS; }
			set { _audioCodecPriorityMacOS = value; SelectAudioCodec(); }
		}

		public int TimelapseScale
		{
			get { return _timelapseScale; }
			set { _timelapseScale = value; }
		}

		public FrameUpdateMode FrameUpdate
		{
			get { return _frameUpdateMode; }
			set { _frameUpdateMode = value; }
		}

		public DownScale ResolutionDownScale
		{
			get { return _downScale; }
			set { _downScale = value; }
		}

		public Vector2 ResolutionDownscaleCustom
		{
			get { return _maxVideoSize; }
			set { _maxVideoSize = value; }
		}

		public bool FlipVertically
		{
			get { return _flipVertically; }
			set { _flipVertically = value; }
		}

		public bool UseWaitForEndOfFrame
		{
			get { return _useWaitForEndOfFrame; }
			set { _useWaitForEndOfFrame = value; }
		}

		public bool LogCaptureStartStop
		{
			get { return _logCaptureStartStop; }
			set { _logCaptureStartStop = value; }
		}

#if false
		public PostCaptureSettings PostCapture
		{
			get { return _postCaptureSettings; }
		}
#endif

		public bool AllowOfflineVSyncDisable
		{
			get { return _allowVSyncDisable; }
			set { _allowVSyncDisable = value; }
		}

		public bool SupportTextureRecreate
		{
			get { return _supportTextureRecreate; }
			set { _supportTextureRecreate = value; }
		}

		#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
		public TimelineController TimelineController
		{
			get { return _timelineController; }
			set { _timelineController = value; }
		}
		#endif

		#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
		public VideoPlayerController VideoPlayerController
		{
			get { return _videoPlayerController; }
			set { _videoPlayerController = value; }
		}
		#endif

		public Codec SelectedVideoCodec
		{
			get { return _selectedVideoCodec; }
		}

		public Codec SelectedAudioCodec
		{
			get { return _selectedAudioCodec; }
		}

		public Device SelectedAudioInputDevice
		{
			get { return _selectedAudioInputDevice; }
		}

		public int NativeForceVideoCodecIndex
		{
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					get { return _forceVideoCodecIndexWindows; }
					set { _forceVideoCodecIndexWindows = value; }
				#elif UNITY_EDITOR_OSX
					get { return _forceVideoCodecIndexMacOS; }
					set { _forceVideoCodecIndexMacOS = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					get { return _forceVideoCodecIndexWindows; }
					set { _forceVideoCodecIndexWindows = value; }
				#elif UNITY_STANDALONE_OSX
					get { return _forceVideoCodecIndexMacOS; }
					set { _forceVideoCodecIndexMacOS = value; }
				#elif UNITY_IOS
					get { return _forceVideoCodecIndexIOS; }
					set { _forceVideoCodecIndexIOS = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#endif
		}

		public int NativeForceAudioCodecIndex
		{
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					get { return _forceAudioCodecIndexWindows; }
					set { _forceAudioCodecIndexWindows = value; }
				#elif UNITY_EDITOR_OSX
					get { return _forceAudioCodecIndexMacOS; }
					set { _forceAudioCodecIndexMacOS = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					get { return _forceAudioCodecIndexWindows; }
					set { _forceAudioCodecIndexWindows = value; }
				#elif UNITY_STANDALONE_OSX
					get { return _forceAudioCodecIndexMacOS; }
					set { _forceAudioCodecIndexMacOS = value; }
				#elif UNITY_IOS
					get { return _forceAudioCodecIndexIOS; }
					set { _forceAudioCodecIndexIOS = value; }
				#else
					get { return -1; }
					set { }
				#endif
			#endif
		}

		public ImageSequenceFormat NativeImageSequenceFormat
		{
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					get { return _imageSequenceFormatWindows; }
					set { _imageSequenceFormatWindows = value; }
				#elif UNITY_EDITOR_OSX
					get { return _imageSequenceFormatMacOS; }
					set { _imageSequenceFormatMacOS = value; }
				#else
					get { return ImageSequenceFormat.PNG; }
					set { }
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					get { return _imageSequenceFormatWindows; }
					set { _imageSequenceFormatWindows = value; }
				#elif UNITY_STANDALONE_OSX
					get { return _imageSequenceFormatMacOS; }
					set { _imageSequenceFormatMacOS = value; }
				#elif UNITY_IOS
					get { return _imageSequenceFormatIOS; }
					set { _imageSequenceFormatIOS = value; }
				#else
					get { return ImageSequenceFormat.PNG; }
					set { }
				#endif
			#endif
		}

		protected static NativePlugin.Platform GetCurrentPlatform()
		{
			NativePlugin.Platform result = NativePlugin.Platform.Unknown;
			#if UNITY_EDITOR
				#if UNITY_EDITOR_WIN
					result = NativePlugin.Platform.Windows;
				#elif UNITY_EDITOR_OSX
					result = NativePlugin.Platform.macOS;
				#endif
			#else
				#if UNITY_STANDALONE_WIN
					result = NativePlugin.Platform.Windows;
				#elif UNITY_STANDALONE_OSX
					result = NativePlugin.Platform.macOS;
				#elif UNITY_IOS
					result = NativePlugin.Platform.iOS;
				#endif
			#endif
			return result;
		}

		public EncoderHints GetEncoderHints(NativePlugin.Platform platform = NativePlugin.Platform.Current)
		{
			EncoderHints result = null;

			if (platform == NativePlugin.Platform.Current)
			{
				platform = GetCurrentPlatform();
			}
			switch (platform)
			{
				case NativePlugin.Platform.Windows:
					result = _encoderHintsWindows;
					break;
				case NativePlugin.Platform.macOS:
					result = _encoderHintsMacOS;
					break;
				case NativePlugin.Platform.iOS:
					result = _encoderHintsIOS;
					break;
			}
			return result;
		}

		public void SetEncoderHints(EncoderHints hints, NativePlugin.Platform platform = NativePlugin.Platform.Current)
		{
			if (platform == NativePlugin.Platform.Current)
			{
				platform = GetCurrentPlatform();
			}
			switch (platform)
			{
				case NativePlugin.Platform.Windows:
					_encoderHintsWindows = hints;
					break;
				case NativePlugin.Platform.macOS:
					_encoderHintsMacOS = hints;
					break;
				case NativePlugin.Platform.iOS:
					_encoderHintsIOS = hints;
					break;
			}
		}

		protected virtual void Awake()
		{
			if (!_isInitialised)
			{
				try
				{
					string pluginVersionString = NativePlugin.GetPluginVersionString();

					// Check that the plugin version number is not too old
					if (!pluginVersionString.StartsWith(NativePlugin.ExpectedPluginVersion))
					{
						Debug.LogWarning("[AVProMovieCapture] Plugin version number " + pluginVersionString + " doesn't match the expected version number " + NativePlugin.ExpectedPluginVersion + ".  It looks like the plugin didn't upgrade correctly.  To resolve this please restart Unity and try to upgrade the package again.");
					}

		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			#if !UNITY_2017_1_OR_NEWER
					if (SystemInfo.graphicsDeviceVersion.StartsWith("Metal"))
					{
						Debug.LogError("[AVProMovieCapture] Metal is not supported below Unity 2017, please switch to OpenGLCore in Player Settings.");
						return;
					}
			#endif
		#elif !UNITY_EDITOR && UNITY_IOS && !UNITY_2017_1_OR_NEWER
					if (Application.isPlaying)
					{
						Debug.LogError("[AVProMovieCapture] iOS is not supported below Unity 2017.");
						return;
					}
		#endif
					if (NativePlugin.Init())
					{
						Debug.Log("[AVProMovieCapture] Init plugin version: " + pluginVersionString + " (script v" + NativePlugin.ScriptVersion +") with GPU " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceVersion + " OS: " + SystemInfo.operatingSystem);
						_isInitialised = true;
					}
					else
					{
						Debug.LogError("[AVProMovieCapture] Failed to initialise plugin version: " + pluginVersionString + " (script v" + NativePlugin.ScriptVersion + ") with GPU " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceVersion + " OS: " + SystemInfo.operatingSystem);
					}
				}
				catch (DllNotFoundException e)
				{
					string missingDllMessage = string.Empty;
					missingDllMessage = "Unity couldn't find the plugin DLL. Please select the native plugin files in 'Plugins/RenderHeads/AVProMovieCapture/Plugins' folder and select the correct platform in the Inspector.";
					Debug.LogError("[AVProMovieCapture] " + missingDllMessage);
	#if UNITY_EDITOR
					UnityEditor.EditorUtility.DisplayDialog("Plugin files not found", missingDllMessage, "Ok");
	#endif
					throw e;
				}
			}

			_isDirectX11 = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11");

			SelectVideoCodec();
			SelectAudioCodec();
			SelectAudioInputDevice();

			if (_persistAcrossSceneLoads)
			{
				GameObject.DontDestroyOnLoad(this.gameObject);
			}
		}

		static CaptureBase()
		{
			#if UNITY_EDITOR
			SetupEditorPlayPauseSupport();
			#endif
		}

		public virtual void Start()
		{
			Application.runInBackground = true;
			_waitForEndOfFrame = new WaitForEndOfFrame();

			if (_startTrigger == StartTriggerMode.OnStart)
			{
				StartCapture();
			}
		}

		// Select the best codec based on criteria
		private static bool SelectCodec(ref Codec codec, CodecList codecList, int forceCodecIndex, string[] codecPriorityList, MediaApi matchMediaApi, bool allowFallbackToFirstCodec, bool logFallbackWarning)
		{
			codec = null;

			// The user has specified their own codec index
			if (forceCodecIndex >= 0)
			{
				if (forceCodecIndex < codecList.Count)
				{
					codec = codecList.Codecs[forceCodecIndex];
				}
			}
			else
			{
				// The user has specified an ordered list of codec name to search for
				if (codecPriorityList != null && codecPriorityList.Length > 0)
				{
					foreach (string codecName in codecPriorityList)
					{
						codec = codecList.FindCodec(codecName.Trim(), matchMediaApi);
						if (codec != null)
						{
							break;
						}
					}
				}
			}

			// If the found codec doesn't match the required MediaApi, set it to null
			if (codec != null && matchMediaApi != MediaApi.Unknown)
			{
				if (codec.MediaApi != matchMediaApi)
				{
					codec = null;
				}
			}

			// Fallback to the first codec
			if (codec == null && allowFallbackToFirstCodec)
			{
				if (codecList.Count > 0)
				{
					if (matchMediaApi != MediaApi.Unknown)
					{
						codec = codecList.GetFirstWithMediaApi(matchMediaApi);
					}
					else
					{
						codec = codecList.Codecs[0];
					}
					if (logFallbackWarning)
					{
						Debug.LogWarning("[AVProMovieCapture] Codec not found. Using the first codec available.");
					}
				}
			}

			return (codec != null);
		}

		public Codec SelectVideoCodec(bool isStartingCapture = false)
		{
			_selectedVideoCodec = null;
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, _videoCodecPriorityWindows, MediaApi.Unknown, true, isStartingCapture);
#elif UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, _videoCodecPriorityMacOS, MediaApi.Unknown, true, isStartingCapture);
#elif !UNITY_EDITOR && UNITY_IOS
			SelectCodec(ref _selectedVideoCodec, CodecManager.VideoCodecs, NativeForceVideoCodecIndex, null, MediaApi.Unknown, true, isStartingCapture);
#endif

			if (isStartingCapture && _selectedVideoCodec == null)
			{
				Debug.LogError("[AVProMovieCapture] Failed to select a suitable video codec");
			}
			return _selectedVideoCodec;
		}

		public Codec SelectAudioCodec()
		{
			_selectedAudioCodec = null;
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			// Audio codec selection requires a video codec to be selected first on Windows
			if (_selectedVideoCodec != null)
			{
				SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, _audioCodecPriorityWindows, _selectedVideoCodec.MediaApi, true, false);
			}
#elif UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
			SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, _audioCodecPriorityMacOS, MediaApi.Unknown, true, false);
#elif !UNITY_EDITOR && UNITY_IOS
			SelectCodec(ref _selectedAudioCodec, CodecManager.AudioCodecs, NativeForceAudioCodecIndex, null, MediaApi.Unknown, true, false);
#endif

			if (_selectedAudioCodec == null)
			{
				//Debug.LogError("[AVProMovieCapture] Failed to select a suitable audio codec");
			}
			return _selectedAudioCodec;
		}

		public Device SelectAudioInputDevice()
		{
			_selectedAudioInputDevice = null;

			// Audio input device selection requires a video codec to be selected first
			if (_selectedVideoCodec != null)
			{
				if (_forceAudioInputDeviceIndex >= 0 && _forceAudioInputDeviceIndex < DeviceManager.AudioInputDevices.Count)
				{
					_selectedAudioInputDevice = DeviceManager.AudioInputDevices.Devices[_forceAudioInputDeviceIndex];
				}

				// If the found codec doesn't match the required MediaApi, set it to null
				if (_selectedAudioInputDevice != null && _selectedAudioInputDevice.MediaApi != _selectedVideoCodec.MediaApi)
				{
					_selectedAudioInputDevice = null;
				}

				// Fallback to the first device
				if (_selectedAudioInputDevice == null)
				{
					if (DeviceManager.AudioInputDevices.Count > 0)
					{
						_selectedAudioInputDevice = DeviceManager.AudioInputDevices.GetFirstWithMediaApi(_selectedVideoCodec.MediaApi);
					}
				}
			}
			return _selectedAudioInputDevice;
		}

		public static Vector2 GetRecordingResolution(int width, int height, DownScale downscale, Vector2 maxVideoSize)
		{
			int targetWidth = width;
			int targetHeight = height;
			if (downscale != DownScale.Custom)
			{
				targetWidth /= (int)downscale;
				targetHeight /= (int)downscale;
			}
			else
			{
				if (maxVideoSize.x >= 1.0f && maxVideoSize.y >= 1.0f)
				{
					targetWidth = Mathf.FloorToInt(maxVideoSize.x);
					targetHeight = Mathf.FloorToInt(maxVideoSize.y);
				}
			}

			// Some codecs like Lagarith in YUY2 mode need size to be multiple of 4
			targetWidth = NextMultipleOf4(targetWidth);
			targetHeight = NextMultipleOf4(targetHeight);

			return new Vector2(targetWidth, targetHeight);
		}

		public void SelectRecordingResolution(int width, int height)
		{
			_targetWidth = width;
			_targetHeight = height;
			if (_downScale != DownScale.Custom)
			{
				_targetWidth /= (int)_downScale;
				_targetHeight /= (int)_downScale;
			}
			else
			{
				if (_maxVideoSize.x >= 1.0f && _maxVideoSize.y >= 1.0f)
				{
					_targetWidth = Mathf.FloorToInt(_maxVideoSize.x);
					_targetHeight = Mathf.FloorToInt(_maxVideoSize.y);
				}
			}

			// Some codecs like Lagarith in YUY2 mode need size to be multiple of 4
			_targetWidth = NextMultipleOf4(_targetWidth);
			_targetHeight = NextMultipleOf4(_targetHeight);
		}

		public virtual void OnDestroy()
		{
			_waitForEndOfFrame = null;
			StopCapture(true, true);
			FreePendingFileWrites();

			// Make sure there are no other capture instances running and then deinitialise the plugin
			if (_isApplicationQuiting && _isInitialised)
			{
				// TODO: would it be faster to just look for _pendingFileWrites?
				bool anyCapturesRunning = false;
#if UNITY_EDITOR
				// In editor we have to search hidden objects as well, as the editor window components are created hidden
				CaptureBase[] captures = (CaptureBase[])Resources.FindObjectsOfTypeAll(typeof(CaptureBase));
#else
				CaptureBase[] captures = (CaptureBase[])Component.FindObjectsOfType(typeof(CaptureBase));
#endif
				foreach (CaptureBase capture in captures)
				{
					if (capture != null && capture.IsCapturing())
					{
						anyCapturesRunning = true;
						break;
					}
				}
				if (!anyCapturesRunning)
				{
					NativePlugin.Deinit();
					_isInitialised = false;
				}
			}
		}

		private void FreePendingFileWrites()
		{
			foreach (FileWritingHandler handler in _pendingFileWrites)
			{
				handler.Dispose();
			}
			_pendingFileWrites.Clear();

		}

		private void OnApplicationQuit()
		{
			_isApplicationQuiting = true;
		}

		protected void EncodeTexture(Texture2D texture)
		{
			Color32[] bytes = texture.GetPixels32();
			GCHandle _frameHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

			EncodePointer(_frameHandle.AddrOfPinnedObject());

			if (_frameHandle.IsAllocated)
			{
				_frameHandle.Free();
			}
		}

		protected bool IsUsingUnityAudio()
		{
			return (
			#if !AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
					_isRealTime &&
			#endif
					_outputTarget == OutputTarget.VideoFile && _audioCaptureSource == AudioCaptureSource.Unity && _unityAudioCapture != null);
		}

		protected bool IsUsingMotionBlur()
		{
			return (_useMotionBlur && !_isRealTime && _motionBlur != null);
		}

		public virtual void EncodePointer(System.IntPtr ptr)
		{
			if (!IsUsingUnityAudio())
			{
				NativePlugin.EncodeFrame(_handle, ptr);
			}
			else
			{
				int audioDataLength = 0;
				System.IntPtr audioDataPtr = _unityAudioCapture.ReadData(out audioDataLength);
				if (audioDataLength > 0)
				{
					NativePlugin.EncodeFrameWithAudio(_handle, ptr, audioDataPtr, (uint)audioDataLength);
				}
				else
				{
					NativePlugin.EncodeFrame(_handle, ptr);
				}
			}
		}

		public bool IsPrepared()
		{
			return (_handle >= 0);
		}

		public bool IsCapturing()
		{
			return _capturing;
		}

		public bool IsPaused()
		{
			return _paused;
		}

		public int GetRecordingWidth()
		{
			return _targetWidth;
		}

		public int GetRecordingHeight()
		{
			return _targetHeight;
		}

		protected virtual string GenerateTimestampedFilename(string filenamePrefix, string filenameExtension)
		{
			// TimeSpan span = (DateTime.Now - DateTime.Now.Date);
			// string filename = string.Format("{0}-{1}-{2}-{3}-{4}s-{5}x{6}", filenamePrefix, DateTime.Now.Year, DateTime.Now.Month.ToString("D2"), DateTime.Now.Day.ToString("D2"), ((int)(span.TotalSeconds)).ToString(), _targetWidth, _targetHeight);
			// [MOZ] Use actual time in place of seconds
			string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			string filename = string.Format("{0}_{1}_{2}x{3}", filenamePrefix, dateTime, _targetWidth, _targetHeight);
			// [MOZ] File extension is now optional
			if (!string.IsNullOrEmpty(filenameExtension))
			{
				filename = filename + "." + filenameExtension;
			}
			return filename;
		}

		private static string GetFolder(OutputPath outputPathType, string path)
		{
#if !UNITY_EDITOR && UNITY_IOS
			if (outputPathType != OutputPath.RelativeToPeristentData)
			{
				Debug.LogWarning("Only OutputPath.RelativeToPeristentData is supported on iOS");
				outputPathType = OutputPath.RelativeToPeristentData;
			}
			if (path.Length > 0)
			{
				Debug.LogWarning("Subfolders not supported on iOS");
				path = "";
			}
#endif
			string fileFolder = string.Empty;
			if (outputPathType == OutputPath.RelativeToProject)
			{
				string projectFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
				fileFolder = System.IO.Path.Combine(projectFolder, path);
			}
			else if (outputPathType == OutputPath.RelativeToPeristentData)
			{
				string dataFolder = System.IO.Path.GetFullPath(Application.persistentDataPath);
				fileFolder = System.IO.Path.Combine(dataFolder, path);
			}
			else if (outputPathType == OutputPath.RelativeToDesktop)
			{
				string desktopFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
				fileFolder = System.IO.Path.Combine(desktopFolder, path);
			}
			else if (outputPathType == OutputPath.Absolute)
			{
				fileFolder = path;
			}
			return fileFolder;
		}

		private static string GenerateFilePath(OutputPath outputPathType, string path, string filename)
		{
			// Resolve folder
			string fileFolder = GetFolder(outputPathType, path);

			// Combine path and filename
			return System.IO.Path.Combine(fileFolder, filename);
		}

		protected static bool HasExtension(string path, string extension)
		{
			return path.ToLower().EndsWith(extension, StringComparison.OrdinalIgnoreCase);
		}

		protected void GenerateFilename()
		{
			string filename = string.Empty;
			if (_outputTarget == OutputTarget.VideoFile)
			{
				if (!_allowManualFileExtension)
				{
					if (_selectedVideoCodec == null)
					{
						SelectVideoCodec();
						SelectAudioCodec();
					}
					_filenameExtension = NativePlugin.GetContainerFileExtensions(_selectedVideoCodec.Index, (_selectedAudioCodec!= null)?_selectedAudioCodec.Index:-1)[0];
				}
				filename = _filenamePrefix + "." + _filenameExtension;
				if (_appendFilenameTimestamp)
				{
					filename = GenerateTimestampedFilename(_filenamePrefix, _filenameExtension);
				}
			}
			else if (_outputTarget == OutputTarget.ImageSequence)
			{
				// [MOZ] Made the enclosing folder uniquely named, easier for extraction on iOS and simplifies scripts for processing the frames
				string fileExtension = Utils.GetImageFileExtension(NativeImageSequenceFormat);
				filename = GenerateTimestampedFilename(_filenamePrefix, null) + "/frame" + string.Format("-%0{0}d.{1}", _imageSequenceZeroDigits, fileExtension);
			}
			else if (_outputTarget == OutputTarget.NamedPipe)
			{
				_filePath = _namedPipePath;
			}

			if (_outputTarget == OutputTarget.VideoFile ||
				_outputTarget == OutputTarget.ImageSequence)
			{
				_filePath = GenerateFilePath(_outputFolderType, _outputFolderPath, filename);

				// Create target directory if doesn't exist
				String directory = Path.GetDirectoryName(_filePath);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
			}
		}

		public UnityAudioCapture FindOrCreateUnityAudioCapture(bool logWarnings)
		{
			UnityAudioCapture result = null;

			Type audioCaptureType = null;
			if (_isRealTime)
			{
				 audioCaptureType = typeof(CaptureAudioFromAudioListener);
			}
			else
			{
				#if AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
				audioCaptureType = typeof(CaptureAudioFromAudioRenderer);
				#endif
			}

			if (audioCaptureType != null)
			{
				// Try to find an existing matching component locally
				result = (UnityAudioCapture)this.GetComponent(audioCaptureType);
				if (result == null)
				{
					// Try to find an existing matching component globally
					result = (UnityAudioCapture)GameObject.FindObjectOfType(audioCaptureType);
				}

				// No existing component was found, so create one
				if (result == null)
				{
					// Find a suitable gameobject to add the component to
					GameObject parentGameObject = null;
					if (_isRealTime)
					{
						// Find an AudioListener to attach the UnityAudioCapture component to
						AudioListener audioListener = this.GetComponent<AudioListener>();
						if (audioListener == null)
						{
							audioListener = GameObject.FindObjectOfType<AudioListener>();
						}
						parentGameObject = audioListener.gameObject;
					}
					else
					{
						parentGameObject = this.gameObject;
					}

					// Create the component
					if (_isRealTime)
					{
						if (parentGameObject != null)
						{
							result = (UnityAudioCapture)parentGameObject.AddComponent(audioCaptureType);
							if (logWarnings)
							{
								Debug.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to create one manually (very slow).  Consider adding a UnityAudioCapture component to your scene and assigned it to this MovieCapture component.");
							}
						}
						else
						{
							if (logWarnings)
							{
								Debug.LogWarning("[AVProMovieCapture] No AudioListener found in scene.  Unable to capture audio from Unity.");
							}
						}
					}
					else
					{
						#if AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
						result = (UnityAudioCapture)parentGameObject.AddComponent(audioCaptureType);
						((CaptureAudioFromAudioRenderer)result).Capture = this;
						if (logWarnings)
						{
							Debug.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to create one manually (very slow).  Consider adding a UnityAudioCapture component to your scene and assigned it to this MovieCapture component.");
						}
						#endif
					}
				}
				else
				{
					if (logWarnings)
					{
						Debug.LogWarning("[AVProMovieCapture] Capturing audio from Unity without an UnityAudioCapture assigned so we had to search for one manually (very slow)");
					}
				}
			}
			return result;
		}

		public virtual bool PrepareCapture()
		{
			// Delete file if it already exists
			if (_outputTarget == OutputTarget.VideoFile && File.Exists(_filePath))
			{
				File.Delete(_filePath);
			}

			_stats = new CaptureStats();

#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			if (_minimumDiskSpaceMB > 0 && _outputTarget == OutputTarget.VideoFile)
			{
				ulong freespace = 0;
				if (Utils.DriveFreeBytes(System.IO.Path.GetPathRoot(_filePath), out freespace))
				{
					_freeDiskSpaceMB = (long)(freespace / (1024 * 1024));
				}

				if (!IsEnoughDiskSpace())
				{
					Debug.LogError("[AVProMovieCapture] Not enough free space to start capture.  Stopping capture.");
					return false;
				}
			}
#endif

			if (_isRealTime)
			{
				/*if (_allowFrameRateChange)
				{
					_oldTargetFrameRate = Application.targetFrameRate;
					Application.targetFrameRate = (int)_frameRate;
				}*/
			}
			else
			{
				// Disable vsync
				#if !UNITY_EDITOR_OSX && UNITY_IOS
					if (_allowVSyncDisable)
					{
						// iOS doesn't support disabling vsync so use _oldVsyncCount to store the current target framerate.
						_oldVSyncCount = Application.targetFrameRate;
						// We want to runs as fast as possible.
						Application.targetFrameRate = 300;
					}
				#else
					if (_allowVSyncDisable && !Screen.fullScreen && QualitySettings.vSyncCount > 0)
					{
						_oldVSyncCount = QualitySettings.vSyncCount;
						QualitySettings.vSyncCount = 0;
					}
				#endif

				if (_useMotionBlur && _motionBlurSamples > 1)
				{
					#if AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
					Time.captureDeltaTime = 1f / (_motionBlurSamples * _frameRate);
					#else
					Time.captureFramerate = (int)(_motionBlurSamples * _frameRate);
					#endif

					// FromTexture and FromCamera360 captures don't require a camera for rendering, so set up the motion blur component differently
					if (this is CaptureFromTexture || this is CaptureFromCamera360 || this is CaptureFromCamera360ODS)
					{
						if (_motionBlur == null)
						{
							_motionBlur = this.GetComponent<MotionBlur>();
						}
						if (_motionBlur == null)
						{
							_motionBlur = this.gameObject.AddComponent<MotionBlur>();
						}
						if (_motionBlur != null)
						{
							_motionBlur.NumSamples = _motionBlurSamples;
							_motionBlur.SetTargetSize(_targetWidth, _targetHeight);
							_motionBlur.enabled = false;
						}
					}
					// FromCamera and FromScreen use this path
					else if (_motionBlurCameras.Length > 0)
					{
						// Setup the motion blur filters where cameras are used
						foreach (Camera camera in _motionBlurCameras)
						{
							MotionBlur mb = camera.GetComponent<MotionBlur>();
							if (mb == null)
							{
								mb = camera.gameObject.AddComponent<MotionBlur>();
							}
							if (mb != null)
							{
								mb.NumSamples = _motionBlurSamples;
								mb.enabled = true;
								_motionBlur = mb;
							}
						}
					}
				}
				else
				{
					#if AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
					Time.captureDeltaTime = 1f / _frameRate;
					#else
					Time.captureFramerate = (int)_frameRate;
					#endif
				}

				// Change physics update speed
				_oldFixedDeltaTime = Time.fixedDeltaTime;
				#if AVPRO_MOVIECAPTURE_CAPTUREDELTA_SUPPORT
				Time.fixedDeltaTime = Time.captureDeltaTime;
				#else
				Time.fixedDeltaTime = 1.0f / Time.captureFramerate;
				#endif
			}

			// Resolve desired audio source
			_stats.AudioCaptureSource = AudioCaptureSource.None;
			if (_audioCaptureSource != AudioCaptureSource.None && _outputTarget == OutputTarget.VideoFile)
			{
				if (_audioCaptureSource == AudioCaptureSource.Microphone && _isRealTime)
				{
					if (_selectedAudioInputDevice != null)
					{
						_stats.AudioCaptureSource = AudioCaptureSource.Microphone;
					}
					else
					{
						Debug.LogWarning("[AVProMovieCapture] No microphone found");
					}
				}
				else if (_audioCaptureSource == AudioCaptureSource.Unity)
				{
					// If there is already a capture component, make sure it's the right one otherwise remove it
					if (_unityAudioCapture != null)
					{
						bool removeComponent = false;
						if (_isRealTime)
						{
							removeComponent = !(_unityAudioCapture is CaptureAudioFromAudioListener);
						}
						else
						{
							removeComponent = (_unityAudioCapture is CaptureAudioFromAudioListener);
						}

						if (removeComponent)
						{
							Destroy(_unityAudioCapture);
							_unityAudioCapture = null;
						}
					}

					// We if try to capture audio from Unity but there isn't an UnityAudioCapture component set
					if (_unityAudioCapture == null)
					{
						_unityAudioCapture = FindOrCreateUnityAudioCapture(true);
					}
					if (_unityAudioCapture != null)
					{
						_unityAudioCapture.PrepareCapture();
						_stats.UnityAudioSampleRate = AudioSettings.outputSampleRate;
						_stats.UnityAudioChannelCount = _unityAudioCapture.ChannelCount;
						_stats.AudioCaptureSource = AudioCaptureSource.Unity;
					}
					else
					{
						Debug.LogWarning("[AVProMovieCapture] Unable to create AudioCapture component");
					}
				}
				else if (_audioCaptureSource == AudioCaptureSource.Manual)
				{
					_stats.UnityAudioSampleRate = _manualAudioSampleRate;
					_stats.UnityAudioChannelCount = _manualAudioChannelCount;
					_stats.AudioCaptureSource = AudioCaptureSource.Manual;
				}
			}

			string info = string.Empty;
			if (_logCaptureStartStop)
			{
				info = string.Format("{0}x{1} @ {2}fps [{3}]", _targetWidth, _targetHeight, _frameRate.ToString("F2"), _pixelFormat.ToString());
				if (_outputTarget == OutputTarget.VideoFile)
				{
					info += string.Format(" vcodec:'{0}'", _selectedVideoCodec.Name);
					if (_stats.AudioCaptureSource != AudioCaptureSource.None)
					{
						if (_audioCaptureSource == AudioCaptureSource.Microphone && _selectedAudioInputDevice != null)
						{
							info += string.Format(" audio source:'{0}'", _selectedAudioInputDevice.Name);
						}
						else if (_audioCaptureSource == AudioCaptureSource.Unity)
						{
							info += string.Format(" audio source:'Unity' {0}hz {1} channels", _stats.UnityAudioSampleRate, _stats.UnityAudioChannelCount);
						}
						else if (_audioCaptureSource == AudioCaptureSource.Manual)
						{
							info += string.Format(" audio source:'Manual' {0}hz {1} channels", _stats.UnityAudioSampleRate, _stats.UnityAudioChannelCount);
						}
						if (_selectedAudioCodec != null)
						{
							info += string.Format(" acodec:'{0}'", _selectedAudioCodec.Name);
						}
					}

					info += string.Format(" to file: '{0}'", _filePath);
				}
				else if (_outputTarget == OutputTarget.ImageSequence)
				{
					info += string.Format(" to file: '{0}'", _filePath);
				}
				else if (_outputTarget == OutputTarget.NamedPipe)
				{
					info += string.Format(" to pipe: '{0}'", _filePath);
				}
			}

			// If the user has overriden the vertical flip
			if (_flipVertically)
			{
				_isTopDown = !_isTopDown;
			}

			if (_outputTarget == OutputTarget.VideoFile)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Start File Capture: " + info);
				}
				bool useRealtimeClock = (_isRealTime && _timelapseScale <= 1);
				_handle = NativePlugin.CreateRecorderVideo(_filePath, (uint)_targetWidth, (uint)_targetHeight, _frameRate, (int)_pixelFormat, useRealtimeClock, _isTopDown,
																	_selectedVideoCodec.Index, _stats.AudioCaptureSource, _stats.UnityAudioSampleRate,
																	_stats.UnityAudioChannelCount, (_selectedAudioInputDevice != null)?_selectedAudioInputDevice.Index:-1,
																	(_selectedAudioCodec != null)?_selectedAudioCodec.Index:-1,	_forceGpuFlush, GetEncoderHints().videoHints);
			}
			else if (_outputTarget == OutputTarget.ImageSequence)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Start Images Capture: " + info);
				}
				bool useRealtimeClock = (_isRealTime && _timelapseScale <= 1);
				_handle = NativePlugin.CreateRecorderImages(_filePath, (uint)_targetWidth, (uint)_targetHeight, _frameRate,
																	(int)_pixelFormat, useRealtimeClock, _isTopDown,
																	(int)NativeImageSequenceFormat, _forceGpuFlush, _imageSequenceStartFrame, GetEncoderHints().imageHints);
			}
			else if (_outputTarget == OutputTarget.NamedPipe)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Start Pipe Capture: " + info);
				}
				_handle = NativePlugin.CreateRecorderPipe(_filePath, (uint)_targetWidth, (uint)_targetHeight, _frameRate,
																	 (int)_pixelFormat, _isTopDown, GetEncoderHints().videoHints.supportTransparency, _forceGpuFlush);
			}

			if (_handle < 0)
			{
				Debug.LogError("[AVProMovieCapture] Failed to create recorder");

				// Try to give a reason why it failed
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
				if (_selectedVideoCodec.MediaApi == MediaApi.MediaFoundation)
				{
					if (!HasExtension(_filePath, ".mp4"))
					{
						Debug.LogError("[AVProMovieCapture] When using a MediaFoundation codec the MP4 extension must be used");
					}

					// MF H.264 encoder has a limit of Level 5.2 which is 9,437,184 luma pixels
					// but we've seen it fail slightly below this limit, so we test against 9360000
					// to offer a useful potential error message
					if (((_targetWidth * _targetHeight) >= 9360000) && _selectedVideoCodec.Name.Contains("H264"))
					{
						Debug.LogError("[AVProMovieCapture] Resolution is possibly too high for the MF H.264 codec");
					}
				}
				else if (_selectedVideoCodec.MediaApi == MediaApi.DirectShow)
				{
					if (HasExtension(_filePath, ".mp4") && _selectedVideoCodec.Name.Contains("Uncompressed"))
					{
						Debug.LogError("[AVProMovieCapture] Uncompressed video codec not supported with MP4 extension, use AVI instead for uncompressed");
					}
				}
#endif

				StopCapture();
			}

// (mac|i)OS only for now
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))
			SetupErrorHandler();
#endif
			return (_handle >= 0);
		}

#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))

		static Dictionary<int, CaptureBase> _HandleToCaptureMap = new Dictionary<int, CaptureBase>();

		private void SetupErrorHandler()
		{
			NativePlugin.ErrorHandlerDelegate errorHandlerDelegate = new NativePlugin.ErrorHandlerDelegate(ErrorHandler);
			System.IntPtr func = Marshal.GetFunctionPointerForDelegate(errorHandlerDelegate);
			NativePlugin.SetErrorHandler(_handle, func);
			_HandleToCaptureMap.Add(_handle, this);
		}

		private void CleanupErrorHandler()
		{
			_HandleToCaptureMap.Remove(_handle);
		}

#if ENABLE_IL2CPP
		[MonoPInvokeCallback(typeof(NativePlugin.ErrorHandlerDelegate))]
#endif
		private static void ErrorHandler(int handle, int domain, int code, string message)
		{
			CaptureBase capture;
			if (_HandleToCaptureMap.TryGetValue(handle, out capture))
			{
				capture.ActualErrorHandler(domain, code, message);
			}
		}

		private void ActualErrorHandler(int domain, int code, string message) {
			if (_capturing)
			{
				CancelCapture();
				Debug.LogError("Capture cancelled");
			}
			Debug.LogErrorFormat("Error: domain: {0}, code: {1}, message: {2}", domain, code, message);
		}
#endif

		public void QueueStartCapture()
		{
			_queuedStartCapture = true;
			_stats = new CaptureStats();
		}

		public bool IsStartCaptureQueued()
		{
			return _queuedStartCapture;
		}

		public bool StartCapture()
		{
			if (_capturing)
			{
				return false;
			}

			if (_waitForEndOfFrame == null)
			{
				// Start() hasn't happened yet, so queue the StartCapture
				QueueStartCapture();
				return false;
			}

			if (_handle < 0)
			{
				if (!PrepareCapture())
				{
					return false;
				}
			}

			if (_handle >= 0)
			{
				if (IsUsingUnityAudio())
				{
					_unityAudioCapture.StartCapture();
				}

				if (!NativePlugin.Start(_handle))
				{
					StopCapture(true);
					Debug.LogError("[AVProMovieCapture] Failed to start recorder");
					return false;
				}
				ResetFPS();
				_captureStartTime = Time.realtimeSinceStartup;
				_capturePrePauseTotalTime = 0f;

				// NOTE: We set this to the elapsed time so that the first frame is captured immediately
				_timeSinceLastFrame = GetSecondsPerCaptureFrame();

				#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
				if (!_isRealTime && _timelineController != null)
				{
					_timelineController.StartCapture();
				}
				#endif
				#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
				if (!_isRealTime && _videoPlayerController != null)
				{
					_videoPlayerController.StartCapture();
				}
				#endif

				_capturing = true;
				_paused = false;

				if (_startDelay != StartDelayMode.None)
				{
					_startDelayTimer = 0f;
					_startPaused = true;
					PauseCapture();
				}

				#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPaused)
				{
					PauseCapture();
				}
				#endif
			}

			return _capturing;
		}

		public void PauseCapture()
		{
			if (_capturing && !_paused)
			{
				if (IsUsingUnityAudio())
				{
					_unityAudioCapture.enabled = false;
				}
				NativePlugin.Pause(_handle);

				if (!_isRealTime)
				{
					// TODO: should be store the timeScale value and restore it instead of assuming timeScale == 1.0?
					Time.timeScale = 0f;
				}

				_paused = true;
				ResetFPS();
			}
		}

		public void ResumeCapture()
		{
			if (_capturing && _paused)
			{
				if (IsUsingUnityAudio())
				{
					_unityAudioCapture.FlushBuffer();
					_unityAudioCapture.enabled = true;
				}

				NativePlugin.Start(_handle);

				if (!_isRealTime)
				{
					Time.timeScale = 1f;
				}

				_paused = false;
				if (_startPaused)
				{
					_captureStartTime = Time.realtimeSinceStartup;
					_capturePrePauseTotalTime = 0f;
					_startPaused = false;
				}
			}
		}

		public void CancelCapture()
		{
			StopCapture(true);

			// Delete file
			if (_outputTarget == OutputTarget.VideoFile && File.Exists(_filePath))
			{
				File.Delete(_filePath);
			}
		}

		public virtual void UnprepareCapture()
		{
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))
			CleanupErrorHandler();
#endif
		}

		public static string LastFileSaved
		{
			get
			{
#if UNITY_EDITOR
				return UnityEditor.EditorPrefs.GetString("AVProMovieCapture-LastSavedFile", string.Empty);
#else
				return PlayerPrefs.GetString("AVProMovieCapture-LastSavedFile", string.Empty);
#endif
			}
			set
			{
				PlayerPrefs.SetString("AVProMovieCapture-LastSavedFile", value);
#if UNITY_EDITOR
				UnityEditor.EditorPrefs.SetString("AVProMovieCapture-LastSavedFile", value);
#endif
			}
		}

		protected void RenderThreadEvent(NativePlugin.PluginEvent renderEvent)
		{
			NativePlugin.RenderThreadEvent(renderEvent, _handle);
		}

		public virtual void StopCapture(bool skipPendingFrames = false, bool ignorePendingFileWrites = false)
		{
			UnprepareCapture();

			if (_capturing)
			{
				if (_logCaptureStartStop)
				{
					Debug.Log("[AVProMovieCapture] Stopping capture " + _handle);
				}
				_capturing = false;
			}

			bool applyPostOperations = false;
			FileWritingHandler fileWritingHandler = null;
			if (_handle >= 0)
			{
				NativePlugin.Stop(_handle, skipPendingFrames);

				if (_outputTarget == OutputTarget.VideoFile)
				{
					applyPostOperations = true;
				}

				fileWritingHandler = new FileWritingHandler(_filePath, _handle);

				// Free the recorder, or if the file is still being written, store the action to be invoked where it is complete
				bool canFreeRecorder = (ignorePendingFileWrites || NativePlugin.IsFileWritingComplete(_handle));

				if (canFreeRecorder)
				{
					NativePlugin.FreeRecorder(_handle);
					RenderThreadEvent(NativePlugin.PluginEvent.FreeResources);
				}
				else
				{
					// If no external action has been set up for the checking when the file writing begins and end,
					// add it to an internal list so we can make sure it completes
					if (_beginFinalFileWritingAction == null)
					{
						_pendingFileWrites.Add(fileWritingHandler);
					}

					if (applyPostOperations && CanApplyPostOperations(_filePath))
					{
						fileWritingHandler.AddPostOperation(GetEncoderHints().videoHints);
						applyPostOperations = false;
					}
				}

				// If there is an external action set up, then notify it that writing has begun
				if (_beginFinalFileWritingAction != null)
				{
					_beginFinalFileWritingAction.Invoke(fileWritingHandler);
				}

				_handle = -1;

				// Save the last captured path
				if (!string.IsNullOrEmpty(_filePath))
				{
					if (_outputTarget == OutputTarget.VideoFile)
					{
						LastFileSaved = _filePath;
					}
					else if (_outputTarget == OutputTarget.ImageSequence)
					{
						LastFileSaved = System.IO.Path.GetDirectoryName(_filePath);
					}
				}
				#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
				if (_videoPlayerController != null)
				{
					_videoPlayerController.StopCapture();
				}
				#endif
				#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
				if (_timelineController != null)
				{
					_timelineController.StopCapture();
				}
				#endif
			}

			_fileInfo = null;

			if (_unityAudioCapture)
			{
				_unityAudioCapture.StopCapture();
			}
			if (_motionBlur)
			{
				_motionBlur.enabled = false;
			}

			// Restore Unity timing
			Time.captureFramerate = 0;
			//Application.targetFrameRate = _oldTargetFrameRate;
			//_oldTargetFrameRate = -1;

			if (_oldFixedDeltaTime > 0f)
			{
				Time.fixedDeltaTime = _oldFixedDeltaTime;
			}
			_oldFixedDeltaTime = 0f;

			#if !UNITY_EDITOR_OSX && UNITY_IOS
				// iOS doesn't support disabling vsync so _oldVsyncCount is actually the target framerate before we started capturing.
				if (_oldVSyncCount != 0)
				{
					Application.targetFrameRate = _oldVSyncCount;
					_oldVSyncCount = 0;
				}
			#else
				if (_oldVSyncCount > 0)
				{
					QualitySettings.vSyncCount = _oldVSyncCount;
					_oldVSyncCount = 0;
				}
			#endif

			_motionBlur = null;

			if (_texture != null)
			{
				Destroy(_texture);
				_texture = null;
			}

			if (applyPostOperations)
			{
				ApplyPostOperations(_filePath);
			}
		}

		private bool CanApplyPostOperations(string filePath)
		{
			bool result = false;
			#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			if (GetEncoderHints().videoHints.allowFastStartStreamingPostProcess && HasExtension(filePath, ".mp4"))
			{
				result = true;
			}
			#endif
			return result;
		}

		protected void ApplyPostOperations(string filePath)
		{
			if (CanApplyPostOperations(filePath))
			{
				try
				{
					if (!MP4FileProcessing.ApplyFastStart(filePath, false))
					/*MP4FileProcessing.Options options = new MP4FileProcessing.Options();
					options.applyFastStart = true;
					options.applyStereoMode = false;
					options.stereoMode = MP4FileProcessing.StereoMode.StereoLeftRight;
					if (MP4FileProcessing.Process(path, false, options))*/
					{
						Debug.LogWarning("[AVProMovieCapture] failed moving atom 'moov' to start of file for fast streaming");
					}
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		private void ToggleCapture()
		{
			if (_capturing)
			{
				//_queuedStopCapture = true;
				//_queuedStartCapture = false;
				StopCapture();
			}
			else
			{
				//_queuedStartCapture = true;
				//_queuedStopCapture = false;
				StartCapture();
			}
		}

		private bool IsEnoughDiskSpace()
		{
			bool result = true;
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
			long fileSizeMB = GetCaptureFileSize() / (1024 * 1024);

			if ((_freeDiskSpaceMB - fileSizeMB) < _minimumDiskSpaceMB)
			{
				result = false;
			}
#endif
			return result;
		}

		protected bool CanContinue()
		{
			bool result = true;
			#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
			if (IsCapturing() && !IsPaused() && !_isRealTime && _videoPlayerController != null)
			{
				result = _videoPlayerController.CanContinue();
				//Debug.Log(result);
			}
			#endif
			return result;
		}

		private void LateUpdate()
		{
			if (_handle >= 0 && !_paused)
			{
				CheckFreeDiskSpace();
			}

			if (_captureKey != KeyCode.None)
			{
				if (Input.GetKeyDown(_captureKey))
				{
					ToggleCapture();
				}
			}

			RemoveCompletedFileWrites();

			if (_frameUpdateMode == FrameUpdateMode.Automatic)
			{
				// Resume capture if a start delay has been specified
				if (IsCapturing() && IsPaused() && _stats.NumEncodedFrames == 0)
				{
					float delta = 0f;
					if (_startDelay == StartDelayMode.GameSeconds)
					{
						if (!_isRealTime)
						{
							// In offline render mode Time.deltaTime is always zero due Time.timeScale being set to zero, 
							// so just use the real world time
							delta = Time.unscaledDeltaTime;
						}
						else
						{
							delta = Time.deltaTime;
						}
					}
					else if (_startDelay == StartDelayMode.RealSeconds)
					{
						delta = Time.unscaledDeltaTime;
					}
					if (delta > 0f)
					{
						_startDelayTimer += delta;
						if (IsStartDelayComplete())
						{
							ResumeCapture();
						}
					}
				}

				PreUpdateFrame();
				UpdateFrame();
			}
		}

		private void RemoveCompletedFileWrites()
		{
			for (int i = _pendingFileWrites.Count - 1; i >= 0; i--)
			{
				FileWritingHandler handler = _pendingFileWrites[i];
				if (handler.IsFileReady())
				{
					_pendingFileWrites.RemoveAt(i);
				}
			}
		}

		private void CheckFreeDiskSpace()
		{
			if (_minimumDiskSpaceMB > 0)
			{
				if (!IsEnoughDiskSpace())
				{
					Debug.LogWarning("[AVProMovieCapture] Free disk space getting too low.  Stopping capture.");
					StopCapture(true);
				}
			}
		}

		protected bool IsStartDelayComplete()
		{
			bool result = false;
			if (_startDelay == StartDelayMode.None)
			{
				result = true;
			}
			else if (_startDelay == StartDelayMode.GameSeconds ||
					_startDelay == StartDelayMode.RealSeconds)
			{
				result = (_startDelayTimer >= _startDelaySeconds);
			}
			return result;
		}

		protected bool IsProgressComplete()
		{
			bool result = false;
			if (_stopMode != StopMode.None)
			{
				switch (_stopMode)
				{
					case StopMode.FramesEncoded:
						result = (_stats.NumEncodedFrames >= _stopFrames);
						break;
					case StopMode.SecondsEncoded:
						result = (_stats.TotalEncodedSeconds >= _stopSeconds);
						break;
					case StopMode.SecondsElapsed:
						if (!_startPaused && !_paused)
						{
							float timeSinceLastEditorPause = (Time.realtimeSinceStartup - _captureStartTime);
							result = (timeSinceLastEditorPause + _capturePrePauseTotalTime) >= _stopSeconds;
						}
						break;
				}
			}
			return result;
		}

		public float GetProgress()
		{
			float result = 0f;
			if (_stopMode != StopMode.None)
			{
				switch (_stopMode)
				{
					case StopMode.FramesEncoded:
						result = (_stats.NumEncodedFrames / (float)_stopFrames);
						break;
					case StopMode.SecondsEncoded:
						result = ((_stats.NumEncodedFrames / _frameRate) / _stopSeconds);
						break;
					case StopMode.SecondsElapsed:
						if (!_startPaused && !_paused)
						{
							float timeSinceLastEditorPause = (Time.realtimeSinceStartup - _captureStartTime);
							result = (timeSinceLastEditorPause + _capturePrePauseTotalTime) / _stopSeconds;
						}
						break;
				}
			}
			return result;
		}

		protected float GetSecondsPerCaptureFrame()
		{
			float timelapseScale = (float)_timelapseScale;
			if (!_isRealTime)
			{
				timelapseScale = 1f;
			}
			float captureFrameRate = _frameRate / timelapseScale;
			float secondsPerFrame = 1f / captureFrameRate;
			return secondsPerFrame;
		}

		protected bool CanOutputFrame()
		{
			bool result = false;
			if (_handle >= 0)
			{
				if (_isRealTime)
				{
					if (NativePlugin.IsNewFrameDue(_handle))
					{
						result = (_timeSinceLastFrame >= GetSecondsPerCaptureFrame());
						//result = true;
					}
				}
				else
				{
					const int WatchDogLimit = 1000;
					int watchdog = 0;
					if (_outputTarget != OutputTarget.NamedPipe)
					{
						// Wait for the encoder to have an available buffer
						// The watchdog prevents an infinite while loop
						while (_handle >= 0 && !NativePlugin.IsNewFrameDue(_handle) && watchdog < WatchDogLimit)
						{
							System.Threading.Thread.Sleep(1);
							watchdog++;
						}
					}

					// Return handle status as it may have closed elsewhere
					result = (_handle >= 0) && (watchdog < WatchDogLimit);
				}
			}
			return result;
		}

		protected void TickFrameTimer()
		{
			_timeSinceLastFrame += Time.deltaTime;//unscaledDeltaTime;
		}

		protected void RenormTimer()
		{
			float secondsPerFrame = GetSecondsPerCaptureFrame();
			if (_timeSinceLastFrame >= secondsPerFrame)
			{
				_timeSinceLastFrame -= secondsPerFrame;
			}
		}

		public virtual Texture GetPreviewTexture()
		{
			return null;
		}

		protected void EncodeUnityAudio()
		{
			if (IsUsingUnityAudio())
			{
				int audioDataLength = 0;
				System.IntPtr audioDataPtr = _unityAudioCapture.ReadData(out audioDataLength);
				if (audioDataLength > 0)
				{
					NativePlugin.EncodeAudio(_handle, audioDataPtr, (uint)audioDataLength);
				}
			}
		}

		public void EncodeAudio(float[] audioData)
		{
			if (audioData.Length > 0)
			{
				int byteCount = Marshal.SizeOf(audioData[0]) * audioData.Length;

				// Copy the array to unmanaged memory.
				System.IntPtr pointer = Marshal.AllocHGlobal(byteCount);
				Marshal.Copy(audioData, 0, pointer, audioData.Length);

				// Encode
				NativePlugin.EncodeAudio(_handle, pointer, (uint)audioData.Length);

				// Free the unmanaged memory.
				Marshal.FreeHGlobal(pointer);
			}
		}

		public virtual void PreUpdateFrame()
		{
			#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
			if (IsCapturing() && !IsPaused() && !_isRealTime && _timelineController != null)
			{
				_timelineController.UpdateFrame();
			}
			#endif
			#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
			if (IsCapturing() && !IsPaused() && !_isRealTime && _videoPlayerController != null)
			{
				_videoPlayerController.UpdateFrame();
			}
			#endif
		}

		public virtual void UpdateFrame()
		{
			if (_handle >= 0 && !_paused)
			{
				_stats.NumDroppedFrames = NativePlugin.GetNumDroppedFrames(_handle);
				_stats.NumDroppedEncoderFrames = NativePlugin.GetNumDroppedEncoderFrames(_handle);
				_stats.NumEncodedFrames = NativePlugin.GetNumEncodedFrames(_handle);
				_stats.TotalEncodedSeconds = NativePlugin.GetEncodedSeconds(_handle);

				if (IsProgressComplete())
				{
					_queuedStopCapture = true;
				}
			}

			if (_queuedStopCapture)
			{
				_queuedStopCapture = false;
				_queuedStartCapture = false;
				StopCapture();
			}
			if (_queuedStartCapture)
			{
				_queuedStartCapture = false;
				StartCapture();
			}
		}

		protected void ResetFPS()
		{
			_stats.ResetFPS();
		}

		public void UpdateFPS()
		{
			_stats.UpdateFPS();
		}

		protected int GetCameraAntiAliasingLevel(Camera camera)
		{
			int aaLevel = QualitySettings.antiAliasing;
			if (aaLevel == 0)
			{
				aaLevel = 1;
			}

			if (_renderAntiAliasing > 0)
			{
				aaLevel = _renderAntiAliasing;
			}

			if (aaLevel != 1 && aaLevel != 2 && aaLevel != 4 && aaLevel != 8)
			{
				Debug.LogWarning("[AVProMovieCapture] Invalid antialiasing value, must be 1, 2, 4 or 8.  Defaulting to 1. >> " + aaLevel);
				aaLevel = 1;
			}

			if (aaLevel != 1)
			{
				if (camera.actualRenderingPath == RenderingPath.DeferredLighting || camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					Debug.LogWarning("[AVProMovieCapture] Not using antialiasing because MSAA is not supported by camera render path " + camera.actualRenderingPath);
					aaLevel = 1;
				}
			}
			return aaLevel;
		}

		public long GetCaptureFileSize()
		{
			long result = 0;
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS))
			result = NativePlugin.GetFileSize(_handle);
#elif !UNITY_WEBPLAYER
			if (_handle >= 0 && _outputTarget == OutputTarget.VideoFile)
			{
				if (_fileInfo == null && File.Exists(_filePath))
				{
					_fileInfo = new System.IO.FileInfo(_filePath);
				}
				if (_fileInfo != null)
				{
					_fileInfo.Refresh();
					result = _fileInfo.Length;
				}
			}
#endif
			return result;
		}

		public static void GetResolution(Resolution res, ref int width, ref int height)
		{
			switch (res)
			{
				case Resolution.POW2_8192x8192:
					width = 8192; height = 8192;
					break;
				case Resolution.POW2_8192x4096:
					width = 8192; height = 4096;
					break;
				case Resolution.POW2_4096x4096:
					width = 4096; height = 4096;
					break;
				case Resolution.POW2_4096x2048:
					width = 4096; height = 2048;
					break;
				case Resolution.POW2_2048x4096:
					width = 2048; height = 4096;
					break;
				case Resolution.UHD_3840x2160:
					width = 3840; height = 2160;
					break;
				case Resolution.UHD_3840x2048:
					width = 3840; height = 2048;
					break;
				case Resolution.UHD_3840x1920:
					width = 3840; height = 1920;
					break;
				case Resolution.POW2_2048x2048:
					width = 2048; height = 2048;
					break;
				case Resolution.POW2_2048x1024:
					width = 2048; height = 1024;
					break;
				case Resolution.HD_1920x1080:
					width = 1920; height = 1080;
					break;
				case Resolution.HD_1280x720:
					width = 1280; height = 720;
					break;
				case Resolution.SD_1024x768:
					width = 1024; height = 768;
					break;
				case Resolution.SD_800x600:
					width = 800; height = 600;
					break;
				case Resolution.SD_800x450:
					width = 800; height = 450;
					break;
				case Resolution.SD_640x480:
					width = 640; height = 480;
					break;
				case Resolution.SD_640x360:
					width = 640; height = 360;
					break;
				case Resolution.SD_320x240:
					width = 320; height = 240;
					break;
			}
		}

		// Returns the next multiple of 4 or the same value if it's already a multiple of 4
		protected static int NextMultipleOf4(int value)
		{
			return (value + 3) & ~0x03;
		}
	}
}