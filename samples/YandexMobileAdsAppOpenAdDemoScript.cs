using System;
using UnityEngine;
using UnityEngine.UI;
using YandexMobileAds;
using YandexMobileAds.Base;

public class YandexMobileAdsAppOpenAdDemoScript : MonoBehaviour
{
    private AppOpenAdLoader appOpenAdLoader;
    private AppOpenAd appOpenAd;
    private bool isColdStartAdShown = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SetupLoader();
        AppStateObserver.OnAppStateChanged += HandleAppStateChanged;
        RequestAppOpenAd();
    }

    private void Start()
    {
        ShowAppOpenAd();
    }

    private void OnDestroy()
    {
        AppStateObserver.OnAppStateChanged -= HandleAppStateChanged;
    }

    private void SetupLoader()
    {
        appOpenAdLoader = new AppOpenAdLoader();
        appOpenAdLoader.OnAdLoaded += HandleAdLoaded;
        appOpenAdLoader.OnAdFailedToLoad += HandleAdFailedToLoad;
    }

    private void HandleAppStateChanged(object sender, AppStateChangedEventArgs args)
    {
        if (!args.IsInBackground)
        {
            ShowAppOpenAd();
        }
    }

    private void ShowAppOpenAd()
    {
        if (appOpenAd != null)
        {
            appOpenAd.Show();
        }
    }

    private void RequestAppOpenAd()
    {
        string adUnitId = "R-M-18411280-4"; // replace with "R-M-XXXXXX-Y"
        AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(adUnitId).Build();
        appOpenAdLoader.LoadAd(adRequestConfiguration);
    }

    public void HandleAdLoaded(object sender, AppOpenAdLoadedEventArgs args)
    {
        // The ad was loaded successfully. Now you can handle it.
        appOpenAd = args.AppOpenAd;

        // Add events handlers for ad actions
        appOpenAd.OnAdClicked += HandleAdClicked;
        appOpenAd.OnAdShown += HandleAdShown;
        appOpenAd.OnAdFailedToShow += HandleAdFailedToShow;
        appOpenAd.OnAdDismissed += HandleAdDismissed;
        appOpenAd.OnAdImpression += HandleImpression;

        if (!isColdStartAdShown)
        {
            ShowAppOpenAd();
            isColdStartAdShown = true;
        }
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // Ad {args.AdUnitId} failed for to load with {args.Message}
        // Attempting to load a new ad from the OnAdFailedToLoad event is strongly discouraged.
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        // Called when ad is dismissed.

        // Clear resources after Ad dismissed.
        DestroyAppOpenAd();

        // Now you can preload the next ad.
        RequestAppOpenAd();
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        // Called when an ad failed to show.

        // Clear resources.
        DestroyAppOpenAd();

        // Now you can preload the next ad.
        RequestAppOpenAd();
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        // Called when a click is recorded for an ad.
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
        // Called when ad is shown.
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        // Called when an impression is recorded for an ad.
    }

    public void DestroyAppOpenAd()
    {
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }
    }
}