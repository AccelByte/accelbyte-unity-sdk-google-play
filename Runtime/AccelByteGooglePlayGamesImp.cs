// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.
using AccelByte.Core;
using System;

namespace AccelByte.ThirdParties.GooglePlayGames
{
    public class AccelByteGooglePlayGamesImp : IGooglePlayGamesImp
    {
        public AccelByteGooglePlayGamesImp()
        {
            GooglePlayGamesExtension.Initialize();
        }

        public Models.AccelByteResult<SignInGooglePlayGamesResult, Core.Error> GetGooglePlayGamesSignInToken()
        {
            var retval = new Models.AccelByteResult<SignInGooglePlayGamesResult, Core.Error>();
            
            GooglePlayGamesExtension.SignIn()
                .OnSuccess((authCode) =>
                {
                    var result = new SignInGooglePlayGamesResult()
                    {
                        AuthCode = authCode,
                        IdToken = GooglePlayGamesExtension.GetIdToken()
                    };
                    retval.Resolve(result);
                })
                .OnFailed((error) =>
                {
                    retval.Reject(error);
                });

            return retval;
        }
    }
}
