using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class RDialogue : MonoBehaviour
{
    [SerializeField] private PDialogue dialoguePagePrefab;
    [HideInInspector] public PDialogue dialoguePage;

    private Dictionary<string,EventDialogue> events=new Dictionary<string, EventDialogue>();
    private Dictionary<string,Texture2D> pictures=new Dictionary<string, Texture2D>();
    private Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
    private Dictionary<string, Texture2D> backgrounds = new Dictionary<string, Texture2D>();
    public Dictionary<string, string> convert = new Dictionary<string, string>();

    public EventDialogue currentEvent;
    public string currentCharacter="";
    public int currentIndex;

    private string beginning = "";

    private void Start()
    {
        //��ȡ�����ļ�
        ReadSettingFile();
        //��ȡͼƬ
        ReadPicture();
        //��ȡ�¼��ļ�
        ReadEventFiles();
        //��ȡת���ַ���
        GetConvertString();
        //��ȡͼ���ļ�
        ReadIcons();
        //��ȡ����
        ReadBackgrounds();
        SCommand.Declare(@"���� �¼� Ϊ \S+?", SetCurrentEvent);
        SCommand.Declare(@"��ʾ ��һ��", SetNextContent);
        SCommand.Execute("���� �¼� Ϊ "+beginning);

    }

    //��ȡ�����ļ�
    public void ReadSettingFile()
    {
        string content = File.ReadAllText(Application.streamingAssetsPath + "/" + "�����ļ�.ini");
        var contents = content.Split('\n');
        for(int i = 0; i < contents.Length; i++)
        {
            var slices = contents[i].Split(':','��');
            Debug.Log(slices[0]);
            if (slices[0] == "��ʼ�¼�")
            {
                beginning = slices[1];
            }
        }
    }
    public void SetContent(string value,CommandData commandData)
    {
        string[] values = value.Split(' ');
        if (dialoguePage == null)
        {
            dialoguePage = Instantiate(dialoguePagePrefab, FindObjectOfType<Canvas>().transform);
        }
        dialoguePage.content.text = values[1];
    }

    public void SetNextContent(string value,CommandData commandData)
    {
        if (dialoguePage == null)
        {
            dialoguePage = Instantiate(dialoguePagePrefab, FindObjectOfType<Canvas>().transform);
        }

        if (dialoguePage.isTextPlaying)
        {
            dialoguePage.isTextPlaying = false;
            dialoguePage.SetTextImmediate();
            return;
        }


        currentIndex++;
        if (currentIndex < currentEvent.content.Count)
        {
            SetDialogueText(currentEvent.content[currentIndex]);
        }
        else
        {
            Destroy(dialoguePage.gameObject);
        }

    }

    public void SetCurrentEvent(string value, CommandData commandData)
    {
        string[] values=value.Split(' ');
        SetEventWithDisplay(values[3]);
        
    }

    public void CloseDialogue(string value,CommandData commandData)
    {
        Destroy(dialoguePage.gameObject);
    }

    //����

    //�ļ���ȡ
    public void ReadEventFile(string fileName)
    {
        string eventText= File.ReadAllText(fileName);
        EventDialogue eventDialogue = new EventDialogue(eventText);
        //Debug.Log(fileName.Split('\\', '/','.')[^2]);
        events.Add(fileName.Split('\\','/','.')[^2], eventDialogue);
    }

    //��ȡת���ַ���
    public void ReadCovertStringFile(string fileName)
    {
        string convert_string = File.ReadAllText(fileName);
        string[] convert_strings = convert_string.Split('\n');
        for(int i=0;i< convert_strings.Length; i++)
        {
            
            string[] convert_strings_slices = convert_strings[i].Split(',', '��');
            convert.Add(convert_strings_slices[0], convert_strings_slices[1]);
            //Debug.LogFormat("����ת���ַ�����{0}to{1}", convert_strings_slices[0], convert_strings_slices[1]);
        }
    }

    //�����ļ���
    public void ReadEventFiles()
    {
        string path = Application.streamingAssetsPath+"/Events";
        DirectoryInfo folder = new DirectoryInfo(path);
        foreach (FileInfo file in folder.GetFiles(@"*.txt",SearchOption.AllDirectories))
        {
            //Debug.LogFormat("��ȡ�����ļ�·����{0}",file.FullName);
            ReadEventFile(file.FullName);
        }
    }

    public void GetConvertString()
    {
        string path = Application.streamingAssetsPath + "/Convert";
        DirectoryInfo folder = new DirectoryInfo(path);
        foreach (FileInfo file in folder.GetFiles(@"*.txt", SearchOption.AllDirectories))
        {
            //Debug.LogFormat("��ȡת���ַ����ļ�·����{0}",file.FullName);
            ReadCovertStringFile(file.FullName);
        }
    }

    public void SetDialogueText(string content)
    {
        string[] slices = content.Split('��',':');

        UnityAction next = delegate
        {
            currentIndex++;
            SetDialogueText(currentEvent.content[currentIndex]);
        };

        if (slices.Length == 1)
        {
            Debug.Log("ִ���ַ�������:" + slices[0]);
            if (slices[0][0] == '#')
            {
                string[] command = slices[0].Split('#');
                //Debug.LogFormat("{0} {1}", command[1], command[1].Length);
                switch (command[1].Trim('\0'))
                {
                    case "��ת":
                        SetEventWithDisplay(command[2]);
                        break;
                    case "��":
                        SetShake();
                        next();
                        break;
                    case "ѡ��":
                        SetSelection(command);
                        break;
                    case "��ʾ����":
                        if (command.Length == 4)
                            DisplayPictures(command[2], "0", "0");
                        else if (command.Length == 5)
                        {
                            DisplayPictures(command[2], command[3], "0");
                        }
                        else if (command.Length == 6)
                            DisplayPictures(command[2], command[3], command[4]);
                        next();
                        break;
                    case "����λ��":
                        if (command.Length == 5)
                            SetPicturesPosition(command[2],new Vector3(float.Parse(command[3]), 0, 0));
                        else if(command.Length == 6)
                            SetPicturesPosition(command[2],new Vector3(float.Parse(command[3]), float.Parse(command[4]), 0));
                        break;
                    case "�ƶ�����":
                        if (command.Length == 5)
                            MovePictures(command[2],command[3], "0", "0.5");
                        else if (command.Length == 6)
                            MovePictures(command[2],command[3], command[4], "0.5");
                        else if (command.Length == 7)
                            MovePictures(command[2],command[3], command[4], command[5]);
                        next();
                        break;
                    case "��������":
                        HiddenPictures(command[2]);
                        next();
                        break;
                    case "����ͼ��":
                        HiddenIcon();
                        next();
                        break;
                    case "����":
                        DisplayIcon("Perceive", "flicker");
                        if (command.Length > 3 && command[2]=="����")
                            next();
                        break;
                    case "��ʾ����":
                        DisplayBackground(command[2]);
                        next();
                        break;
                    case "���س���":
                        HiddenBackground();
                        next();
                        break;
                    case "ת��Ч��":
                        TransitionSceneEffect();
                        next();
                        break;
                    case "��ʾ�Ի�":
                        DisplayDialogue();
                        break;
                    case "���ضԻ�":
                        HiddenDialogue();
                        break;
                }

            }
            else
            {

                dialoguePage.character.text = currentCharacter;
                dialoguePage.SetText(slices[0]);
            }

        }
        else if(slices.Length==2)
        {
            currentCharacter = slices[0];

            dialoguePage.character.text = slices[0];
            dialoguePage.SetText(slices[1]);
        }
    }


    public void SetEventWithDisplay(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            currentEvent = events[eventName];
            currentIndex = 0;
            if (dialoguePage == null)
            {
                dialoguePage = Instantiate(dialoguePagePrefab, FindObjectOfType<Canvas>().transform);
            }
            SetDialogueText(currentEvent.content[currentIndex]);
        }
    }

    public void ReadPicture()
    {
        string path = Application.streamingAssetsPath+"/Pictures";
        DirectoryInfo folder = new DirectoryInfo(path);
        foreach (FileInfo file in folder.GetFiles(@"*.png", SearchOption.AllDirectories))
        {
            pictures.Add(file.FullName.Split('\\', '/', '.')[^2], GetTexture2D(file.FullName));
            //Debug.Log(file.FullName.Split('\\', '/', '.')[^2]);
        }
    }

    public void ReadIcons()
    {
        string path = Application.streamingAssetsPath + "/Icons";
        DirectoryInfo folder = new DirectoryInfo(path);
        foreach (FileInfo file in folder.GetFiles(@"*.png", SearchOption.AllDirectories))
        {
            icons.Add(file.FullName.Split('\\', '/', '.')[^2], GetTexture2D(file.FullName));
            //Debug.Log(file.FullName.Split('\\', '/', '.')[^2]);
        }
    }

    public void ReadBackgrounds()
    {
        string path = Application.streamingAssetsPath + "/Backgrounds";
        DirectoryInfo folder = new DirectoryInfo(path);
        foreach (FileInfo file in folder.GetFiles(@"*.png", SearchOption.AllDirectories))
        {
            backgrounds.Add(file.FullName.Split('\\', '/', '.')[^2], GetTexture2D(file.FullName));
            //Debug.Log(file.FullName.Split('\\', '/', '.')[^2]);
        }
    }

    public Texture2D GetTexture2D(string imgPath)
    {

        FileStream files = new FileStream(imgPath, FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();

        Texture2D tex = new(100, 100);
        tex.LoadImage(imgByte);
        tex.Apply();
        return tex;
    }

    //��
    public async void SetShake()
    {
        RectTransform rtransform = dialoguePage.transform.GetComponent<RectTransform>();
        for(int i = 0; i < 20; i++)
        {
            rtransform.localPosition -= Vector3.right*5;
            await new WaitForFixedUpdate();
            rtransform.localPosition += Vector3.right * 5;
            await new WaitForFixedUpdate();
        }
    }

    //ѡ��
    public void SetSelection(string[] command)
    {
        dialoguePage.SetSelection(command);
        dialoguePage.enableClick = false;
    }

    //��ʾ���棨֧�ֶ�����棩
    public void DisplayPictures(string character,string pos_x,string pos_y)
    {
        if (pictures.ContainsKey(character))
        {
            var picture = Instantiate(dialoguePage.picturePrefab,dialoguePage.pictureParent);
            dialoguePage.pictures.Add(character, picture);
            picture.color = new Color(1, 1, 1, 1);
            picture.sprite = Sprite.Create(pictures[character], new Rect(0, 0, pictures[character].width, pictures[character].height), Vector2.zero);
            picture.rectTransform.localPosition = new Vector3(float.Parse(pos_x), float.Parse(pos_y), 0);

        }
    }

    //��ʾ����
    public void DisplayPicture(string character,string pos_x,string pos_y)
    {
        if (convert.ContainsKey(character) && pictures.ContainsKey(convert[character]))
        {
            dialoguePage.picture.color = new Color(1, 1, 1, 1);
            dialoguePage.picture.sprite = Sprite.Create(pictures[convert[character]], new Rect(0, 0, pictures[convert[character]].width, pictures[convert[character]].height), Vector2.zero);
            Debug.Log(pos_x);
            dialoguePage.picture.rectTransform.localPosition = new Vector3(float.Parse(pos_x),float.Parse(pos_y),0);
        }
    }

    //�����ƶ�(�ƶ������ϵĳ���֧�ֶ������)
    public async void MovePictures(string character,string x, string y, string t)
    {
        Debug.LogFormat("����x��λ��:{0};����ֵ����:{1}", x, x.Length);
        Vector3 direction = new Vector3(float.Parse(x), float.Parse(y));
        float totalTime = float.Parse(t);
        Vector3 targetPosition = dialoguePage.pictures[character].rectTransform.localPosition + direction;
        Vector3 startPosition = dialoguePage.pictures[character].rectTransform.localPosition;
        float currentTime = 0;
        while (currentTime < totalTime)
        {
            dialoguePage.pictures[character].rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, currentTime / totalTime);
            currentTime += Time.deltaTime;
            await new WaitForUpdate();
        }
    }
    //�����ƶ�(�ƶ������ϵĳ���)
    public async void MovePicture(string x,string y,string t)
    {
        Debug.LogFormat("����x��λ��:{0};����ֵ����:{1}",x,x.Length);
        Vector3 direction = new Vector3(float.Parse(x), float.Parse(y));
        float totalTime =float.Parse(t);
        Vector3 targetPosition = dialoguePage.picture.rectTransform.localPosition+direction;
        Vector3 startPosition = dialoguePage.picture.rectTransform.localPosition;
        float currentTime=0;
        while (currentTime < totalTime)
        {
            dialoguePage.picture.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, currentTime / totalTime);
            currentTime+=Time.deltaTime;
            await new WaitForUpdate();
        }
    }

    //��������λ��(֧�ֶ������)
    public void SetPicturesPosition(string character,Vector3 pos)
    {
        dialoguePage.pictures[character].rectTransform.localPosition = pos;
    }
    //��������λ��
    public void SetPicturePosition(Vector3 pos)
    {
        dialoguePage.picture.rectTransform.localPosition = pos;
    }

    //��������(֧�ֶ������)
    public void HiddenPictures(string character)
    {
        dialoguePage.pictures[character].color = new Color(1, 1, 1, 0);
        Destroy(dialoguePage.pictures[character].gameObject);
        dialoguePage.pictures.Remove(character);
    }
    //��������
    public void HiddenPicture()
    {
        dialoguePage.picture.color = new Color(1, 1, 1, 0);
    }

    //��ʾ����
    public async void DisplayIcon(string iconName,string aniName)
    {
        dialoguePage.icon.sprite = Sprite.Create(icons[iconName], new Rect(0, 0, icons[iconName].width, icons[iconName].height), Vector2.zero);

        if (aniName!=null)
        {
            dialoguePage.icon.color = new Color(1, 1, 1, 1);
        }
        if (aniName == "flicker")
        {
            dialoguePage.icon.color = new Color(1, 1, 1, 0);
            await new WaitForSeconds(0.05f);
            dialoguePage.icon.color = new Color(1, 1, 1, 1);
            await new WaitForSeconds(0.05f);
            dialoguePage.icon.color = new Color(1, 1, 1, 0);
            await new WaitForSeconds(0.05f);
            dialoguePage.icon.color = new Color(1, 1, 1, 1);
            await new WaitForSeconds(0.05f);
            dialoguePage.icon.color = new Color(1, 1, 1, 0);
            await new WaitForSeconds(0.05f);
            dialoguePage.icon.color = new Color(1, 1, 1, 1);
        }
    }

    public void HiddenIcon()
    {
        dialoguePage.icon.color=new Color(1, 1, 1, 0);
    }

    public void DisplayBackground(string backgroundName)
    {
        dialoguePage.scene.sprite = Sprite.Create(backgrounds[backgroundName], new Rect(0, 0, backgrounds[backgroundName].width, backgrounds[backgroundName].height), Vector2.zero);
        dialoguePage.scene.gameObject.SetActive(true);
    }

    public void HiddenBackground()
    {
        dialoguePage.scene.gameObject.SetActive(false);
    }

    public void DisplayDialogue()
    {
        dialoguePage.dialogue.gameObject.SetActive(true);
    }

    public void HiddenDialogue()
    {
        dialoguePage.dialogue.gameObject.SetActive(false);
    }

    public async void TransitionSceneEffect()
    {
        var transition=dialoguePage.transition;
        transition.gameObject.SetActive(true);
        dialoguePage.enableClick = false;
        dialoguePage.dialogue.gameObject.SetActive(false);
        while (transition.color.a < 1)
        {
            transition.color +=Color.black * Time.deltaTime;
            await new WaitForUpdate();
        }
        await new WaitForSeconds(0.3f);
        while (transition.color.a > 0)
        {
            transition.color -= Color.black * Time.deltaTime;
            await new WaitForUpdate();
        }
        transition.gameObject.SetActive(false);
        dialoguePage.dialogue.gameObject.SetActive(true);
        dialoguePage.enableClick = true;
    }
}

public class EventDialogue
{
    public List<string> content=new List<string>();
    public EventDialogue(string content)
    {
        string[] contents = content.Split('\n');
        for (int i = 0; i < contents.Length; i++)
        {

            this.content.Add(contents[i]);
        }
    }
}

public class EventDialogueElement
{
    public string character;
    public string content;
}
