using GameEngine.Core.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Core.Utilities.Models
{
    /// <summary>
    /// An object representation of a file or directory path, providing normalization, formatting, comparators and 
    /// easy access to the parts of the path
    /// </summary>
    public class Path
    {
        private const string EMPTY_FILE_MESSAGE = "Path does not contain a file";
        private const string MISPLACED_ROOT_PATH_MESSAGE = "Root path is not in the first position of a combined sequence";
        private const string MISPLACED_FILE_PATH_MESSAGE = "File path is not in the last position of a combine sequence";

        private string m_Root;
        private List<string> m_Folders;
        private string m_FileName;

        /// <summary>
        /// Initialize a new instance of Path
        /// </summary>
        /// <param name="path">A string containing path information</param>
        public Path(string path)
        {
            PathUtils.Decompose(path, out m_Root, out m_Folders, out m_FileName);
        }

        /// <summary>
        /// Initialize a new instance of Path
        /// </summary>
        /// <param name="path">A string containing path information</param>
        /// <param name="isFile">Whether the path represents a file (true) or a directory (false)</param>
        public Path(string path, bool isFile)
        {
            PathUtils.Decompose(path, out m_Root, out m_Folders, out m_FileName, isFile);

            if (isFile && string.IsNullOrEmpty(m_FileName))
                throw new ArgumentException(EMPTY_FILE_MESSAGE, nameof(path));
        }

        /// <summary>
        /// Initialize a new instance of Path
        /// </summary>
        /// <param name="paths">An array of strings containing relative consecutive parts of a path</param>
        public Path(params string[] paths)
        {
            ExceptionUtils.CheckNonNullCollection(paths, nameof(paths));
            for (int i = 1; i < paths.Length; i++)
            {
                if (PathUtils.IsRooted(paths[i]))
                    throw new ArgumentException(MISPLACED_ROOT_PATH_MESSAGE);
            }

            string path = PathUtils.Join(paths);
            PathUtils.Decompose(path, out m_Root, out m_Folders, out m_FileName);
        }

        /// <summary>
        /// Initialize a new instance of Path
        /// </summary>
        /// <param name="path1">A base Path instance</param>
        /// <param name="path2">A relative Path instance to combine with the base</param>
        public Path(Path path1, Path path2)
        {
            ExceptionUtils.CheckNonNull(path1, path2);

            if (path2.IsRooted())
                throw new ArgumentException(MISPLACED_ROOT_PATH_MESSAGE);

            if (!path1.IsDirectory())
                throw new ArgumentException(MISPLACED_FILE_PATH_MESSAGE);

            m_Root = path1.m_Root;
            m_Folders = path1.m_Folders.Concat(path2.m_Folders).ToList();
            m_FileName = path2.m_FileName;

            PathUtils.Simplify(ref m_Folders);
        }

        /// <summary>
        /// Initialize a new instance of Path
        /// </summary>
        /// <param name="path1">A base Path instance</param>
        /// <param name="path2">A relative path string to combine with the base</param>
        public Path(Path path1, string path2)
        {
            ExceptionUtils.CheckNonNull(path1, path2);

            if (PathUtils.IsRooted(path2))
                throw new ArgumentException(MISPLACED_ROOT_PATH_MESSAGE);

            if (!path1.IsDirectory())
                throw new ArgumentException(MISPLACED_FILE_PATH_MESSAGE);

            PathUtils.Decompose(path2, out _, out List<string> folders2, out m_FileName, true, false);
            m_Root = path1.m_Root;
            m_Folders = path1.m_Folders.Concat(folders2).ToList();

            PathUtils.Simplify(ref m_Folders);
        }

        /// <summary>
        /// Initialize a new instance of Path
        /// </summary>
        /// <param name="path1">A string containing the base path information</param>
        /// <param name="path2">A relative Path instance to combine with the base</param>
        public Path(string path1, Path path2)
        {
            ExceptionUtils.CheckNonNull(path1, path2);

            if (path2.IsRooted())
                throw new ArgumentException(MISPLACED_ROOT_PATH_MESSAGE);

            PathUtils.Decompose(path1, out m_Root, out List<string> folders1, out _, false, true);
            m_Folders = folders1.Concat(path2.m_Folders).ToList();
            m_FileName = path2.m_FileName;

            PathUtils.Simplify(ref m_Folders);
        }

        /// <summary>
        /// Check if the path contains a root
        /// </summary>
        /// <returns>A boolean indicating if the path is rooted</returns>
        public bool IsRooted() => !string.IsNullOrEmpty(m_Root);

        /// <summary>
        /// Check if the path represents a directory
        /// </summary>
        /// <returns>A boolean indicating if the path is a directory</returns>
        public bool IsDirectory() => string.IsNullOrEmpty(m_FileName);

        /// <summary>
        /// Check if the path contains a file name with an extension
        /// </summary>
        /// <returns>A boolean indicating if the path has a file extension</returns>
        public bool HasExtension() => !string.IsNullOrEmpty(PathUtils.GetFileExtension(m_FileName));

        /// <summary>
        /// Get the path root
        /// </summary>
        /// <returns>The path root, eventually empty</returns>
        public string GetRoot() => m_Root;

        /// <summary>
        /// Get the ordered list of folder names constituting the path structure
        /// </summary>
        /// <returns>The path's list of folders, eventually empty</returns>
        public List<string> GetFolders() => m_Folders;

        /// <summary>
        /// Get the name of the first folder in the path structure
        /// </summary>
        /// <returns>The first folder appearing in the path, eventually empty</returns>
        public string GetRootFolderName()
        {
            if (m_Folders.Count > 0)
                return m_Folders[0];

            return string.Empty;
        }

        /// <summary>
        /// Get the name of the last folder in the path structure
        /// </summary>
        /// <returns>The last folder appearing in the path, eventually empty</returns>
        public string GetLastFolderName()
        {
            if (m_Folders.Count > 0)
                return m_Folders[m_Folders.Count - 1];

            return string.Empty;
        }

        /// <summary>
        /// Get the complete directory information for the path, in a normalized format
        /// </summary>
        /// <returns>The path directory</returns>
        public string GetDirectory()
        {
            return m_Root + string.Join(PathUtils.DirectorySeparator.ToString(), m_Folders);
        }

        /// <summary>
        /// Get the path file name
        /// </summary>
        /// <returns>The path file name, eventually empty</returns>
        public string GetFileName() => m_FileName;

        /// <summary>
        /// Get the path file name without extension
        /// </summary>
        /// <returns>The path file name without extension, eventually empty</returns>
        public string GetFileNameWithoutExtension()
        {
            string extension = PathUtils.GetFileExtension(m_FileName);
            return m_FileName.Substring(0, m_FileName.Length - extension.Length);
        }

        /// <summary>
        /// Get the path file extension
        /// </summary>
        /// <returns>The file extension (including '.'), eventually empty</returns>
        public string GetExtension() => PathUtils.GetFileExtension(m_FileName);

        /// <summary>
        /// Check if the path contains a device root, i.e a root beginning with "\\?\" or "\\.\"
        /// </summary>
        /// <returns>A boolean indicating if the path is a device path</returns>
        public bool IsDevice() => PathUtils.IsDevice(m_Root);

        /// <summary>
        /// Check if the path root respects the Universal Naming Convention: "\\ComputerName\SharedFolder\[ResourcePath]"
        /// </summary>
        /// <returns>A boolean indicating if the path respects the UNC</returns>
        public bool IsUNCFormatted() => PathUtils.IsUNCFormatted(m_Root);

        /// <summary>
        /// Check if the path is fully qualified, i.e does not depend on the current drive or current directory
        /// </summary>
        /// <returns>A boolean indicating if the path is fully qualified</returns>
        public bool IsFullyQualified() => PathUtils.IsFullyQualified(m_Root);

        /// <summary>
        /// Change the file name in the path
        /// </summary>
        /// <param name="fileName">The new file name</param>
        /// <returns>The Path instance that has been modified</returns>
        public Path ChangeFile(string fileName)
        {
            PathUtils.CheckSegmentValidity(fileName);
            fileName = fileName.TrimEnd(' ', '.');
            m_FileName = fileName;
            return this;
        }

        /// <summary>
        /// Change the file extension in the path
        /// </summary>
        /// <param name="extension">The new extension</param>
        /// <returns>The Path instance that has been modified</returns>
        public Path ChangeExtension(string extension)
        {
            PathUtils.CheckSegmentValidity(extension);
            extension = extension.TrimEnd(' ', '.');
            if (extension.Length > 0 && extension[0] != '.')
                extension = $".{extension}";

            m_FileName = $"{GetFileNameWithoutExtension()}{extension}";
            return this;
        }

        /// <summary>
        /// Remove the file name from the path
        /// </summary>
        /// <returns>The Path instance that has been modified</returns>
        public Path RemoveFile()
        {
            m_FileName = string.Empty;
            return this;
        }

        /// <summary>
        /// Combine the current path to an additional relative path
        /// </summary>
        /// <param name="path">a string containing the relative path information</param>
        /// <returns>The Path instance that has been modified</returns>
        public Path Join(string path)
        {
            ExceptionUtils.CheckNonNull(path);
            if (!IsDirectory())
                throw new ArgumentException(MISPLACED_FILE_PATH_MESSAGE);
            if (PathUtils.IsRooted(path))
                throw new ArgumentException(MISPLACED_ROOT_PATH_MESSAGE);

            PathUtils.Decompose(path, out _, out List<string> pathFolders, out m_FileName, true, false);
            m_Folders = m_Folders.Concat(pathFolders).ToList();
            PathUtils.Simplify(ref m_Folders);

            return this;
        }

        /// <summary>
        /// Combine the current path to an additional relative path
        /// </summary>
        /// <param name="path">The relative Path instance to combine with the path</param>
        /// <returns>The Path instance that has been modified</returns>
        public Path Join(Path path)
        {
            ExceptionUtils.CheckNonNull(path);
            if (!IsDirectory())
                throw new ArgumentException(MISPLACED_FILE_PATH_MESSAGE);
            if (path.IsRooted())
                throw new ArgumentException(MISPLACED_ROOT_PATH_MESSAGE);

            m_Folders = m_Folders.Concat(path.m_Folders).ToList();
            m_FileName = path.m_FileName;
            PathUtils.Simplify(ref m_Folders);

            return this;
        }

        /// <summary>
        /// Shape the path in an absolute format
        /// </summary>
        /// <returns>The Path instance that has been modified</returns>
        public Path SetFullPath()
        {
            PathUtils.ApplyFullRoot(ref m_Root, ref m_Folders);
            return this;
        }

        /// <summary>
        /// Shape the path in a format that is relative to another path
        /// </summary>
        /// <param name="basePath">The base path to which the current path should be relative to</param>
        /// <returns>The Path instance that has been modified</returns>
        public Path SetRelativePath(string basePath)
        {
            PathUtils.Decompose(basePath, out string baseRoot, out List<string> baseFolders, out string _, false);
            PathUtils.ApplyFullRoot(ref m_Root, ref m_Folders);
            PathUtils.ApplyFullRoot(ref baseRoot, ref baseFolders);
            PathUtils.SetRelativeTo(ref m_Root, ref m_Folders, baseRoot, baseFolders);
            return this;
        }

        /// <summary>
        /// Represent the path information as a single string, with custom format options
        /// </summary>
        /// <param name="separatorType">Type of directory separator character to use</param>
        /// <param name="trimEndSeparator">Trim the path's ending separator (for directory paths)</param>
        /// <returns>The formatted path string</returns>
        public string Format(PathSeparatorType separatorType = PathSeparatorType.StdSeparator, bool trimEndSeparator = false)
        {
            return PathUtils.Format(m_Root, m_Folders, m_FileName, separatorType, trimEndSeparator);
        }

        /// <summary>
        /// Returns a string that represents the current path
        /// </summary>
        /// <returns>A string that represents the current path</returns>
        public override string ToString()
        {
            return Format();
        }

        /// <summary>
        /// Determines whether two paths are equal
        /// </summary>
        /// <param name="obj">The path object to compare with the current path</param>
        /// <returns>If the given path is equal to the current path</returns>
        public override bool Equals(object obj)
        {
            Path path = obj as Path;
            if (path == null)
            {
                string pathStr = obj as string;
                if (pathStr == null)
                    return false;

                path = new Path(pathStr);
            }

            return this.m_Root == path.m_Root
                && this.m_Folders.SequenceEqual(path.m_Folders)
                && this.m_FileName == path.m_FileName;
        }

        /// <summary>
        /// Default Hash function
        /// </summary>
        /// <returns>The hash code for the current Path instance</returns>
        public override int GetHashCode()
        {
            var hashCode = -1651685193;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Root);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(m_Folders);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_FileName);
            return hashCode;
        }
    }
}
