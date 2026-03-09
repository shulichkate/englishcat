using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearnedWordItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI wordNumberText;
    public TextMeshProUGUI wordText;
    public TextMeshProUGUI correctCountText;
    public GameObject starIcon;

    [Header("Colors")]
    public Color masteredColor = Color.green;
    public Color learningColor = Color.yellow;
    public Color defaultColor = Color.white;

    public void Initialize(int number, string text, int correctCount)
    {
        if (wordNumberText != null)
            wordNumberText.text = number > 0 ? number.ToString() : "";

        if (wordText != null)
            wordText.text = text;

        if (correctCountText != null)
            correctCountText.text = correctCount >= 5 ? "ИЗУЧЕНО" : $"{correctCount}/5";

        // Визуальное оформление в зависимости от статуса
        Image background = GetComponent<Image>();
        if (background != null)
        {
            if (correctCount >= 5)
            {
                background.color = masteredColor;
                if (starIcon != null) starIcon.SetActive(true);
            }
            else if (correctCount > 0)
            {
                background.color = learningColor;
                if (starIcon != null) starIcon.SetActive(false);
            }
            else
            {
                background.color = defaultColor;
                if (starIcon != null) starIcon.SetActive(false);
            }
        }
    }
}