// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
	#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

// ReSharper disable once CheckNamespace
namespace Steamworks {
	public static class SteamUserStat {
		/// <summary>
		/// <para> Uploads a user score to the Steam back-end.</para>
		/// <para> This call is asynchronous, with the result returned in LeaderboardScoreUploaded_t</para>
		/// <para> Details are extra game-defined information regarding how the user got that score</para>
		/// <para> pScoreDetails points to an array of int32's, cScoreDetailsCount is the number of int32's in the list</para>
		/// </summary>
		public static SteamAPICall_t UploadLeaderboardScore(SteamLeaderboard_t hSteamLeaderboard, ELeaderboardUploadScoreMethod eLeaderboardUploadScoreMethod, int nScore, int[] pScoreDetails, int cScoreDetailsCount) {
			InteroHelp.TestIfAvailableClient();
			return (SteamAPICall_t)NativeMethod.ISteamUserStats_UploadLeaderboardScore(SteamAPIContext.GetSteamUserStats(), hSteamLeaderboard, eLeaderboardUploadScoreMethod, nScore, pScoreDetails, cScoreDetailsCount);
		}
    }
}

#endif // !DISABLESTEAMWORKS
