/* TheoraPlay# - C# Wrapper for TheoraPlay
 *
 * Copyright (c) 2013-2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */

using System;
using System.Runtime.InteropServices;

// TODO: THEORAPLAY_Io and its functions.

public static class TheoraPlay
{
	const string theoraplay_libname = "libtheoraplay.dll";

	public enum THEORAPLAY_VideoFormat
	{
		THEORAPLAY_VIDFMT_YV12,
		THEORAPLAY_VIDFMT_IYUV,
		THEORAPLAY_VIDFMT_RGB,
		THEORAPLAY_VIDFMT_RGBA
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct THEORAPLAY_VideoFrame32
	{
		[FieldOffset(0)]
			public uint playms;
		[FieldOffset(4)]
			public double fps;
		[FieldOffset(12)]
			public uint width;
		[FieldOffset(16)]
			public uint height;
		[FieldOffset(20)]
			public THEORAPLAY_VideoFormat format;
		[FieldOffset(24)]
			public IntPtr pixels;	// unsigned char*
		[FieldOffset(32)]
			public IntPtr next;	// struct THEORAPLAY_VideoFrame*
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct THEORAPLAY_VideoFrame
	{
		public uint playms;
		public double fps;
		public uint width;
		public uint height;
		public THEORAPLAY_VideoFormat format;
		public IntPtr pixels;	// unsigned char*
		public IntPtr next;	// struct THEORAPLAY_VideoFrame*
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct THEORAPLAY_AudioPacket
	{
		public uint playms;	// playback start time in milliseconds.
		public int channels;
		public int freq;
		public int frames;
		public IntPtr samples;	// float*; frames * channels float32 samples.
		public IntPtr next;	// struct THEORAPLAY_AudioPacket*
	}


	/* Note: The IntPtr return value is a THEORAPLAY_Decoder. */
	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr THEORAPLAY_startDecodeFile(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
			string fname,	// const char*
		uint maxframes,
		THEORAPLAY_VideoFormat vidfmt
	);

	/* Decoder State Functions
	 * The IntPtr parameter is a THEORAPLAY_Decoder*.
	 */

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern void THEORAPLAY_stopDecode(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern int THEORAPLAY_isDecoding(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern int THEORAPLAY_decodingError(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern int THEORAPLAY_isInitialized(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern int THEORAPLAY_hasVideoStream(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern int THEORAPLAY_hasAudioStream(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint THEORAPLAY_availableVideo(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint THEORAPLAY_availableAudio(IntPtr decoder);

	/* Audio Data Functions
	 * For these functions, IntPtr refers to a THEORAPLAY_AudioPacket*.
	 * The exception is the decoder, which is still a THEORAPLAY_Decoder*.
	 */

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr THEORAPLAY_getAudio(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern void THEORAPLAY_freeAudio(IntPtr item);

	/* Video Data Functions
	 * For these functions, IntPtr refers to a THEORAPLAY_VideoFrame*.
	 * The exception is the decoder, which is still a THEORAPLAY_Decoder*.
	 */

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr THEORAPLAY_getVideo(IntPtr decoder);

	[DllImport(theoraplay_libname, CallingConvention = CallingConvention.Cdecl)]
	public static extern void THEORAPLAY_freeVideo(IntPtr item);

	/* These are public methods that are NOT a part of TheoraPlay.
	 * We have this to prevent architecture incompatibility.
	 */

	public static unsafe THEORAPLAY_VideoFrame getVideoFrame(IntPtr frame)
	{
		THEORAPLAY_VideoFrame theFrame;
		unsafe
		{
			/* This is only a problem for Mono. Ignore for Win32 */
			if (	Environment.OSVersion.Platform != PlatformID.Win32NT &&
				IntPtr.Size == 4	)
			{
				THEORAPLAY_VideoFrame32* frame32Ptr = (THEORAPLAY_VideoFrame32*) frame;
				THEORAPLAY_VideoFrame32 frame32 = *frame32Ptr;
				theFrame.playms = frame32.playms;
				theFrame.fps = frame32.fps;
				theFrame.width = frame32.width;
				theFrame.height = frame32.height;
				theFrame.format = frame32.format;
				theFrame.pixels = frame32.pixels;
				theFrame.next = frame32.next;
			}
			else
			{
				THEORAPLAY_VideoFrame* framePtr = (THEORAPLAY_VideoFrame*) frame;
				theFrame = *framePtr;
			}
		}
		return theFrame;
	}

	public static unsafe THEORAPLAY_AudioPacket getAudioPacket(IntPtr packet)
	{
		THEORAPLAY_AudioPacket thePacket;
		unsafe
		{
			THEORAPLAY_AudioPacket* packetPtr = (THEORAPLAY_AudioPacket*) packet;
			thePacket = *packetPtr;
		}
		return thePacket;
	}

	public static float[] getSamples(IntPtr samples, int packetSize)
	{
		float[] theSamples = new float[packetSize];
		Marshal.Copy(samples, theSamples, 0, packetSize);
		return theSamples;
	}

	public static byte[] getPixels(IntPtr pixels, int imageSize)
	{
		byte[] thePixels = new byte[imageSize];
		Marshal.Copy(pixels, thePixels, 0, imageSize);
		return thePixels;
	}
}
