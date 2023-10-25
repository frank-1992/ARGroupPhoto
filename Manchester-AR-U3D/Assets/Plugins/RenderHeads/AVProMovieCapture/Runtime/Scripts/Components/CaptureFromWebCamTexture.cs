using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{
	/// <summary>
	/// Capture from a WebCamTexture object
	/// </summary>
	[AddComponentMenu("AVPro Movie Capture/Capture From WebCamTexture", 3)]
	public class CaptureFromWebCamTexture : CaptureFromTexture
	{
		private WebCamTexture _webcam = null;

		public WebCamTexture WebCamTexture
		{
			get { return _webcam; }
			set { _webcam = value; SetSourceTexture(_webcam); }
		}

		public override void UpdateFrame()
		{
			// WebCamTexture doesn't update every Unity frame
			if (_webcam != null && _webcam.didUpdateThisFrame)
			{
				UpdateSourceTexture();
			}

			base.UpdateFrame();
		}
	}
}