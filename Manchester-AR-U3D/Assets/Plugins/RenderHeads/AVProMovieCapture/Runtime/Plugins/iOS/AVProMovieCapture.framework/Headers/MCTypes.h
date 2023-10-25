//
//  MCTypes.h
//  MovieCapture
//
//  Created by Morris Butler on 30/11/2020.
//  Copyright © 2020 RenderHeads. All rights reserved.
//

@import Foundation;

//
typedef struct __attribute__((packed)) VideoEncoderHints {
	uint32_t averageBitrate;
	uint32_t maximumBitrate;		// Unsupported
	float    quality;
	uint32_t keyframeInterval;
	bool	 allowFastStartStreamingPostProcess;
	bool	 supportTransparency;
	bool     useHardwareEncoding;	// Unsupported
} VideoEncoderHints;

//
typedef struct __attribute__((packed)) ImageEncoderHints {
	float quality;
	bool  supportTransparency;
} ImageEncoderHints;

// MARK: Ambisonics

typedef void *MCAmbisonicSourceRef;

typedef NS_ENUM(int, MCAmbisonicOrder)
{
	MCAmbisonicOrderFirst,
	MCAmbisonicOrderSecond,
	MCAmbisonicOrderThird
};

typedef NS_ENUM(int, MCAmbisonicChannelOrder)
{
	MCAmbisonicChannelOrderFuMa,
	MCAmbisonicChannelOrderACN
};
