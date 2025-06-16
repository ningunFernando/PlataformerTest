using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AssetOrganizerWelcomeWindow : EditorWindow
{
    private bool _dontShowAgain;
    private const string kShownKey = AssetOrganizerWelcomeInitializer.WelcomeShownKey;

    public static void ShowWindow()
    {
        var w = GetWindow<AssetOrganizerWelcomeWindow>(true, "Welcome: Asset Organizer", true);
        w.position = new Rect(
            Screen.width / 2f - 200, 
            Screen.height / 2f - 100,
            600, 250
        );
    }

    void OnGUI()
    {
        GUILayout.Space(16);
        EditorGUILayout.LabelField("Global Asset Organizer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This assistant helps you:\n" +
            "  • Create standard project folders (Models, Textures, Scripts, etc.).\n" +
            "  • Preload default folder mappings for the Asset Organizer.\n" +
            "  • Open an empty organizer without any preset mappings.\n" +
            "\n" +
            "In the Asset Organizer window, drag a folder from the Project view into each 'Target Folder' field.\n" +
            "Use name prefixes like 'T_' (textures), 'M_' (Materials), 'SG_' (Shadergraph), etc.\n" +
            "When two or more assets share the same prefix and keyword, they will be grouped into a subfolder named after that keyword.",
            MessageType.Info
        );

        GUILayout.Space(32);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Blank Organizer", GUILayout.Height(28)))
        {
            GlobalAssetOrganizer.ShowWindow();
            Close();
        }

        if (GUILayout.Button("Create Project Folders & Organizer", GUILayout.Height(28)))
        {
            ProjectFolderCreator.CreateProjectFolders();
            
            var baseMappings = new List<MappingEntry>
            {
                new MappingEntry {
                    prefix     = "T_",
                    folderPath = "Assets/Textures",
                    extensions = new[]{".png", ".jpg", ".jpeg"}
                },
                new MappingEntry {
                    prefix     = "",
                    folderPath = "Assets/Prefabs",
                    extensions = new[]{".prefab"}
                },
                new MappingEntry {
                    prefix     = "",
                    folderPath = "Assets/Scripts",
                    extensions = new[]{".cs"}
                },
                new MappingEntry
                {
                    prefix     = "SG_",
                    folderPath = "Assets/Shaders",
                    extensions = new[]{".shadergraph" , ".shader", ".compute"}
                },
                new MappingEntry
                {
                    prefix     = "AUD_",
                    folderPath = "Assets/Audios",
                    extensions = new[]{".mp3", ".flac"}
                },
                new MappingEntry
                {
                prefix     = "",
                folderPath = "Assets/Assets",
                extensions = new[]{".gltf", ".obj", ".fbx"}
                },
                new MappingEntry
                {
                    prefix     = "VFX",
                    folderPath = "Assets/VFX",
                    extensions = new[]{".vfx"}
                },
                new MappingEntry
                {
                    prefix     = "ANIM",
                    folderPath = "Assets/Animations",
                    extensions = new[]{".controller"}
                },
                
                
            };
            var baseIgnored = new string[]{ "Plugins", "Editor", "Resources", "AssetOrganizer" };
            
            GlobalAssetOrganizer.PresetMappings(
                baseMappings,
                baseIgnored,
                defaultOrganizeParticles: true,
                defaultShowLogs: false
            );

            EditorUtility.DisplayDialog(
                "Template Configured",
                "The base folders have been created and the initial mapping has been loaded.",
                "¡GREAT!"
            );
            Close();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        _dontShowAgain = EditorGUILayout.ToggleLeft("Don't show this again", _dontShowAgain);
        if (GUI.changed && _dontShowAgain)
            EditorPrefs.SetBool(kShownKey, true);
    }
}



