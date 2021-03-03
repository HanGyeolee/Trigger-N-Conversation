using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataSet;

[RequireComponent(typeof(Collider2D), typeof(ChangeScene))]
public class ConversationScript : MonoBehaviour
{
    public NPC_Conversation NPC;

    public static ConversationDialog Conversation;
    public static OptionsDialog Options;

    private string lastConversation;
    private bool exit = false;
    private bool select = false;
    private string dungeon_id = null;
    private int select_index = 0;
    private int Options_Length = 0;

    private readonly WaitForFixedUpdate WFFU = new WaitForFixedUpdate();

    // Start is called before the first frame update
    void Start()
    {
        // 저장된 데이터와 해당 아이디간의 관계를 따지고

        // 어디까지 대화를 들었는 지 체크
#if DEBUG
        Debug.Log("대화 불러오기 중" + DataManager.Data.isinitialized[3]);
#endif
        if (DataManager.Data.isinitialized[3])
        {
            try
            {
                NPC.initialize(DataManager.Data.NPC_Conversation[NPC.FileName]);
#if DEBUG
                Debug.Log("데이터 불러오기 성공");
#endif
            }
            catch (KeyNotFoundException)
            {
                NPC.initialize();
#if DEBUG
                Debug.Log("데이터 불러오기 실패");
#endif
            }
        }
        else
        {
            NPC.initialize();
#if DEBUG
            Debug.Log("데이터 없음");
#endif
        }
        // 해당 아이디가 가진 대화 스크립트 저장하기
        DataManager.Data.SaveConversation(NPC.FileName, NPC.GetCurrentConversation_ID());
        // 대화 스크립트
        NPC.LoadConversation(NPC.GetCurrentConversation_ID());
    }

    /// <summary>
    /// Rigidbody 에서 Sleepmode를 켜두면 작동이 안될 수 도 있다.
    /// </summary>
    /// <param name="collision"></param>
    public void OnTriggerStay2D(Collider2D collision)
    {
        // 대화 중일 때에는 더 이상 입력받지 않도록 한다.
        if (collision.transform.CompareTag("Player") && !DataManager.IsConversationAndCutScene)
        {
            if (Input.GetKeyDown(DataManager.UserInterface["Interact"]))   // 대화 상호작용
            {
                SetConversation();
            }
        }
    }

    private bool Interactived = false;
    private void Update()
    {
        if (DataManager.IsConversationAndCutScene)
        {
            if (Input.GetKeyDown(DataManager.UserInterface["Interact"]))   // 대화 상호작용
            {
                Interactived = true;
            } 
            else if (select) //선택지 좌우 이동
                SetOptionSelector();
            
        }
    }

    private void FixedUpdate()
    {
        if(Interactived)
        {
            DataManager.Data.SaveConversation(NPC.FileName, NPC.GetCurrentConversation_ID());
            //Debug.Log("Pressed Z");
            if (Conversation.isPlay || Options.isPlay) // 아직 다이얼로그가 작성중이면
            {
                //Debug.Log("Skipped");
                Conversation.skip = true; // 스킵한다.
                Options.skip = true;
            }
            else if (exit)
            {
                //Debug.Log("Dialog End");
                OffDialog();    // 다이얼로그에서 한 번 더 상호작용했을 때 꺼진다.
                exit = false;

                DataManager.IsConversationAndCutScene = false;
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

            Interactived = false;
        }
    }

    public void SetConversation()
    {
        DataManager.IsConversationAndCutScene = true;

        lastConversation = NPC.GetCurrentConversation_ID();
        if (!string.IsNullOrEmpty(NPC.GetNext_Conversation_ID()) && // 다음 대화가 존재할 때
            CheckCondition(NPC.GetCondition_ID()))                  // 대화 조건이 성립했을 때
        {
            if (NPC.GetType() == 'C')
            {
                select = false;
                // Conversation 게임 오브젝트 생성, 대화문 넣기
                Options.SetActive(false); 
                Conversation.SetActive(true);

                // 대화문 넣기
                Conversation.SetName(NPC.Name);
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

            // 대화문 넣기
            Conversation.SetName(NPC.Name);
            StartCoroutine(Conversation.SetContent<string>(NPC.GetContent(), NPC.Waitfor()));

            exit = NPC.GetExit();   // 나갈 수 있다.
            dungeon_id = NPC.GetDunGeon_ID();  // 전투가 진행될 수 있다.
        }
    }

    public bool CheckCondition(string Condition_ID)
    {
        if (Condition_ID != null)
        {
            if (DataManager.Data.Conditions.ContainsKey(Condition_ID))
                return DataManager.Data.Conditions[Condition_ID];
            else
            {
                DataManager.Data.SaveConditions(Condition_ID, false);
                DataManager.SaveData.SaveConditions(Condition_ID, false);
            }
        }
        return true;
    }

    /// <summary>
    /// 옵션 버그 수정용 서브 메소드
    /// </summary>
    /// <param name="opt"></param>
    private void SubMethod(params string[] opt)
    {
        Options.SetOptions(opt);

        Options_Length = NPC.GetOptions().Length;
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
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            select_index++;
        else if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            select_index--;

        if (Options_Length == select_index)
            select_index = 0;
        else if (select_index == -1)
            select_index = Options_Length - 1;

        //Options 내에 있는 커서를 움직인다.
        if(last != select_index)
            Options.MovingCursor(select_index);
    }

    private void OffDialog() // 대화 창을 끈다.
    {
        Options.SetActive(false);
        Conversation.SetActive(false);

        if(dungeon_id != null)
        {
            DataManager.SetBindableData(new Dungeon(dungeon_id));
            dungeon_id = null;
            GetComponent<ChangeScene>().LoadScene();
        }
    }
}
