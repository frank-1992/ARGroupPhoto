//
//  MovieCapture.h
//  MovieCapture
//
//  Created by Morris Butler on 13/05/2019.
//  Copyright © 2019 RenderHeads. All rights reserved.
//

@import Foundation;

#ifdef __cplusplus
#define MC_EXTERN extern "C"
#else
#define MC_EXTERN
#endif

#ifndef MC_EXPORT
#define MC_EXPORT
#endif

// Unity
struct IUnityInterfaces;
MC_EXTERN void  MC_EXPORT UnityPluginLoad(struct IUnityInterfaces *unityInterfaces);
MC_EXTERN void  MC_EXPORT UnityPluginUnload(void);

// AVProMovieCapture
#include "MCTypes.h"

// MARK: -

MC_EXTERN void *MC_EXPORT GetRenderEventFunc(void);
MC_EXTERN void *MC_EXPORT GetFreeResourcesEventFunc(void);
MC_EXTERN void  MC_EXPORT RegisterPlugin(void);
MC_EXTERN bool  MC_EXPORT Init(void);
MC_EXTERN void  MC_EXPORT Deinit(void);
MC_EXTERN void  MC_EXPORT SetMicrophoneRecordingHint(bool enabled);
MC_EXTERN bool  MC_EXPORT IsTrialVersion(void);
MC_EXTERN int   MC_EXPORT GetVideoCodecMediaApi(int index);
MC_EXTERN int   MC_EXPORT GetVideoCodecCount(void);
MC_EXTERN bool  MC_EXPORT IsConfigureVideoCodecSupported(int index);
MC_EXTERN void  MC_EXPORT ConfigureVideoCodec(int index);
MC_EXTERN int   MC_EXPORT GetAudioCodecMediaApi(int index);
MC_EXTERN int   MC_EXPORT GetAudioCodecCount(void);
MC_EXTERN bool  MC_EXPORT IsConfigureAudioCodecSupported(int index);
MC_EXTERN void  MC_EXPORT ConfigureAudioCodec(int index);
MC_EXTERN int   MC_EXPORT GetAudioInputDeviceCount(void);
MC_EXTERN int   MC_EXPORT CreateRecorderVideo(const unichar *filename, uint width, uint height, float frameRate, int format, bool isRealTime, bool isTopDown, int videoCodecIndex, int audioSource, int audioSampleRate, int audioChannelCount, int audioInputDeviceIndex, int audioCodecIndex, bool forceGpuFlush, VideoEncoderHints *hints);
MC_EXTERN int   MC_EXPORT CreateRecorderImages(const unichar *filename, uint width, uint height, float frameRate, int format, bool isRealTime, bool isTopDown, int imageFormatType, bool forceGpuFlush, int startFrame, ImageEncoderHints *hints);

MC_EXTERN int   MC_EXPORT CreateRecorderPipe(const unichar *filename, uint width, uint height, float frameRate, int format, bool isTopDown, bool supportAlpha, bool forceGpuFlush);
MC_EXTERN bool  MC_EXPORT Start(int handle);
MC_EXTERN bool  MC_EXPORT IsNewFrameDue(int handle);
MC_EXTERN void  MC_EXPORT EncodeFrame(int handle, void *data);
MC_EXTERN void  MC_EXPORT EncodeAudio(int handle, void *data, uint length);
MC_EXTERN void  MC_EXPORT EncodeFrameWithAudio(int handle, void *videoData, void *audioData, uint audioLength);
MC_EXTERN void  MC_EXPORT Pause(int handle);
MC_EXTERN void  MC_EXPORT Stop(int handle, bool skipPendingFrames);
MC_EXTERN bool  MC_EXPORT IsFileWritingComplete(int handle);
MC_EXTERN void  MC_EXPORT SetTexturePointer(int handle, void *texture);
MC_EXTERN void  MC_EXPORT FreeRecorder(int handle);
MC_EXTERN uint  MC_EXPORT GetNumDroppedFrames(int handle);
MC_EXTERN uint  MC_EXPORT GetNumDroppedEncoderFrames(int handle);
MC_EXTERN uint  MC_EXPORT GetNumEncodedFrames(int handle);
MC_EXTERN uint  MC_EXPORT GetEncodedSeconds(int handle);
MC_EXTERN uint  MC_EXPORT GetFileSize(int handle);
MC_EXTERN void *MC_EXPORT GetPluginVersion(void);
MC_EXTERN bool  MC_EXPORT GetVideoCodecName(int index, unichar *name, int nameBufferLength);
MC_EXTERN bool  MC_EXPORT GetAudioCodecName(int index, unichar *name, int nameBufferLength);
MC_EXTERN bool  MC_EXPORT GetAudioInputDeviceName(int index, unichar *name, int nameBufferLength);
MC_EXTERN bool  MC_EXPORT GetContainerFileExtensions(int videoCodecIndex, int audioCodecIndex, unichar *extensions, int extensionsLength);
MC_EXTERN void  MC_EXPORT SetLogFunction(void *logFunction);
MC_EXTERN void  MC_EXPORT SetErrorHandler(int index, void *errorHandler);

// Ambisonic support
MC_EXTERN MCAmbisonicSourceRef MC_EXPORT AddAmbisonicSource(int maxCoefficients);
MC_EXTERN void  MC_EXPORT RemoveAmbisonicSource(MCAmbisonicSourceRef source);
MC_EXTERN void  MC_EXPORT UpdateAmbisonicSourceWeights(MCAmbisonicSourceRef source,
													   float azimuth,
													   float elevation,
													   MCAmbisonicOrder ambisonicOrder,
													   MCAmbisonicChannelOrder channelOrder,
													   float *weights);
MC_EXTERN void  MC_EXPORT EncodeMonoToAmbisonicSource(MCAmbisonicSourceRef source,
													  float *inSamples, int inOffset, int inCount,
													  int numChannels,
													  void *outSamples, int outOffset, int outCount,
													  MCAmbisonicOrder ambisonicOrder);
