// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.
using AccelByte.Core;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace AccelByte.ThirdParties.GooglePlayGames
{
    [Preserve]
    internal static class AccelByteGooglePlayGamesBootstrap
    {
        private static AccelByteGooglePlayGamesImp imp;

        [Preserve, RuntimeInitializeOnLoadMethod]
        private static void StartAccelByteSDK()
        {
            AttachImp();
        }

        private static void AttachImp()
        {
            AccelByteGooglePlayGames.ImpGetter = () =>
            {
                if (imp == null)
                {
                    imp = new AccelByteGooglePlayGamesImp();
                }
                return imp;
            };
        }
    }
}