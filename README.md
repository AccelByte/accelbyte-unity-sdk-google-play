# AccelByte Unity SDK Google Play Games

Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
This is licensed software from AccelByte Inc, for limitations
and restrictions contact your company contract manager.

# Overview
Unity SDK Google Play Games is extension package to enable Accelbyte SDK support for Android Google Play Games.
> In present state, our service only able to support GooglePlayGamesPlugin-0.10.14 (Play Game Services V1). Support for Play Game Services V2 is under development.

## Prerequisites
Require ([AccelByte Unity SDK](https://github.com/AccelByte/accelbyte-unity-sdk)) package. Minimum version: 16.24.0.

For more information about configuring AccelByte Unity SDK, see [Install and configure the SDK](https://docs.accelbyte.io/gaming-services/getting-started/setup-game-sdk/unity-sdk/#install-and-configure).

## How to Install
1. In your Unity project, go to `Window > Package Manager`.
2. Click the + icon on the Package Manager window and click `Add package from git URL...`
3. Paste the following link into the URL field and click Add: `https://github.com/AccelByte/accelbyte-unity-sdk-google-play.git`
4. Install `GooglePlayGamesPlugin-0.10.14-AccelByte.unitypackage` inside `_Install` directory. These package is a fork build based on ([Play Games For Unity v10.14](https://github.com/playgameservices/play-games-plugin-for-unity/blob/v10.14)). Play Games For Unity v11.01 is unable to support external parties integration
5. Add assembly reference of `Assets/AccelByteExtensions/GooglePlayGames/com.AccelByte.GooglePlayGamesExtension` to your project.

# Features Usage

## Sign In with Google Play Games

We provide easier way to let player perfrom Sign in With Google Play Games platform. Therefore player doesn't need to register a new account to AGS to utilize the AGS features.

### Code Implementation

1. Header Initialization
```csharp
using AccelByte.Core;
using AccelByte.Models;
```

2. Get Google Play Games Id Token
```csharp
private string googlePlayGamesIdToken = "";

void Start()
{
    AccelByte.ThirdParties.GooglePlayGames.GooglePlayGamesExtension.Initialize();
}

private void GetGooglePlayGamesIdToken()
{
    AccelByte.ThirdParties.GooglePlayGames.GooglePlayGamesExtension.SignIn()
    .OnSuccess((authCode) =>
    {
        googlePlayGamesIdToken = AccelByte.ThirdParties.GooglePlayGames.GooglePlayGamesExtension.GetIdToken();
        UnityEngine.Debug.Log("Obtain Google Play Games Id Token Success");
    })
    .OnFailed((error) =>
    {
        UnityEngine.Debug.LogWarning($"Obtain Google Play Games Id Token Failed: {error.Message}");
    });
}
```

3. Login to AGS
```csharp
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
                UnityEngine.Debug.LogError($"Failed to Login with Google Play Games Platfrom [{result.Error.error}]: {result.Error.error_description}");
                return;
            }
            UnityEngine.Debug.Log("Login with AccelByte IAM success");
        });
    }
}
```

## In-App Purchasing

### Configure Your Game

> Please contact AccelByte support for guideline document

### Prerequisites

1. Import package [Unity In App Purchasing (IAP)](https://docs.unity3d.com/Packages/com.unity.purchasing@4.11/manual/index.html) library to the project. We recommend you to use IAP v4.11. In present state, The SDK has compatibility issue with IAP v4.12 and newer.
2. Please refers to official [Unity documentation](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html) on how to install it.

### Code Implementation

1. Sign in With Google Play Games, please refer to [previous part](https://github.com/AccelByte/accelbyte-unity-sdk-google-play?tab=readme-ov-file#sign-in-with-google-play-games)

2. Please create `MonoBehavior` class implementing `IDetailedStoreListener`. Unity IAP will handle the purchase and trigger callbacks using this interface. Then prepare the following variables
```csharp
public Button buyButton;
    
IStoreController storeController;
private string productId = "item_gold"; // assume that the registered product id is named Item_gold
private ProductType productType = ProductType.Consumable; // assume that "item_gold" is a Consumables
Product purchasedProduct;
```

3. Prepare a [Button](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-Button.html) to trigger the purchasing event. Using Unity Editor's inspector, attach this button into `public Button buyButton;`

4. Prepare a function that will be trigger the purchasing event
```csharp
private void BuyGold()
{
    storeController.InitiatePurchase(productId);
}
```

5. Initialize Purchasing
```csharp
void Start()
{
    InitializePurchasing();
}

void InitializePurchasing()
{
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

    builder.AddProduct(productId, productType);
    UnityPurchasing.Initialize(this, builder);

    buyButton.onClick.AddListener(BuyGold);
}

public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
{
    Debug.Log("In-App Purchasing successfully initialized");
    storeController = controller;
}

public void OnInitializeFailed(InitializationFailureReason error, string message)
{
    var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

    if (message != null)
    {
        errorMessage += $" More details: {message}";
    }

    UnityEngine.Debug.Log(errorMessage);
}
```

6. Handle Process Purchase. Please note that it **must** return `PurchaseProcessingResult.Pending` because purchased item will be synchronized with AccelByte's Backend. [reference](https://docs.unity3d.com/2021.3/Documentation/Manual/UnityIAPProcessingPurchases.html). If client successfully purchase item from Google Play Store, `ProcessPurchase` will be triggered, else `OnPurchaseFailed` will be triggered
```csharp
public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
{
    //Retrieve the purchased product
    purchasedProduct = args.purchasedProduct;
    UnityEngine.Debug.Log($"Purchase Complete - Product: {purchasedProduct.definition.id}");
    string receiptPayload = JObject.Parse(purchasedProduct.receipt)["Payload"].ToString();
    var receiptJson = JObject.Parse(receiptPayload)["json"].ToString();
    receiptObject = JObject.Parse(receiptJson);
    var orderId = ((string)receiptObject["orderId"]);
    var packageName = ((string)receiptObject["packageName"]);
    var productId = ((string)receiptObject["productId"]);
    var purchaseTime = ((long)receiptObject["purchaseTime"]);
    var purchaseToken = ((string)receiptObject["purchaseToken"]);
    var autoAck = false; // set autoAck as true if it is durable product or item
    AGSEntitlementSync(orderId, packageName, productId, purchaseTime, purchaseToken, autoAck);
    // mark the process as pending, confirm the pending purchase after sync with AGS
    return PurchaseProcessingResult.Pending;
}

public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
{
    UnityEngine.Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
        $" Purchase failure reason: {failureDescription.reason}," +
        $" Purchase failure details: {failureDescription.message}");
}
```

7. Sync Purchased Product with AGS
```csharp
private void AGSEntitlementSync(string orderId
        , string packageName
        , string productId
        , long purchaseTime
        , string purchaseToken
        , bool autoAck)
    {
        // Please note that Sync will work after the player is logged in using AB service
        // Please refer to https://github.com/AccelByte/accelbyte-unity-sdk-google-play?tab=readme-ov-file#sign-in-with-google-play-games for implementation
        try
        {
            AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement()
            .SyncMobilePlatformPurchaseGoogle(
                orderId
                , packageName
                , productId
                , purchaseTime
                , purchaseToken
                , autoAck
                , syncResult =>
                {
                    if (syncResult.IsError)
                    {
                        AccelByteDebug.Log(syncResult.Error.Message);
                        return;
                    }

                    if (syncResult.Value.NeedConsume)
                    {
                        FinalizePurchase(purchasedProduct);
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to sync with AB {e.Message}");
        }
    }

```

8. Finalize Pending Purchase
```csharp
private void FinalizePurchase(Product purchasedProduct)
{
    Debug.Log($"Confirm Pending Purchase for: {purchasedProduct.definition.id}");
    storeController.ConfirmPendingPurchase(purchasedProduct);
}
```

This is the complete script:
```csharp
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using AccelByte.Core;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System;

public class NewBehaviourScript : MonoBehaviour, IDetailedStoreListener
{
    public Button buyButton;
    IStoreController storeController; // The Unity Purchasing system.
    private string productId = "your_product_id";
    private ProductType productType = ProductType.Consumable;
    Product purchasedProduct;
    JObject receiptObject;

    void Start()
    {
        InitializePurchasing();
    }

    /// <summary>
    /// Trigger purchasing initialization
    /// </summary>
    void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(productId, productType);
        UnityPurchasing.Initialize(this, builder);

        buyButton.onClick.AddListener(BuyGold);
    }

    /// <summary>
    /// A callback that will be triggered when the Initialization step is done
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error) { }

    /// <summary>
    /// A callback will be triggered when the Initialization step is failed, with detailed message
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        if (message != null)
        {
            errorMessage += $" More details: {message}";
        }

        UnityEngine.Debug.Log(errorMessage);
    }

    /// <summary>
    /// Confirm the pending purchase after sync with AB is done
    /// It is required because there is a synchronization step
    /// </summary>
    private void FinalizePurchase(Product purchasedProduct)
    {
        Debug.Log($"Confirm Pending Purchase for: {purchasedProduct.definition.id}");
        storeController.ConfirmPendingPurchase(purchasedProduct);
    }

    /// <summary>
    /// This function will trigger the purchasing event
    /// </summary>
    private void BuyGold()
    {
        storeController.InitiatePurchase(productId);
    }

    /// <summary>
    /// A callback will be triggered when the purchasing is success
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        UnityEngine.Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
            $" Purchase failure reason: {failureDescription.reason}," +
            $" Purchase failure details: {failureDescription.message}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }

    /// <summary>
    /// A callback will be triggered when the purchasing is success
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        purchasedProduct = args.purchasedProduct;

        UnityEngine.Debug.Log($"Purchase Complete - Product: {purchasedProduct.definition.id}");

        string receiptPayload = JObject.Parse(purchasedProduct.receipt)["Payload"].ToString();
        var receiptJson = JObject.Parse(receiptPayload)["json"].ToString();
        var receiptObject = JObject.Parse(receiptJson);

        var orderId = ((string)receiptObject["orderId"]);
        var packageName = ((string)receiptObject["packageName"]);
        var productId = ((string)receiptObject["productId"]);
        var purchaseTime = ((long)receiptObject["purchaseTime"]);
        var purchaseToken = ((string)receiptObject["purchaseToken"]);
        var autoAck = false; // set autoAck as true if it is durable product or item

        AGSEntitlementSync(orderId, packageName, productId, purchaseTime, purchaseToken, autoAck);

        // mark the process as pending, confirm the pending purchase after sync with AGS
        return PurchaseProcessingResult.Pending;
    }

    /// <summary>
    /// Synchronize the purchased product with AccelByte's server using AccelByte's SDK
    /// </summary>
    private void AGSEntitlementSync(string orderId
        , string packageName
        , string productId
        , long purchaseTime
        , string purchaseToken
        , bool autoAck)
    {
        // Please note that Sync will work after the player is logged in using AB service
        // Please refer to https://github.com/AccelByte/accelbyte-unity-sdk-google-play?tab=readme-ov-file#sign-in-with-google-play-games for implementation
        try
        {
            AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement()
            .SyncMobilePlatformPurchaseGoogle(
                orderId
                , packageName
                , productId
                , purchaseTime
                , purchaseToken
                , autoAck
                , syncResult =>
                {
                    if (syncResult.IsError)
                    {
                        AccelByteDebug.Log(syncResult.Error.Message);
                        return;
                    }

                    if (syncResult.Value.NeedConsume)
                    {
                        FinalizePurchase(purchasedProduct);
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to sync with AB {e.Message}");
        }
    }
}

```