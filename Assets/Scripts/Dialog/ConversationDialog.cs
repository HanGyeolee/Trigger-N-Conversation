using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ConversationDialog : MonoBehaviour
{
    public GameObject Self;
    protected WaitForSeconds WFS = new WaitForSeconds(0.0625f);
    protected WaitForSeconds WFS1 = new WaitForSeconds(0.01f);

    public Text Name;
    public Text Content;
    public GameObject NextConversationMark;

    [HideInInspector]
    public bool skip = false;
    [HideInInspector]
    public bool isPlay { get; private set; } = false;

    private const int MaxSIN = 20;
    private Vector3 Position;
    private Vector3 tmp;
    private float[] sin_value;

    public void Start()
    {
        if (NextConversationMark != null)
            Position = NextConversationMark.transform.localPosition;
        sin_value = new float[MaxSIN];
        for (int i = 0; i < MaxSIN; i++)
        {
            sin_value[i] = Mathf.Sin(2 * i * Mathf.PI / MaxSIN) * 4f;
        }

        tmp = new Vector3(0, sin_value[0], 0);
    }

    public void SetActive(bool active)
    {
        Self.SetActive(active);
    }

    public void SetName(string name)
    {
        Name.text = name;
    }

    /// <summary>
    /// 대화를 애니메이션으로 띄운다.
    /// </summary>
    /// <param name="content">내용</param>
    /// <param name="time">다음 대화가 나오기까지 시간</param>
    /// <param name="Complete">대화가 완료된 후 동작</param>
    /// <param name="Complete_string">입력 매개 변 </param>
    /// <returns></returns>
    public IEnumerator SetContent<T>(string content, int time = 100, Action<T> Complete = null, params T[] Complete_string)
    {
        if (!isPlay)
        {
            if(NextConversationMark != null)
                NextConversationMark.SetActive(false);
            isPlay = true;
            skip = false;
            Content.text = "";
            foreach (var c in content)
            {
                if(!skip) yield return WFS;
                else yield return WFS1;
                Content.text += c;
            }
            yield return new WaitForSeconds((float)(time * 0.001));
            if (NextConversationMark != null)
            {
                NextConversationMark.SetActive(true);
                StartCoroutine(bouncing());
            }
            skip = isPlay = false;
            Complete?.Invoke(Complete_string[0]);
        }
    }
    
    private IEnumerator bouncing()
    {
        int count = 0;
        while (NextConversationMark.activeSelf)
        {
            tmp.y = sin_value[count];

            NextConversationMark.transform.localPosition = Position + tmp;

            count++;
            if (count == MaxSIN)
                count = 0;

            yield return WFS;
        }
    }
}
