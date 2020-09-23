using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Vintecc.TortoiseGitForUnity.UserInterface
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandLabelStyle;
        public static readonly GUIStyle commandButtonStyleMid;
        public static readonly GUIStyle commandButtonStyleLeft;
        public static readonly GUIStyle commandButtonStyleRight;

        public static readonly GUIStyle commandMiniPopupStyle;
        
        private const float height = 22;
        private const float width = 34;
        private const int padding = 2;

        static ToolbarStyles()
        {
            var pd = new RectOffset(padding, padding, padding, padding);
            var margin = new RectOffset(0, 0, 3, 0);
            var h = height - 3;

            commandLabelStyle = new GUIStyle()
            {
                fixedHeight = h,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 2, 0),
            };

            commandButtonStyleMid = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = h,
                fixedWidth = width,
                padding = pd,
                margin = margin,
                /*fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold*/
            };
            commandButtonStyleLeft = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = h,
                fixedWidth = width,
                padding = pd,
                margin = margin,
            };
            commandButtonStyleRight = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = h,
                fixedWidth = width,
                padding = pd,
                margin = margin,
            };
            
            commandMiniPopupStyle = new GUIStyle(EditorStyles.toolbarPopup)
            {
                fixedHeight = h,
                fixedWidth = 17,
                margin = margin,
            };
        }
    }

    [InitializeOnLoad]
    public class TortoiseGitForUnityLoader
    {
        // .. FIELDS

        private const string TortoiseGitForUnityEnableMenuPath = "Tools/Enable TortoiseGitForUnity";
        private const string TortoiseGitForUnityEnabledKey = "TortoiseGitForUnityEnabled";

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(TortoiseGitForUnityEnabledKey, false);
            set
            {
                EditorPrefs.SetBool(TortoiseGitForUnityEnabledKey, value);
                Menu.SetChecked(TortoiseGitForUnityEnableMenuPath, value);
                UpdateEnabled();
            }
        }
        private static TortoiseGitForUnityWindow window;

        // .. INITIALIZATION

        static TortoiseGitForUnityLoader()
        {
            UpdateEnabled();
        }

        // .. OPERATIONS

        private static void UpdateEnabled()
        {
            if (Enabled && window == null)
            {
                window = new TortoiseGitForUnityWindow();
                ToolbarExtender.RightToolbarGUI.Add(window.OnToolBarGUI);
            }
            else if (!Enabled && window != null)
            {
                ToolbarExtender.RightToolbarGUI.Remove(window.OnToolBarGUI);
                window = null;
            }
        }

        [MenuItem(TortoiseGitForUnityEnableMenuPath)]
        private static void Toggle()
        {
            Enabled = !Enabled;
        }
    }

    public class TortoiseGitForUnityWindow
    {
        // .. FIELDS

        private const string RepositoryListKey = "TortoiseGitForUnityRepositories";
        private const string NoReposValue = "No repos!";
        private const string AssetPath = "Packages/com.vintecc.tortoisegit-for-unity/Editor Resources/";

        private string[] repositoryPaths = new string[0];
        private int selectedRepositoryIndex = 0;

        private GUIContent tortoiseIcon;
        private GUIContent logIcon;
        private GUIContent fetchIcon;
        private GUIContent commitIcon;

        private GUILayoutOption repoDropdownMinWidth;

        // .. INITIALIZATION

        public TortoiseGitForUnityWindow()
        {
            InitResources();
            RefreshRepositories();
        }

        private void InitResources()
        {
            var add = EditorGUIUtility.isProSkin ? "_Light" : "";
            
            var tortoiseTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + "TortoiseGit.png", typeof(Texture2D));
            var logTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + $"Search{add}.png", typeof(Texture2D));
            var fetchTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + $"Sync{add}.png", typeof(Texture2D));
            var commitTex = (Texture2D) AssetDatabase.LoadAssetAtPath(AssetPath + $"Upload{add}.png", typeof(Texture2D));

            tortoiseIcon = new GUIContent(tortoiseTex, "TortoiseGitForUnity");
            logIcon = new GUIContent(logTex, "Log");
            fetchIcon = new GUIContent(fetchTex, "Fetch");
            commitIcon = new GUIContent(commitTex, "Commit");

            repoDropdownMinWidth = GUILayout.Width(20);
            
            Debug.Log("TortoiseGit Resources Initialized");
        }

        private void RefreshRepositories()
        {
            var storedRepos = EditorPrefs.GetString(RepositoryListKey, string.Empty);

            if (string.IsNullOrEmpty(storedRepos))
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

                EditorPrefs.SetString(RepositoryListKey, storedRepos);
            }

            if (storedRepos == NoReposValue)
            {
                repositoryPaths = new string[0];
                return;
            }

            repositoryPaths = storedRepos.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToArray();
        }

        // .. OPERATIONS

        public void OnToolBarGUI()
        {
            GUILayout.Space(10);

            GUILayout.Label(tortoiseIcon, ToolbarStyles.commandLabelStyle);

            if (GUILayout.Button(logIcon, ToolbarStyles.commandButtonStyleLeft))
                ExecuteCommand(TortoiseGitRunner.Command.Log);
            if (GUILayout.Button(commitIcon, ToolbarStyles.commandButtonStyleMid))
                ExecuteCommand(TortoiseGitRunner.Command.Commit);
            if (GUILayout.Button(fetchIcon, ToolbarStyles.commandButtonStyleMid))
                ExecuteCommand(TortoiseGitRunner.Command.Fetch);

            selectedRepositoryIndex = EditorGUILayout.Popup( "", selectedRepositoryIndex, repositoryPaths, ToolbarStyles.commandMiniPopupStyle);
            
            GUILayout.FlexibleSpace();
        }

        void OnGUI()
        {
            /*if(GUILayout.Button("test"))
                RefreshRepositories();*/

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            

            /*
            
            if (GUILayout.Button(logIcon, btnWidth, btnHeight))
                ExecuteCommand(TortoiseGitRunner.Command.Log);
            if (GUILayout.Button(commitIcon, btnWidth, btnHeight))
                ExecuteCommand(TortoiseGitRunner.Command.Commit);
            if (GUILayout.Button(fetchIcon, btnWidth, btnHeight))
                ExecuteCommand(TortoiseGitRunner.Command.Fetch);
            */
            GUILayout.EndHorizontal();
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