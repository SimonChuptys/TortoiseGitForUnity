using UnityEditor;
using Vintecc.TortoiseGitForUnity;

[InitializeOnLoad]
public class TortoiseGitForUnityDefaultInitialization
{
    private const string TortoiseGitForUnityEnableMenuPath = "Tools/Enable TortoiseGitForUnity";

    // .. INITIALIZATION

    static TortoiseGitForUnityDefaultInitialization()
    {
        TortoiseGitForUnityLoader.InitializeDefaults();
    }

    // .. OPERATIONS

    [MenuItem(TortoiseGitForUnityEnableMenuPath)]
    private static void ToggleTortoiseGitForUnity()
    {
        Menu.SetChecked(TortoiseGitForUnityEnableMenuPath, TortoiseGitForUnityLoader.ToggleEnabled());
    }
}
