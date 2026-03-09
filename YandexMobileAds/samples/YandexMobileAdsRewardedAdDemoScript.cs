using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;
//using TMPro;

public class YandexMobileAdsRewardedAdDemoScript : MonoBehaviour
{
    // Replace demo Unit ID 'demo-rewarded-yandex' with actual Ad Unit ID
    [SerializeField] private string adUnitId = "R-M-5107956-4";
    [SerializeField] private int AmountReward;

   // [SerializeField] private TMP_Text TextDebug;

    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;

    private String message = "";

    public void Awake()
    {
        this.rewardedAdLoader = new RewardedAdLoader();
        this.rewardedAdLoader.OnAdLoaded += this.HandleAdLoaded;
        this.rewardedAdLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
    }

    public void OnGUI()
    {
#if UNITY_EDITOR
        this.message = "Mobile ads SDK is not available in editor. Only Android and iOS environments are supported";

#endif

    //    GUILayout.Label(this.message, labelStyle);
    }

    public void RequestRewardedAd()
    {
        this.DisplayMessage("RewardedAd is not ready yet");
        //Sets COPPA restriction for user age under 13
        MobileAds.SetAgeRestrictedUser(true);

        if (this.rewardedAd != null)
        {
            this.rewardedAd.Destroy();
        }

        this.rewardedAdLoader.LoadAd(this.CreateAdRequest(adUnitId));
        this.DisplayMessage("Rewarded Ad is requested");
    }

    private void ShowRewardedAd()
    {
        if (this.rewardedAd == null)
        {
            this.DisplayMessage("RewardedAd is not ready yet");
            return;
        }

        this.rewardedAd.OnAdClicked += this.HandleAdClicked;
        this.rewardedAd.OnAdShown += this.HandleAdShown;
        this.rewardedAd.OnAdFailedToShow += this.HandleAdFailedToShow;
        this.rewardedAd.OnAdImpression += this.HandleImpression;
        this.rewardedAd.OnAdDismissed += this.HandleAdDismissed;
        this.rewardedAd.OnRewarded += this.HandleRewarded;

        this.rewardedAd.Show();
    }

    private AdRequestConfiguration CreateAdRequest(string adUnitId)
    {
        return new AdRequestConfiguration.Builder(adUnitId).Build();
    }

    private void DisplayMessage(String message)
    {
        this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
        MonoBehaviour.print(message);
   //     TextDebug.text = this.message;
    }

    #region Rewarded Ad callback handlers

    public void HandleAdLoaded(object sender, RewardedAdLoadedEventArgs args)
    {        
        this.rewardedAd = args.RewardedAd;

        ShowRewardedAd();

        this.DisplayMessage("HandleAdLoaded event received");
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        this.DisplayMessage(
            $"HandleAdFailedToLoad event received with message: {args.Message}");
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdClicked event received");
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdShown event received");
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdDismissed event received");

        this.rewardedAd.Destroy();
        this.rewardedAd = null;
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
        this.DisplayMessage($"HandleImpression event received with data: {data}");
    }

    public void HandleRewarded(object sender, Reward args)
    {       
        //AddGold.AppendGold(AmountReward);

        this.DisplayMessage($"HandleRewarded event received\nAdd {AmountReward} Gold");
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        this.DisplayMessage(
            $"HandleAdFailedToShow event received with message: {args.Message}");
    }

    #endregion
}
