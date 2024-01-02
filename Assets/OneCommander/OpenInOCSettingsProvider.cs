using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || PLATFORM_STANDALONE_WIN
using Microsoft.Win32;
#endif
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace OneCommander {

    public enum OpenInOC_AppVersion {
        /// <summary> Not yet determined. </summary>
        /// <remarks> This is not a valid option for the user to select, and is only included for internal use. <br/>
        /// When the version is 'Unknown', the version will be determined via the executable information, and set from there. </remarks>
        Unknown = 0,

        /// <summary> A custom version. </summary>
        Custom = -1,

        /// <summary> Any legacy version (pre-3.16.3). </summary>
        Legacy = 1,

        /// <summary> Version 3.16.3 and above. </summary>
        Three_16_3 = 3_16_3
    }

    public static class OpenInOCSettings {

        [Conditional("UNITY_EDITOR")]
        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        #if NET6_0_OR_GREATER
        [StackTraceHidden]
        #endif
        static void LogVerbose( string Context, string Message ) {
            if (DebugVerbose) {
                Debug.Log($"[{nameof(OneCommander)}/{nameof(OpenInOCSettings)}.{Context}] {Message}");
            }
        }

        [Conditional("UNITY_EDITOR")]
        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        #if NET6_0_OR_GREATER
        [StackTraceHidden]
        #endif
        static void LogWarning( string Context, string Message ) {
            if (DebugWarn) {
                Debug.LogWarning($"[{nameof(OneCommander)}/{nameof(OpenInOCSettings)}.{Context}] {Message}");
            }
        }

        [Conditional("UNITY_EDITOR")]
        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        #if NET6_0_OR_GREATER
        [StackTraceHidden]
        #endif
        static void LogError( string Context, string Message ) {
            if (DebugError) {
                Debug.LogError($"[{nameof(OneCommander)}/{nameof(OpenInOCSettings)}.{Context}] {Message}");
            }
        }

        static bool GetBool( string Key, bool Default ) => EditorPrefs.GetBool($"{nameof(OneCommander)}.{Key}", Default);
        static void SetBool( string Key, bool Value )   => EditorPrefs.SetBool($"{nameof(OneCommander)}.{Key}", Value);

        static string GetString( string Key, string Default ) => EditorPrefs.GetString($"{nameof(OneCommander)}.{Key}", Default);
        static void   SetString( string Key, string Value )   => EditorPrefs.SetString($"{nameof(OneCommander)}.{Key}", Value);

        public static class EditorPrefsEnumHelper<TEnum> where TEnum : struct, Enum, IComparable, IConvertible, IFormattable {
            static readonly Dictionary<TEnum, int> _Values;
            static readonly Dictionary<int, TEnum> _Underlying;
            static EditorPrefsEnumHelper() {
                // Type must be an enum
                if (!typeof(TEnum).IsEnum) {
                    throw new ArgumentException($"{nameof(TEnum)} must be an enumerated type.");
                }

                // Enum must have 'int' as its underlying type (EditorPrefs only supports int and float as numeric types, and enums cannot be float)
                if (typeof(TEnum).GetEnumUnderlyingType() != typeof(int)) {
                    throw new ArgumentException($"{nameof(TEnum)} must have an underlying type of 'int'. This class is not intended for external use, and only exists to support the {nameof(OpenInOC_AppVersion)} enum for EditorPrefs (de/)serialisation purposes.");
                }

                Array Vals = Enum.GetValues(typeof(TEnum));
                int   Ln   = Vals.Length;
                _Values     = new Dictionary<TEnum, int>(Ln);
                _Underlying = new Dictionary<int, TEnum>(Ln);

                for (int I = 0; I < Ln; I++) {
                    TEnum Val        = (TEnum)Vals.GetValue(I);
                    int   Underlying = Val.ToInt32(null);
                    _Values.Add(Val, Underlying);
                    _Underlying.Add(Underlying, Val);
                    if (Underlying == 0) {
                        Default = Val;
                    }
                }
            }

            /// <summary>
            ///     Gets the default value for the enum.
            /// </summary>
            public static TEnum Default { get; }

            /// <summary>
            ///     Converts the given enum value to an integer.
            /// </summary>
            /// <param name="Value"> The enum value to convert. </param>
            /// <returns> The integer value of the enum. </returns>
            public static int ToInt( TEnum Value ) => _Values[Value];

            /// <summary>
            ///     Converts the given integer to an enum value.
            /// </summary>
            /// <param name="Value"> The integer value to convert. </param>
            /// <returns> The enum value of the integer. </returns>
            public static TEnum FromInt( int Value ) => _Underlying[Value];
        }

        static TEnum GetEnum<TEnum>( string Key, TEnum Default ) where TEnum : struct, Enum, IComparable, IConvertible, IFormattable {
            int Underlying = EditorPrefs.GetInt($"{nameof(OneCommander)}.{Key}", EditorPrefsEnumHelper<TEnum>.ToInt(Default));
            return EditorPrefsEnumHelper<TEnum>.FromInt(Underlying);
        }

        static void SetEnum<TEnum>( string Key, TEnum Value ) where TEnum : struct, Enum, IComparable, IConvertible, IFormattable => EditorPrefs.SetInt($"{nameof(OneCommander)}.{Key}", EditorPrefsEnumHelper<TEnum>.ToInt(Value));

        internal static bool TryGetFileInfo(
            string Path,
            #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            [MaybeNullWhen(false)]
            #endif
            out FileInfo Info
        ) {
            try {
                Info = new FileInfo(Path);
                return true;
            } catch (Exception) {
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                Info = null;
                #else
                Info = null!;
                #endif
                return false;
            }
        }

        static FileInfo GetFileInfo( string Key, [CanBeNull] FileInfo Default ) {
            string Path = GetString(Key, Default?.FullName ?? string.Empty);
            if (TryGetFileInfo(Path, out var Info)) {
                return Info;
            }

            SetFileInfo(Key, Default);
            return Default;
        }

        static void SetFileInfo( string Key, [CanBeNull] FileInfo Value ) => SetString(Key, Value?.FullName ?? string.Empty);

        static bool _DidSearchForOC = false;
        static bool TryFindOC(
            #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            [MaybeNullWhen(false)]
            #endif
            out FileInfo OC
        ) {
            // First check program files ('%ProgramFiles%\OneCommander\OneCommander.exe'), as there is NOW an MSI installer for OneCommander which uses this as default. When the plugin was first written, there was no MSI installer.
            if (TryGetFileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "OneCommander", "OneCommander.exe"), out OC) && OC.Exists) {
                return true;
            }
            OC = null;

            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || PLATFORM_STANDALONE_WIN
            // Read the REG_SZ value in 'Computer\HKEY_CLASSES_ROOT\Directory\shell\OpenInOneCommander\command'.
            // Within the path, '%1' will be replaced with the path of the selected directory/file when the user clicks the context menu item.

            var Key = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\OpenInOneCommander\command");
            if (Key is null) {
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                OC = null;
                #else
                OC = null!;
                #endif
                return false;
            }

            if (!(Key.GetValue(null) is string FoundOCPath) || string.IsNullOrEmpty(FoundOCPath)) {
                #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER || !CSHARP_8_OR_GREATER
                OC = null;
                #else
                OC = null!;
                #endif
                return false;
            }

            // String will be something like: "C:\Program Files\OneCommander\OneCommander.exe" -"%1"
            // If the path starts with a quote, grab all text up to the next quote. (then remove both quotes for the path)
            // Otherwise, grab all text up to the first space. (then remove the space for the path)
            if (FoundOCPath.StartsWith("\"")) {
                FoundOCPath = FoundOCPath.Substring(1, FoundOCPath.IndexOf('"', 1) - 1);
                FoundOCPath = FoundOCPath.Substring(0, FoundOCPath.Length          - 1);
            } else {
                FoundOCPath = FoundOCPath.Substring(0, FoundOCPath.IndexOf(' '));
            }

            return TryGetFileInfo(FoundOCPath, out OC);
            #else
            OC = null;
            return false;
            #endif
        }

        /// <summary>
        ///     Gets or sets the path to the OneCommander executable.
        /// </summary>
        /// <remarks>
        ///     Value may be <see langword="null"/>.
        /// </remarks>
        #if CSHARP_8_OR_GREATER
        public static FileInfo? OCPath {
        #else
        public static FileInfo OCPath {
            #endif
            get {
                var OC = GetFileInfo(nameof(OCPath), null);
                if (OC is null && !_DidSearchForOC) {
                    _DidSearchForOC = true;
                    if (TryFindOC(out OC)) {
                        SetFileInfo(nameof(OCPath), OC);
                        DetermineAppVersion(OC);
                        ResetArgs();
                    }
                }

                return OC;
            }
            set => SetFileInfo(nameof(OCPath), value);
        }

        /// <summary>
        ///     Attempts to auto-detect the path to the OneCommander executable.
        /// </summary>
        public static void ResetOCPath() {
            _DidSearchForOC = false;
            OCPath          = null;
        }

        /// <summary>
        ///     Gets or sets the version of OneCommander.
        /// </summary>
        /// <remarks>
        ///     Note that this is not the version of the plugin, but the version of the OneCommander executable. <br/>
        ///     This is only used to assign the correct command line arguments presets, so we allow the user to manually set it if they want.
        /// </remarks>
        public static OpenInOC_AppVersion OCVersion {
            get => GetEnum(nameof(OCVersion), OpenInOC_AppVersion.Unknown);
            set => SetEnum(nameof(OCVersion), value);
        }

        /// <summary>
        ///     Gets or sets the custom command line arguments to use when highlighting a file.
        /// </summary>
        public static string ArgsHighlightFile {
            get => GetString(nameof(ArgsHighlightFile), string.Empty);
            set => SetString(nameof(ArgsHighlightFile), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Show in OC' quick button is enabled for files.
        /// </summary>
        public static bool UseArgsHighlightFile {
            get => GetBool(nameof(UseArgsHighlightFile), true);
            set => SetBool(nameof(UseArgsHighlightFile), value);
        }

        /// <summary>
        ///     Gets or sets the custom command line arguments to use when opening the containing directory for a file.
        /// </summary>
        public static string ArgsOpenFileParentFolder {
            get => GetString(nameof(ArgsOpenFileParentFolder), string.Empty);
            set => SetString(nameof(ArgsOpenFileParentFolder), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Open Containing Folder' quick button is enabled for files.
        /// </summary>
        public static bool UseArgsOpenFileParentFolder {
            get => GetBool(nameof(UseArgsOpenFileParentFolder), false);
            set => SetBool(nameof(UseArgsOpenFileParentFolder), value);
        }

        /// <summary>
        ///     Gets or sets the custom command line arguments to use when highlighting a folder.
        /// </summary>
        public static string ArgsHighlightFolder {
            get => GetString(nameof(ArgsHighlightFolder), string.Empty);
            set => SetString(nameof(ArgsHighlightFolder), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Show in OC' quick button is enabled for folders.
        /// </summary>
        public static bool UseArgsHighlightFolder {
            get => GetBool(nameof(UseArgsHighlightFolder), false);
            set => SetBool(nameof(UseArgsHighlightFolder), value);
        }

        /// <summary>
        ///     Gets or sets the custom command line arguments to use when opening a folder.
        /// </summary>
        public static string ArgsOpenFolder {
            get => GetString(nameof(ArgsOpenFolder), string.Empty);
            set => SetString(nameof(ArgsOpenFolder), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Open in OC' quick button is enabled for folders.
        /// </summary>
        public static bool UseArgsOpenFolder {
            get => GetBool(nameof(UseArgsOpenFolder), true);
            set => SetBool(nameof(UseArgsOpenFolder), value);
        }

        /// <summary>
        ///     Gets whether the current chosen version supports highlighting folders.
        /// </summary>
        public static bool CanHighlightFolder
            #if CSHARP_9_OR_GREATER
            => OCVersion is OpenInOC_AppVersion.Custom or >= OpenInOC_AppVersion.Three_16_3;
            #else
            => OCVersion == OpenInOC_AppVersion.Custom || OCVersion >= OpenInOC_AppVersion.Three_16_3;
        #endif

        /// <summary>
        ///     Resets the command line arguments to their default values for the current chosen version.
        /// </summary>
        public static void ResetArgs() {
            switch (OCVersion) {
                case OpenInOC_AppVersion.Legacy:
                    ArgsHighlightFile        = "-pathToFile \"%1\"";
                    ArgsOpenFileParentFolder = "-\"%1-folder\"";
                    ArgsHighlightFolder      = ArgsOpenFolder = "-\"%1\""; // Legacy versions do not support highlighting folders, and so share the same arguments as opening them.
                    break;
                case OpenInOC_AppVersion.Three_16_3:
                    ArgsHighlightFile        = ArgsHighlightFolder = "select,\"%1\"";
                    ArgsOpenFolder           = "-\"%1\"";
                    ArgsOpenFileParentFolder = "-\"%1-folder\"";
                    break;
                // case OpenInOC_AppVersion.Custom: // Do not reset custom arguments.
                //     break;
            }
        }

        static Version GetVersion( FileVersionInfo FileVersion ) => new Version(FileVersion.FileMajorPart, FileVersion.FileMinorPart, FileVersion.FileBuildPart, FileVersion.FilePrivatePart);

        static readonly Version _Version_3_16_3 = new Version(3, 16, 3, 0);

        /// <summary>
        ///     Determines the app version of the given OneCommander executable.
        /// </summary>
        /// <param name="Executable"> The path to the executable. </param>
        public static void DetermineAppVersion( [CanBeNull] FileInfo Executable ) {
            if (Executable is null) {
                LogWarning(nameof(DetermineAppVersion), "Executable is null.");
                OCVersion = OpenInOC_AppVersion.Unknown;
                return;
            }

            if (!Executable.Name.Equals("OneCommander.exe", StringComparison.InvariantCultureIgnoreCase)) {
                // If the path is not to 'OneCommander.exe', then we assume the user will specify custom arguments.
                LogVerbose(nameof(DetermineAppVersion), "Executable is not 'OneCommander.exe'.");
                OCVersion = OpenInOC_AppVersion.Custom;
                return;
            }

            try {
                FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Executable.FullName);
                Version         Version     = GetVersion(VersionInfo);
                LogVerbose(nameof(DetermineAppVersion), $"Executable version: {VersionInfo.FileVersion} ({Version})");
                if (Version >= _Version_3_16_3) {
                    LogVerbose(nameof(DetermineAppVersion), "Executable is 3.16.3 or newer.");
                    OCVersion = OpenInOC_AppVersion.Three_16_3;
                    return;
                }
            } catch (Exception E) {
                LogWarning(nameof(DetermineAppVersion), $"Failed to get version info for executable. {E.Message}");
                // Ignore.
            }

            LogVerbose(nameof(DetermineAppVersion), "Executable is 'OneCommander.exe', but is not a known supported version.");
            OCVersion = OpenInOC_AppVersion.Legacy;
        }

        /// <summary>
        ///     Gets or sets whether the 'Show in OC' context menu is enabled.
        /// </summary>
        public static bool ShowContextMenu {
            get => GetBool(nameof(ShowContextMenu), false);
            set => SetBool(nameof(ShowContextMenu), value);
        }

        /// <summary>
        ///     Gets or sets whether the buttons that appear over items in the Project window are enabled.
        /// </summary>
        public static bool ShowQuickButtons {
            get => GetBool(nameof(ShowQuickButtons), true);
            set => SetBool(nameof(ShowQuickButtons), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Show in OC' quick button is enabled for files.
        /// </summary>
        /// <remarks>
        ///     This is only used if <see cref="ShowQuickButtons"/> is <see langword="true"/>.
        /// </remarks>
        public static bool ShowQuickButton_HighlightFile {
            get => GetBool(nameof(ShowQuickButton_HighlightFile), true);
            set => SetBool(nameof(ShowQuickButton_HighlightFile), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Show in OC' quick button is enabled for folders.
        /// </summary>
        /// <remarks>
        ///     This is only used if <see cref="ShowQuickButtons"/> is <see langword="true"/>.
        /// </remarks>
        public static bool ShowQuickButton_HighlightFolder {
            get => GetBool(nameof(ShowQuickButton_HighlightFolder), true);
            set => SetBool(nameof(ShowQuickButton_HighlightFolder), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Open Containing Folder' quick button is enabled for files.
        /// </summary>
        /// <remarks>
        ///     This is only used if <see cref="ShowQuickButtons"/> is <see langword="true"/>.
        /// </remarks>
        public static bool ShowQuickButton_OpenFileParentFolder {
            get => GetBool(nameof(ShowQuickButton_OpenFileParentFolder), true);
            set => SetBool(nameof(ShowQuickButton_OpenFileParentFolder), value);
        }

        /// <summary>
        ///     Gets or sets whether the 'Open in OC' quick button is enabled for folders.
        /// </summary>
        /// <remarks>
        ///     This is only used if <see cref="ShowQuickButtons"/> is <see langword="true"/>.
        /// </remarks>
        public static bool ShowQuickButton_OpenFolder {
            get => GetBool(nameof(ShowQuickButton_OpenFolder), true);
            set => SetBool(nameof(ShowQuickButton_OpenFolder), value);
        }

        /// <summary>
        ///     Gets or sets whether the directory context menu should open to the parent directory, and highlight the subdirectory, or just open to the subdirectory.
        /// </summary>
        public static bool DirectoryContextMenuHighlight {
            get => GetBool(nameof(DirectoryContextMenuHighlight), true);
            set => SetBool(nameof(DirectoryContextMenuHighlight), value);
        }

        /// <summary>
        ///     Gets or sets whether verbose logging is enabled.
        /// </summary>
        public static bool DebugVerbose {
            get => GetBool(nameof(DebugVerbose), false);
            set => SetBool(nameof(DebugVerbose), value);
        }

        /// <summary>
        ///     Gets or sets whether warning logging is enabled.
        /// </summary>
        public static bool DebugWarn {
            get => GetBool(nameof(DebugWarn), true);
            set => SetBool(nameof(DebugWarn), value);
        }

        /// <summary>
        ///     Gets or sets whether error logging is enabled.
        /// </summary>
        public static bool DebugError {
            get => GetBool(nameof(DebugError), true);
            set => SetBool(nameof(DebugError), value);
        }

        /// <summary>
        ///     Resets all settings to their default values.
        /// </summary>
        public static void ClearPreferences() {
            LogVerbose(nameof(ClearPreferences), "Clearing preferences.");
            ResetOCPath();
            ResetArgs();

            _ = OCPath; // Rescan as necessary.

            ShowContextMenu                      = false;
            ShowQuickButtons                     = true;
            ShowQuickButton_HighlightFile        = true;
            ShowQuickButton_HighlightFolder      = true;
            ShowQuickButton_OpenFileParentFolder = true;
            ShowQuickButton_OpenFolder           = true;
            DirectoryContextMenuHighlight        = true;
            DebugVerbose                         = false;
            DebugWarn                            = true;
            DebugError                           = true;
        }

    }

    public sealed class OpenInOCSettingsProvider : EditorWindow {

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider() =>
            new SettingsProvider("Preferences/OneCommander", SettingsScope.User) {
                label      = "OneCommander",
                guiHandler = DrawSettings
            };

        #region Editor Content

        // Alt
        static readonly Lazy<Texture>
            _FolderIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("Folder Icon").image),
            _ExeProgramIcon = new Lazy<Texture>(
                () => {
                    try {
                        return EditorGUIUtility.IconContent("Profiler.UIDetails").image;
                    } catch {
                        return EditorGUIUtility.IconContent("Assembly Icon").image;
                    }
                }
            ),
            _VersionIcon = new Lazy<Texture>(
                () => {
                    try {
                        return EditorGUIUtility.IconContent("UnityEditor.Graphs.AnimatorControllerTool").image;
                    } catch {
                        return EditorGUIUtility.IconContent("PrefabVariant Icon").image;
                    }
                }
            ),
            _EyeIcon     = new Lazy<Texture>(() => EditorGUIUtility.IconContent("ViewToolOrbit").image),
            _TextBoxIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("VerticalSplit").image),
            _ReloadIcon  = new Lazy<Texture>(() => EditorGUIUtility.IconContent("Refresh").image),
            // ...
            _ListIcon   = new Lazy<Texture>(() => EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow").image),
            _ButtonIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("Toolbar Plus More").image),
            // ...
            _BugIcon  = new Lazy<Texture>(() => EditorGUIUtility.IconContent("console.erroricon.sml").image),
            _WarnIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("console.warnicon.sml").image),
            _InfoIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("console.infoicon.sml").image),
            _TrashIcon = new Lazy<Texture>(() => EditorGUIUtility.IconContent("TreeEditor.Trash").image),
            // ...
            _PersonIcon = new Lazy<Texture>(
                () => {
                    try {
                        return EditorGUIUtility.IconContent("Collab").image;
                    } catch {
                        return EditorGUIUtility.IconContent("NavMeshAgent Icon").image;
                    }
                }
            );

        static readonly Lazy<GUIContent>
            _Heading_Paths                     = new Lazy<GUIContent>(() => new GUIContent("Paths" /*, _FolderIcon.Value*/)),
            _Info_Paths                        = new Lazy<GUIContent>(() => new GUIContent("The path to the OneCommander executable, and any arguments to use when selecting files and folders.")),
            _Field_OCPath                      = new Lazy<GUIContent>(() => new GUIContent("Executable Path", _ExeProgramIcon.Value, "The path to the OneCommander executable.")),
            _Button_OCPath_AutoDetect          = new Lazy<GUIContent>(() => new GUIContent(_ReloadIcon.Value, "Auto-detect the installation path of OneCommander.\n\nNOTE: This will only work if OneCommander is configured as the default file manager via the relevant setting in the OneCommander settings menu.")),
            _Field_OCVersion                   = new Lazy<GUIContent>(() => new GUIContent("Executable Version", _VersionIcon.Value, "The version of OneCommander that is installed. This is used to swap between various command line argument presets.")),
            _Field_OCVersion_Custom            = new Lazy<GUIContent>(() => new GUIContent("Custom", "Use custom command line arguments.")),
            _Field_OCVersion_Legacy            = new Lazy<GUIContent>(() => new GUIContent("Legacy", "Use the command line arguments preset for legacy versions of OneCommander (pre-3.16.3).")),
            _Field_OCVersion_3_16_3            = new Lazy<GUIContent>(() => new GUIContent("3.16.3+", "Use the command line arguments preset for OneCommander version 3.16.3 and above.")),
            _Button_OCVersion_AutoDetect       = new Lazy<GUIContent>(() => new GUIContent(_ReloadIcon.Value, "Auto-detect the version of OneCommander that is installed.\n\nDisabled if the executable path is empty.")),
            _Field_OCHighlightFileArgs         = new Lazy<GUIContent>(() => new GUIContent("Highlight File Args", _TextBoxIcon.Value, "The arguments to use when highlighting a file.")),
            _Toggle_OCHighlightFileArgs        = new Lazy<GUIContent>(() => new GUIContent(string.Empty, "Toggle the visibility of the 'Show in OC' quick button for files.")),
            _Field_OCOpenFileParentFolderArgs  = new Lazy<GUIContent>(() => new GUIContent("Open File Dir Args", _TextBoxIcon.Value, "The arguments to use when opening the parent folder of a file.")),
            _Toggle_OCOpenFileParentFolderArgs = new Lazy<GUIContent>(() => new GUIContent(string.Empty, "Toggle the visibility of the 'Open Containing Directory' quick button for files.")),
            _Field_OCOpenFolderArgs            = new Lazy<GUIContent>(() => new GUIContent("Open Folder Args", _TextBoxIcon.Value, "The arguments to use when opening a folder.")),
            _Toggle_OCOpenFolderArgs           = new Lazy<GUIContent>(() => new GUIContent(string.Empty, "Toggle the visibility of the 'Open in OC' quick button for folders.")),
            // ...
            _Heading_Menus          = new Lazy<GUIContent>(() => new GUIContent("Menus" /*, _MenuIcon.Value*/)),
            _Info_Menus             = new Lazy<GUIContent>(() => new GUIContent("Decide which ways to display the 'Show in OC' menu.")),
            _Field_ShowContextMenu  = new Lazy<GUIContent>(() => new GUIContent("Context Menu", _ListIcon.Value, "Enables 'Show in OC' in the context menu of items in the Project window.\n\nWARNING: Due to limitations in the UnityEditor API, toggling this setting will require Unity to recompile.")),
            _Field_ShowQuickButtons = new Lazy<GUIContent>(() => new GUIContent("Quick Buttons", _ButtonIcon.Value, "Enables buttons that appear when hovering over items in the Project window.")),
            // ...
            _Heading_DirectoryContextMenu         = new Lazy<GUIContent>(() => new GUIContent("Directory Context Menu" /*, _FolderIcon.Value*/)),
            _Info_DirectoryContextMenu            = new Lazy<GUIContent>(() => new GUIContent("When opening a directory from the context menu, should the directory be opened to the parent directory, and highlight the subdirectory, or should the directory just be opened normally?")),
            _Field_DirectoryContextMenu_Highlight = new Lazy<GUIContent>(() => new GUIContent(" Highlight in Parent", _EyeIcon.Value, "When selecting a directory, the parent directory will be opened, and the selected directory be highlighted.")),
            _Field_DirectoryContextMenu_Open      = new Lazy<GUIContent>(() => new GUIContent(" Open Normally", _FolderIcon.Value, "When selecting a directory, the directory will be opened.")),
            // ...
            _Heading_Debug       = new Lazy<GUIContent>(() => new GUIContent("Debug" /*, _ConsoleIcon.Value*/)),
            _Info_Debug          = new Lazy<GUIContent>(() => new GUIContent("Enable or disable types of diagnostic logging.")),
            _Field_Debug_Verbose = new Lazy<GUIContent>(() => new GUIContent("Verbose", _InfoIcon.Value, "Enables verbose logging.")),
            _Field_Debug_Warn    = new Lazy<GUIContent>(() => new GUIContent("Warnings", _WarnIcon.Value, "Enables warning logging.")),
            _Field_Debug_Error   = new Lazy<GUIContent>(() => new GUIContent("Errors", _BugIcon.Value, "Enables error logging.")),
            _Button_Debug_Clear  = new Lazy<GUIContent>(() => new GUIContent("Clear Preferences", _TrashIcon.Value, "Clears all preferences for this plugin, rescanning for OneCommander and resetting all settings to their default values.\n\nThis is mainly intended for diagnostic purposes, and is useful if you are having issues with the plugin, or have changed the location of OneCommander.")),
            // ...
            _Button_OCPath_Browse = new Lazy<GUIContent>(() => new GUIContent("...", "Browse for the OneCommander executable.")),
            // ...
            _Heading_Contributors = new Lazy<GUIContent>(() => new GUIContent("Contributors", _PersonIcon.Value)),
            _Info_Contributors    = new Lazy<GUIContent>(() => new GUIContent("Thank you to the following people for their contributions to this Unity plugin:"));

        readonly struct Contributor {
            readonly GUIContent _Label;
            readonly string     _Url;

            public Contributor( GUIContent Label, string Url ) {
                _Label = Label;
                _Url   = Url;
            }

            #if UNITY_2020_1_OR_NEWER
            static GUIStyle LinkStyle => EditorStyles.linkLabel;
            #else
            static GUIStyle LinkStyle { get; } = new GUIStyle(EditorStyles.label) {
                normal = { textColor = new Color(0.0f, 0.0f, 1.0f, 1.0f) },
            };
            #endif

            public void Draw() {
                if (GUILayout.Button(_Label, LinkStyle, GUILayout.ExpandWidth(false))) {
                    Application.OpenURL(_Url);
                }
            }
        }

        static readonly Lazy<IReadOnlyList<Contributor>> _Contributors = new Lazy<IReadOnlyList<Contributor>>(
            () => new[] {
                new Contributor(
                    new GUIContent("Cody Bock", "Cody Bock: Made the initial version of the Unity plugin. Added support for v3.16.3 command line arguments."),
                    "https://neonalig.itch.io/"
                ),
                new Contributor(
                    new GUIContent("Milos Paripovic", "Milos Paripovic: Added the /select,\"\" command line argument to OneCommander v3.16.3 for enhanced file/folder selection functionality."),
                    "https://www.onecommander.com/"
                )
            }
        );

        static readonly Lazy<GUIStyle>
            _HeadingStyle = new Lazy<GUIStyle>(
                () => new GUIStyle( /*EditorStyles.boldLabel*/) {
                    fontSize  = 12,
                    fontStyle = FontStyle.Bold,
                    normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.2f, 0.2f, 0.2f) }
                }
            ), // Style for headings in the settings menu.
            _InfoStyle = new Lazy<GUIStyle>(
                () => new GUIStyle( /*EditorStyles.wordWrappedLabel*/) {
                    fontSize  = 12,
                    alignment = TextAnchor.MiddleLeft,
                    margin    = new RectOffset(0, 0, 0, 10),
                    normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f) },
                    wordWrap  = true
                }
            ); // Style for info text in the settings menu.

        #if CSHARP_8_OR_GREATER
        static GUIContent _Field_OCHighlightFolderArgs = null!, _Toggle_OCHighlightFolderArgs = null!;
        #else
        static GUIContent _Field_OCHighlightFolderArgs = null, _Toggle_OCHighlightFolderArgs = null;
        #endif

        static void Fix_FieldToggle_OCHighlightFolderArgs() {
            if (OpenInOCSettings.CanHighlightFolder) {
                _Field_OCHighlightFolderArgs  = new GUIContent("Highlight Folder Args", _TextBoxIcon.Value, "The arguments to use when highlighting a folder.");
                _Toggle_OCHighlightFolderArgs = new GUIContent(string.Empty, "Toggle the visibility of the 'Show in OC' quick button for folders.");
            } else {
                _Field_OCHighlightFolderArgs  = new GUIContent("Highlight Folder Args", _TextBoxIcon.Value, "The arguments to use when highlighting a folder.\n\nWARNING: This feature is not supported by the currently selected version.");
                _Toggle_OCHighlightFolderArgs = new GUIContent(string.Empty, "Toggle the visibility of the 'Show in OC' quick button for folders.\n\nWARNING: This feature is not supported by the currently selected version.");
            }
        }

        #endregion

        static bool RadioButton( GUIContent Content, bool Value ) {
            Rect        MainRect   = EditorGUILayout.GetControlRect();
            const float RadioWidth = 16f;
            Rect        RadioRect  = new Rect(MainRect.x, MainRect.y, RadioWidth, MainRect.height);
            const float Spacing    = 4f;
            Rect        LabelRect  = new Rect(RadioRect.xMax + Spacing, MainRect.y, MainRect.width - RadioWidth - Spacing, MainRect.height);

            // Prepare ID for control
            int ControlID = GUIUtility.GetControlID(FocusType.Keyboard, RadioRect);

            // Draw label
            EditorGUI.HandlePrefixLabel(MainRect, LabelRect, Content, ControlID, EditorStyles.label);

            // Draw radio button
            EditorGUI.BeginChangeCheck();
            Value = GUI.Toggle(RadioRect, Value, GUIContent.none, EditorStyles.radioButton);
            if (EditorGUI.EndChangeCheck()) {
                GUI.changed                = true;
                GUIUtility.keyboardControl = ControlID;
            }

            // Check if the radio button has focus
            if (GUIUtility.keyboardControl == ControlID) {
                switch (Event.current.type) {
                    #if CSHARP_9_OR_GREATER
                    case EventType.KeyDown when Event.current.keyCode is KeyCode.Space or KeyCode.Return:
                    #else
                    case EventType.KeyDown when Event.current.keyCode == KeyCode.Space || Event.current.keyCode == KeyCode.Return:
                        #endif
                        // Check if the user pressed space/enter
                        Value       = true;
                        GUI.changed = true;
                        break;
                    case EventType.MouseDown when MainRect.Contains(Event.current.mousePosition):
                        // Check if the user pressed mouse button (and the mouse is over the radio button - invert value)
                        Value       = !Value;
                        GUI.changed = true;
                        break;
                    case EventType.MouseDown:
                        // Check if the user pressed mouse button (and the mouse is not over the radio button - clear focus)
                        GUIUtility.keyboardControl = 0;
                        break;
                }
            }

            return Value;
        }

        static void AddScriptingDefine( string Define ) {
            string Defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (Defines.Contains(Define)) {
                return;
            }

            Defines += $";{Define}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, Defines);
        }

        static void RemoveScriptingDefine( string Define ) {
            string Defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!Defines.Contains(Define)) {
                return;
            }

            Defines = Defines.Replace($";{Define}", string.Empty);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, Defines);
        }

        static TEnum EnumToggle<TEnum>( Rect R, TEnum DisplayedValue, GUIContent Content, TEnum Value, GUIStyle Style ) where TEnum : struct, Enum, IComparable, IConvertible, IFormattable => GUI.Toggle(R, Value.CompareTo(DisplayedValue) == 0, Content, Style) ? DisplayedValue : Value;

        #if CSHARP_8_OR_GREATER
        static FileInfo?
        #else
        static FileInfo
            #endif
            FileInfoField( Rect R, GUIContent Content, GUIContent BrowseButtonContent, [CanBeNull] FileInfo Value, string[] Filters, ref string TemporaryValue, out bool Changed ) {
            // TemporaryValue is used to store the partial path when the user is typing it in manually.
            // Without this, the user would have to paste the entire path at once, which is not very user-friendly.
            Changed = false;

            #if CSHARP_9_OR_GREATER
            if (Value is not null
            #else
            if (Value != null
                #endif
                && string.IsNullOrEmpty(TemporaryValue)) {
                TemporaryValue = Value.FullName;
            }

            float       BrowseButtonWidth = EditorStyles.miniButton.CalcSize(BrowseButtonContent).x;
            const float Spacing           = 4f;
            Rect        BrowseButtonRect  = new Rect(R.xMax            - BrowseButtonWidth, R.y, BrowseButtonWidth, R.height);
            Rect        TextFieldRect     = new Rect(R.x, R.y, R.width - BrowseButtonWidth - Spacing, R.height);

            EditorGUI.BeginChangeCheck();
            TemporaryValue = EditorGUI.TextField(TextFieldRect, Content, TemporaryValue);
            if (EditorGUI.EndChangeCheck()) {
                if (OpenInOCSettings.TryGetFileInfo(TemporaryValue, out var Info)) {
                    Changed        = true;
                    Value          = Info;
                    TemporaryValue = Value.FullName;
                }
            }

            if (GUI.Button(BrowseButtonRect, BrowseButtonContent, EditorStyles.miniButton)) {
                string Folder = Value?.DirectoryName ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string Path   = EditorUtility.OpenFilePanelWithFilters("Select OneCommander executable", Folder, Filters);
                if (string.IsNullOrEmpty(Path)) {
                    return Value;
                }

                if (OpenInOCSettings.TryGetFileInfo(Path, out var Info)) {
                    Changed        = true;
                    Value          = Info;
                    TemporaryValue = Value.FullName;
                }
            }

            return Value;
        }

        static void TextFieldWithToggle( Rect R, GUIContent Content, GUIContent ToggleContent, in bool ToggleValue, out bool NewToggleValue, in string Value, out string NewValue, ref bool ValueChanged ) {
            float       ToggleWidth   = EditorStyles.toggle.CalcSize(ToggleContent).x;
            const float Spacing       = 4f;
            Rect        ToggleRect    = new Rect(R.xMax            - ToggleWidth, R.y, ToggleWidth, R.height);
            Rect        TextFieldRect = new Rect(R.x, R.y, R.width - ToggleWidth - Spacing, R.height);

            NewToggleValue = ToggleValue;
            NewValue       = Value;

            EditorGUI.BeginChangeCheck();
            bool WasEnabled = GUI.enabled;
            GUI.enabled = NewToggleValue;
            NewValue    = EditorGUI.TextField(TextFieldRect, Content, NewValue);
            GUI.enabled = WasEnabled;

            if (EditorGUI.EndChangeCheck()) {
                NewToggleValue = !string.IsNullOrEmpty(NewValue);
                ValueChanged   = true;
            }

            EditorGUI.BeginChangeCheck();
            NewToggleValue = GUI.Toggle(ToggleRect, NewToggleValue, ToggleContent);
            if (EditorGUI.EndChangeCheck()) {
                // if (!NewToggleValue) {
                //     NewValue     = string.Empty;
                //     ValueChanged = true;
                // }
            }
        }

        const string _Define_ShowContextMenu = "OPENINOC_SHOW_CONTEXT_MENU";

        static readonly string[] _OCPathFilters = { "Any Executable File", "exe", "Any File", "*" };
        static          string   _Temp_OCPath   = string.Empty;

        static bool _DidInit = false;
        static void Init() {
            if (_DidInit) { return; }

            _DidInit = true;

            Fix_FieldToggle_OCHighlightFolderArgs();
        }
        static Vector2 _Scroll;

        static void DrawSettings( string SearchContext ) {
            Init();
            _Scroll = EditorGUILayout.BeginScrollView(_Scroll);
            {
                const float MaxButtonWidth = 420f;

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField(_Heading_Paths.Value, _HeadingStyle.Value);
                    EditorGUILayout.LabelField(_Info_Paths.Value, _InfoStyle.Value);
                    EditorGUILayout.Space();

                    Rect        R                = EditorGUILayout.GetControlRect();
                    float       ReloadBtnWidth   = EditorStyles.miniButton.CalcSize(_Button_OCPath_AutoDetect.Value).x;
                    const float ReloadBtnSpacing = 4f;
                    Rect        ReloadBtnR       = new Rect(R.xMax            - ReloadBtnWidth, R.y, ReloadBtnWidth, R.height);
                    Rect        TextFieldR       = new Rect(R.x, R.y, R.width - ReloadBtnWidth - ReloadBtnSpacing, R.height);
                    OpenInOCSettings.OCPath = FileInfoField(TextFieldR, _Field_OCPath.Value, _Button_OCPath_Browse.Value, OpenInOCSettings.OCPath, _OCPathFilters, ref _Temp_OCPath, out bool OCPathChanged);
                    if (GUI.Button(ReloadBtnR, _Button_OCPath_AutoDetect.Value, EditorStyles.miniButton)) {
                        var OldOCPath = OpenInOCSettings.OCPath;
                        OpenInOCSettings.ResetOCPath();
                        if (OpenInOCSettings.OCPath != OldOCPath) {
                            OCPathChanged = true;
                            _Temp_OCPath  = OpenInOCSettings.OCPath?.FullName ?? string.Empty;
                        }
                    }

                    if (OCPathChanged) {
                        OpenInOCSettings.DetermineAppVersion(OpenInOCSettings.OCPath);
                        OpenInOCSettings.ResetArgs();
                        Fix_FieldToggle_OCHighlightFolderArgs();
                    }

                    const float ReloadButtonWidth = 30f, ReloadButtonSpacing = 4f;

                    R = EditorGUILayout.GetControlRect(true);
                    Rect  LblR  = new Rect(R) { width = EditorGUIUtility.labelWidth };
                    float Width = Mathf.Min((R.width - LblR.width - ReloadButtonWidth - ReloadButtonSpacing) / 3f, MaxButtonWidth);
                    Rect  R1    = new Rect(R) { x  = LblR.xMax, width = Width };
                    Rect  R2    = new Rect(R1) { x = R1.xMax };
                    Rect  R3    = new Rect(R2) { x = R2.xMax };
                    Rect  R4    = new Rect(R3) { x = R3.xMax + ReloadButtonSpacing, width = ReloadButtonWidth };

                    EditorGUI.HandlePrefixLabel(new Rect(R) { xMin = R.xMin, xMax = R1.xMax }, LblR, _Field_OCVersion.Value, 0, EditorStyles.label);

                    EditorGUI.BeginChangeCheck();
                    OpenInOCSettings.OCVersion = EnumToggle(R1, OpenInOC_AppVersion.Custom, _Field_OCVersion_Custom.Value, OpenInOCSettings.OCVersion, EditorStyles.miniButtonLeft);
                    OpenInOCSettings.OCVersion = EnumToggle(R2, OpenInOC_AppVersion.Legacy, _Field_OCVersion_Legacy.Value, OpenInOCSettings.OCVersion, EditorStyles.miniButtonMid);
                    OpenInOCSettings.OCVersion = EnumToggle(R3, OpenInOC_AppVersion.Three_16_3, _Field_OCVersion_3_16_3.Value, OpenInOCSettings.OCVersion, EditorStyles.miniButtonRight);
                    if (EditorGUI.EndChangeCheck()) {
                        OpenInOCSettings.ResetArgs();
                        Fix_FieldToggle_OCHighlightFolderArgs();
                    }

                    if (GUI.Button(R4, _Button_OCVersion_AutoDetect.Value, EditorStyles.miniButton) || (OpenInOCSettings.OCVersion == OpenInOC_AppVersion.Unknown && OpenInOCSettings.OCPath != null)) {
                        OpenInOCSettings.DetermineAppVersion(OpenInOCSettings.OCPath);
                        OpenInOCSettings.ResetArgs();
                        Fix_FieldToggle_OCHighlightFolderArgs();
                    }

                    bool AnyUseArgsChanged = false;

                    TextFieldWithToggle(EditorGUILayout.GetControlRect(), _Field_OCHighlightFileArgs.Value, _Toggle_OCHighlightFileArgs.Value, OpenInOCSettings.UseArgsHighlightFile, out bool NewUseArgsHighlightFile, OpenInOCSettings.ArgsHighlightFile, out string NewArgsHighlightFile, ref AnyUseArgsChanged);
                    OpenInOCSettings.UseArgsHighlightFile = NewUseArgsHighlightFile;
                    OpenInOCSettings.ArgsHighlightFile    = NewArgsHighlightFile;

                    bool WasEnabled = GUI.enabled;
                    GUI.enabled = OpenInOCSettings.CanHighlightFolder;
                    TextFieldWithToggle(EditorGUILayout.GetControlRect(), _Field_OCHighlightFolderArgs, _Toggle_OCHighlightFolderArgs, OpenInOCSettings.UseArgsHighlightFolder, out bool NewUseArgsHighlightFolder, OpenInOCSettings.ArgsHighlightFolder, out string NewArgsHighlightFolder, ref AnyUseArgsChanged);
                    OpenInOCSettings.UseArgsHighlightFolder = NewUseArgsHighlightFolder;
                    OpenInOCSettings.ArgsHighlightFolder    = NewArgsHighlightFolder;
                    GUI.enabled                             = WasEnabled;

                    TextFieldWithToggle(EditorGUILayout.GetControlRect(), _Field_OCOpenFileParentFolderArgs.Value, _Toggle_OCOpenFileParentFolderArgs.Value, OpenInOCSettings.UseArgsOpenFileParentFolder, out bool NewUseArgsOpenFileParentFolder, OpenInOCSettings.ArgsOpenFileParentFolder, out string NewArgsOpenFileParentFolder, ref AnyUseArgsChanged);
                    OpenInOCSettings.UseArgsOpenFileParentFolder = NewUseArgsOpenFileParentFolder;
                    OpenInOCSettings.ArgsOpenFileParentFolder    = NewArgsOpenFileParentFolder;

                    TextFieldWithToggle(EditorGUILayout.GetControlRect(), _Field_OCOpenFolderArgs.Value, _Toggle_OCOpenFolderArgs.Value, OpenInOCSettings.UseArgsOpenFolder, out bool NewUseArgsOpenFolder, OpenInOCSettings.ArgsOpenFolder, out string NewArgsOpenFolder, ref AnyUseArgsChanged);
                    OpenInOCSettings.UseArgsOpenFolder = NewUseArgsOpenFolder;
                    OpenInOCSettings.ArgsOpenFolder    = NewArgsOpenFolder;

                    if (AnyUseArgsChanged) {
                        OpenInOCSettings.OCVersion = OpenInOC_AppVersion.Custom;
                        Fix_FieldToggle_OCHighlightFolderArgs();
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField(_Heading_Menus.Value, _HeadingStyle.Value);
                    EditorGUILayout.LabelField(_Info_Menus.Value, _InfoStyle.Value);
                    EditorGUILayout.Space();

                    Rect  R     = EditorGUILayout.GetControlRect();
                    float Width = Mathf.Min(R.width / 2f, MaxButtonWidth);
                    Rect  R1    = new Rect(R.x, R.y, Width, R.height);
                    Rect  R2    = new Rect(R.x + Width, R.y, Width, R.height);

                    // using (new EditorGUI.DisabledScope(true)) {
                    // Unfortunately, Unity only allows the creation of context menu items at compile time and not runtime. This means that, whilst it would be nice to let the user decide whether to show the context menu item or not via a checkbox, it is not possible to do so. The most we could do is disable the context menu item at all times, but that would be confusing to the user, and since it already wastes context menu real estate, it would be better to just remove it entirely (which can't be done).
                    // If you want to enable/disable the context menu item, you can do so by uncommenting or commenting the '#define OPENINOC_SHOW_CONTEXT_MENU' option in the 'OpenInOC.cs' file.

                    // UPDATE: This option has been re-added. Instead, when the toggle is changed, the 'OPENINOC_SHOW_CONTEXT_MENU' option is added to the project's 'Scripting Define Symbols' list.

                    EditorGUI.BeginChangeCheck();
                    OpenInOCSettings.ShowContextMenu = GUI.Toggle(R1, OpenInOCSettings.ShowContextMenu, _Field_ShowContextMenu.Value, EditorStyles.miniButtonLeft);
                    if (EditorGUI.EndChangeCheck()) {
                        if (OpenInOCSettings.ShowContextMenu) {
                            AddScriptingDefine(_Define_ShowContextMenu);
                        } else {
                            RemoveScriptingDefine(_Define_ShowContextMenu);
                        }
                    }

                    OpenInOCSettings.ShowQuickButtons = GUI.Toggle(R2, OpenInOCSettings.ShowQuickButtons, _Field_ShowQuickButtons.Value, EditorStyles.miniButtonRight);
                }
                EditorGUILayout.EndVertical();

                if (OpenInOCSettings.ShowContextMenu) {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.LabelField(_Heading_DirectoryContextMenu.Value, _HeadingStyle.Value);
                        EditorGUILayout.LabelField(_Info_DirectoryContextMenu.Value, _InfoStyle.Value);
                        EditorGUILayout.Space();

                        EditorGUILayout.BeginHorizontal();
                        {
                            OpenInOCSettings.DirectoryContextMenuHighlight = RadioButton(_Field_DirectoryContextMenu_Highlight.Value, OpenInOCSettings.DirectoryContextMenuHighlight);
                            OpenInOCSettings.DirectoryContextMenuHighlight = !RadioButton(_Field_DirectoryContextMenu_Open.Value, !OpenInOCSettings.DirectoryContextMenuHighlight);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField(_Heading_Debug.Value, _HeadingStyle.Value);
                    EditorGUILayout.LabelField(_Info_Debug.Value, _InfoStyle.Value);
                    EditorGUILayout.Space();

                    Rect  R     = EditorGUILayout.GetControlRect();
                    float Width = Mathf.Min(R.width / 3f, MaxButtonWidth);
                    Rect  R_1    = new Rect(R.x, R.y, Width, R.height);
                    Rect  R_2    = new Rect(R.x + Width, R.y, Width, R.height);
                    Rect  R_3    = new Rect(R.x + Width * 2f, R.y, Width, R.height);

                    OpenInOCSettings.DebugVerbose = GUI.Toggle(R_1, OpenInOCSettings.DebugVerbose, _Field_Debug_Verbose.Value, EditorStyles.miniButtonLeft);
                    OpenInOCSettings.DebugWarn    = GUI.Toggle(R_2, OpenInOCSettings.DebugWarn, _Field_Debug_Warn.Value, EditorStyles.miniButtonMid);
                    OpenInOCSettings.DebugError   = GUI.Toggle(R_3, OpenInOCSettings.DebugError, _Field_Debug_Error.Value, EditorStyles.miniButtonRight);

                    Rect R2 = EditorGUILayout.GetControlRect();
                    if (GUI.Button(R2, _Button_Debug_Clear.Value, EditorStyles.miniButton)) {
                        OpenInOCSettings.ClearPreferences();
                    }
                }
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField(_Heading_Contributors.Value, _HeadingStyle.Value);
                    EditorGUILayout.LabelField(_Info_Contributors.Value, _InfoStyle.Value);
                    EditorGUILayout.Space();
                    foreach (Contributor Contributor in _Contributors.Value) {
                        Contributor.Draw();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

    }
}
