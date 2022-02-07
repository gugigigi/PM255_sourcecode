using System;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.AddressableAssets;
using Arbor;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    private RewardedAd rewardedAd;

    public Text messageText;
    int count = 0;
    bool isRewarded;

    private System.Collections.Generic.List<ItemInstance> Inventory;
    private string InstanceID;
    private string InstanceID_Half;
    // Use this for initialization
    void Start()
    {
#if UNITY_ANDROID
        string appId = "ca-app-pub-3582945866871283~8144577195";
#elif UNITY_IPHONE
        string appId = "ca-app-pub-3582945866871283~9843022199";
#else
        string appId = "unexpected_platform";
#endif
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });
    }
    /*--------------------------------------------------------------------------------------------------------------
        スタミナアイテムの残数を取得
    --------------------------------------------------------------------------------------------------------------*/
    public void StaminaSettings()
    {
        var Inventory = GameObject.FindGameObjectWithTag("DataSlot").GetComponent<SaveDataManager>().GetInventory();

        GameObject ItemRecharge_Half = GameObject.Find("RechargeIcon_Half");
        GameObject ItemRecharge = GameObject.Find("RechargeIcon_Full");

        TextMeshProUGUI ItemCount = GameObject.Find("XCount_Full").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ItemCount_Half = GameObject.Find("XCount_Half").GetComponent<TextMeshProUGUI>();

        if (Inventory.Exists(i => i.ItemId == "RechargeItem_01"))
        {
            var item = Inventory.Find(i => i.ItemId == "RechargeItem_01");
            InstanceID = item.ItemInstanceId;
            ItemCount.text = "x" + item.RemainingUses.Value.ToString();
        }
        else ItemRecharge.SetActive(false);

        if (Inventory.Exists(i => i.ItemId == "RechargeItem_02"))
        {
            var item = Inventory.Find(i => i.ItemId == "RechargeItem_02");
            InstanceID_Half = item.ItemInstanceId;
            ItemCount_Half.text = "x" + item.RemainingUses.Value.ToString();
        }
        else ItemRecharge_Half.SetActive(false);

    }
    /*--------------------------------------------------------------------------------------------------------------
        広告リワードの確認
    --------------------------------------------------------------------------------------------------------------*/
    public void Reward_Ad_Check()
    {
        var Data = GameObject.FindGameObjectWithTag("DataSlot").GetComponent<SaveDataManager>().GetUserData();
        var scene = GameObject.Find("SceneScan").GetComponent<Arbor.ArborFSM>();
        if (Data.TryGetValue("AdBool_Charge", out var AdBool_Charge) == true && Boolean.Parse(AdBool_Charge.Value) == false)
        {
            scene.SendTrigger("Check");
        }
        else { scene.SendTrigger("False"); }

    }
    public void Reward_Ad()
    {
        var updateDataDict = new Dictionary<string, string>() {
            {"AdBool_Charge","true"},};

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = updateDataDict }, resultCallback =>
        {
            Debug.Log(resultCallback);
            RequestRewardAd_Charge();
        }, errorCallback => { Debug.LogError(errorCallback); });

    }
    /*--------------------------------------------------------------------------------------------------------------
        ゲームオーバー時の広告リワードの確認
    --------------------------------------------------------------------------------------------------------------*/
    public void GameOver_Ad()
    {
        var Data = GameObject.FindGameObjectWithTag("DataSlot").GetComponent<SaveDataManager>().GetUserData();
        int Record = 0;
        if (Data.TryGetValue("AdBool_Failed", out var AdBool_Failed) == true && Boolean.Parse(AdBool_Failed.Value) == false)
        {
            var updateDataDict = new Dictionary<string, string>() {
            {"AdBool_Failed","true"},};

            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = updateDataDict }, resultCallback =>
            {
                Debug.Log(resultCallback);
                RequestRewardAd();
            }, errorCallback => { Debug.LogError(errorCallback); });
        }
    }
    /*--------------------------------------------------------------------------------------------------------------
        広告リワードのリクエスト
        各ユニットに応じてUnitIDを指定します
    --------------------------------------------------------------------------------------------------------------*/
    public void RequestRewardAd()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3582945866871283/9074515486";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3582945866871283/7029156595";
#else
        string adUnitId = "unexpected_platform";
#endif
        this.rewardedAd = new RewardedAd(adUnitId);
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(request);
    }
    public void RequestRewardAd_Charge()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3582945866871283/9074515486";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3582945866871283/7029156595";
#else
        string adUnitId = "unexpected_platform";
#endif
        this.rewardedAd = new RewardedAd(adUnitId);

        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward_Charge;
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(request);

    }
    public void RequestRewardAd_Watch()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3582945866871283/9074515486";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3582945866871283/7029156595";
#else
        string adUnitId = "unexpected_platform";
#endif
        this.rewardedAd = new RewardedAd(adUnitId);
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward_Stamina;

        AdRequest request = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(request);

    }
    public void RechargeItem()
    {
        Debug.Log("RechargeItem_Full");

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "ItemRecharge",
            FunctionParameter = new { inputValue = InstanceID },
            GeneratePlayStreamEvent = true,
        }, Success, Error);
    }
    public void RechargeItem02()
    {
        Debug.Log("RechargeItem_Half");

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "ItemRecharge_Half",
            FunctionParameter = new { inputValue = InstanceID_Half },
            GeneratePlayStreamEvent = true,
        }, Success, Error);

    }
    private static void Success(ExecuteCloudScriptResult result)
    {
        GameObject.Find("GameStartSettings").GetComponent<ArborFSM>().SendTrigger("Reload");
    }

    private static void Error(PlayFabError error)
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().ErrorDialog(error);
        Debug.LogError(error.ErrorDetails);

    }

    public void ShowRewardResult()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AdmobRecharge",
            GeneratePlayStreamEvent = true,
        }, Success, Error);
    }

    /*--------------------------------------------------------------------------------------------------------------
        広告リワードのロード
    --------------------------------------------------------------------------------------------------------------*/
    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdLoaded event received");
        if (this.rewardedAd.IsLoaded())
        {
            isRewarded = false;
            this.rewardedAd.Show();
        }
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        Debug.LogError(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);

    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.Log(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdClosed event received");
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("HandleRewardedAdRewarded event received for " + amount.ToString() + " " + type);
    }
    public void HandleUserEarnedReward_Stamina(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("HandleRewardedAdRewarded event received for " + amount.ToString() + " " + type);
        ShowRewardResult();

    }
    public void HandleUserEarnedReward_Charge(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("HandleRewardedAdRewarded event received for " + amount.ToString() + " " + type);
        PurchaseReward();
    }
    void PurchaseReward()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AdmobReward",
            GeneratePlayStreamEvent = true,
        }, resultCallback => { Debug.Log(resultCallback); GameObject.Find("SceneScan").GetComponent<ArborFSM>().SendTrigger("Reload"); },
        errorCallback => { Debug.LogError(errorCallback); StartCoroutine(_waitConnect()); });
    }

    IEnumerator _waitConnect()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);

            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                PurchaseReward();
                break;
            }
        }
    }
}



