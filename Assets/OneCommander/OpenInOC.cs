// #define OPENINOC_SHOW_CONTEXT_MENU

// Uncomment the above line to show context menu options. Unfortunately, Unity only allows disabling context menu items, not hiding them (/creating them dynamically). Therefore, we can't allow the user to change the context menu options at runtime, and as such, the settings are hard-coded in the code. If you want to change the context menu options, you have to do it here.
// UPDATE: This is now added to the settings menu, but required re-compilation to take effect. This is because the context menu must be defined at compile time, and cannot be changed at runtime.

using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace OneCommander {

    [InitializeOnLoad]
    public static class OpenInOC {

        static readonly Lazy<Texture>
            _FolderIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("Folder Icon").image),
            _FileIcon   = new Lazy<Texture>(() => EditorGUIUtility.IconContent("TextAsset Icon").image),
            _EyeIcon    = new Lazy<Texture>(() => EditorGUIUtility.IconContent("d_ViewToolOrbit On").image);

        static readonly Lazy<GUIContent>
            _Button_ShowFileInOC          = new Lazy<GUIContent>(() => new GUIContent(_FileIcon.Value, "Show the selected file in OneCommander.")),        // 'Show in OC' for files.
            _Button_OpenContainingDirInOC = new Lazy<GUIContent>(() => new GUIContent(_FolderIcon.Value, "Open the containing folder in OneCommander.")),  // 'Open containing folder in OC' for files.
            _Button_ShowDirInOC           = new Lazy<GUIContent>(() => new GUIContent(_EyeIcon.Value, "Show the selected directory in OneCommander.")),    // 'Show in OC' for directories. (Opens to the parent directory, highlights the subdirectory.)
            _Button_OpenDirInOC           = new Lazy<GUIContent>(() => new GUIContent(_FolderIcon.Value, "Open the selected directory in OneCommander.")); // 'Open in OC' for directories. (Opens to the directory.)

        static OpenInOC() =>
            // There are two main ways that OC can be used.
            // First, the user can right-click a file or directory in the Project window, and select 'Show in OC'.
            // Second, buttons will be displayed on items in the Project window, which will open the file or directory in OC.
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;

        [Conditional("UNITY_EDITOR")]
        static void LogVerbose( string Context, string Message ) {
            if (OpenInOCSettings.DebugVerbose) {
                Debug.Log($"[{nameof(OneCommander)}/{nameof(OpenInOC)}.{Context}] {Message}");
            }
        }

        [Conditional("UNITY_EDITOR")]
        static void LogWarning( string Context, string Message ) {
            if (OpenInOCSettings.DebugWarn) {
                Debug.LogWarning($"[{nameof(OneCommander)}/{nameof(OpenInOC)}.{Context}] {Message}");
            }
        }

        [Conditional("UNITY_EDITOR")]
        static void LogError( string Context, string Message ) {
            if (OpenInOCSettings.DebugError) {
                Debug.LogError($"[{nameof(OneCommander)}/{nameof(OpenInOC)}.{Context}] {Message}");
            }
        }

        // [InitializeOnLoadMethod]
        // static void Init() {
        //     Debug.Log("OneCommander: OpenInOC.Init()");
        // }

        static bool TryGetFilePath(
            string Path,
            #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            [MaybeNullWhen(false)]
            #endif
            out FileInfo Found
        ) {
            try {
                FileAttributes Attributes = File.GetAttributes(Path);
                if (Attributes.HasFlag(FileAttributes.Directory)) {
                    #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                    Found = null;
                    #else
                    Found = null!;
                    #endif
                    return false;
                }

                Found = new FileInfo(Path);
                return true;
            } catch (Exception E) {
                LogError(nameof(TryGetFilePath), E.Message);
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                Found = null;
                #else
                Found = null!;
                #endif
                return false;
            }
        }

        static bool TryGetDirectoryPath(
            string Path,
            #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            [MaybeNullWhen(false)]
            #endif
            out DirectoryInfo Found
        ) {
            try {
                FileAttributes Attributes = File.GetAttributes(Path);
                if (!Attributes.HasFlag(FileAttributes.Directory)) {
                    #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                    Found = null;
                    #else
                    Found = null!;
                    #endif
                    return false;
                }

                Found = new DirectoryInfo(Path);
                return true;
            } catch (Exception E) {
                LogError(nameof(TryGetDirectoryPath), E.Message);
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                Found = null;
                #else
                Found = null!;
                #endif
                return false;
            }
        }

        static void OnProjectWindowItemOnGUI( string Guid, Rect SelectionRect ) {
            if (!OpenInOCSettings.ShowQuickButtons) { return; }

            Event CurrentEvent = Event.current;
            if ( /*CurrentEvent.type != EventType.ContextClick || */!SelectionRect.Contains(CurrentEvent.mousePosition)) {
                return; // Ignore events not related to the context menu.
            }

            var AssetPath = AssetDatabase.GUIDToAssetPath(Guid);
            if (string.IsNullOrEmpty(AssetPath)) { return; }

            // Debug.Log($"[{nameof(OneCommander)}/{nameof(OpenInOC)}.{nameof(OnProjectWindowItemOnGUI)}] Guid: {Guid}, SelectionRect: {SelectionRect}");

            const float ButtonAW = 26f,
                ButtonBW         = 26f,
                ButtonsW         = ButtonAW + ButtonBW + 2f,
                ButtonMaxH       = 18f;

            float H = Mathf.Min(SelectionRect.height, ButtonMaxH);

            Rect Buttons = new Rect(SelectionRect) {
                x      = SelectionRect.xMax - ButtonsW,
                width  = ButtonsW,
                height = H
            };
            Rect ButtonLeft  = new Rect(Buttons) { width = ButtonAW, height            = H };
            Rect ButtonRight = new Rect(Buttons) { x     = ButtonLeft.xMax + 2f, width = ButtonBW, height = H };
            bool UsedRight   = false;

            if (TryGetFilePath(AssetPath, out var AssetFile)) {
                if (OpenInOCSettings.UseArgsHighlightFile) {
                    UsedRight = true;
                    if (GUI.Button(ButtonRight, _Button_ShowFileInOC.Value, EditorStyles.miniButton)) {
                        LogVerbose(nameof(OnProjectWindowItemOnGUI), $"Highlighting file in OneCommander: {AssetFile.FullName}");
                        HighlightFileInOneCommander(AssetFile);
                    }
                }

                if (OpenInOCSettings.UseArgsOpenFileParentFolder) {
                    Rect Button = UsedRight ? ButtonLeft : ButtonRight;
                    if (GUI.Button(Button, _Button_OpenContainingDirInOC.Value, EditorStyles.miniButton)) {
                        LogVerbose(nameof(OnProjectWindowItemOnGUI), $"Opening containing directory in OneCommander: {AssetFile.DirectoryName}");
                        OpenFolderInOneCommander(AssetFile.Directory);
                    }
                }
            } else if (TryGetDirectoryPath(AssetPath, out var AssetDir)) {
                if (OpenInOCSettings.CanHighlightFolder && OpenInOCSettings.UseArgsHighlightFolder) {
                    UsedRight = true;
                    if (GUI.Button(ButtonRight, _Button_ShowDirInOC.Value, EditorStyles.miniButton)) {
                        LogVerbose(nameof(OnProjectWindowItemOnGUI), $"Highlighting directory in OneCommander: {AssetDir.FullName}");
                        HighlightFolderInOneCommander(AssetDir);
                    }
                }

                if (OpenInOCSettings.UseArgsOpenFolder) {
                    Rect Button = UsedRight ? ButtonLeft : ButtonRight;
                    if (GUI.Button(Button, _Button_OpenDirInOC.Value, EditorStyles.miniButton)) {
                        LogVerbose(nameof(OnProjectWindowItemOnGUI), $"Opening directory in OneCommander: {AssetDir.FullName}");
                        OpenFolderInOneCommander(AssetDir);
                    }
                }
            }
        }

        #if OPENINOC_SHOW_CONTEXT_MENU
        [MenuItem("Assets/Show in OneCommander", true, 19)]
        static bool Context_ShowInOC_Validate() {
            var AssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(AssetPath)) { return false; }

            return TryGetFilePath(AssetPath, out _) || TryGetDirectoryPath(AssetPath, out _);
        }

        [MenuItem("Assets/Show in OneCommander", false, 19)]
        static void Context_ShowInOC() {
            var AssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(AssetPath)) { return; }

            if (TryGetFilePath(AssetPath,
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
                out FileInfo? AssetFile)) {
                #else
                out FileInfo AssetFile)) {
                #endif
                LogVerbose(nameof(Context_ShowInOC), $"Highlighting file in OneCommander: {AssetFile.FullName}");
                HighlightFileInOneCommander(AssetFile);
            } else if (TryGetDirectoryPath(AssetPath,
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
                out FileInfo? AssetDir)) {
                #else
                out DirectoryInfo AssetDir)) {
                #endif
                LogVerbose(nameof(Context_ShowInOC), $"Opening directory in OneCommander: {AssetDir.FullName}");
                if (OpenInOCSettings.DirectoryContextMenuHighlight) {
                    HighlightFolderInOneCommander(AssetDir);
                } else {
                    OpenFolderInOneCommander(AssetDir);
                }
            }
        }
        #endif

        static void RunWithOC( string Context, string Args, [CanBeNull] string Path ) {
            if (string.IsNullOrEmpty(Path)) { return; }

            if (OpenInOCSettings.OCPath is null) {
                LogError(Context, "OneCommander path is null or empty. OneCommander is not installed, or the correct location has not been set in the settings window. Go to 'Edit > Preferences > OneCommander' to set the path.");
                return;
            }

            string RunArgs = Args.Replace("%1", Path);
            LogVerbose(Context, $"Invoking '{OpenInOCSettings.OCPath}' with arguments: '{RunArgs}'");
            Process.Start(OpenInOCSettings.OCPath.FullName, RunArgs);
        }

        static void OpenFolderInOneCommander( [CanBeNull] DirectoryInfo Folder ) => RunWithOC(nameof(OpenFolderInOneCommander), OpenInOCSettings.ArgsOpenFolder, Folder.FullName);

        static void HighlightFolderInOneCommander( [CanBeNull] DirectoryInfo Folder ) => RunWithOC(nameof(HighlightFolderInOneCommander), OpenInOCSettings.ArgsHighlightFolder, Folder.FullName);

        static void HighlightFileInOneCommander( [CanBeNull] FileInfo File ) => RunWithOC(nameof(HighlightFileInOneCommander), OpenInOCSettings.ArgsHighlightFile, File.FullName);

    }
}
