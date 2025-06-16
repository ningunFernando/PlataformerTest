using UnityEditor;

[InitializeOnLoad]
static class AssetOrganizerWelcomeInitializer
{
    public const string WelcomeShownKey = "GlobalAssetOrganizer.WelcomeShown";

    static AssetOrganizerWelcomeInitializer()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool(WelcomeShownKey, false))
                AssetOrganizerWelcomeWindow.ShowWindow();
        };
    }
}