using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProjectFolderCreator : EditorWindow
{
    public static void CreateProjectFolders()
    {
        string[] folders = {
            "Animations",
            "Audios",
            "Materials",
            "Assets",
            "Prefabs",
            "Scripts",
            "Shaders",
            "Textures",
            "VFX"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + folder))
            {
                AssetDatabase.CreateFolder("Assets", folder);
            }
        }
        AssetDatabase.Refresh();
    }
}