using System.Collections.Generic;
using System.Threading;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{
	/// Allows the user to monitor a capture that has completed, but the file is still being written to asynchronously
	public class FileWritingHandler : System.IDisposable
	{
		private string _path;
		private int _handle;
		private VideoEncoderHints _videoEncodingHints;
		private ManualResetEvent _postProcessEvent;

		public string Path
		{
			get { return _path; }
		}

		internal FileWritingHandler(string path, int handle)
		{
			_path = path;
			_handle = handle;
		}

		internal void AddPostOperation(VideoEncoderHints videoEncodingHints)
		{
			_videoEncodingHints = videoEncodingHints;
		}

		private bool StartPostProcess()
		{
			UnityEngine.Debug.Assert(_postProcessEvent == null);
			if (_videoEncodingHints.allowFastStartStreamingPostProcess)
			{
				_postProcessEvent = MP4FileProcessing.ApplyFastStartAsync(_path, false);
				if (_postProcessEvent == null)
				{
					UnityEngine.Debug.LogWarning("[AVProMovieCapture] failed moving atom 'moov' to start of file for fast streaming");
				}
			}
			return true;
		}

		public bool IsFileReady()
		{
			bool result = true;
			if (_handle >= 0)
			{
				result = NativePlugin.IsFileWritingComplete(_handle);
				if (result)
				{
					if (_videoEncodingHints != null)
					{
						result = StartPostProcess();
						_videoEncodingHints = null;
					}
					if (_postProcessEvent != null)
					{
						result = _postProcessEvent.WaitOne(1);
					}
					if (result)
					{
						Dispose();
					}
				}
			}
			return result;
		}

		public void Dispose()
		{
			if (_handle >= 0)
			{
				NativePlugin.FreeRecorder(_handle);
				_handle = -1;

				// Issue the free resources plugin event
				NativePlugin.RenderThreadEvent(NativePlugin.PluginEvent.FreeResources, -1);
			}

			_videoEncodingHints = null;
			_postProcessEvent = null;
		}

		// Helper method for cleaning up a list
		// TODO: add an optional System.Action callback for each time the file writer completes
		public static bool Cleanup(List<FileWritingHandler> list)
		{
			bool anyRemoved = false;
			// NOTE: We iterate in reverse order as we're removing elements from the list
			for (int i = list.Count - 1; i >= 0; i--)
			{
				FileWritingHandler handler = list[i];
				if (handler.IsFileReady())
				{
					list.RemoveAt(i);
					anyRemoved = true;
				}
			}
			return anyRemoved;
		}
	}
}