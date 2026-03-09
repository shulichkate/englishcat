using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;
//using TMPro;

public class YandexMobileAdsInterstitialDemoScript : MonoBehaviour
{    

    // Replace demo Unit ID 'demo-interstitial-yandex' with actual Ad Unit ID
    [SerializeField] private string adUnitId = "R-M-5107956-2";

   // [SerializeField] private TMP_Text TextDebug;

    private InterstitialAdLoader interstitialAdLoader;
    private Interstitial interstitial;

    private String message = "";

    public void Awake()
    {
        this.interstitialAdLoader = new InterstitialAdLoader();
        this.interstitialAdLoader.OnAdLoaded += this.HandleAdLoaded;
        this.interstitialAdLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
    }

    public void OnGUI()
    {
#if UNITY_EDITOR
        this.message = "Mobile ads SDK is not available in editor. Only Android and iOS environments are supported";
#endif

    //    GUILayout.Label(this.message, labelStyle);
    }

    public void RequestInterstitial()
    {
        //Sets COPPA restriction for user age under 13
        MobileAds.SetAgeRestrictedUser(true);

        if (this.interstitial != null)
        {
            this.interstitial.Destroy();
        }

        this.interstitialAdLoader.LoadAd(this.CreateAdRequest(adUnitId));
        this.DisplayMessage("Interstitial is requested");
    }

    private void ShowInterstitial()
    {
        if (this.interstitial == null)
        {
            this.DisplayMessage("Interstitial is not ready yet");
            return;
        }

        this.interstitial.OnAdClicked += this.HandleAdClicked;
        this.interstitial.OnAdShown += this.HandleAdShown;
        this.interstitial.OnAdFailedToShow += this.HandleAdFailedToShow;
        this.interstitial.OnAdImpression += this.HandleImpression;
        this.interstitial.OnAdDismissed += this.HandleAdDismissed;

        this.interstitial.Show();
    }

    private AdRequestConfiguration CreateAdRequest(string adUnitId)
    {
        return new AdRequestConfiguration.Builder(adUnitId).Build();
    }

    private void DisplayMessage(String message)
    {
        this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
        MonoBehaviour.print(message);
        //TextDebug.text = this.message;
    }

    #region Interstitial callback handlers

    public void HandleAdLoaded(object sender, InterstitialAdLoadedEventArgs args)
    {       
        this.interstitial = args.Interstitial;

        ShowInterstitial();

        this.DisplayMessage("HandleAdLoaded event received");
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        this.DisplayMessage($"HandleAdFailedToLoad event received with message: {args.Message}");
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

        this.interstitial.Destroy();
        this.interstitial = null;
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
        this.DisplayMessage($"HandleImpression event received with data: {data}");
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        this.DisplayMessage($"HandleAdFailedToShow event received with message: {args.Message}");
    }

    #endregion
}
