using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DataSet;
#if DEBUG
using UnityEditor;
using UnityEditor.Events;
#endif

[RequireComponent(typeof(Collider2D), typeof(ChangeScene))]
public class Trigger : MonoBehaviour
{
    public enum TriggerType { Enter, Stay, Exit };

    public bool IsTrigger = false;
    public bool DeleteAfter = false;
    public TriggerType type = TriggerType.Enter;
    public UnityEvent Before;
    public UnityEvent After;

    public string condition = null;
    public NPC_Conversation NPC;
    public List<TagAnimation> tagAnimations;

    public List<Conditional_Conversation> conditional_Conversations;

    public static ConversationDialog Conversation;
    public static OptionsDialog Options;

    private string lastConversation;
    private bool exit = false;
    private bool select = false;
    private string dungeon_id = null;
    private int select_index = 0;
    private int Options_Length = 0;
    private bool IsAnimeRunning = false;

    protected int index = -1;

    private readonly WaitForEndOfFrame WFEOF = new WaitForEndOfFrame();

    // Start is called before the first frame update
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if(sr != null) sr.enabled = !IsTrigger;

        // 저장된 데이터와 해당 아이디간의 관계를 따지고
        if (DeleteAfter)
        {
#if DEBUG
            var targetinfo = UnityEventBase.GetValidMethodInfo(DataManager.instance, "SaveTrigger", new Type[] { typeof(GameObject) });
            UnityAction<GameObject> action =
                Delegate.CreateDelegate(typeof(UnityAction<GameObject>), DataManager.instance, targetinfo, false) as UnityAction<GameObject>;
            UnityEventTools.AddObjectPersistentListener(After, action, gameObject);
#else
            After.AddListener(delegate { DataManager.instance.SaveTrigger(gameObject); });
#endif
        }

        if (conditional_Conversations.Count > 0)
        {
            // 반대로 해야하는 이유는, 선행되는 조건에 대화가 묶여있는 것을 예방하기 위함
            for (int i = conditional_Conversations.Count - 1; i > -1; i--)
            {
                bool CanOpen = CheckCondition(conditional_Conversations[i].condition);

                if (CanOpen)
                {
                    index = i;
                    break;
                }
            }
        }

        if (index == -1)
        {
            // 어디까지 대화를 들었는 지 체크
            if (DataManager.Data.isinitialized[3])
            {
                try
                {
                    NPC.initialize(DataManager.Data.NPC_Conversation[NPC.FileName]);
                }
                catch (KeyNotFoundException)
                {
                    NPC.initialize();
                }
            }
            else
            {
                NPC.initialize();
            }
            // 해당 아이디가 가진 대화 스크립트 저장하기
            DataManager.Data.SaveConversation(NPC.FileName, NPC.GetCurrentConversation_ID());
            //Debug.Log(DataManager.Data.NPC_Conversation);
            // 대화 스크립트

            NPC.LoadConversation(NPC.GetCurrentConversation_ID());
        }
        else
        {
            var NPC = conditional_Conversations[index].NPC;

            if (DataManager.Data.isinitialized[3])
            {
                try
                {
                    NPC.initialize(DataManager.Data.NPC_Conversation[NPC.FileName]);
                }
                catch (KeyNotFoundException)
                {
                    NPC.initialize();
                }
            }
            else
            {
                NPC.initialize();
            }

            NPC.LoadConversation(NPC.GetCurrentConversation_ID());
        }
    }


    private bool CheckCondition(string Condition_ID)
    {
        if (!string.IsNullOrWhiteSpace(Condition_ID))
        {
            if (DataManager.Data.Conditions.ContainsKey(Condition_ID))
                return DataManager.Data.Conditions[Condition_ID];
            else
            {
                DataManager.Data.SaveConditions(Condition_ID, false);
                DataManager.SaveData.SaveConditions(Condition_ID, false);
                return false;
            }
        }
        return false;
    }

    bool on = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (type == TriggerType.Enter)
            if (collision.CompareTag("Player") && !DataManager.IsConversationAndCutScene)
            {
                on = true;
            }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (type == TriggerType.Stay)
            if (collision.CompareTag("Player") && !DataManager.IsConversationAndCutScene)
            {
                on = true;
            }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (type == TriggerType.Exit)
            if (collision.CompareTag("Player") && !DataManager.IsConversationAndCutScene)
            {
                on = true;
            }
    }

    private void Update()
    {
        if(on)
        {
            if (type == TriggerType.Stay)
            {
                if (IsTrigger || Input.GetKeyDown(DataManager.UserInterface["Interact"]))   // 대화 상호작용
                {// 트리거 실행
                    if (string.IsNullOrWhiteSpace(condition) || CheckCondition(condition))
                    {
                        Before?.Invoke();

                        SetConversation();
                        StartCoroutine(Conversationing());
                    }
                    on = false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(condition) || CheckCondition(condition))
                {
                    Before?.Invoke();

                    SetConversation();
                    StartCoroutine(Conversationing());
                }
                on = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (on)
        {
            if (type == TriggerType.Stay)
            {
                if (IsTrigger || Input.GetKeyDown(DataManager.UserInterface["Interact"]))   // 대화 상호작용
                {// 트리거 실행
                    if (string.IsNullOrWhiteSpace(condition) || CheckCondition(condition))
                    {
                        Before?.Invoke();

                        SetConversation();
                        StartCoroutine(Conversationing());
                    }
                    on = false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(condition) || CheckCondition(condition))
                {
                    Before?.Invoke();

                    SetConversation();
                    StartCoroutine(Conversationing());
                }
                on = false;
            }
        }
    }

    bool SaveKeyEvent = false;

    IEnumerator Conversationing()
    {
        // 키보드 입력이 두번 들어가게 하는 것을 막기 위해 한 프레임을 쉰다.
        yield return WFEOF;
        while (DataManager.IsConversationAndCutScene)
        {
            if (!SaveKeyEvent)
                SaveKeyEvent = Input.GetKeyDown(DataManager.UserInterface["Interact"]);

            if (index == -1)
            {
                if (SaveKeyEvent)   // 대화 상호작용
                {
                    DataManager.Data.SaveConversation(NPC.FileName, NPC.GetCurrentConversation_ID());
                    //Debug.Log("Pressed Z");
                    if ((Conversation.isPlay || Options.isPlay)) // 아직 다이얼로그가 작성중이면
                    {
                        //Debug.Log("Skipped");
                        if (!IsAnimeRunning)
                        {
                            Conversation.skip = true; // 스킵한다.
                            Options.skip = false;
                            IsAnimeRunning = false;
                        }
                    }
                    else if (exit)
                    {
                        //Debug.Log("Dialog End");
                        OffDialog();    // 다이얼로그에서 한 번 더 상호작용했을 때 꺼진다.
                        exit = false;
                        break;
                    }
                    else if (select)
                    {
                        //Debug.Log("Selected");
                        NPC.SetChoosing(select_index);
                        NPC.LoadConversation(NPC.GetNext_Conversation_ID());
                        select = false;
                        SetConversation();
                    }
                    else
                        SetConversation();

                    SaveKeyEvent = false;
                }
            }
            else
            {
                var NPC = conditional_Conversations[index].NPC;
                if (SaveKeyEvent)   // 대화 상호작용
                {
                    DataManager.Data.SaveConversation(
                        NPC.FileName,
                        NPC.GetCurrentConversation_ID());
                    //Debug.Log("Pressed Z");
                    if ((Conversation.isPlay || Options.isPlay)) // 아직 다이얼로그가 작성중이면
                    {
                        //Debug.Log("Skipped");
                        if (!IsAnimeRunning)
                        {
                            Conversation.skip = true; // 스킵한다.
                            Options.skip = true;
                            IsAnimeRunning = false;
                        }
                    }
                    else if (exit)
                    {
                        //Debug.Log("Dialog End");
                        OffDialog();    // 다이얼로그에서 한 번 더 상호작용했을 때 꺼진다.
                        exit = false;
                        break;
                    }
                    else if (select)
                    {
                        //Debug.Log("Selected");
                        NPC.SetChoosing(select_index);
                        NPC.LoadConversation(NPC.GetNext_Conversation_ID());
                        select = false;
                        SetConversation();
                    }
                    else
                        SetConversation();

                    SaveKeyEvent = false;
                }
            }

            //선택지 좌우 이동
            if (select)
                SetOptionSelector();

            yield return null;
        }

        yield return WFEOF;
        DataManager.IsConversationAndCutScene = false;

        if (DeleteAfter)
            gameObject.SetActive(false);

        OffDialog();
        After?.Invoke();
    }

    public void SetConversation()
    {
        DataManager.IsConversationAndCutScene = true;

        if (index == -1)
        {
            lastConversation = NPC.GetCurrentConversation_ID();
            if (!string.IsNullOrEmpty(NPC.GetNext_Conversation_ID()))    // 다음 대화가 존재할 때
            {
                if (NPC.GetType() == 'C')
                {
                    select = false;
                    // Conversation 게임 오브젝트 생성, 대화문 넣기
                    Options.SetActive(false);
                    Conversation.SetActive(true);

                    // 대화문 넣기
                    Conversation.SetName(NPC.Name);

                    var anime = NPC.GetAnimeID();
#if DEBUG
                    Debug.Log(anime);
#endif
                    if (anime != null)
                        foreach (var tag in tagAnimations)
                        {
                            if (tag.ID.CompareTo(anime) == 0) // 아이디가 같을 때
                            {
                                tag.ID = "";
                                IsAnimeRunning = true;
                                tag.anime?.Invoke();
                                break;
                            }
                        }
                    else
                        IsAnimeRunning = false;

                    if (string.IsNullOrWhiteSpace(NPC.GetContent()))
                    {
                        DataManager.IsConversationAndCutScene = false;
                        NPC.LoadConversation(NPC.GetNext_Conversation_ID());
                        exit = NPC.GetExit();   // 나갈 수 있다.
                        OffDialog();
                        return;
                    }

                    StartCoroutine(Conversation.SetContent(NPC.GetContent(), NPC.Waitfor(), NPC.LoadConversation, NPC.GetNext_Conversation_ID()));

                    exit = NPC.GetExit();   // 나갈 수 있다.
                    dungeon_id = NPC.GetDunGeon_ID();  // 전투가 진행될 수 있다.
                }
                else if (NPC.GetType() == 'O')
                {
                    // Options 게임 오브젝트 생성, 선택지 넣기
                    Options.SetActive(true);
                    Conversation.SetActive(false);

                    Options.ClearOptions();
                    // 대화문 넣기
                    Options.SetName(NPC.Name);

                    var anime = NPC.GetAnimeID();
#if DEBUG
                    Debug.Log(anime);
#endif
                    if (anime != null)
                        foreach (var tag in tagAnimations)
                        {
                            if (tag.ID.CompareTo(anime) == 0) // 아이디가 같을 때
                            {
                                tag.ID = "";
                                IsAnimeRunning = true;
                                tag.anime?.Invoke();
                                break;
                            }
                        }
                    else
                        IsAnimeRunning = false;

                    StartCoroutine(Options.SetContent(NPC.GetContent(), NPC.Waitfor(), SubMethod, NPC.GetOptions()));
                }
            }
            else
            {
                select = false;
                NPC.LoadConversation(lastConversation);

                // Conversation 게임 오브젝트 생성, 대화문 넣기
                Options.SetActive(false);
                Conversation.SetActive(true);

                var anime = NPC.GetAnimeID();
#if DEBUG
                Debug.Log(anime);
#endif
                if (anime != null)
                    foreach (var tag in tagAnimations)
                    {
                        if (tag.ID.CompareTo(anime) == 0) // 아이디가 같을 때
                        {
                            tag.ID = "";
                            IsAnimeRunning = true;
                            tag.anime?.Invoke();
                            break;
                        }
                    }
                else
                    IsAnimeRunning = false;

                // 대화문 넣기
                Conversation.SetName(NPC.Name);

                if (string.IsNullOrWhiteSpace(NPC.GetContent()))
                {
                    DataManager.IsConversationAndCutScene = false;
                    exit = NPC.GetExit();   // 나갈 수 있다.
                    OffDialog();
                    return;
                }

                StartCoroutine(Conversation.SetContent<string>(NPC.GetContent(), NPC.Waitfor()));

                exit = NPC.GetExit();   // 나갈 수 있다.
                dungeon_id = NPC.GetDunGeon_ID();  // 전투가 진행될 수 있다.
            }
        }
        else
        {
            var NPC = conditional_Conversations[index].NPC;
            var tagAnimations = conditional_Conversations[index].tagAnimations;

            lastConversation = NPC.GetCurrentConversation_ID();
            if (!string.IsNullOrEmpty(NPC.GetNext_Conversation_ID()))    // 다음 대화가 존재할 때
            {
                if (NPC.GetType() == 'C')
                {
                    select = false;
                    // Conversation 게임 오브젝트 생성, 대화문 넣기
                    Options.SetActive(false);
                    Conversation.SetActive(true);

                    // 대화문 넣기
                    Conversation.SetName(NPC.Name);

                    var anime = NPC.GetAnimeID();
#if DEBUG
                    Debug.Log(anime);
#endif
                    if (anime != null)
                        foreach (var tag in tagAnimations)
                        {
                            if (tag.ID.CompareTo(anime) == 0) // 아이디가 같을 때
                            {
                                tag.ID = "";
                                IsAnimeRunning = true;
                                tag.anime?.Invoke();
                                break;
                            }
                        }
                    else
                        IsAnimeRunning = false;

                    if (string.IsNullOrWhiteSpace(NPC.GetContent()))
                    {
                        DataManager.IsConversationAndCutScene = false;
                        NPC.LoadConversation(NPC.GetNext_Conversation_ID());
                        exit = NPC.GetExit();   // 나갈 수 있다.
                        OffDialog();
                        return;
                    }

                    StartCoroutine(Conversation.SetContent(NPC.GetContent(), NPC.Waitfor(), NPC.LoadConversation, NPC.GetNext_Conversation_ID()));

                    exit = NPC.GetExit();   // 나갈 수 있다.
                    dungeon_id = NPC.GetDunGeon_ID();  // 전투가 진행될 수 있다.
                }
                else if (NPC.GetType() == 'O')
                {
                    // Options 게임 오브젝트 생성, 선택지 넣기
                    Options.SetActive(true);
                    Conversation.SetActive(false);

                    Options.ClearOptions();
                    // 대화문 넣기
                    Options.SetName(NPC.Name);

                    var anime = NPC.GetAnimeID();
#if DEBUG
                    Debug.Log(anime);
#endif
                    if (anime != null)
                        foreach (var tag in tagAnimations)
                        {
                            if (tag.ID.CompareTo(anime) == 0) // 아이디가 같을 때
                            {
                                tag.ID = "";
                                IsAnimeRunning = true;
                                tag.anime?.Invoke();
                                break;
                            }
                        }
                    else
                        IsAnimeRunning = false;

                    StartCoroutine(Options.SetContent(NPC.GetContent(), NPC.Waitfor(), SubMethod, NPC.GetOptions()));
                }
            }
            else
            {
                select = false;
                NPC.LoadConversation(lastConversation);

                // Conversation 게임 오브젝트 생성, 대화문 넣기
                Options.SetActive(false);
                Conversation.SetActive(true);

                var anime = NPC.GetAnimeID();
#if DEBUG
                Debug.Log(anime);
#endif
                if (anime != null)
                    foreach (var tag in tagAnimations)
                    {
                        if (tag.ID.CompareTo(anime) == 0) // 아이디가 같을 때
                        {
                            tag.ID = "";
                            IsAnimeRunning = true;
                            tag.anime?.Invoke();
                            break;
                        }
                    }
                else
                    IsAnimeRunning = false;

                // 대화문 넣기
                Conversation.SetName(NPC.Name);

                if (string.IsNullOrWhiteSpace(NPC.GetContent()))
                {
                    DataManager.IsConversationAndCutScene = false;
                    exit = NPC.GetExit();   // 나갈 수 있다.
                    OffDialog();
                    return;
                }

                StartCoroutine(Conversation.SetContent<string>(NPC.GetContent(), NPC.Waitfor()));

                exit = NPC.GetExit();   // 나갈 수 있다.
                dungeon_id = NPC.GetDunGeon_ID();  // 전투가 진행될 수 있다.
            }
        }
    }

    /// <summary>
    /// 옵션 버그 수정용 서브 메소드
    /// </summary>
    /// <param name="opt"></param>
    private void SubMethod(params string[] opt)
    {
        Options.SetOptions(opt);

        if (index == -1)
            Options_Length = NPC.GetOptions().Length;
        else
            Options_Length = conditional_Conversations[index].NPC.GetOptions().Length;
        Options.MovingCursor(select_index);

        exit = false;
        select = true;
    }

    /// <summary>
    /// 선택지 선택하기
    /// </summary>
    private void SetOptionSelector()//TODO 버그 수정 Need
    {
        var last = select_index;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            select_index++;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            select_index--;

        if (Options_Length == select_index)
            select_index = 0;
        else if (select_index == -1)
            select_index = Options_Length - 1;

        //Options 내에 있는 커서를 움직인다.
        if (last != select_index)
            Options.MovingCursor(select_index);
    }

    private void OffDialog() // 대화 창을 끈다.
    {
        Options.SetActive(false);
        Conversation.SetActive(false);

        if (dungeon_id != null)
        {
            DataManager.SetBindableData(new Dungeon(dungeon_id));
            dungeon_id = null;
            GetComponent<ChangeScene>().LoadScene();
        }
    }

    [System.Serializable]
    public class TagAnimation
    {
        public string ID;
        public UnityEvent anime;
    }

    [System.Serializable]
    public class Conditional_Conversation
    {
        public string condition;
        public NPC_Conversation NPC;
        public List<TagAnimation> tagAnimations;
    }
}
