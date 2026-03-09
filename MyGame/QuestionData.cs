using UnityEngine;
using System;

[Serializable]
public class QuestionData
{
    public string text;
    public string imageName;
    public string[] answers = new string[3];
    public int correctAnswerIndex;
    public int wordIndex; // Индекс слова в модуле
    public int module; // Номер модуля
    public Sprite cachedSprite; // Кэшированный спрайт

    public QuestionData(int index, int moduleNum)
    {
        wordIndex = index;
        module = moduleNum;

        string[] vopros, kartink, otvetone, otvettwo, otvetthree;
        int[] verno;

        switch (moduleNum)
        {
            case 1:
                vopros = GameData1.vopros;
                kartink = GameData1.kartink;
                otvetone = GameData1.otvetone;
                otvettwo = GameData1.otvettwo;
                otvetthree = GameData1.otvetthree;
                verno = GameData1.verno;
                break;
            case 2:
                vopros = GameData2.vopros;
                kartink = GameData2.kartink;
                otvetone = GameData2.otvetone;
                otvettwo = GameData2.otvettwo;
                otvetthree = GameData2.otvetthree;
                verno = GameData2.verno;
                break;
            case 3:
                vopros = GameData3.vopros;
                kartink = GameData3.kartink;
                otvetone = GameData3.otvetone;
                otvettwo = GameData3.otvettwo;
                otvetthree = GameData3.otvetthree;
                verno = GameData3.verno;
                break;
            default:
                vopros = GameData1.vopros;
                kartink = GameData1.kartink;
                otvetone = GameData1.otvetone;
                otvettwo = GameData1.otvettwo;
                otvetthree = GameData1.otvetthree;
                verno = GameData1.verno;
                break;
        }

        if (vopros == null || vopros.Length == 0)
        {
            text = "Ошибка загрузки вопроса";
            imageName = "";
            answers[0] = "Ответ 1";
            answers[1] = "Ответ 2";
            answers[2] = "Ответ 3";
            correctAnswerIndex = 0;
            return;
        }

        if (index < 0 || index >= vopros.Length)
        {
            index = Mathf.Clamp(index, 0, vopros.Length - 1);
        }

        text = vopros[index] ?? "Вопрос не загружен";

        if (kartink != null && index < kartink.Length)
            imageName = kartink[index] ?? "";
        else
            imageName = "";

        if (otvetone != null && index < otvetone.Length)
            answers[0] = otvetone[index] ?? "Ответ 1";
        else
            answers[0] = "Ответ 1";

        if (otvettwo != null && index < otvettwo.Length)
            answers[1] = otvettwo[index] ?? "Ответ 2";
        else
            answers[1] = "Ответ 2";

        if (otvetthree != null && index < otvetthree.Length)
            answers[2] = otvetthree[index] ?? "Ответ 3";
        else
            answers[2] = "Ответ 3";

        if (verno != null && index < verno.Length)
        {
            int correct = verno[index];
            correctAnswerIndex = Mathf.Clamp(correct - 1, 0, 2);
        }
        else
        {
            correctAnswerIndex = 0;
        }
    }

    // Метод для загрузки спрайта (с кэшированием)
    public Sprite LoadSprite()
    {
        if (cachedSprite != null)
            return cachedSprite;

        if (string.IsNullOrEmpty(imageName))
            return null;

        // Пробуем разные пути загрузки
        string[] paths = {
            "kart/" + imageName,
            "Images/" + imageName,
            "Sprites/" + imageName,
            imageName
        };

        foreach (string path in paths)
        {
            cachedSprite = Resources.Load<Sprite>(path);
            if (cachedSprite != null)
            {
                Debug.Log($" Загружена картинка: {path}");
                return cachedSprite;
            }
        }

        Debug.LogWarning($" Не найдена картинка: {imageName}");
        return null;
    }
}