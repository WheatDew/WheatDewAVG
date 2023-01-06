using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PDialogue : MonoBehaviour,IPointerClickHandler
{
    public TMP_Text content,character;
    public Image picture,icon,scene;
    public PSelection selectionPrefab;
    public Transform selectionParent;
    public Image dialogue;
    public Image transition;

    public bool enableClick;
    [HideInInspector] public bool isTextPlaying = false;
    [HideInInspector] public string textBuffer;



    public void SetText(string text)
    {
        textBuffer = text;
        StartCoroutine(SetTextFunction());
    }

    IEnumerator SetTextFunction()
    {
        isTextPlaying = true;

        int n = 0;
        while (n < textBuffer.Length)
        {
            n++;
            content.text = textBuffer[..n];
            yield return new WaitForSeconds(0.02f);
        }
        isTextPlaying = false;
    }

    public void SetTextImmediate()
    {
        StopAllCoroutines();
        isTextPlaying = false;
        content.text = textBuffer;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(enableClick)
            SCommand.Execute("显示 下一句");
    }

    public void SetSelection(string[] command)
    {
        for(int i = 3; i < command.Length; i += 2)
        {
            PSelection selection = Instantiate(selectionPrefab, selectionParent);
            selection.page = this;
            selection.content.text = command[i-1];
            selection.SetClick(string.Format("设置 事件 为 {0}", command[i]));
        }
    }

    public void ClearSelection()
    {
        for (int i = 0; i < selectionParent.childCount; i++)
        {
            Destroy(selectionParent.GetChild(i).gameObject);
        }
    }
}
