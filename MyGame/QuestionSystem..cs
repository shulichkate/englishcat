using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionSystem : MonoBehaviour
{
    // ИСПРАВЛЕНО: Теперь принимает QuestionData
    public void LoadQuestion(QuestionData question, TextMeshProUGUI questionText,
                            Image questionImage, Button[] answerButtons, TextMeshProUGUI[] answerTexts,
                            SaverTest saver)
    {
        if (question == null) return;

        questionText.text = !string.IsNullOrEmpty(question.text) ? question.text : "Вопрос не загружен";

        if (!string.IsNullOrEmpty(question.imageName))
        {
            Sprite sprite = Resources.Load<Sprite>("kart/" + question.imageName);
            if (sprite != null)
            {
                questionImage.sprite = sprite;
            }
        }

        for (int i = 0; i < answerButtons.Length && i < question.answers.Length; i++)
        {
            string answerText = i < question.answers.Length ? question.answers[i] : "";
            answerTexts[i].text = !string.IsNullOrEmpty(answerText) ? answerText : "Ответ не загружен";
            answerButtons[i].image.color = Color.white;

            int answerIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() =>
                saver.OnAnswerSelected(answerIndex, question.correctAnswerIndex, question.wordIndex));
            answerButtons[i].interactable = true;
        }
    }
}