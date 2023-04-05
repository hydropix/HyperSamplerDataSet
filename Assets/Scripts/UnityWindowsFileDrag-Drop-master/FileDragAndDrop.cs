using System.Collections.Generic;
using UnityEngine;
using B83.Win32;
using UnityEngine.UI;
using System.IO;

[RequireComponent(typeof(InputField))]
public class FileDragAndDrop : MonoBehaviour
{
    private InputField inputField;

    private void OnEnable()
    {
        inputField = GetComponent<InputField>();
        
        //Install the hook
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }

    private void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    private void OnFiles(List<string> aFiles, POINT aPos)
    {
        //We only care about the first file
        FileAttributes attributes = File.GetAttributes(aFiles[0]);
        
        //Detect whether its a directory or file
        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            inputField.text = aFiles[0];
        else
            inputField.text = aFiles[0].Substring(0, aFiles[0].LastIndexOf("\\"));
    }
}