using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameEngine.Core.Utilities;
using System;
using System.IO;
using System.Collections.Generic;

namespace GameEnginesTest.UnitTests.Core
{
    /// <summary>
    /// Unit tests for the class PathUtils
    /// <see cref="PathUtils"/>
    /// </summary>
    [TestClass]
    public class PathUtilsTest
    {
        [TestMethod]
        public void DecomposeTest()
        {
            string validRoot = "C:\\";
            string validBaseFolder = "Base_Folder";
            string validSubFolder = "Sub Folder";
            string validFileName = "file.txt";

            // Null path throws ArgumentNullException
            Assert.ThrowsException<ArgumentNullException>(() => PathUtils.Decompose(null, out _, out _, out _));

            // Path with more than 32767 characters throws PathTooLongException
            string veryLongPath = new string('a', short.MaxValue + 1);
            Assert.ThrowsException<PathTooLongException>(() => PathUtils.Decompose(veryLongPath, out _, out _, out _));

            // Path with invalid characters throws ArgumentException
            string invalidPath = "C:/Path<>With|Invalid\"Chars";
            Assert.ThrowsException<ArgumentException>(() => PathUtils.Decompose(invalidPath, out _, out _, out _));

            // Empty path has empty decomposition
            PathUtils.Decompose("", out string root, out List<string> folders, out string fileName);
            Assert.AreEqual(string.Empty, root);
            Assert.AreEqual(0, folders.Count);
            Assert.AreEqual(string.Empty, fileName);

            // For a valid rooted file path -> correct decomposition in root, folders, filename
            string validPath = $"{validRoot}{validBaseFolder}/{validSubFolder}/{validFileName}";
            PathUtils.Decompose(validPath, out root, out folders, out fileName);
            Assert.AreEqual(validRoot, root);
            Assert.AreEqual(2, folders.Count);
            Assert.AreEqual(validBaseFolder, folders[0]);
            Assert.AreEqual(validSubFolder, folders[1]);
            Assert.AreEqual(validFileName, fileName);

            // For a valid rooted directory path -> correct decomposition in root, folders
            string validDirectory = $"{validRoot}{validBaseFolder}/{validSubFolder}";
            PathUtils.Decompose(validDirectory, out root, out folders, out fileName, false);
            Assert.AreEqual(validRoot, root);
            Assert.AreEqual(2, folders.Count);
            Assert.AreEqual(string.Empty, fileName);

            // For a valid unrooted path -> correct decomposition in folders, filename
            string validUnrooted = $"{validBaseFolder}/{validSubFolder}/{validFileName}";
            PathUtils.Decompose(validUnrooted, out root, out folders, out fileName, true, false);
            Assert.AreEqual(string.Empty, root);
            Assert.AreEqual(2, folders.Count);
            Assert.AreEqual(validFileName, fileName);

            // Path with invalid file or folder name characters throws ArgumentException
            string invalidFolderPath = "C:/Invalid:Folder/";
            string invalidFilePath = "invalid?file.json";
            Assert.ThrowsException<ArgumentException>(() => PathUtils.Decompose(invalidFolderPath, out _, out _, out _, false));
            Assert.ThrowsException<ArgumentException>(() => PathUtils.Decompose(invalidFilePath, out _, out _, out _, false, true));

            // Path with relative directory components is correctly evaluated
            string pathToSimplify = $"../{validBaseFolder}/Folder1/./Folder2/../../{validSubFolder}/";
            PathUtils.Decompose(pathToSimplify, out root, out folders, out fileName, false, false);
            Assert.AreEqual(3, folders.Count);
            Assert.AreEqual("..", folders[0]);
            Assert.AreEqual(validBaseFolder, folders[1]);
            Assert.AreEqual(validSubFolder, folders[2]);

            // Path components with unnecessary characters are trimmed
            string pathToTrim = $"{validBaseFolder}./{validFileName}...  ";
            PathUtils.Decompose(pathToTrim, out root, out folders, out fileName);
            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual(validBaseFolder, folders[0]);
            Assert.AreEqual(validFileName, fileName);
        }

        [TestMethod]
        public void NormalizeTest()
        {
            // Null path throws ArgumentNullException
            Assert.ThrowsException<ArgumentNullException>(() => PathUtils.Normalize(null));

            // Path with more than 32767 characters throws PathTooLongException
            string veryLongPath = new string('a', short.MaxValue + 1);
            Assert.ThrowsException<PathTooLongException>(() => PathUtils.Normalize(veryLongPath));

            // Path with invalid characters throws ArgumentException
            string invalidPath = "C:/Path<>With|Invalid\"Chars";
            Assert.ThrowsException<ArgumentException>(() => PathUtils.Normalize(invalidPath));

            // Path with invalid file or folder name characters throws ArgumentException
            string invalidSegmentsPath = "C:/Invalid:Folder/invalid?file.json";
            Assert.ThrowsException<ArgumentException>(() => PathUtils.Normalize(invalidSegmentsPath));

            // All separators are normalized in standard or alternative form according to useAltSeparator
            string validPath = "//Computer\\Share\\Dir1/Dir2\\Dir3/file.html";
            Assert.AreEqual("\\\\Computer\\Share\\Dir1\\Dir2\\Dir3\\file.html", 
                PathUtils.Normalize(validPath, true, false));
            Assert.AreEqual("//Computer/Share/Dir1/Dir2/Dir3/file.html",
                PathUtils.Normalize(validPath, true, true));

            // Directory path ends with a separator, unless dirEndSeparator is false
            Assert.AreEqual($"C:/Directory/", PathUtils.Normalize("C:/Directory", false, true, true));
            Assert.AreEqual($"C:/Directory", PathUtils.Normalize("C:/Directory/", false, true, false));

            // Path with relative directory components is correctly evaluated
            string pathToSimplify = "Dir1/./Dir2/../Dir3/Dir4/../";
            Assert.AreEqual("Dir1/Dir3/", PathUtils.Normalize(pathToSimplify, false, true));

            // Path components with unnecessary characters are trimmed
            string pathToTrim = "/Directory./.file ...";
            Assert.AreEqual("/Directory/.file", PathUtils.Normalize(pathToTrim, true, true));
        }

        [TestMethod]
        public void GetFullPathTest()
        {
            // Null path throws ArgumentNullException
            Assert.ThrowsException<ArgumentNullException>(() => PathUtils.GetFullPath(null));

            // Invalid paths throws ArgumentException
            string invalidPath = "C:/Path<>With|Invalid\"Chars";
            string invalidSegmentsPath = "C:/Invalid:Folder/invalid?file.json";
            Assert.ThrowsException<ArgumentException>(() => PathUtils.GetFullPath(invalidPath));
            Assert.ThrowsException<ArgumentException>(() => PathUtils.GetFullPath(invalidSegmentsPath));

            // Absolute paths do not change, except for normalization
            string absoluteNormalizedPath = "C:\\Absolute\\Normalized\\Path\\";
            string absoluteNotNormalizedPath = "C:/Absolute/./Not/../Normalized./Path";
            Assert.AreEqual(absoluteNormalizedPath, PathUtils.GetFullPath(absoluteNormalizedPath, false));
            Assert.AreEqual(absoluteNormalizedPath, PathUtils.GetFullPath(absoluteNotNormalizedPath, false));

            // Relative paths are correctly modified, taking as reference the result of System.IO.Path.GetFullPath()
            string unrootedPath = "Folder\\file.txt";
            string currentDrivePath = "\\Folder\\file.txt";
            string driveRelativePath = "C:Folder\\file.txt";
            Assert.AreEqual(Path.GetFullPath(unrootedPath), PathUtils.GetFullPath(unrootedPath));
            Assert.AreEqual(Path.GetFullPath(currentDrivePath), PathUtils.GetFullPath(currentDrivePath));
            Assert.AreEqual(Path.GetFullPath(driveRelativePath), PathUtils.GetFullPath(driveRelativePath));
        }

        [TestMethod]
        public void GetRelativePathTest()
        {
            string sourceDirectory = "C:\\BaseFolder\\SubFolder";
            string relativeDirectory = "RelativeFolder\\file.txt";
            string mainPath = $"{sourceDirectory}\\{relativeDirectory}";

            // One of the paths is null -> throws ArgumentNullException
            Assert.ThrowsException<ArgumentNullException>(() => PathUtils.GetRelativePath(null, sourceDirectory));
            Assert.ThrowsException<ArgumentNullException>(() => PathUtils.GetRelativePath(mainPath, null));

            // One of the paths is invalid -> throws ArgumentException
            string invalidPath = "C:/Path<>With|Invalid\"Chars";
            string invalidSegmentsPath = "C:/Invalid:Folder/invalid?file.json";
            Assert.ThrowsException<ArgumentException>(() => PathUtils.GetRelativePath(invalidPath, sourceDirectory));
            Assert.ThrowsException<ArgumentException>(() => PathUtils.GetRelativePath(mainPath, invalidPath));
            Assert.ThrowsException<ArgumentException>(() => PathUtils.GetRelativePath(invalidSegmentsPath, sourceDirectory));
            Assert.ThrowsException<ArgumentException>(() => PathUtils.GetRelativePath(mainPath, invalidSegmentsPath));

            // When the source path is fully included in the main path
            string sourcePath = sourceDirectory;
            Assert.AreEqual(relativeDirectory, PathUtils.GetRelativePath(mainPath, sourcePath));

            // When the source path is partially included in the main path
            sourcePath = $"{sourceDirectory}\\OtherFolder\\";
            Assert.AreEqual($"..\\{relativeDirectory}", PathUtils.GetRelativePath(mainPath, sourcePath));

            // When the source path is completely different from the main path
            sourcePath = $"D:\\OtherFolder\\";
            Assert.AreEqual(mainPath, PathUtils.GetRelativePath(mainPath, sourcePath));

            // Same results are obtained when main and source paths are relative paths
            sourceDirectory = "SubFolder";
            mainPath = $"{sourceDirectory}\\{relativeDirectory}";
            Assert.AreEqual(relativeDirectory, PathUtils.GetRelativePath(mainPath, sourceDirectory));
            Assert.AreEqual($"..\\{relativeDirectory}", PathUtils.GetRelativePath(mainPath, $"{sourceDirectory}\\OtherFolder\\"));
        }

        [TestMethod]
        public void JoinTest()
        {
            // With empty list of paths -> result is empty
            Assert.AreEqual(string.Empty, PathUtils.Join(new string[0]));

            // Joined paths are separated by a single standard directory separator
            string[] basicPaths = new string[3] { "pathA", "pathB", "pathC" };
            Assert.AreEqual("pathA\\pathB\\pathC", PathUtils.Join(basicPaths));

            // The separators in the given paths remain unchanged, unless they are at the end
            string[] pathsWithSeparators = new string[3] { "pathA/pathA1\\", "pathB\\pathB1/", "pathC/" };
            Assert.AreEqual("pathA/pathA1\\pathB\\pathB1\\pathC/", PathUtils.Join(pathsWithSeparators));
        }

        [TestMethod]
        public void GetRootTest()
        {
            string relativePath = "Folder/file.txt";

            // For empty paths -> root is empty
            Assert.AreEqual(string.Empty, PathUtils.GetRoot(""));

            // For relative paths -> root is empty
            Assert.AreEqual(string.Empty, PathUtils.GetRoot(relativePath));

            // For absolute paths on the current drive -> root is "\"
            Assert.AreEqual("\\", PathUtils.GetRoot($"/{relativePath}"));

            // For relative paths on a given X drive -> root is "X:"
            Assert.AreEqual("C:", PathUtils.GetRoot($"C:{relativePath}"));

            // For absolute paths on a given X drive -> root is "X:\"
            Assert.AreEqual("C:\\", PathUtils.GetRoot($"C:/{relativePath}"));

            // For UNC paths -> root is "\\ComputerName\SharedFolder\" (or part of it)
            Assert.AreEqual("\\\\ComputerName\\SharedFolder\\", PathUtils.GetRoot($"//ComputerName/SharedFolder/{relativePath}"));
            Assert.AreEqual("\\\\ComputerName\\SharedFolder", PathUtils.GetRoot($"//ComputerName/SharedFolder"));
            Assert.AreEqual("\\\\ComputerName", PathUtils.GetRoot($"//ComputerName"));

            // For device path -> root begins with "\\?\", followed by the drive reference
            Assert.AreEqual("\\\\?\\C:", PathUtils.GetRoot($"//?/C:{relativePath}"));
            Assert.AreEqual("\\\\?\\C:", PathUtils.GetRoot($"//./C:{relativePath}"));
            Assert.AreEqual("\\\\?\\C:\\", PathUtils.GetRoot($"//?/C:/{relativePath}"));
            Assert.AreEqual("\\\\?\\C:\\", PathUtils.GetRoot($"//./C:/{relativePath}"));

            // For UNC device path -> root is "\\?\UNC\DeviceName\SharedFolder\" (or part of it)
            Assert.AreEqual("\\\\?\\UNC\\DeviceName\\SharedFolder\\", PathUtils.GetRoot($"//?/UNC/DeviceName/SharedFolder/{relativePath}"));
            Assert.AreEqual("\\\\?\\UNC\\DeviceName\\SharedFolder\\", PathUtils.GetRoot($"//./UNC/DeviceName/SharedFolder/{relativePath}"));
            Assert.AreEqual("\\\\?\\UNC\\DeviceName", PathUtils.GetRoot($"//?/UNC/DeviceName"));
        }

        [TestMethod]
        public void GetFileNameTest()
        {
            string fileName = "fileExample.png";

            // The filename can be found in the path when isolated, after some folders or after the root
            Assert.AreEqual(fileName, PathUtils.GetFileName(fileName));
            Assert.AreEqual(fileName, PathUtils.GetFileName($"Folder/{fileName}"));
            Assert.AreEqual(fileName, PathUtils.GetFileName($"C:{fileName}"));

            // Directory paths have empty filename
            Assert.AreEqual(string.Empty, PathUtils.GetFileName($"C:/Folder/"));
        }

        [TestMethod]
        public void GetFoldersTest()
        {
            string folder1 = "Folder.1";
            string folder2 = "Folder_2";
            string folder3 = "Folder 3";

            // The folder's names can be found in the path when isolated
            List<string> folders = PathUtils.GetFolders($"{folder1}/{folder2}/{folder3}/");
            Assert.AreEqual(3, folders.Count);
            Assert.AreEqual(folder1, folders[0]);
            Assert.AreEqual(folder2, folders[1]);
            Assert.AreEqual(folder3, folders[2]);

            // The folder's names can be found in the path after the root and/or before the filename
            folders = PathUtils.GetFolders($"C:/{folder1}/{folder2}/file.txt");
            Assert.IsTrue(folders.Count == 2 && folders[0] == folder1 && folders[1] == folder2);
            folders = PathUtils.GetFolders($"C:/{folder1}/");
            Assert.IsTrue(folders.Count == 1 && folders[0] == folder1);
            folders = PathUtils.GetFolders($"{folder1}/file.txt");
            Assert.IsTrue(folders.Count == 1 && folders[0] == folder1);
        }

        [TestMethod]
        public void GetFileExtensionTest()
        {
            string fileName = "file_name";

            // For an empty file -> extension is empty
            Assert.AreEqual(string.Empty, PathUtils.GetFileExtension(""));

            // For a file with no extension seperator -> extension is empty
            Assert.AreEqual(string.Empty, PathUtils.GetFileExtension(fileName));

            // For a file with one extension seperator -> extension is the second part (including the '.')
            Assert.AreEqual(".jpeg", PathUtils.GetFileExtension($"{fileName}.jpeg"));

            // For a file with several extension seperators -> extension is the last part (including the '.')
            Assert.AreEqual(".json", PathUtils.GetFileExtension($"{fileName}.test.user.json"));

            // For a file ending with an extension separator -> extension is empty
            Assert.AreEqual(string.Empty, PathUtils.GetFileExtension($"{fileName}."));
        }

        [TestMethod]
        public void IsRootedTest()
        {
            string basePath = "Folder/file.txt";

            // If path has a root of any type -> returns true
            Assert.IsTrue(PathUtils.IsRooted($"/{basePath}"));
            Assert.IsTrue(PathUtils.IsRooted($"C:/{basePath}"));
            Assert.IsTrue(PathUtils.IsRooted($"C:{basePath}"));
            Assert.IsTrue(PathUtils.IsRooted($"//ComputerName/SharedFolder/{basePath}"));
            Assert.IsTrue(PathUtils.IsRooted($"//./C:/{basePath}"));
            Assert.IsTrue(PathUtils.IsRooted($"//?/UNC/Device/Share/{basePath}"));

            // If path is empty or relative -> returns false
            Assert.IsFalse(PathUtils.IsRooted(""));
            Assert.IsFalse(PathUtils.IsRooted(basePath));
        }

        [TestMethod]
        public void IsDeviceTest()
        {
            string basePath = "Folder/file.txt";

            // If path begins with "//?/" or "//./" -> returns true
            Assert.IsTrue(PathUtils.IsDevice($"//?/C:/{basePath}"));
            Assert.IsTrue(PathUtils.IsDevice($"//./C:{basePath}"));
            Assert.IsTrue(PathUtils.IsDevice($"//?/UNC/Device/Share/{basePath}"));
            Assert.IsTrue(PathUtils.IsDevice($"//./UNC/Device/Share/{basePath}"));

            // In any other case -> returns false
            Assert.IsFalse(PathUtils.IsDevice(""));
            Assert.IsFalse(PathUtils.IsDevice(basePath));
            Assert.IsFalse(PathUtils.IsDevice($"/{basePath}"));
            Assert.IsFalse(PathUtils.IsDevice($"C:/{basePath}"));
            Assert.IsFalse(PathUtils.IsDevice($"C:{basePath}"));
            Assert.IsFalse(PathUtils.IsDevice($"//ComputerName/SharedFolder/{basePath}"));
        }

        [TestMethod]
        public void IsUNCFormatted()
        {
            string basePath = "Folder/file.txt";

            // If path begins with "//" or "//?/UNC/" or "//./UNC/" -> returns true
            Assert.IsTrue(PathUtils.IsUNCFormatted($"//ComputerName/SharedFolder/{basePath}"));
            Assert.IsTrue(PathUtils.IsUNCFormatted($"//ComputerName"));
            Assert.IsTrue(PathUtils.IsUNCFormatted($"//?/UNC/DeviceName/SharedFolder/{basePath}"));
            Assert.IsTrue(PathUtils.IsUNCFormatted($"//./UNC/DeviceName/SharedFolder/{basePath}"));

            // In any other case -> returns false
            Assert.IsFalse(PathUtils.IsUNCFormatted(""));
            Assert.IsFalse(PathUtils.IsUNCFormatted(basePath));
            Assert.IsFalse(PathUtils.IsUNCFormatted($"/{basePath}"));
            Assert.IsFalse(PathUtils.IsUNCFormatted($"C:/{basePath}"));
            Assert.IsFalse(PathUtils.IsUNCFormatted($"C:{basePath}"));
            Assert.IsFalse(PathUtils.IsUNCFormatted($"//?/C:/{basePath}"));
            Assert.IsFalse(PathUtils.IsUNCFormatted($"//./C:{basePath}"));
        }

        [TestMethod]
        public void IsFullyQualifiedTest()
        {
            string basePath = "Folder/file.txt";

            // If path is absolute on a given drive or absolute on a given device or UNC formatted -> return true
            Assert.IsTrue(PathUtils.IsFullyQualified($"C:/{basePath}"));
            Assert.IsTrue(PathUtils.IsFullyQualified($"//ComputerName/SharedFolder/{basePath}"));
            Assert.IsTrue(PathUtils.IsFullyQualified($"//?/C:/{basePath}"));
            Assert.IsTrue(PathUtils.IsFullyQualified($"//./C:{basePath}"));
            Assert.IsTrue(PathUtils.IsFullyQualified($"//?/UNC/Device/Share/{basePath}"));

            // If path is relative or absolute on the current drive -> returns false
            Assert.IsFalse(PathUtils.IsFullyQualified(""));
            Assert.IsFalse(PathUtils.IsFullyQualified(basePath));
            Assert.IsFalse(PathUtils.IsFullyQualified($"/{basePath}"));
            Assert.IsFalse(PathUtils.IsFullyQualified($"C:{basePath}"));
        }
    }
}
