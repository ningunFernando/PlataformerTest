using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class MappingEntry
{
    public string prefix;
    public string folderPath;
    public string[] extensions;
}

public class GlobalAssetOrganizer : EditorWindow
{
    
    
    // — UI settings (persisted) —
    [SerializeField] private List<MappingEntry> mappings = new List<MappingEntry>();
    [SerializeField] private string[] ignoredFolders = new string[0];
    [SerializeField] private bool organizeParticlePrefabs = true;
    [SerializeField] private bool showDetailedLogs = true;
    private Vector2 _mappingsScroll;
    private Vector2 _ignoredScroll;

    private SerializedObject so;
    private ReorderableList mappingsList;
    private ReorderableList ignoredList;

    private const string kPrefsKey = "GlobalAssetOrganizer.UISettings";
    private static GlobalAssetOrganizer _instance;

    [MenuItem("Tools/Asset Organizer")]
    public static void ShowWindow()
    {
        _instance = GetWindow<GlobalAssetOrganizer>("Asset Organizer");
        _instance.minSize = new Vector2(450, 350);
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey(kPrefsKey))
        {
            try
            {
                var json = EditorPrefs.GetString(kPrefsKey);
                var data = JsonUtility.FromJson<UISettings>(json);
                if (data != null)
                {
                    mappings               = data.mappings ?? new List<MappingEntry>();
                    ignoredFolders         = data.ignoredFolders ?? new string[0];
                    organizeParticlePrefabs= data.organizeParticlePrefabs;
                    showDetailedLogs       = data.showDetailedLogs;
                }
            }
            catch { }
        }

        so = new SerializedObject(this);

        // Mappings list
        var propMap = so.FindProperty(nameof(mappings));
        mappingsList = new ReorderableList(so, propMap, true, true, true, true);
        mappingsList.drawHeaderCallback = r => EditorGUI.LabelField(r, "Folder Mappings");
        mappingsList.drawElementCallback = (r, i, _, _) =>
        {
            var elt      = propMap.GetArrayElementAtIndex(i);
            var preProp  = elt.FindPropertyRelative("prefix");
            var pathProp = elt.FindPropertyRelative("folderPath");
            var extProp  = elt.FindPropertyRelative("extensions");
            float h = EditorGUIUtility.singleLineHeight, p = 4f;

            // Prefix
            EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, h), preProp);

            // Folder ObjectField
            var cur = AssetDatabase.LoadAssetAtPath<DefaultAsset>(pathProp.stringValue);
            var nw  = (DefaultAsset)EditorGUI.ObjectField(
                new Rect(r.x, r.y+h+p, r.width, h),
                "Target Folder",
                cur,
                typeof(DefaultAsset),
                false
            );
            if (nw != cur)
                pathProp.stringValue = nw != null
                    ? AssetDatabase.GetAssetPath(nw)
                    : "";

            // Extensions
            float eh = EditorGUI.GetPropertyHeight(extProp);
            EditorGUI.PropertyField(
                new Rect(r.x, r.y+2*(h+p), r.width, eh),
                extProp, true
            );
        };
        mappingsList.elementHeightCallback = _ =>
        {
            var e = propMap.GetArrayElementAtIndex(0).FindPropertyRelative("extensions");
            float h = EditorGUIUtility.singleLineHeight, p = 2f;
            return h + p + h + p + EditorGUI.GetPropertyHeight(e) + 4;
        };

        // Ignored folders list
        var propIgn = so.FindProperty(nameof(ignoredFolders));
        ignoredList = new ReorderableList(so, propIgn, true, true, true, true);
        ignoredList.drawHeaderCallback = r =>
            EditorGUI.LabelField(r, "Ignored Folders (relative to Assets/)");
        ignoredList.drawElementCallback = (r, i, _, _) =>
        {
            var elt = propIgn.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(
                new Rect(r.x, r.y+2, r.width, EditorGUIUtility.singleLineHeight),
                elt, GUIContent.none
            );
        };
        ignoredList.elementHeightCallback = _ => EditorGUIUtility.singleLineHeight + 4;
    }



    private void OnGUI()
    {
        so.Update();

        // —— Scrollable Mappings List ——
        EditorGUILayout.LabelField("Folder Mappings", EditorStyles.boldLabel);
        _mappingsScroll = EditorGUILayout.BeginScrollView(
            _mappingsScroll,
            GUILayout.Height(600)
        );
        mappingsList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // —— Scrollable Ignored Folders List ——
        EditorGUILayout.LabelField("Ignored Folders", EditorStyles.boldLabel);
        _ignoredScroll = EditorGUILayout.BeginScrollView(
            _ignoredScroll,
            GUILayout.Height(200)
        );
        ignoredList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        organizeParticlePrefabs = EditorGUILayout.Toggle(
            "Organize Particle Prefabs", organizeParticlePrefabs);

        EditorGUILayout.Space();
        if (GUILayout.Button("Organize Assets", GUILayout.Height(30)))
        {
            so.ApplyModifiedProperties();
            OrganizeAllAssets();
        }

        so.ApplyModifiedProperties();
    }


    private void OnDisable()
    {
        so.ApplyModifiedProperties();
        var data = new UISettings {
            mappings                = mappings,
            ignoredFolders          = ignoredFolders,
            organizeParticlePrefabs = organizeParticlePrefabs,
            showDetailedLogs        = showDetailedLogs
        };
        EditorPrefs.SetString(kPrefsKey, JsonUtility.ToJson(data));
    }

    public static void PresetMappings(
        List<MappingEntry> defaultMappings,
        string[] defaultIgnored,
        bool defaultOrganizeParticles,
        bool defaultShowLogs)
    {
        ShowWindow();
        _instance.mappings               = defaultMappings;
        _instance.ignoredFolders         = defaultIgnored;
        _instance.organizeParticlePrefabs= defaultOrganizeParticles;
        _instance.showDetailedLogs       = defaultShowLogs;
        _instance.so = new SerializedObject(_instance);
        _instance.so.ApplyModifiedProperties();
        _instance.Repaint();
    }

    private void OrganizeAllAssets()
    {
        // Prefab pre-scan
        var psGuids = AssetDatabase.FindAssets("t:Prefab t:ParticleSystem");
        var psPaths = new HashSet<string>(
            psGuids.Select(g => AssetDatabase.GUIDToAssetPath(g))
        );

        // Collect candidates
        var all = AssetDatabase.FindAssets("");
        var cands = new List<Candidate>();
        var counts = new Dictionary<string,int>(StringComparer.Ordinal);
        foreach (var g in all)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (!p.StartsWith("Assets/") ||
                AssetDatabase.IsValidFolder(p) ||
                p.Contains("/Editor/") ||
                IsIgnored(p)) continue;

            string name = Path.GetFileNameWithoutExtension(p);
            string dot  = Path.GetExtension(p).ToLowerInvariant();
            string norm = dot.TrimStart('.');

            foreach (var m in mappings)
            {
                var allow = new HashSet<string>(
                    m.extensions.Select(e => e.TrimStart('.').ToLowerInvariant())
                );
                if (allow.Count>0 && !allow.Contains(norm)) continue;

                if (name.StartsWith(m.prefix, StringComparison.OrdinalIgnoreCase))
                {
                    string kw = name.Substring(m.prefix.Length)
                                    .Split(new[]{'_', '-', ' '},
                                           StringSplitOptions.RemoveEmptyEntries)
                                    .FirstOrDefault();
                    cands.Add(new Candidate{ assetPath = p, entry = m, keyword = kw });
                    if (!string.IsNullOrEmpty(kw))
                    {
                        string key = m.folderPath + "|" + kw;
                        counts[key] = counts.GetValueOrDefault(key) + 1;
                    }
                    break;
                }
            }
        }

        // Move grouped
        int moved = 0;
        var movedSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in cands)
        {
            string baseF = Normalize(c.entry.folderPath);
            string tgt   = baseF;
            if (!string.IsNullOrEmpty(c.keyword))
            {
                string key = c.entry.folderPath + "|" + c.keyword;
                if (counts.TryGetValue(key, out int cnt) && cnt>1)
                    tgt = $"{baseF}/{c.keyword}";
            }
            EnsureFolder(tgt);
            moved += MoveAsset(c.assetPath, tgt);
            movedSet.Add(c.assetPath);
        }

        // Particle fallback
        if (organizeParticlePrefabs)
        {
            foreach (var g in all)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (movedSet.Contains(p)) continue;
                if (Path.GetExtension(p).ToLowerInvariant()==".prefab" &&
                    psPaths.Contains(p))
                {
                    string pf = Normalize("Assets/Particles");
                    EnsureFolder(pf);
                    moved += MoveAsset(p, pf);
                }
            }
        }
        AssetDatabase.Refresh();
    }

    // Helpers & types

    [Serializable]
    private class UISettings
    {
        public List<MappingEntry> mappings;
        public string[] ignoredFolders;
        public bool organizeParticlePrefabs;
        public bool showDetailedLogs;
    }

    [Serializable]
    private class Candidate
    {
        public string assetPath;
        public MappingEntry entry;
        public string keyword;
    }

    private bool IsIgnored(string path)
    {
        return ignoredFolders.Any(ig =>
            path.StartsWith(Normalize("Assets/" + ig) + "/", StringComparison.OrdinalIgnoreCase)
        );
    }

    private static string Normalize(string p)
    {
        p = p.Replace("\\","/").Trim().TrimEnd('/');
        return p.StartsWith("Assets/") ? p : "Assets/" + p.TrimStart('/');
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        var parts = folder.Split('/');
        string acc = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = acc + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(acc, parts[i]);
            acc = next;
        }
    }

    private int MoveAsset(string src, string tgt)
    {
        string file = Path.GetFileName(src);
        string dst  = $"{tgt}/{file}";
        string r    = AssetDatabase.MoveAsset(src, dst);

        if (string.IsNullOrEmpty(r))
        {
            if (showDetailedLogs);
            return 1;
        }
        else if (r.ToLowerInvariant().Contains("same"))
        {
            if (showDetailedLogs);
            return 0;
        }
        else
        {
            return 0;
        }
    }
}
