using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vintecc.TortoiseGitForUnity.UserInterface
{
    public class GitFunctionsWindow : EditorWindow
    {
        // .. FIELDS

        private const string AssetPath = "Packages/com.vintecc.tortoisegit-for-unity/Editor Resources/";

        private string[] repositories = new string[0];
        private int selectedRepositoryIndex = 0;

        private bool resourcesInitialized;
        private GUIContent logIcon;
        private GUIContent fetchIcon;
        private GUIContent commitIcon;

        private GUILayoutOption btnWidth;
        private GUILayoutOption btnHeight;

        // .. INITIALIZATION

        [MenuItem("Vintecc/TortoiseGitForUnity/Git Commands Window")]
        public static void ShowWindow()
        {
            var w = GetWindow(typeof(GitFunctionsWindow)) as GitFunctionsWindow;

            var gitIcon = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + "TortoiseGit.png", typeof(Texture2D));
            w.titleContent = new GUIContent("TortoiseGit for Unity", gitIcon, "TortoiseGit for Unity");

            w.minSize = new Vector2(35 * 3 + 100, 25);

            w.RefreshRepositories();
        }

        private void InitResources()
        {
            if (btnWidth != null)
                return;

            var logTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + "ShowLog_Light.png", typeof(Texture2D));
            var fetchTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + "Fetch.png", typeof(Texture2D));
            var commitTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + "Commit.png", typeof(Texture2D));

            logIcon = new GUIContent(logTex, "Log");
            fetchIcon = new GUIContent(fetchTex, "Fetch");
            commitIcon = new GUIContent(commitTex, "Commit");

            btnWidth = GUILayout.Width(35);
            btnHeight = GUILayout.Height(20);
        }

        // .. OPERATIONS

        void OnGUI()
        {
            InitResources();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            selectedRepositoryIndex = EditorGUILayout.Popup("", selectedRepositoryIndex, repositories);

            if (GUILayout.Button(logIcon, btnWidth, btnHeight))
                TortoiseGitRunner.Do(TortoiseGitRunner.Command.Log, GetSelectedRepositoryPath());

            if (GUILayout.Button(commitIcon, btnWidth, btnHeight))
                TortoiseGitRunner.Do(TortoiseGitRunner.Command.Commit, GetSelectedRepositoryPath());

            if (GUILayout.Button(fetchIcon, btnWidth, btnHeight))
                TortoiseGitRunner.Do(TortoiseGitRunner.Command.Fetch, GetSelectedRepositoryPath());

            GUILayout.EndHorizontal();
        }

        private void RefreshRepositories()
        {
            repositories = new[]
            {
                @"TortoiseGitForUnity\Packages\TortoiseGitForUnity",
                @"TortoiseGitForUnity\Assets\NestedRepository",
                @"TortoiseGitForUnity",
            };
        }

        private string GetSelectedRepositoryPath()
        {
            return Path.GetFullPath("Packages/TortoiseGitForUnity");
        }
    }
}