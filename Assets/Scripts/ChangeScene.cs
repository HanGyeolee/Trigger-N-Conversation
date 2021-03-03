using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DataSet;

public class ChangeScene : MonoBehaviour
{
    public bool Used;
    public bool InDanger = false;
    public bool IsRight = true;
    public UnityEvent Before;

    public bool useBlack = false;
    public Pos position;

    private bool Called = false;

    public void LoadSceneCalledbyTrigger()
    {
        LoadScene();
    }

    public void LoadScene(Action action = null)
    {
        if (!Called)
        {
            Before?.Invoke();
            Called = true;
            if (useBlack)
            {
                DataManager.SetBindableData(new Dungeon(position.SceneName));

                fade(delegate
                {
                    DataManager.IsConversationAndCutScene = true;
                    SceneManager.LoadSceneAsync(position.SceneName).completed += delegate
                    {
                        DataManager.instance.Fade(1, 0);
                        DataManager.MoveToChar(position.x, position.y, IsRight);
                        DataManager.instance.finished = false;
                        DataManager.MoveTrigger(position.SceneName);

                        action?.Invoke();
                    };

                });
            }
            else
            {
                SceneManager.LoadSceneAsync(position.SceneName).completed += delegate
                {
                    DataManager.MoveToChar(position.x, position.y, IsRight);

                    action?.Invoke();
                };
            }
        }
    }

    void fade(Action finised = null)
    {
        StartCoroutine(DataManager.instance.Fade(1, DataManager.instance.FadeTime, finised));
    }
}

[Serializable]
public class Pos
{
    public string SceneName;
    public double x, y;
}
