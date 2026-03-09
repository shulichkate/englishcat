
/*
using UnityEngine;
using TMPro;
using System;

public class LifeTimerManager : MonoBehaviour
{
    // === ÒÀÉÌÅÐ ÆÈÇÍÅÉ ===
    private bool isLifeTimerActive = false;
    private const float LIFE_TIMER_DURATION = 300f; // 5 ìèíóò
    private DateTime lastLifeUpdateTime;
    private float remainingTime = 300f;

    // === ÒÀÉÌÅÐ ÐÅÊËÀÌÛ ===
    private bool isAdTimerActive = false;
    private float adTimerRemaining = 0f;
    private const float AD_INTERVAL = 200f; // 3 ìèíóòû

    // === ÑÈÑÒÅÌÀ ÁÅÑÊÎÍÅ×ÍÛÕ ÆÈÇÍÅÉ ===
    private bool isUnlimitedLivesActive = false;
    private DateTime unlimitedLivesEndTime;
    private int unlimitedHours = 0;

    public bool IsUnlimitedLivesActive => isUnlimitedLivesActive;

    void Start()
    {
        StartLifeTimer();
    }

    public void UpdateTimer(SaverTest saver)
    {
        if (isUnlimitedLivesActive)
        {
            if (DateTime.Now >= unlimitedLivesEndTime)
            {
                DeactivateUnlimitedLives(saver);
            }
            else
            {
                UpdateUnlimitedLivesUI(saver.lifeTimerText);
            }
            return;
        }

        if (isLifeTimerActive && saver.lives < 5)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                AddLifeFromTimer(saver);
            }

            UpdateLifeTimerUI(saver.lifeTimerText, saver.lives);
        }
        else if (saver.lives >= 5)
        {
            if (saver.lifeTimerText != null)
                saver.lifeTimerText.text = "MAX";
            isLifeTimerActive = false;
        }

        if (isAdTimerActive && adTimerRemaining > 0)
        {
            adTimerRemaining -= Time.deltaTime;
        }
    }

    void StartLifeTimer()
    {
        isLifeTimerActive = true;
        // LoadLifeTimerData áóäåò âûçâàí èç OnApplicationPause
    }

    // ÈÑÏÐÀÂËÅÍÍÛÉ ÌÅÒÎÄ - ÄÎÁÀÂËÅÍ ÏÀÐÀÌÅÒÐ SaverTest
    public void LoadLifeTimerData(ref int lives, ref TextMeshProUGUI lifeTimerText, SaverTest saver)
    {
        string lastUpdateStr = PlayerPrefs.GetString("lastLifeUpdateTime", "");

        if (!string.IsNullOrEmpty(lastUpdateStr))
        {
            if (DateTime.TryParse(lastUpdateStr, out DateTime savedTime))
            {
                lastLifeUpdateTime = savedTime;
                TimeSpan timePassed = DateTime.Now - lastLifeUpdateTime;
                float secondsPassed = (float)timePassed.TotalSeconds;

                lives = PlayerPrefs.GetInt("lives", 5);
                remainingTime = PlayerPrefs.GetFloat("remainingTime", LIFE_TIMER_DURATION);

                if (secondsPassed > 0)
                {
                    remainingTime -= secondsPassed;

                    while (remainingTime <= 0 && lives < 5)
                    {
                        lives++;
                        remainingTime += LIFE_TIMER_DURATION;
                    }

                    lastLifeUpdateTime = DateTime.Now;
                }
            }
            else
            {
                lastLifeUpdateTime = DateTime.Now;
                remainingTime = LIFE_TIMER_DURATION;
                lives = PlayerPrefs.GetInt("lives", 5);
            }
        }
        else
        {
            lastLifeUpdateTime = DateTime.Now;
            remainingTime = LIFE_TIMER_DURATION;
            lives = PlayerPrefs.GetInt("lives", 5);
        }

        if (lives > 5) lives = 5;
        SaveLifeTimerData(lives);

        // ÈÑÏÐÀÂËÅÍÎ: èñïîëüçóåì ïóáëè÷íûå ìåòîäû
        if (saver != null)
        {
            saver.lives = lives; // Ïðÿìîå ïðèñâàèâàíèå, òàê êàê lives - public
            saver.UpdateMainScreen();
        }
    }

    public void SaveLifeTimerData(int lives)
    {
        PlayerPrefs.SetInt("lives", lives);
        PlayerPrefs.SetFloat("remainingTime", remainingTime);
        PlayerPrefs.SetString("lastLifeUpdateTime", lastLifeUpdateTime.ToString("o"));
        PlayerPrefs.Save();
    }

    void UpdateLifeTimerUI(TextMeshProUGUI lifeTimerText, int lives)
    {
        if (lifeTimerText != null && !isUnlimitedLivesActive)
        {
            if (lives >= 5)
            {
                lifeTimerText.text = "MAX";
                isLifeTimerActive = false;
            }
            else
            {
                isLifeTimerActive = true;
                if (remainingTime < 0) remainingTime = 0;
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                lifeTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                lifeTimerText.color = Color.white;
            }
        }
    }

    void AddLifeFromTimer(SaverTest saver)
    {
        if (saver.lives < 5 && !isUnlimitedLivesActive)
        {
            saver.lives++;

            if (saver.lives >= 5)
            {
                saver.lives = 5;
                remainingTime = LIFE_TIMER_DURATION;
                isLifeTimerActive = false;
            }
            else
            {
                if (remainingTime <= 0)
                {
                    remainingTime += LIFE_TIMER_DURATION;
                }
            }

            lastLifeUpdateTime = DateTime.Now;

            // ÈÑÏÐÀÂËÅÍÎ: âûçûâàåì public ìåòîä UpdateMainScreen()
            saver.UpdateMainScreen();

            SaveLifeTimerData(saver.lives);
        }
    }

    public void ResetTimer()
    {
        remainingTime = LIFE_TIMER_DURATION;
        lastLifeUpdateTime = DateTime.Now;
    }

    // === ÑÈÑÒÅÌÀ ÁÅÑÊÎÍÅ×ÍÛÕ ÆÈÇÍÅÉ ===

    public void CheckUnlimitedLives(SaverTest saver)
    {
        string endTimeStr = PlayerPrefs.GetString("unlimitedLivesEndTime", "");
        int savedHours = PlayerPrefs.GetInt("unlimitedHours", 0);

        if (!string.IsNullOrEmpty(endTimeStr) && savedHours > 0)
        {
            if (DateTime.TryParse(endTimeStr, out DateTime savedEndTime))
            {
                if (DateTime.Now < savedEndTime)
                {
                    isUnlimitedLivesActive = true;
                    unlimitedLivesEndTime = savedEndTime;
                    unlimitedHours = savedHours;
                    isLifeTimerActive = false;
                    UpdateUnlimitedLivesUI(saver.lifeTimerText);
                }
                else
                {
                    DeactivateUnlimitedLives(saver);
                }
            }
        }
    }

    // Â LifeTimerManager.cs äîáàâüòå:
    public void ActivateUnlimitedLives(int hours, SaverTest saver)
    {
        isUnlimitedLivesActive = true;
        unlimitedHours = hours;
        unlimitedLivesEndTime = DateTime.Now.AddHours(hours);
        isLifeTimerActive = false;

        PlayerPrefs.SetString("unlimitedLivesEndTime", unlimitedLivesEndTime.ToString("o"));
        PlayerPrefs.SetInt("unlimitedHours", hours);
        PlayerPrefs.Save();

        UpdateUnlimitedLivesUI(saver.lifeTimerText);

        // Îáíîâëÿåì èíòåðôåéñ
        saver.UpdateMainScreen();
    }

    void DeactivateUnlimitedLives(SaverTest saver)
    {
        isUnlimitedLivesActive = false;
        unlimitedHours = 0;
        isLifeTimerActive = true;
        remainingTime = LIFE_TIMER_DURATION;
        lastLifeUpdateTime = DateTime.Now;

        PlayerPrefs.DeleteKey("unlimitedLivesEndTime");
        PlayerPrefs.DeleteKey("unlimitedHours");
        PlayerPrefs.Save();

        UpdateUnlimitedLivesUI(saver.lifeTimerText);

        // ÈÑÏÐÀÂËÅÍÎ: âûçûâàåì public ìåòîä UpdateMainScreen()
        saver.UpdateMainScreen();
    }

    void UpdateUnlimitedLivesUI(TextMeshProUGUI lifeTimerText)
    {
        if (lifeTimerText != null)
        {
            if (isUnlimitedLivesActive)
            {
                TimeSpan remaining = unlimitedLivesEndTime - DateTime.Now;

                if (remaining.TotalSeconds > 0)
                {
                    if (remaining.TotalHours >= 1)
                    {
                        lifeTimerText.text = $"MAX {remaining.Hours}:{remaining.Minutes:D2}";
                    }
                    else
                    {
                        lifeTimerText.text = $"MAX {remaining.Minutes:D2}:{remaining.Seconds:D2}";
                    }
                    lifeTimerText.color = Color.green;
                }
                else
                {
                    lifeTimerText.text = "MAX";
                }
            }
            else
            {
                if (lifeTimerText != null)
                    lifeTimerText.text = "MAX";
            }
        }
    }

    // === ÒÀÉÌÅÐ ÐÅÊËÀÌÛ ===

    public bool CanShowAd(bool noRek)
    {
        if (noRek) return false;
        if (adTimerRemaining > 0) return false;
        return true;
    }

    public void ResetAdTimer()
    {
        adTimerRemaining = AD_INTERVAL;
    }

    public void Reward(SaverTest saver)
    {
        if (!isUnlimitedLivesActive)
        {
            saver.lives += 1;
            saver.UpdateMainScreen();
        }
    }

}
*/