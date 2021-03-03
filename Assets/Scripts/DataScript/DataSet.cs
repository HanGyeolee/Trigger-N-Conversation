using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace DataSet
{
    public enum SceneState { Main, Option }
    public enum LookAt { None, Left, Up, Right, Down };
    [Serializable]
    public enum MonsterType { None, Melee, Bullet}
    public enum Skill_Button { heal, dash, ghostmode, air_jump, air_dash };

    [Serializable]
    public enum Buff_Code { 없음, 기절, 속박, 혼란, 둔화, 회복불능, 질병 };

    [System.Serializable]
    public class NPC_Conversation
    {
        [SerializeField]
        public string FileName;            //NPC_ID

        [HideInInspector]
        public string Name;              //Conversator_Name

        private string DunGeon_ID = null;//대화 끝나고 던전(혹은 전투)로 넘어간다면, 던전의 ID
        private string Condition_ID = null;  //대화 시작의 조건
        private string Conversation_ID;  //Conversation_ID

        private int Wait;                //Waiting
        private char Type;               //Conversation_Type
        private string AnimationID;
        private string Content;          //Conversation_Content 
        private string[] Options;        //Optional Conversation

        private int Choose;
        private string[] Option_Next_Conversation;
        private string Next_Conversation_ID; //Next_Conversation_ID

        private string filepath;         //Filepath of NPC Conversation scripts

        private bool CanExit = false;    //대화에서 탈출할 수 있는 지

        private XmlParser parser;
        private bool CanParser = false;

        public void initialize()
        {
            filepath = "Scripts/NPCs/" + FileName;
            parser = new XmlParser();
            //parser.CreateConvXml(filepath, NPC_ID, Name);
            CanParser = parser.LoadXml(filepath, FileName);

            if(CanParser)
            {
                if (parser.node != null)
                {
                    var id = parser.node.SelectSingleNode("ID").InnerText;
                    if (FileName != id)
                    {
                        CanParser = false;
                        return;
                    }

                    Conversation_ID = parser.node.SelectSingleNode("FirstC").InnerText;
                }
            }
        }

        public void initialize(string C_ID)
        {
            filepath = "Scripts/NPCs/"+FileName;

            Conversation_ID = C_ID;

            parser = new XmlParser();
            //parser.CreateConvXml(filepath, NPC_ID, Name);
            CanParser = parser.LoadXml(filepath, FileName);
        }

        /// <summary>
        /// 저장된 아이디로 파일을 검색하고, 다음 대화내역을 가져온다.
        /// </summary>
        public void LoadConversation(string C_ID)
        {
            //NPC_ID 로 파일 검색 후 Conversation_ID 로 대화 내역 검색 후,
            if (CanParser)
            {
                Conversation_ID = C_ID;
                if (parser.node != null)
                {
                    var tmp = parser.node.SelectSingleNode(Conversation_ID);

                    if (tmp != null)
                    {
                        Name = tmp.SelectSingleNode("Name")?.InnerText ?? null;
                        Wait = int.Parse(tmp.SelectSingleNode("Wait").InnerText);
                        Type = tmp.SelectSingleNode("Type").InnerText[0];
                        Content = tmp.SelectSingleNode("Content")?.InnerText ?? null;
                        CanExit = tmp.SelectSingleNode("CanExit").InnerText.Equals("true");
                        DunGeon_ID = tmp.SelectSingleNode("DunGeon")?.InnerText ?? null;
                        AnimationID = tmp.SelectSingleNode("Anime")?.InnerText ?? null;
                        Condition_ID = tmp.SelectSingleNode("Condition")?.InnerText ?? null;

                        if (Type == 'C')
                        {
                            try
                            {
                                Next_Conversation_ID = tmp.SelectSingleNode("Next_C_ID").InnerText;
                            }
                            catch (Exception)
                            {
                                Next_Conversation_ID = null;
                            }
                        }
                        else
                        {
                            var opts = tmp.SelectSingleNode("Options").InnerText.Split(new string[] { "::" }, System.StringSplitOptions.None);
                            Options = new string[opts.Length];
                            for (int i = 0; i < opts.Length; i++)
                            {
                                Options[i] = opts[i];
                            }

                            var opts_c = tmp.SelectSingleNode("Option_C_ID").InnerText.Split(new string[] { "::" }, System.StringSplitOptions.None);
                            Option_Next_Conversation = new string[opts_c.Length];
                            for (int i = 0; i < opts_c.Length; i++)
                            {
                                Option_Next_Conversation[i] = opts_c[i];
                            }
                        }
                    }
                }
                else

                    Conversation_ID = null;
            }

        }

        /// <summary>
        /// 대화가 시작해야하는 조건
        /// </summary>
        /// <returns></returns>
        public string GetCondition_ID()
        {
            return Condition_ID;
        }

        /// <summary>
        /// 현재 지문이 끝나고 전투가 진행되는 지 확인
        /// </summary>
        /// <returns></returns>
        public string GetDunGeon_ID()
        {
            return DunGeon_ID;
        }

        /// <summary>
        /// 현재 지문에서 나갈 수 있는 지 확인
        /// </summary>
        /// <returns></returns>
        public bool GetExit()
        {
            return CanExit;
        }

        /// <summary>
        /// 다음으로 이어질 대화의 아이디를 가져온다.
        /// </summary>
        /// <returns></returns>
        public string GetNext_Conversation_ID()
        {
            if(Type == 'C') // Normal Conversation
            {
                if (Next_Conversation_ID == null)   // Next Conversation is not exist
                    return null;                
                return Next_Conversation_ID;
            }
            else if(Type == 'O') // Optional Conversation
            {
                if (Option_Next_Conversation == null)   // Next Conversation is not exist
                    return null;
                return Option_Next_Conversation[Choose];
            }
            return null;
        }

        /// <summary>
        /// 현재 대화 스크립트의 아이디를 가져온다.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentConversation_ID()
        {
            return Conversation_ID;
        }

        /// <summary>
        /// 몇 초 동안 기다렸다가 대화를 이어나갈지, 기다리는 시간을 가져온다.
        /// </summary>
        /// <returns></returns>
        public int Waitfor()
        {
            return Wait;
        }

        public string GetAnimeID()
        {
            return AnimationID;
        }

        /// <summary>
        /// 대화의 타입을 가져온다.
        /// </summary>
        /// <returns></returns>
        public new char GetType()
        {
            return Type;
        }

        /// <summary>
        /// 대화 내용 문자열을 가져온다.
        /// </summary>
        /// <returns></returns>
        public string GetContent()
        {
            if (string.IsNullOrWhiteSpace(Content))
                return null;
            else
                return Content;
        }

        /// <summary>
        /// 선택지 문자열을 가져온다.
        /// </summary>
        /// <returns></returns>
        public string[] GetOptions()
        {
            if (Type == 'O') // Optional Conversation
            {
                return Options;
            }
            return null;
        }

        /// <summary>
        /// 선택지를 결정한다.
        /// </summary>
        /// <param name="index"></param>
        public void SetChoosing(int index)
        {
            Choose = index;
        }
    }

    [Serializable]
    public class Dungeon
    {
        [SerializeField]
        public string Dungeon_ID { get; private set; }        //Dungeon_ID
        [SerializeField]
        public string Name { get; private set; }              //Dungeon_Name

        private string filepath;         //Filepath of Dungeon scripts

        private XmlParser parser;
        private bool CanParser = false;

        public Dungeon(string ID)
        {
            Dungeon_ID = ID;
            filepath = "Scripts/Dungeons/" + Dungeon_ID;

            parser = new XmlParser();

            CanParser = parser.LoadXml(filepath, Dungeon_ID);

            if (CanParser)
            {
                if (parser.node != null)
                {
                    Name = parser.node.SelectSingleNode("Name").InnerText;
                }
            }
        }
    }

    [Serializable]
    public class Buff
    {
        public Buff_Code Buff_Code { get; private set; }
        public float Buff_Time { get; private set; } = 0;
        public event Action action;

        [SerializeField]
        private WaitForSeconds WFS = new WaitForSeconds(0.0625f);

        public Buff(Buff_Code Code, float time)
        {
            Buff_Code = Code;
            Buff_Time = time;
        }

        public void SetTime(float time)
        {
            Buff_Time = time;
        }

        public IEnumerator Counter()
        {
            while(true)
            {
                yield return WFS;

                if (Buff_Time < 0)
                    break;

#if DEBUG
                Debug.Log($"{Buff_Time}초간 {Buff_Code}");
#endif

                Buff_Time -= 0.0625f;
            }

            action?.Invoke();
        }
    }

    [Serializable]
    public class ElementClass<TValue>
    {
        public string Key;
        public TValue Value;
    }

    [Serializable]
    public class Saving
    {
        //위치 데이터
        public string Scene_Name;// { get; private set; }
        public Vector2 Position;// { get; private set; }
        public bool InDanger;// { get; private set; }

        //사용자 데이터
        public int Level;// { get; private set; }
        public float MaxHP;// { get; private set; }
        public float HP;// { get; private set; }
        public float MaxStemina;// { get; private set; }
        public float Stemina;// { get; private set; }
        public int MaxMana;//{ get; private set; }
        public int Mana;// { get; private set; }
        public float GrowingMana;//{ get; private set; }
        public ushort SkillUnLocked;// { get; private set; }
        public int Soul;

        [XmlArray]
        public bool[] isinitialized = new bool[6] { false, false, false, false, false, false };

        //현재 상태 데이터
        [XmlIgnore]
        public List<Buff> buffs = new List<Buff>();//{ get; private set; }

        [XmlArray]
        [XmlArrayItem(ElementName = "NPC_Conversation")]
        public List<ElementClass<string>> Text { get; private set; } = new List<ElementClass<string>>();

        [XmlIgnore]
        public Dictionary<string, string> NPC_Conversation
        {
            get { return Text.ToDictionary(x => x.Key, x => x.Value); }
            set { Text = value.Select(x => new ElementClass<string> { Key = x.Key, Value = x.Value }).ToList(); }
        }

        [XmlArray]
        [XmlArrayItem(ElementName = "Conditions")]
        public List<ElementClass<bool>> CText { get; private set; } = new List<ElementClass<bool>>();

        [XmlIgnore]
        public Dictionary<string, bool> Conditions
        {
            get { return CText.ToDictionary(x => x.Key, x => x.Value); }
            set { CText = value.Select(x => new ElementClass<bool> { Key = x.Key, Value = x.Value }).ToList(); }
        }

        [XmlArray]
        [XmlArrayItem(ElementName = "Triggers")]
        public List<ElementClass<TriggerState>> TText { get; private set; } = new List<ElementClass<TriggerState>>();

        [XmlIgnore]
        public Dictionary<string, TriggerState> Triggers
        {
            get { return TText.ToDictionary(x => x.Key, x => x.Value); }
            set { TText = value.Select(x => new ElementClass<TriggerState> { Key = x.Key, Value = x.Value }).ToList(); }
        }

        public ElementClass<float> BGM = null;

        public void SavePosition(string SceneName, Vector3 Character, bool Danger)
        {
            Scene_Name = SceneName;
            Position = new Vector2(Character.x, Character.y);
            InDanger = Danger;

            isinitialized[0] = true;
        }

        public void SaveState(ushort Skill, params float[] vs)
        {
            SkillUnLocked = Skill;

            try { 
                MaxHP = vs[0];
                HP = vs[1];
                MaxStemina = vs[2];
                Stemina = vs[3];
                MaxMana = (int)vs[4];
                Mana = (int)vs[5];
                GrowingMana = vs[6];
                Soul =(int)vs[7];
            } catch (IndexOutOfRangeException)
            { }

            isinitialized[1] = true;
        }

        public void SaveBuffs(List<Buff> bfs)
        {
            buffs = bfs;

            isinitialized[2] = true;
        }

        public void SaveConversation(string NPCID, string Conversation_Code)
        {
            var dictionary = NPC_Conversation;
            if (dictionary.ContainsKey(NPCID))
                 dictionary[NPCID] = Conversation_Code;
            else
                dictionary.Add(NPCID, Conversation_Code);
            NPC_Conversation = dictionary;

            isinitialized[3] = true;
        }

        public void SaveConditions(string Condition_ID, bool Completed)
        {
            var dictionary = Conditions;
            if (dictionary.ContainsKey(Condition_ID))
                dictionary[Condition_ID] = Completed;
            else
                dictionary.Add(Condition_ID, Completed);
            Conditions = dictionary;

            isinitialized[4] = true;
        }

        public void SaveTriggers(ref Dictionary<string, TriggerState> keyValuePairs)
        {
            Triggers = keyValuePairs;

            isinitialized[5] = true;
        }

        public void SaveTriggers(string Trigger_ID, TriggerState state)
        {
            var dictionary = Triggers;
            if (dictionary.ContainsKey(Trigger_ID))
                dictionary[Trigger_ID] = state;
            else
                dictionary.Add(Trigger_ID, state);
            Triggers = dictionary;

            isinitialized[5] = true;
        }

        public void SaveBGM(AudioSource source)
        {
            if (source.isPlaying)
            {
                BGM = new ElementClass<float>();
                BGM.Key = source.clip.name;
                BGM.Value = source.time;
            }
        }

        public void SaveBGM(string name, float time)
        {
            if (name != null)
            {
                BGM = new ElementClass<float>();
                BGM.Key = name;
                BGM.Value = time;
            }
            else
                BGM = null;
        }

        public static bool CanLoadbyKey(string name)
        {
            return PlayerPrefs.HasKey(name);
        }

        public static Saving LoadbyFile()
        {
            var result = Get<Saving>("SaveFile0");
            result.isinitialized = new bool[6]
                {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true
                };
            return result;
        }

        public static void Set<T>(string name, T instance)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, instance);
                PlayerPrefs.SetString(name, System.Convert.ToBase64String(ms.ToArray()));
            }
        }

        public static T Get<T>(string name) where T : new()
        {
            if (!PlayerPrefs.HasKey(name)) return default(T);
            byte[] bytes = System.Convert.FromBase64String(PlayerPrefs.GetString(name));
            using (var ms = new MemoryStream(bytes))
            {
                object obj = new BinaryFormatter().Deserialize(ms);
                return (T)obj;
            }
        }
    }

    [Serializable]
    public class Options
    {
        [XmlIgnore]
        public bool isinitialized = false;
        //키 설정
        public KeyCode Attack_Key { get; private set; }
        public KeyCode Guard_Key { get; private set; }
        public KeyCode Jump_Key { get; private set; }
        public KeyCode Heal_Key { get; private set; }
        public KeyCode Dash_Key { get; private set; }
        public KeyCode Interact_Key { get; private set; }
        public KeyCode GhostMode_Key { get; private set; }

        public void SaveKeys(Dictionary<string, KeyCode> keys)
        {
            Attack_Key = keys["attack"];
            Guard_Key = keys["guard"];
            Jump_Key = keys["jump"];
            Heal_Key = keys["heal"];
            Dash_Key = keys["dash"];
            Interact_Key = keys["Interact"];
            GhostMode_Key = keys["ghostmode"];

            isinitialized = true;
        }
    }

    public static class DictionaryXML
    {
        public static void Serialize(TextWriter writer, IDictionary dictionary)
        {
            List<Entry> entries = new List<Entry>(dictionary.Count);
            foreach (object key in dictionary.Keys)
            {
                entries.Add(new Entry(key, dictionary[key]));
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
            serializer.Serialize(writer, entries);
        }
        public static void Deserialize(TextReader reader, IDictionary dictionary)
        {
            dictionary.Clear();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
            List<Entry> list = (List<Entry>)serializer.Deserialize(reader);
            foreach (Entry entry in list)
            {
                dictionary[entry.Key] = entry.Value;
            }
        }
        public class Entry
        {
            public object Key;
            public object Value;
            public Entry()
            {
            }

            public Entry(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }
    }

    [Serializable]
    public class TriggerState
    {
        public string SceneName { get; set; }
        public string Trigger_ID { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 localScale { get; set; }
        public string Parent_ID { get; set; }

        public bool IsActivated { get; set; }

        public void SaveState(GameObject gameObject)
        {
            var transform = gameObject.transform;
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;

            SceneName = SceneManager.GetActiveScene().name;
            Trigger_ID = transform.name;

            Parent_ID = transform?.parent?.name ?? null;
            IsActivated = gameObject.activeInHierarchy;
        }
    }

    public class IEEE754
    {
        public enum Formats { binary16, binary32, binary64, binary128, binary256 };

        public Formats GetFormats { get; private set; }

        bool sign;
        bool[] Significand_bits;
        bool[] Exponent_bits; // signed = 0

        public IEEE754(bool s, bool[] S_b, bool[] E_b)
        {
            sign = s;

            Significand_bits = new bool[S_b.LongLength];
            Exponent_bits = new bool[E_b.LongLength];

            S_b.CopyTo(Significand_bits,0);
            E_b.CopyTo(Exponent_bits, 0);
        }

        public IEEE754(string value = null ,Formats formats = Formats.binary64)
        {
            GetFormats = formats;

            // Formats.binary64
            int S_bits = 53, E_bits = 11;
            switch(formats)
            {
                case Formats.binary16:
                    S_bits = 11;
                    E_bits = 5;
                    break;
                case Formats.binary32:
                    S_bits = 24;
                    E_bits = 8;
                    break;
                case Formats.binary128:
                    S_bits = 113;
                    E_bits = 15;
                    break;
                case Formats.binary256:
                    S_bits = 237;
                    E_bits = 19;
                    break;
            }

            Significand_bits = new bool[S_bits];
            Exponent_bits = new bool[E_bits];
        }

        public IEEE754(ref IEEE754 iEEE)
        {
            GetFormats = iEEE.GetFormats;

            Significand_bits = new bool[iEEE.Significand_bits.LongLength];
            Exponent_bits = new bool[iEEE.Exponent_bits.LongLength];

            sign = iEEE.sign;
            iEEE.Significand_bits.CopyTo(Significand_bits, 0);
            iEEE.Exponent_bits.CopyTo(Exponent_bits, 0);
        }

        public IEEE754(ref IEEE754 iEEE, Formats formats)
        {
            var tmp = new IEEE754(iEEE.ToString(), formats);

            Significand_bits = new bool[tmp.Significand_bits.LongLength];
            Exponent_bits = new bool[tmp.Exponent_bits.LongLength];

            sign = tmp.sign;
            tmp.Significand_bits.CopyTo(Significand_bits, 0);
            tmp.Exponent_bits.CopyTo(Exponent_bits, 0);
        }

        public static IEEE754 operator +(IEEE754 a) => a;
        public static IEEE754 operator -(IEEE754 a)
        {
            a.sign = !a.sign;
            return new IEEE754(ref a);
        }

        public static IEEE754 operator +(IEEE754 a, IEEE754 b)
        {
            IEEE754 a_256 = (a.GetFormats == Formats.binary256)
                ? a : new IEEE754(ref a, Formats.binary256);
            IEEE754 b_256 = (b.GetFormats == Formats.binary256)
                ? b : new IEEE754(ref b, Formats.binary256);

            var a_e = ArrayToInt(ref a.Exponent_bits);
            var b_e = ArrayToInt(ref b.Exponent_bits);

            bool sign = false;
            bool[] Significand_bits = new bool[a.Significand_bits.LongLength];
            bool[] Exponent_bits = new bool[a.Exponent_bits.LongLength];

            if (a_e > b_e)
            {
                var gap = a_e - b_e;
                Shift(ref b_256.Significand_bits, gap);

                a_256.Exponent_bits.CopyTo(Exponent_bits, 0);
            }
            else
            {
                var gap = b_e - a_e;
                Shift(ref a_256.Significand_bits, gap);

                b_256.Exponent_bits.CopyTo(Exponent_bits, 0);
            }

            Minus(a_256.sign, ref a_256.Significand_bits);
            Minus(b_256.sign, ref b_256.Significand_bits);
            var tmp = Add(ref a_256.Significand_bits, ref b_256.Significand_bits);

            sign = tmp[0];
            Minus(sign, ref tmp);
            tmp.CopyTo(Significand_bits, 0);

            return new IEEE754(sign, Significand_bits, Exponent_bits);
        }

        public static IEEE754 operator -(IEEE754 a, IEEE754 b)
            => a + (-b);

        public static void Shift(ref bool[] array, long vector)
        {
            if (vector > 0)
            {
                for (long i = array.LongLength - 1 - vector; i >= 0; i--)
                {
                    array[i + vector] = array[i];
                }
                for (long i = 0; i < vector; i++)
                    array[i] = false;
            }
            else if(vector < 0)
            {
                for (long i = vector; i < array.LongLength; i++)
                {
                    array[i - vector] = array[i];
                }
                for (long i = array.LongLength - 1; i > array.LongLength - 1 - vector; i--)
                    array[i] = false;
            }
        }

        public static uint ArrayToUint(ref bool[] array)
        {
            uint result = 0;
            for(long i = 0; i < array.LongLength; i++)
            {
                result |= (uint)(array[i] ? 1 : 0);
                result <<= 1;
            }

            return result;
        }

        public static int ArrayToInt(ref bool[] array)
        {
            int result = 0;

            if (array[0]) // -
            {
                result = -1;
                for (long i = 1; i < array.LongLength; i++)
                {
                    result &= (array[i] ? 1 : 0);
                    result <<= 1;
                }
            }
            else
            {
                for (long i = 1; i < array.LongLength; i++)
                {
                    result |= (array[i] ? 1 : 0);
                    result <<= 1;
                }
            }

            return result;
        }

        public static void ToArray(ref bool[] array, long value)
        {
            for (long i = array.LongLength - 1; i >= 0 ; i--)
            {
                array[i] = (value & 1) == 1;
                value >>= 1;
            }
        }

        public static bool[] Add(ref bool[] a, ref bool[] b)
        {
            bool C = false;
            bool[] result = new bool[a.LongLength];
            for(long i = a.LongLength - 1; i >= 0; i-- )
            {
                result[i] = a[i] ^ b[i] ^ C;
                C = (a[i] & b[i]) | (C & (a[i] ^ b[i]));
            }

            return result;
        }

        public static void Minus(bool sign, ref bool[] a)
        {
            if(sign)
            {
                for(long i = 0; i < a.LongLength - 1; i++)
                {
                    a[i] = !a[i];
                }

                var b = new bool[a.LongLength];
                b[b.LongLength - 1] = true;

                Add(ref a, ref b).CopyTo(a, 0);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
