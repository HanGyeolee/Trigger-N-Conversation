using System;
using System.Collections;
using System.Collections.Generic;
using DataSet;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public static Saving Data;
    public static Saving SaveData;
    public static Options options;

    /// <summary>
    /// 키와 동작 함수들을 보관하는 메모리
    /// </summary>
    public static Dictionary<string, KeyCode> UserInterface;
    public static bool IsConversationAndCutScene = false;

    [Range(3.5f,5f)]
    public float Orthographic_Size = 4f;// 인게임에서 4가 제일 보기 좋다

    /// <summary>
    /// 게임 오브젝트들 설정
    /// </summary>
    public GameObject Conversation;
    public GameObject Options;
    public Image BlackScene;

    public static float Blocking_time { get; private set; }
    public static float Attacking_time { get; private set; }
    public static float Dash_time { get; private set; }
    public static float Healing_time { get; private set; }
    public static float Stemina_time { get; private set; }

    /// <summary>
    /// 카메라 위치
    /// </summary>
    public static Vector3? Gap = null;

    /// <summary>
    /// 씬 전환 시 주고 받을 데이터
    /// </summary>
    private static object Bindable_Data;
    private AudioSource source;
    private static XmlParser parser = new XmlParser();

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);

            Data = new Saving();
            SaveData = new Saving();
            options = new Options();

            Trigger.Conversation = Conversation.GetComponent<ConversationDialog>();
            Trigger.Options = Options.GetComponent<OptionsDialog>();
            source = GetComponent<AudioSource>();

            if (parser.SaveFileExist())
                LoadFile();
            if (parser.OptionFileExist())
                options = parser.LoadOptionFile();

            if (Saving.CanLoadbyKey("SaveFile0"))
                Data = Saving.LoadbyFile();

            if (options.isinitialized)
            {
                UserInterface = new Dictionary<string, KeyCode>
                {
                    { "attack", options.Attack_Key },
                    { "guard", options.Guard_Key},
                    { "jump", options.Jump_Key },
                    { "heal", options.Heal_Key },
                    { "dash", options.Dash_Key },
                    { "Interact", options.Interact_Key },
                    { "ghostmode", options.GhostMode_Key }
                };
            }
            else
            {
                UserInterface = new Dictionary<string, KeyCode>
                {
                    { "attack", KeyCode.X },
                    { "guard", KeyCode.Z},
                    { "jump", KeyCode.C },
                    { "heal", KeyCode.V },
                    { "dash", KeyCode.LeftControl },
                    { "Interact", KeyCode.Space },
                    { "ghostmode", KeyCode.Tab }
                };
                options.SaveKeys(UserInterface);
            }
        }
        else
        {
            if (!Equals(instance))
                Destroy(gameObject);
        }
    }

    float repeat = 0;
    [HideInInspector]
    public bool finished = true;
    [HideInInspector]
    public float FadeTime = 64;
    float gap = 0;
    int Oldsoul;

    private void Update()
    {
        if (!instance.finished)
            FadeIn(0, instance.FadeTime);
    }

    public void SaveTrigger(GameObject gameObject)
    {
        //string Id = CreateKey();

        var item = new TriggerState();
        item.SaveState(gameObject);

        string Id = gameObject.name +"_" + item.SceneName;

#if DEBUG
        Debug.Log(Id);
#endif
        Data.SaveTriggers(Id, item);
        //SaveData.SaveTriggers(Id, item);
    }

    public void FadeIn(float alpha, float time)
    {
        if (gap == 0)
            gap = (alpha - (BlackScene.color.a + 0.1f));
        if (time != 0)
        {
            if (repeat < time + 1)
            {
                BlackScene.color += new Color(0, 0, 0, gap / (time - 1));

                repeat += 1;
            }
            else
            {
                repeat = 0;
                gap = 0;
                instance.finished = true;
                IsConversationAndCutScene = false;
            }
        }
        else
            BlackScene.color += new Color(0, 0, 0, gap);
    }

    /// <summary>
    /// 씬과 씬 사이에 주고 받을 데이터를 할당한다.
    /// </summary>
    /// <param name="Data">데이터</param>
    /// <param name="type">데이터 타입</param>
    public static void SetBindableData(object Data)
    {
        Bindable_Data = Data;
    }

    public static T GetBindableDate<T>()
    {
        T data = default;
        if (Bindable_Data is T)
            data = (T)Bindable_Data;

        Bindable_Data = null;
        return data;
    }

    private static void ResetTrigger(GameObject gameObject, TriggerState state)
    {
#if DEBUG
        Debug.Log(state.SceneName + " " + state.Trigger_ID + " :" + state.IsActivated);
#endif
        gameObject.transform.position = new Vector3(state.position.x, state.position.y); 
        gameObject.transform.rotation = state.rotation;
        gameObject.transform.localScale = state.localScale;

        gameObject.SetActive(state.IsActivated);
    }

    public static void MoveTrigger(string scenename)
    {
        foreach (var s in SaveData.Triggers)
        {
#if DEBUG
            Debug.Log(s.Value.SceneName + " " + s.Value.Parent_ID + " " + s.Value.Trigger_ID);
#endif
            if (scenename == s.Value.SceneName)
            {
#if DEBUG
                Debug.Log("같은 씬");
#endif
                GameObject g;
                if (s.Value.Parent_ID != null)
                {
                    var root = GameObject.Find(s.Value.Parent_ID);
                    g = FindInChildrenIncludingInactive(root, s.Value.Trigger_ID);
                }
                else
                {
                    g = FindIncludingInactive(s.Value.Trigger_ID);
                }

                if (g != null) ResetTrigger(g, s.Value);
            }
        }
    }

    public void ChangeMusic(AudioClip clip)
    {
        instance.source.clip = clip;
        instance.source.volume = 0.5f;
        instance.source.Play();
    }

    public void MusicStop()
    {
        if(instance.source.isPlaying)
            instance.source.Pause();
    }

    #region Condition
    public void OnCondition(string ID)
    {
        Data.SaveConditions(ID, true);
        SaveData.SaveConditions(ID, true);
    }

    public void SetCondition(string ID, bool condition_toggled)
    {
        Data.SaveConditions(ID, condition_toggled);
        SaveData.SaveConditions(ID, condition_toggled);
    }
    #endregion

    public static void MoveToChar(double x, double y, bool isright = true)
    {
#if DEBUG
        Debug.Log("x : " + x + " y : " + y);
#endif
        var gs = GameObject.FindGameObjectsWithTag("Player");
        foreach (var g in gs)
        {
            g.transform.position = new Vector3((float)x, (float)y, g.transform.position.z);
        }

        /* 
        // if you want to flip to character, make new class named PlayerInput
        // PlayerInput class must have itself named instance.
        if(PlayerInput.instance !=null)
            PlayerInput.instance.Flip(isright);
        //*/
    }

    #region FileLoad and Save
    public static void SaveFile(bool isoverwrite = true)
    {
#if DEBUG
        Debug.Log("저장됨");
#endif
        var g = GameObject.FindWithTag("Player");
        //var pi = g.GetComponent<PlayerInput>();

        if (isoverwrite)
        {
            Data.SavePosition(
                SceneManager.GetActiveScene().name, 
                g.transform.position,
                false //pi.InDanger
                );
        }

        if(instance.source.isPlaying)
        {
            Data.SaveBGM(instance.source);
        }

        /*
        Data.SaveState(PlayerState.SkillData, new float[]
            {
                PlayerState.MaxHP,
                PlayerState.HP,
                PlayerState.MaxStemina,
                PlayerState.Stemina,
                PlayerState.MaxMana,
                PlayerState.Mana,
                PlayerState.GrowingMana,
                PlayerState.Soul
            });
        //*/

        parser.CreateSaveFile(Data);
        SaveData = Data;
    }

    public void LoadFile()
    {
#if DEBUG
        Debug.Log("불러오기");
#endif
        parser.LoadSaveFile(out Data);
        parser.LoadSaveFile(out SaveData);

        var s = SceneManager.GetActiveScene().name;
        if (SaveData.Scene_Name == s)
        {
            MoveToChar(SaveData.Position.x, SaveData.Position.y);

            DataManager.MoveTrigger(s);
            DataManager.IsConversationAndCutScene = false;
        }
        else
        {
            SceneManager.LoadSceneAsync(SaveData.Scene_Name).completed += delegate
            {
                MoveToChar(SaveData.Position.x, SaveData.Position.y);

                DataManager.MoveTrigger(SaveData.Scene_Name);
                DataManager.IsConversationAndCutScene = false;
            };
        }

        if (SaveData.BGM != null)
        {
            string clipname = SaveData.BGM.Key;
            var clip = Resources.Load("Music/BGM/" + clipname) as AudioClip;

            ChangeMusic(clip);
            instance.source.time = SaveData.BGM.Value;
        }
    }
    #endregion

    #region Fade
    public void FadeIn(float time)
    {
        if (time < 0) time = FadeTime;
        instance.StartCoroutine(instance.Fade(1, time));
    }

    public void FadeOut(float time)
    {
        if (time < 0) time = FadeTime;
        instance.StartCoroutine(instance.Fade(0, time));
    }

    public IEnumerator Fade(float alpha, float time, Action finish = null)
    {
        IsConversationAndCutScene = true;

        float repeat = 0;
        float gap = (alpha - (BlackScene.color.a));
        if (time != 0)
        {
            while (repeat < time + 1)
            {
                BlackScene.color += new Color(0, 0, 0, gap / time);

                yield return new WaitForFixedUpdate();

                repeat += 1;
            }

        }
        else
            BlackScene.color += new Color(0, 0, 0, gap);

        if(finish != null)
            IsConversationAndCutScene = false;
        finish?.Invoke();
    }
    #endregion

    public UnityEvent afterdie;
    public void Died(bool killed = true)
    {
#if false
        afterdie.AddListener(delegate
        {
            Time.timeScale = 1;
        });
        Time.timeScale = 0;
#endif

        StartCoroutine(instance.Fade(1, instance.FadeTime, delegate {
            IsConversationAndCutScene = true;
            // 죽었다는 글자 표시
            // 2초 뒤에 

            if (Data.isinitialized[0])
            {
                if(killed) SaveFile(false);

#if DEBUG
                Debug.Log("로드 저장된 씬 이름 : " + Data.Scene_Name + " 현재 씬 이름 : " + SceneManager.GetActiveScene().name);
#endif
                if (Data.Scene_Name == SceneManager.GetActiveScene().name)
                {
                    // Set Character's Position

                    StartCoroutine(instance.Fade(0, instance.FadeTime, delegate {
                        instance.finished = false;
                    }));
                }
                else
                {
                    // 씬 이동 하고, 위치 이동과, Danger 표ㅅ
                    StartCoroutine(LoadScene(Data));
                }
            }
            else // 인위적으로 세이브파일을 지웠을 때
            {
                // 맨처음 맵으로 이동, 재화 0 으로 만듬
            }

            afterdie?.Invoke();
        }
        ));
    }

    private IEnumerator LoadScene(Saving d = null)
    {
        if (d == null)
            d = SaveData;

        var s = SceneManager.LoadSceneAsync(d.Scene_Name);
        s.allowSceneActivation = false;
        s.completed += delegate
        {
            MoveToChar(d.Position.x, d.Position.y);

            StartCoroutine(instance.Fade(0, instance.FadeTime, delegate {
                instance.finished = false;
            }));
        };

        s.allowSceneActivation = true;

        while (!s.isDone)
        {
            // progrees bar
            yield return null;
        }

    }

    static int Seed = 0;
    private static string CreateKey()
    {
        string t = Time.time.ToString() + SceneManager.GetActiveScene().name + Seed.ToString();
        Seed++;
        return t;
    }

    //hideously slow as it iterates all objects, so don't overuse!
    public static GameObject FindInChildrenIncludingInactive(GameObject go, string name)
    {

        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).gameObject.name == name) return go.transform.GetChild(i).gameObject;
            GameObject found = FindInChildrenIncludingInactive(go.transform.GetChild(i).gameObject, name);
            if (found != null) return found;
        }

        return null;  //couldn't find crap
    }

    //hideously slow as it iterates all objects, so don't overuse!
    public static GameObject FindIncludingInactive(string name)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            //no scene loaded
            return null;
        }

        var game_objects = new List<GameObject>();
        scene.GetRootGameObjects(game_objects);

        foreach (GameObject obj in game_objects)
        {
            if (obj.transform.name == name) return obj;

            GameObject found = FindInChildrenIncludingInactive(obj, name);
            if (found) return found;
        }

        return null;
    }
}
