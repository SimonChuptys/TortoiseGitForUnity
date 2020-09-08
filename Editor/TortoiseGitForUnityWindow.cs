using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vintecc.TortoiseGitForUnity.UserInterface
{
    public class TortoiseGitForUnityWindow : EditorWindow
    {
        // .. FIELDS

        private const string AssetPath = "Packages/com.vintecc.tortoisegit-for-unity/Editor Resources/";
        private const int RepoDropdownMinWidth = 100;
        private const int BtnWidth = 35;

        private string[] repositoryPaths = new string[0];

        private int selectedRepositoryIndex = 0;

        private bool resourcesInitialized;
        private GUIContent logIcon;
        private GUIContent fetchIcon;
        private GUIContent commitIcon;

        private GUILayoutOption repoDropdownMinWidth;
        private GUILayoutOption btnWidth;
        private GUILayoutOption btnHeight;

        // .. INITIALIZATION

        [MenuItem("Tools/TortoiseGit for Unity")]
        public static void ShowWindow()
        {
            var w = GetWindow(typeof(TortoiseGitForUnityWindow)) as TortoiseGitForUnityWindow;
            var gitIcon = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + "TortoiseGit.png", typeof(Texture2D));
            w.titleContent = new GUIContent("TortoiseGit for Unity", gitIcon, "TortoiseGit for Unity");
            w.minSize = new Vector2(BtnWidth * 3 + RepoDropdownMinWidth, 25);

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

            repoDropdownMinWidth = GUILayout.MinWidth(RepoDropdownMinWidth);
            btnWidth = GUILayout.Width(BtnWidth);
            btnHeight = GUILayout.Height(20);
            
            RefreshRepositories();
        }

        // .. OPERATIONS

        void OnGUI()
        {
            InitResources();

            /*if(GUILayout.Button("test"))
                RefreshRepositories();*/
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            selectedRepositoryIndex = EditorGUILayout.Popup("", selectedRepositoryIndex, repositoryPaths, repoDropdownMinWidth);

            if (GUILayout.Button(logIcon, btnWidth, btnHeight))
                ExecuteCommand(TortoiseGitRunner.Command.Log);
            if (GUILayout.Button(commitIcon, btnWidth, btnHeight))
                ExecuteCommand(TortoiseGitRunner.Command.Commit);
            if (GUILayout.Button(fetchIcon, btnWidth, btnHeight))
                ExecuteCommand(TortoiseGitRunner.Command.Fetch);

            GUILayout.EndHorizontal();
        }

        private void RefreshRepositories()
        {
            var repos = new List<string>();
            var unityProjectDir = Directory.GetCurrentDirectory();
            
            var pathsToCheck = new List<string>
            {
                Directory.GetParent(unityProjectDir).FullName, //unity project parent dir
                unityProjectDir, //unity project dir
            };
            
            pathsToCheck.AddRange(Directory.GetDirectories(unityProjectDir + @"\Packages")); //unity project packages

            var assetsDir = unityProjectDir + @"\Assets";
            pathsToCheck.Add(assetsDir); //unity assets directory
            
            pathsToCheck.AddRange(GetAllSubDirs(assetsDir)); //all directories inside unity assets directory

            foreach (var path in pathsToCheck)
            {
                if(CheckIsRepository(path))
                    repos.Add(path);
            }
            repositoryPaths = repos.ToArray();
        }

        private List<string> GetAllSubDirs(string dir)
        {
            var allSubDirs = new List<string>();

            var directSubs = Directory.GetDirectories(dir);
            foreach (var subDir in directSubs)
            {
                allSubDirs.Add(subDir);
                allSubDirs.AddRange(GetAllSubDirs(subDir));
            }

            return allSubDirs;
        }

        private bool CheckIsRepository(string dir)
        {
            const string gitFolder = "/.git";
            var fullPath = Path.GetFullPath(dir + gitFolder);
            return Directory.Exists(fullPath);
        }

        private void ExecuteCommand(TortoiseGitRunner.Command cmd)
        {
            TortoiseGitRunner.Do(cmd, GetSelectedRepositoryPath());
        }

        private string GetSelectedRepositoryPath()
        {
            return Path.GetFullPath("Packages/TortoiseGitForUnity");
        }
    }
}