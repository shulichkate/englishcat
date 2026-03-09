using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class QuestionCell : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI questionNameText;
    public GameObject checkmarkImage;
    public GameObject crossImage;
    public Button cellButton;

    [Header("States")]
    public Color normalColor = Color.white;
    public Color answeredColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

    private int questionIndex = -1;
    private Action<int> onClickCallback;
    private bool isAnswered = false;

    // ИСПРАВЛЕНО: Теперь принимает QuestionData а не SaverTest.QuestionData
    public void Initialize(int index, QuestionData data, Action<int> callback)
    {
        questionIndex = index;
        onClickCallback = callback;

        questionNumberText.text = (index + 1).ToString();

        // Показываем сокращенный текст вопроса (первые 15 символов)
        string questionText = data.text;
        if (questionText.Length > 15)
        {
            questionText = questionText.Substring(0, 15) + "...";
        }
        questionNameText.text = questionText;

        ResetState();

        cellButton.onClick.RemoveAllListeners();
        cellButton.onClick.AddListener(OnCellClicked);
    }

    public void SetState(bool answered, bool isCorrect)
    {
        isAnswered = answered;

        if (answered)
        {
            cellButton.interactable = false;
            cellButton.image.color = answeredColor;

            checkmarkImage.SetActive(isCorrect);
            crossImage.SetActive(!isCorrect);
        }
        else
        {
            ResetState();
        }
    }

    void ResetState()
    {
        checkmarkImage.SetActive(false);
        crossImage.SetActive(false);
        cellButton.interactable = true;
        cellButton.image.color = normalColor;
        isAnswered = false;
    }

    public void MakeNonClickable()
    {
        cellButton.interactable = false;
    }

    public int GetQuestionIndex()
    {
        return questionIndex;
    }

    void OnCellClicked()
    {
        if (!isAnswered && onClickCallback != null)
        {
            onClickCallback(questionIndex);
        }
    }
}