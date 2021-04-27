using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for paths operations
    /// </summary>
    public static class PathUtils
    {
        private const string EXTENDED_PREFIX = "\\\\?\\";
        private const string UNC_EXTENDED_PREFIX = "\\\\?\\UNC\\";
        private const int MAX_PATH_LENGTH = short.MaxValue;

        private const string NULL_PATH_MESSAGE = "Path cannot be null";
        private const string PATH_TOO_LONG_MESSAGE = "Path is too long";
        private const string INVALID_PATH_CHARS_MESSAGE = "Path contains invalid characters";

        internal static readonly char DirectorySeparator = Path.DirectorySeparatorChar;
        internal static readonly char AltDirectorySeparator = Path.AltDirectorySeparatorChar;
        internal static readonly char VolumeSeparator = Path.VolumeSeparatorChar;
        internal static readonly char ExtensionSeparator = '.';
        internal static readonly string ParentFolder = "..";
        internal static readonly string CurrentFolder = ".";

        internal static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        internal static readonly char[] InvalidSegmentChars = Path.GetInvalidFileNameChars();
        internal static readonly char[] InvalidSegmentCharsDiff = new char[] { '*', '?', '/', '\\', ':' };

        #region public
        /// <summary>
        /// Decompose a path into its base components, i.e the nodes of the path tree structure (root, ordered list of folders, filename)
        /// </summary>
        /// <param name="path">The path to decompose</param>
        /// <param name="root">The root of the path, filled when found</param>
        /// <param name="folders">The folder list of the path, filled when found</param>
        /// <param name="fileName">The file name of the path, filled when found</param>
        /// <param name="isFile">Whether the path represents a file (true) or a directory (false)</param>
        /// <param name="isRooted">If the path is likely to contains a root. Default is true</param>
        public static void Decompose(string path, out string root, out List<string> folders, out string fileName, 
            bool isFile = true, bool isRooted = true)
        {
            CheckPathValidity(path);
            Parse(path, out root, out folders, out fileName, isRooted, isFile);
            CheckSegmentsValidity(folders, fileName, true);
            Simplify(ref folders);
            Trim(ref folders, ref fileName);
        }

        /// <summary>
        /// Performs a normalization operation on the given path. This includes the following steps: 1) check path validity,
        /// 2) evaluate relative directory components, 3) trim unnecessary characters, 4) format directory separators in a customizable way
        /// </summary>
        /// <param name="path">The path to normalize</param>
        /// <param name="isFile">Whether the path represents a file (true) or a directory (false)</param>
        /// <param name="useAltSeparator">Use the alternate platform-specific character to separate directories (instead of standard)</param>
        /// <param name="trimEndSeparator">Trim the path's ending separator (for directory paths)</param>
        /// <returns>The normalized path string</returns>
        public static string Normalize(string path, bool isFile = true, bool useAltSeparator = false, bool trimEndSeparator = false)
        {
            Decompose(path, out string root, out List<string> folders, out string fileName, isFile);
            return Format(root, folders, fileName, useAltSeparator, trimEndSeparator);
        }

        /// <summary>
        /// Compute and return the absolute form of the given path, after normalization
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <param name="isFile">Whether the path represents a file (true) or a directory (false)</param>
        /// <param name="useAltSeparator">Use the alternate platform-specific character to separate directories (instead of standard)</param>
        /// <param name="trimEndSeparator">Trim the path's ending separator (for directory paths)</param>
        /// <returns>The full path string</returns>
        public static string GetFullPath(string path, bool isFile = true, bool useAltSeparator = false, bool trimEndSeparator = false)
        {
            Decompose(path, out string root, out List<string> folders, out string fileName, isFile);
            ApplyFullRoot(ref root, ref folders);
            return Format(root, folders, fileName, useAltSeparator, trimEndSeparator);
        }

        /// <summary>
        /// Compute and return the relative form of a given path compared to another, after normalization
        /// </summary>
        /// <param name="path">The main path to process</param>
        /// <param name="relativeTo">The base path the result should be relative to (always considered a directory)</param>
        /// <param name="isFile">Whether the main path represents a file (true) or a directory (false)</param>
        /// <param name="useAltSeparator">Use the alternate platform-specific character to separate directories (instead of standard)</param>
        /// <param name="trimEndSeparator">Trim the path's ending separator (for directory paths)</param>
        /// <returns>The relative path string</returns>
        public static string GetRelativePath(string path, string relativeTo, bool isFile = true, bool useAltSeparator = false, bool trimEndSeparator = false)
        {
            Decompose(path, out string root, out List<string> folders, out string fileName, isFile);
            ApplyFullRoot(ref root, ref folders);
            Decompose(relativeTo, out string baseRoot, out List<string> baseFolders, out _, false);
            ApplyFullRoot(ref baseRoot, ref baseFolders);
            SetRelativeTo(ref root, ref folders, baseRoot, baseFolders);
            return Format(root, folders, fileName, useAltSeparator, trimEndSeparator);
        }

        /// <summary>
        /// Concatenate an array of paths into a single path, without further checks
        /// </summary>
        /// <param name="paths">The paths to join</param>
        /// <returns>The joined path string</returns>
        public static string Join(params string[] paths)
        {
            if (paths.Length == 0)
                return string.Empty;
            if (paths.Length == 1)
                return paths[0];

            for (int i = 0; i < paths.Length - 1; i++)
            {
                paths[i] = paths[i].TrimEnd(DirectorySeparator, AltDirectorySeparator);
            }

            return string.Join(DirectorySeparator.ToString(), paths);
        }

        /// <summary>
        /// Get the root information contained in the given path, without further checks
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>The path root, eventually empty</returns>
        /// <remarks>
        /// This method differ from <see cref="Path.GetPathRoot(string)"/> in several ways: it makes no check of invalid characters,
        /// normalizes root separators, formats UNC roots with end separator and also supports device roots ("\\?\", "\\.\")
        /// </remarks>
        public static string GetRoot(string path)
        {
            string root = string.Empty;
            int index = 0;

            RootType type = GetRootType(path);
            switch (type)
            {
                case RootType.None:
                    return string.Empty;
                case RootType.CurrentDrive:
                    return $"{DirectorySeparator}";
                case RootType.DriveRelative:
                    return $"{char.ToUpper(path[0])}{VolumeSeparator}";
                case RootType.DriveAbsolute:
                    return $"{char.ToUpper(path[0])}{VolumeSeparator}{DirectorySeparator}";
                case RootType.UNC:
                    root = $"{DirectorySeparator}{DirectorySeparator}";
                    index = 2;
                    break;
                case RootType.Device:
                    root = EXTENDED_PREFIX;
                    index = EXTENDED_PREFIX.Length;
                    break;
                case RootType.DeviceUNC:
                    root = UNC_EXTENDED_PREFIX;
                    index = UNC_EXTENDED_PREFIX.Length;
                    break;
            }

            if (type == RootType.UNC || type == RootType.DeviceUNC)
            {
                int nb = 0;
                while (index < path.Length && (!IsDirectorySeparator(path[index]) || ++nb < 2))
                {
                    root += IsDirectorySeparator(path[index]) ? DirectorySeparator : path[index];
                    index++;
                }
                if (nb == 2)
                    root += DirectorySeparator;
            }
            else if (type == RootType.Device)
            {
                if (index + 1 < path.Length && IsVolumeSeparator(path[index + 1]) && IsValidDrive(path[index]))
                {
                    root += $"{char.ToUpper(path[index])}{VolumeSeparator}";
                    if (index + 2 < path.Length && IsDirectorySeparator(path[index + 2]))
                        root += DirectorySeparator;
                }
                else
                {
                    root = string.Empty;
                }
            }

            return root;
        }

        /// <summary>
        /// Get the file name information contained in the given path, without further checks
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>The path filename, eventually empty</returns>
        public static string GetFileName(string path)
        {
            int index = path.Length;
            while (index > 0 && !IsDirectorySeparator(path[index - 1]) && !IsVolumeSeparator(path[index - 1]))
                index--;

            return path.Substring(index);
        }

        /// <summary>
        /// Get the ordered list of folder names contained in the given path, without further checks
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>The path's list of folders, eventually empty</returns>
        public static List<string> GetFolders(string path)
        {
            Parse(path, out _, out List<string> folders, out _);
            return folders;
        }

        /// <summary>
        /// Get the extension information contained in the given file name, without further checks
        /// </summary>
        /// <param name="fileName">The file name to analyze</param>
        /// <returns>The file extension (including '.'), eventually empty</returns>
        public static string GetFileExtension(string fileName)
        {
            int index = fileName.Length;
            while (index > 0)
            {
                if (IsExtensionSeparator(fileName[index - 1]))
                {
                    if (index == fileName.Length)
                        return string.Empty;
                    return fileName.Substring(index - 1);
                } 
                index--;
            }

            return string.Empty;
        }

        /// <summary>
        /// Check if the given path contains a root
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>A boolean indicating if the path is rooted</returns>
        public static bool IsRooted(string path)
        {
            return (path.Length > 0 && IsDirectorySeparator(path[0]))
                || (path.Length > 1 && IsVolumeSeparator(path[1]) && IsValidDrive(path[0]));
        }

        /// <summary>
        /// Check if the given path contains a device root, i.e a root beginning with "\\?\" or "\\.\"
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>A boolean indicating if the path is a device path</returns>
        public static bool IsDevice(string path)
        {
            RootType prefix = GetRootType(path);
            return prefix == RootType.Device || prefix == RootType.DeviceUNC;
        }

        /// <summary>
        /// Check if the given path respects the Universal Naming Convention: "\\ComputerName\SharedFolder\[ResourcePath]"
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>A boolean indicating if the path respects the UNC</returns>
        public static bool IsUNCFormatted(string path)
        {
            RootType prefix = GetRootType(path);
            return prefix == RootType.UNC || prefix == RootType.DeviceUNC;
        }

        /// <summary>
        /// Check if the given path is fully qualified, i.e does not depend on the current drive or current directory
        /// </summary>
        /// <param name="path">The path to analyze</param>
        /// <returns>A boolean indicating if the path is fully qualified</returns>
        public static bool IsFullyQualified(string path)
        {
            RootType prefix = GetRootType(path);
            return prefix == RootType.DriveAbsolute || prefix == RootType.UNC || prefix == RootType.Device || prefix == RootType.DeviceUNC;
        }
        #endregion

        #region internal
        internal static void CheckPathValidity(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), NULL_PATH_MESSAGE);

            if (path.Length > MAX_PATH_LENGTH)
                throw new PathTooLongException(PATH_TOO_LONG_MESSAGE);

            if (path.IndexOfAny(InvalidPathChars) >= 0)
                throw new ArgumentException(INVALID_PATH_CHARS_MESSAGE, nameof(path));
        }

        internal static void CheckSegmentValidity(string segment, bool additionalCheckOnly = false)
        {
            if (additionalCheckOnly)
            {
                if (segment.IndexOfAny(InvalidSegmentCharsDiff) >= 0)
                    throw new ArgumentException(INVALID_PATH_CHARS_MESSAGE, nameof(segment));
            }
            else
            {
                if (segment == null)
                    throw new ArgumentNullException(nameof(segment), NULL_PATH_MESSAGE);

                if (segment.IndexOfAny(InvalidSegmentChars) >= 0)
                    throw new ArgumentException(INVALID_PATH_CHARS_MESSAGE, nameof(segment));
            }
        }

        internal static void CheckSegmentsValidity(List<string> folders, string fileName, bool additionalCheckOnly = false)
        {
            foreach (string folder in folders)
                CheckSegmentValidity(folder, additionalCheckOnly);

            CheckSegmentValidity(fileName, additionalCheckOnly);
        }

        internal static void Parse(string path, out string root, out List<string> folders, out string fileName,
            bool checkRoot = true, bool checkFile = true)
        {
            root = checkRoot ? GetRoot(path) : string.Empty;
            if (root.Length > 0)
                path = path.Substring(root.Length);

            fileName = checkFile ? GetFileName(path) : string.Empty;
            if (fileName.Length > 0)
                path = path.Substring(0, Math.Max(0, path.Length - fileName.Length - 1));

            char[] separators = new char[2] { DirectorySeparator, AltDirectorySeparator };
            folders = new List<string>(path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
        }

        internal static void Simplify(ref List<string> folders)
        {
            int nbParent = 0;
            for (int i = folders.Count - 1; i >= 0; i--)
            {
                if (folders[i] == ParentFolder)
                {
                    nbParent++;
                }
                else if (folders[i] == CurrentFolder)
                {
                    folders.RemoveAt(i);
                }
                else if (nbParent > 0)
                {
                    nbParent--;
                    folders.RemoveRange(i, 2);
                }
            }
        }

        internal static void Trim(ref List<string> folders, ref string fileName)
        {
            for (int i = 0; i < folders.Count; i++)
            {
                string folder = folders[i];
                if (folder.Length >= 2 && IsExtensionSeparator(folder[folder.Length - 1])
                    && !IsExtensionSeparator(folder[folder.Length - 2]))
                {
                    folders[i] = folder.Remove(folder.Length - 1);
                }
            }
            fileName = fileName.TrimEnd(' ', '.');
        }

        internal static string Format(string root, List<string> folders, string fileName, bool useAltSeparator = false, bool trimEndSeparator = false)
        {
            string separator = (useAltSeparator ? AltDirectorySeparator : DirectorySeparator).ToString();
            string endSeparator = (fileName.Length > 0 || !trimEndSeparator) ? separator : string.Empty;

            string path = "";
            path += useAltSeparator ? root.Replace(DirectorySeparator, AltDirectorySeparator) : root;
            path += string.Join(separator.ToString(), folders);
            path += endSeparator;
            path += fileName;

            return path;
        }

        internal static void ApplyFullRoot(ref string root, ref List<string> folders)
        {
            RootType type = GetRootType(root);
            if (type == RootType.None || type == RootType.CurrentDrive || type == RootType.DriveRelative)
            {
                Parse(Environment.CurrentDirectory, out string fullRoot, out List<string> fullFolders, out _, true, false);

                switch(type)
                {
                    case RootType.None:
                        root = fullRoot;
                        folders = fullFolders.Concat(folders).ToList();
                        break;
                    case RootType.CurrentDrive:
                        root = fullRoot;
                        break;
                    case RootType.DriveRelative:
                        root = root += DirectorySeparator;
                        folders = root == fullRoot ? fullFolders.Concat(folders).ToList() : folders;
                        break;
                }
            }
        }

        internal static void SetRelativeTo(ref string root, ref List<string> folders, string baseRoot, List<string> baseFolders)
        {
            if (root == baseRoot)
            {
                root = string.Empty;
                int index = 0;
                while (index < baseFolders.Count && folders.Count > 0 && baseFolders[index] == folders[0])
                {
                    folders.RemoveAt(0);
                    index++;
                }

                while (index < baseFolders.Count)
                {
                    folders.Insert(0, ParentFolder);
                    index++;
                }
            } 
        }
        #endregion

        #region private
        private enum RootType : byte
        {
            None,
            CurrentDrive,
            DriveRelative,
            DriveAbsolute,
            UNC,
            Device,
            DeviceUNC
        }

        private static bool IsDirectorySeparator(char character)
        {
            return character == DirectorySeparator || character == AltDirectorySeparator;
        }

        private static bool IsExtensionSeparator(char character)
        {
            return character == ExtensionSeparator;
        }

        private static bool IsVolumeSeparator(char character)
        {
            return character == VolumeSeparator;
        }

        private static bool IsValidDrive(char character)
        {
            return (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');
        }

        private static RootType GetRootType(string path)
        {
            if (path.Length > 0 && IsDirectorySeparator(path[0]))
            {
                if (path.Length > 1 && IsDirectorySeparator(path[1]))
                {
                    if (path.Length >= EXTENDED_PREFIX.Length
                        && (path[2] == '?' || path[2] == '.')
                        && IsDirectorySeparator(path[3]))
                    {
                        if (path.Length >= UNC_EXTENDED_PREFIX.Length
                            && path[4] == 'U' && path[5] == 'N' && path[6] == 'C'
                            && IsDirectorySeparator(path[7]))
                        {
                            return RootType.DeviceUNC;
                        }
                        return RootType.Device;
                    }
                    return RootType.UNC;
                }
                return RootType.CurrentDrive;
            }
            else if (path.Length > 1 && IsVolumeSeparator(path[1]) && IsValidDrive(path[0]))
            {
                if (path.Length > 2 && IsDirectorySeparator(path[2]))
                {
                    return RootType.DriveAbsolute;
                }
                return RootType.DriveRelative;
            }
            return RootType.None;
        }
        #endregion
    }
}
