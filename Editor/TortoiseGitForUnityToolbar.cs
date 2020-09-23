using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vintecc.TortoiseGitForUnity.UnityToolbarExtender;

namespace Vintecc.TortoiseGitForUnity
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle CommandButtonStyleMid;
        public static readonly GUIStyle CommandButtonStyleLeft;
        public static readonly GUIStyle CommandButtonStyleRight;
        public static readonly GUIStyle CommandMiniPopupStyle;

        private const float Height = 20;
        private const float Width = 34;
        private const int Padding = 3;

        static ToolbarStyles()
        {
            var pd = new RectOffset(Padding, Padding, Padding, Padding);

            CommandButtonStyleMid = new GUIStyle("AppCommandMid")
            {
                padding = pd,
            };

            CommandButtonStyleLeft = new GUIStyle("AppCommandLeft")
            {
                padding = pd,
            };
            CommandButtonStyleRight = new GUIStyle("AppCommandRight")
            {
                padding = pd,
            };

            CommandMiniPopupStyle = new GUIStyle( /*EditorStyles.toolbarPopup*/"DropDown")
            {
                fixedHeight = 22,
                fixedWidth = 20,
            };
        }
    }

    public static class TortoiseGitForUnityLoader
    {
        // .. FIELDS

        private const string TortoiseGitForUnityEnabledKey = "TortoiseGitForUnityEnabled";

        public static bool IsVisible
        {
            get => EditorPrefs.GetBool(TortoiseGitForUnityEnabledKey, false);
            set
            {
                EditorPrefs.SetBool(TortoiseGitForUnityEnabledKey, value);
                UpdateVisibility();
            }
        }

        private static TortoiseGitForUnityToolbar window;

        // .. INITIALIZATION

        /// <summary>
        /// Call from InitializeOnLoad to initialize TortoiseGitForUnity 
        /// </summary>
        public static void InitializeDefaults()
        {
            UpdateVisibility();
        }

        // .. OPERATIONS

        /// <summary>
        /// Toggle the TortoiseGitForUnity toolbar on/off
        /// </summary>
        /// <returns>Whether to toolbar is currently toggled on</returns>
        public static bool ToggleEnabled()
        {
            var isVisible = !IsVisible;
            IsVisible = isVisible;
            return isVisible;
        }

        private static void UpdateVisibility()
        {
            var isVisible = IsVisible;
            var isWindowNull = window == null;

            if (isVisible && isWindowNull)
            {
                window = new TortoiseGitForUnityToolbar();
                ToolbarExtender.LeftToolbarGUI.Add(window.OnToolBarGUI);
            }
            else if (!isVisible && !isWindowNull)
            {
                ToolbarExtender.LeftToolbarGUI.Remove(window.OnToolBarGUI);
                window = null;
            }
        }
    }

    public class TortoiseGitForUnityToolbar
    {
        // .. FIELDS

        private const string RepositoryListKey = "TortoiseGitForUnityRepositories";
        private const string NoReposValue = "No repos!";
        private const string AssetPath = "Packages/com.vintecc.tortoisegit-for-unity/Editor Resources/";

        private string[] repositoryPaths = new string[0];
        private int selectedRepositoryIndex = 0;

        private GUIContent logIcon;
        private GUIContent fetchIcon;
        private GUIContent commitIcon;

        // .. INITIALIZATION

        public TortoiseGitForUnityToolbar()
        {
            InitResources();
            RefreshRepositories();
        }

        private void InitResources()
        {
            var add = EditorGUIUtility.isProSkin ? "_Light" : "";

            var logTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + $"Search{add}.png", typeof(Texture2D));
            var fetchTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + $"Sync{add}.png", typeof(Texture2D));
            var commitTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + $"Upload{add}.png", typeof(Texture2D));

            logIcon = new GUIContent(logTex, "Log");
            fetchIcon = new GUIContent(fetchTex, "Fetch");
            commitIcon = new GUIContent(commitTex, "Commit");

            //Debug.Log("TortoiseGit Resources Initialized");
        }

        private void RefreshRepositories(bool force = false)
        {
            var storedRepos = PlayerPrefs.GetString(RepositoryListKey, string.Empty);

            if (force || string.IsNullOrEmpty(storedRepos))
            {
                Debug.Log("[TortoiseGitForUnity] Scanning for repositories.");
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
                    if (CheckIsRepository(path))
                        storedRepos += $"{path};";
                }

                if (string.IsNullOrEmpty(storedRepos))
                    storedRepos = NoReposValue;

                PlayerPrefs.SetString(RepositoryListKey, storedRepos);
            }

            const string refreshRepos = "Scan for repositories...";
            if (storedRepos == NoReposValue)
            {
                repositoryPaths = new string[] {NoReposValue, refreshRepos};
                return;
            }
            
            var repos = storedRepos.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToList();
            repos.Add(refreshRepos);
            repositoryPaths = repos.ToArray();
        }

        // .. OPERATIONS

        public void OnToolBarGUI()
        {
            GUILayout.Space(35);

            GUILayout.BeginVertical();
            GUILayout.Space(-4);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(logIcon, ToolbarStyles.CommandButtonStyleLeft))
                ExecuteCommand(TortoiseGitRunner.Command.Log);
            if (GUILayout.Button(commitIcon, ToolbarStyles.CommandButtonStyleMid))
                ExecuteCommand(TortoiseGitRunner.Command.Commit);
            if (GUILayout.Button(fetchIcon, ToolbarStyles.CommandButtonStyleRight))
                ExecuteCommand(TortoiseGitRunner.Command.Fetch);

            GUILayout.Space(-3);

            GUILayout.BeginVertical();
            GUILayout.Space(4);
            selectedRepositoryIndex = EditorGUILayout.Popup("", selectedRepositoryIndex, repositoryPaths, ToolbarStyles.CommandMiniPopupStyle);
            if (selectedRepositoryIndex == repositoryPaths.Length - 1)
            {
                RefreshRepositories(true);
                selectedRepositoryIndex = 0;
            }
            
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
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
            if (repositoryPaths == null || selectedRepositoryIndex >= repositoryPaths.Length)
                return string.Empty;
            return repositoryPaths[selectedRepositoryIndex];
        }
    }
}