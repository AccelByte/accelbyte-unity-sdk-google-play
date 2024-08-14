# AccelByte Unity SDK Google Play Games #

Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
This is licensed software from AccelByte Inc, for limitations
and restrictions contact your company contract manager.

## Overview
Unity SDK Google Play Games is extension package to enable Accelbyte SDK support for Android Google Play Games.

## Prerequisiste ##
Require ([AccelByte Unity SDK](https://github.com/AccelByte/accelbyte-unity-sdk)) package. Minimum version: 16.24.0.

For more information about configuring AccelByte Unity SDK, see [Install and configure the SDK](https://docs.accelbyte.io/gaming-services/getting-started/setup-game-sdk/unity-sdk/#install-and-configure).

## How to Install ##
1. Clone this repository and install the package using UPM with "Add package from disk" option then select the package.json.
2. Install `GooglePlayGamesPlugin-0.10.14-AccelByte.unitypackage` inside `_Install` directory. These package is a fork build based on ([Play Games For Unity v10.14](https://github.com/playgameservices/play-games-plugin-for-unity/blob/v10.14))
3. Add assembly reference of `Assets/AccelByteExtensions/GooglePlayGames/com.AccelByte.GooglePlayGamesExtension` to your project.

## Features Usage ##

### Sign In with Google Play Games ###

We provide easier way to let player perfrom Sign in With Google Play Games platform. Therefore player doesn't need to register a new account to AGS to utilize the AGS features.

### Configure Your Game ###

To integrate Sign in With Google Play Games and AGS to your game, please follow the [official AGS documentation](https://docs.accelbyte.io/gaming-services/services/access/authentication/google-play-identity/#setup-googleplaygames-android-configuration).

### Code Implementation ###

1. Initialization
```
using AccelByte.Core;
using AccelByte.Models;

private string googlePlayGamesIdToken = "";

void Start()
{
    AccelByte.ThirdParties.GooglePlayGames.GooglePlayGamesExtension.Initialize();
}
```

2. Get Google Play Games Id Token
```
private void GetGooglePlayGamesIdToken()
{
    AccelByte.ThirdParties.GooglePlayGames.GooglePlayGamesExtension.SignIn()
    .OnSuccess((authCode) =>
    {
        googlePlayGamesIdToken = AccelByte.ThirdParties.GooglePlayGames.GooglePlayGamesExtension.GetIdToken();
        AccelByteDebug.Log("Obtain Google Play Games Id Token Success");
    })
    .OnFailed((error) =>
    {
        AccelByteDebug.LogWarning($"Obtain Google Play Games Id Token Failed: {error.Message}");
    });
}
```

3. Login to AGS
```
private void AGSLogin()
{
    if (!string.IsNullOrEmpty(googlePlayGamesIdToken))
    {
        AccelByteSDK.GetClientRegistry().GetApi().GetUser().LoginWithOtherPlatformV4(
            AccelByte.Models.PlatformType.GooglePlayGames
            , googlePlayGamesIdToken
            , result =>
        {
            if (result.IsError)
            {
                AccelByteDebug.LogError($"Failed to Login with Google Play Games Platfrom [{result.Error.error}]: {result.Error.error_description}");
                return;
            }
            AccelByteDebug.Log("Login with AccelByte IAM success");
        });
    }
}
```