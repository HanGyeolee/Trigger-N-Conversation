using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;


public class OptionsDialog : ConversationDialog
{
    public Sprite NonSelect;
    public Sprite Selected;

    public Options option1;
    public Options[] options2;
    public Options[] options3;

    [HideInInspector]
    public string[] contents;

    public void SetOptions(params string[] opt)
    {
        int i;
        contents = new string[opt.Length];
        opt.CopyTo(contents,0);
        switch(opt.Length)
        {
            case 1:
                option1.Answer.SetActive(true);
                option1.text.text = opt[0];
                for (i = 0; i < options2.Length; i++)
                {
                    options2[i].Answer.SetActive(false);
                }
                for (i = 0; i < options3.Length; i++)
                {
                    options3[i].Answer.SetActive(false);
                }
                break;
            case 2:
                option1.Answer.SetActive(false);
                for (i = 0; i < options2.Length; i++)
                {
                    options2[i].Answer.SetActive(true);
                    options2[i].text.text = opt[i];
                }
                for (i = 0; i < options3.Length; i++)
                {
                    options3[i].Answer.SetActive(false);
                }
                break;
            case 3:
                option1.Answer.SetActive(false);
                for (i = 0; i < options2.Length; i++)
                {
                    options2[i].Answer.SetActive(false);
                }
                for (i = 0; i < options3.Length; i++)
                {
                    options3[i].Answer.SetActive(true);
                    options3[i].text.text = opt[i];
                }
                break;

            // 오류 목록
            case 0:
                throw new System.ArgumentNullException();
            default:
                throw new System.ArgumentOutOfRangeException();
        }

        MovingCursor(0);
    }

    public void MovingCursor(int index)
    {
        switch (contents.Length)
        {
            case 1:
                option1.text.text = contents[0];
                break;
            case 2:
                options2[0].text.text = contents[0];
                options2[1].text.text = contents[1];

                switch (index)
                {
                    case 0:
                        options2[0].background.sprite = Selected;
                        options2[1].background.sprite = NonSelect;
                        break;
                    case 1:
                        options2[0].background.sprite = NonSelect;
                        options2[1].background.sprite = Selected;
                        break;
                }
                break;
            case 3:
                options3[0].text.text = contents[0];
                options3[1].text.text = contents[1];
                options3[2].text.text = contents[2];
                switch (index)
                {
                    case 0:
                        options3[0].background.sprite = Selected;
                        options3[1].background.sprite = NonSelect;
                        options3[2].background.sprite = NonSelect;
                        break;
                    case 1:
                        options3[0].background.sprite = NonSelect;
                        options3[1].background.sprite = Selected;
                        options3[2].background.sprite = NonSelect;
                        break;
                    case 2:
                        options3[0].background.sprite = NonSelect;
                        options3[1].background.sprite = NonSelect;
                        options3[2].background.sprite = Selected;
                        break;
                }
                break;

            // 오류 목록
            case 0:
                throw new System.ArgumentNullException();
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

    public void ClearOptions()
    {
        int i;
        option1.Answer.SetActive(false);
        for (i = 0; i < options2.Length; i++)
        {
            options2[i].Answer.SetActive(false);
        }
        for (i = 0; i < options3.Length; i++)
        {
            options3[i].Answer.SetActive(false);
        }
        option1.text.text = "";
        options2[0].text.text = "";
        options2[1].text.text = "";
        options3[0].text.text = "";
        options3[1].text.text = "";
        options3[2].text.text = "";
    }

    [Serializable]
    public class Options
    {
        [SerializeField]
        public GameObject Answer;
        [SerializeField]
        public Image background;
        [SerializeField]
        public Text text;

        public Options(GameObject _Answer, Text _text, Image _background)
        {
            Answer = _Answer;
            text = _text;
            background = _background;
        }
    }
}
