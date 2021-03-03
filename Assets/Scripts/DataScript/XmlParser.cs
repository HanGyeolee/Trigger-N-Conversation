using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using DataSet;

public class XmlParser
{
    readonly static string savepath = "Save/";

    XmlDocument xmlDoc;

    public XmlNode node;

    public XmlParser()
    {
        xmlDoc = new XmlDocument();
    }

    public bool LoadXml(string filepath, string ID)
    {
        try
        {
            TextAsset textAsset = (TextAsset)Resources.Load(filepath);
            xmlDoc.LoadXml(textAsset.text);

            string tmp = ID;
            if (ID.Contains(" "))
                tmp = ID.Replace(" ", "_");
            node = xmlDoc.SelectSingleNode(tmp);

            if (node == null)
                return false;
        }
        catch(Exception)
        {
            return false;
        }
        return true;
    }

    public void CreateSaveFile(Saving s)
    {
        xmlDoc = new XmlDocument();
        //Xml을 선언한다(xml의 버전과 인코딩 방식을 정해준다.)
        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));

        //// 루트 노드 생성
        XmlNode root = xmlDoc.CreateNode(XmlNodeType.Element, "root", string.Empty);
        xmlDoc.AppendChild(root);
        AddAttribute(ref root, attr: "Scene_Name", innerText: s.Scene_Name);

        XmlNode pos_node = AddNode(ref root, "Position");
        AddAttribute(ref pos_node, attr: "x", innerText: s.Position.x.ToString());
        AddAttribute(ref pos_node, attr: "y", innerText: s.Position.y.ToString());

        AddAttribute(ref root, attr: "InDanger", innerText: s.InDanger.ToString());

        XmlNode initi_node = AddNode(ref root, "isinitialize");
        root.AppendChild(initi_node);

        for(int i = 0; i < s.isinitialized.Length; i++)
            AddAttribute(ref initi_node, attr:  "init_" + i.ToString(), innerText: s.isinitialized[i].ToString());


        AddAttribute(ref root, attr: "Level", innerText: s.Level.ToString());
        AddAttribute(ref root, attr: "MaxHp", innerText: s.MaxHP.ToString());
        AddAttribute(ref root, attr: "Hp", innerText: s.HP.ToString());
        AddAttribute(ref root, attr: "MaxStemina", innerText: s.MaxStemina.ToString());
        AddAttribute(ref root, attr: "Stemina", innerText: s.Stemina.ToString());
        AddAttribute(ref root, attr: "MaxMana", innerText: s.MaxMana.ToString());
        AddAttribute(ref root, attr: "Mana", innerText: s.Mana.ToString());
        AddAttribute(ref root, attr: "GrowingMana", innerText: s.GrowingMana.ToString());
        AddAttribute(ref root, attr: "Soul", innerText: s.Soul.ToString());

        AddAttribute(ref root, attr: "SkillUnLocked", innerText: s.SkillUnLocked.ToString());

        XmlNode npc_node = AddNode(ref root, "NPC_Conversation");
        for (int i = 0; i < s.Text.Count; i++)
        {
            AddAttribute(ref npc_node,key: s.Text[i].Key, attr: "ID", innerText: s.Text[i].Value);
        }

        XmlNode condition_node = AddNode(ref root, "Conditions");
        for (int i = 0; i < s.CText.Count; i++)
        {
            AddAttribute(ref condition_node, key: s.CText[i].Key, attr: "ID", innerText: s.CText[i].Value.ToString());
        }

        XmlNode trigger_node = AddNode(ref root, "Triggers");
        for (int i = 0; i < s.TText.Count; i++)
        {
            XmlNode tmp_node = AddNode(ref trigger_node, "a" + i.ToString());
            var item = s.TText[i].Value;

            AddAttribute(ref tmp_node, attr: "Scene_Name", innerText: item.SceneName);
            AddAttribute(ref tmp_node, attr: "Trigger_ID", innerText: item.Trigger_ID);

            XmlNode posit_node = AddNode(ref tmp_node, "position");
            AddAttribute(ref posit_node, attr: "x", innerText: item.position.x.ToString());
            AddAttribute(ref posit_node, attr: "y", innerText: item.position.y.ToString());
            AddAttribute(ref posit_node, attr: "z", innerText: item.position.z.ToString());

            XmlNode quate_node = AddNode(ref tmp_node, "rotation");
            AddAttribute(ref quate_node, attr: "x", innerText: item.rotation.x.ToString());
            AddAttribute(ref quate_node, attr: "y", innerText: item.rotation.y.ToString());
            AddAttribute(ref quate_node, attr: "z", innerText: item.rotation.z.ToString());
            AddAttribute(ref quate_node, attr: "w", innerText: item.rotation.w.ToString());

            XmlNode scale_node = AddNode(ref tmp_node, "localScale");
            AddAttribute(ref scale_node, attr: "x", innerText: item.localScale.x.ToString());
            AddAttribute(ref scale_node, attr: "y", innerText: item.localScale.y.ToString());
            AddAttribute(ref scale_node, attr: "z", innerText: item.localScale.z.ToString());

            if (item.Parent_ID != null)
                AddAttribute(ref tmp_node, attr: "Parent_ID", innerText: item.Parent_ID);
            AddAttribute(ref tmp_node, attr: "IsActivated", innerText: item.IsActivated.ToString());
        }

        XmlNode bgm_node = AddNode(ref root, "BGM");
        if ((s.BGM?.Key ?? null) != null)
        {
            AddAttribute(ref bgm_node, attr: "ClipName", innerText: s.BGM.Key);
            AddAttribute(ref bgm_node, attr: "ClipTime", innerText: s.BGM.Value.ToString());
        }

        //AddAttribute(ref root, ToStr<KeyCode>(), attr: "Attack_Key", innerText: s.Attack_Key.ToString());
        //AddAttribute(ref root, ToStr<KeyCode>(), attr: "Guard_Key", innerText: s.Guard_Key.ToString());
        //AddAttribute(ref root, ToStr<KeyCode>(), attr: "Jump_Key", innerText: s.Jump_Key.ToString());
        //AddAttribute(ref root, ToStr<KeyCode>(), attr: "Dash_Key", innerText: s.Dash_Key.ToString());
        //AddAttribute(ref root, ToStr<KeyCode>(), attr: "Interact_Key", innerText: s.Interact_Key.ToString());
        //AddAttribute(ref root, ToStr<KeyCode>(), attr: "GhostMode_Key", innerText: s.GhostMode_Key.ToString());

        xmlDoc.Save("./Assets/Resources/" + savepath + "save.xml");
        xmlDoc = new XmlDocument();

        //var serialize = new XmlSerializer(typeof(Saving));
        //var stream = new FileStream(asset + "Save/save.xml",FileMode.Create);
        //serialize.Serialize(stream, s);
        //stream.Close();
    }

    public void LoadSaveFile(out Saving container)
    {
        TextAsset textXML = (TextAsset)Resources.Load("Save/save");
        if(textXML.text != null)
            xmlDoc.LoadXml(textXML.text);
        container = new Saving();

        XmlNode root = xmlDoc.SelectSingleNode("root");

        if(root != null)
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Scene_Name":
                        container.Scene_Name = node.InnerText;
                        break;
                    case "Position":
                        Vector2 v = new Vector2();
                        foreach (XmlNode pos in node.ChildNodes)
                        {
                            if (pos.Name == "x")
                                v.x = float.Parse(pos.InnerText);
                            else
                                v.y = float.Parse(pos.InnerText);
                        }
                        container.Position = v;
                        break;
                    case "InDanger":
                        container.InDanger = bool.Parse(node.InnerText);
                        break;
                    case "Level":
                        container.Level = int.Parse(node.InnerText);
                        break;
                    case "MaxHp":
                        container.MaxHP = float.Parse(node.InnerText);
                        break;
                    case "Hp":
                        container.HP = float.Parse(node.InnerText);
                        break;
                    case "MaxStemina":
                        container.MaxStemina = float.Parse(node.InnerText);
                        break;
                    case "Stemina":
                        container.Stemina = float.Parse(node.InnerText);
                        break;
                    case "MaxMana":
                        container.MaxMana = int.Parse(node.InnerText);
                        break;
                    case "Mana":
                        container.Mana = int.Parse(node.InnerText);
                        break;
                    case "GrowingMana":
                        container.GrowingMana = float.Parse(node.InnerText);
                        break;
                    case "Soul":
                        container.Soul = int.Parse(node.InnerText);
                        break;
                    case "SkillUnLocked":
                        container.SaveState(ushort.Parse(node.InnerText));
                        break;
                    case "NPC_Conversation":
                        foreach (XmlNode pos in node.ChildNodes)
                        {
                            XmlNode key = pos.Attributes.GetNamedItem("key");
                            container.SaveConversation(key.Value, pos.InnerText);
                        }
                        break;
                    case "Conditions":
                        foreach (XmlNode pos in node.ChildNodes)
                        {
                            XmlNode key = pos.Attributes.GetNamedItem("key");
                            container.SaveConditions(key.Value, bool.Parse(pos.InnerText));
                        }
                        break;
                    case "Triggers":
                        foreach (XmlNode pos in node.ChildNodes)
                        {
                            XmlNode trigger = pos;

                            TriggerState state = new TriggerState();
                            state.SceneName = trigger.SelectSingleNode("Scene_Name").InnerText;
                            state.Trigger_ID = trigger.SelectSingleNode("Trigger_ID").InnerText;

                            var posit = trigger.SelectSingleNode("position");
                            Vector3 position = new Vector3(
                                float.Parse(posit.SelectSingleNode("x").InnerText),
                                float.Parse(posit.SelectSingleNode("y").InnerText),
                                float.Parse(posit.SelectSingleNode("z").InnerText)
                                );
                            state.position = position;

                            var rotate = trigger.SelectSingleNode("rotation");
                            Quaternion rotation = new Quaternion(
                                float.Parse(rotate.SelectSingleNode("x").InnerText),
                                float.Parse(rotate.SelectSingleNode("y").InnerText),
                                float.Parse(rotate.SelectSingleNode("z").InnerText),
                                float.Parse(rotate.SelectSingleNode("w").InnerText)
                                );
                            state.rotation = rotation;

                            var scale = trigger.SelectSingleNode("localScale");
                            Vector3 localScale = new Vector3(
                                float.Parse(scale.SelectSingleNode("x").InnerText),
                                float.Parse(scale.SelectSingleNode("y").InnerText),
                                float.Parse(scale.SelectSingleNode("z").InnerText)
                                );
                            state.localScale = localScale;

                            state.Parent_ID = trigger.SelectSingleNode("Parent_ID")?.InnerText ?? null;
                            state.IsActivated = bool.Parse(trigger.SelectSingleNode("IsActivated").InnerText);

                            container.SaveTriggers(state.Trigger_ID, state);
                        }
                        break;
                    case "BGM":
                        ElementClass<float> tmp = new ElementClass<float>();
                        tmp.Key = node.SelectSingleNode("ClipName")?.InnerText ?? null;
                        tmp.Value = float.Parse(node.SelectSingleNode("ClipTime")?.InnerText ?? "0");

                        container.SaveBGM(tmp.Key, tmp.Value);
                        break;
                    case "isinitialize":
                        List<bool> initialize = new List<bool>();
                        for(int i = 0; i < node.ChildNodes.Count; i++)
                            initialize.Add(bool.Parse(node.SelectSingleNode("init_" + i.ToString()).InnerText));
                            
                        container.isinitialized = initialize.ToArray(); ;
                        break;
                }
            }
    }

    public bool SaveFileExist()
    {
        try
        {
            TextAsset textXML = (TextAsset)Resources.Load("Save/save");
            xmlDoc.LoadXml(textXML.text);

            node = xmlDoc.SelectSingleNode("root");

            if (node == null)
                return false;
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public bool DeleteFile(string s = "./Assets/Resources/Save/save.xml")
    {
        try
        {
            if (File.Exists(s))
            {
                File.Delete(s);
                return true;
            }
            return false;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public void CreateOptionFile(Options o)
    {
        var serialize = new XmlSerializer(typeof(Options));
        var stream = new FileStream("options.xml", FileMode.Create);
        serialize.Serialize(stream, o);
        stream.Close();
    }

    public Options LoadOptionFile()
    {
        var serializer = new XmlSerializer(typeof(Options));
        var stream = new FileStream("options.xml", FileMode.Open);
        var container = serializer.Deserialize(stream) as Options;
        stream.Close();

        return container;
    }

    public bool OptionFileExist()
    {
        return File.Exists("options.xml");
    }


    /// <summary>
    /// Attribute를 노드에 추가하는 함수
    /// </summary>
    /// <param name="node"></param>
    /// <param name="attr"></param>
    /// <param name="innerText"></param>
    public XmlNode AddAttribute(ref XmlNode node,string type_name = null,string key=null, string attr= null, params string[] innerText)
    {
        XmlElement Attr = xmlDoc.CreateElement(attr);
        if(type_name != null)
            Attr.SetAttribute("type", type_name);
        if (key != null)
            Attr.SetAttribute("key", key);
        if (innerText != null)
        {
            Attr.InnerText = innerText[0];
            for (int i = 1; i < innerText.Length; i++)
            {
                Attr.InnerText += "::";
                Attr.InnerText += innerText[i];
            }
        }
        node.AppendChild(Attr);
        return node;
    }

    /// <summary>
    /// node를 추가하는 함수
    /// </summary>
    /// <param name="node"></param>
    /// <param name="attr"></param>
    /// <param name="innerText"></param>
    public XmlNode AddNode(ref XmlNode node, string name)
    {
        XmlNode child = xmlDoc.CreateNode(XmlNodeType.Element, name, string.Empty);
        node.AppendChild(child);
        return child;
    }

    public string ToStr<T>()
    {
        return typeof(T).ToString();
    }
}
