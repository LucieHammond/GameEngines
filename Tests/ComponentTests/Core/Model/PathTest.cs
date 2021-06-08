using GameEngine.Core.Utilities.Enums;
using GameEngine.Core.Utilities.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class Path
    /// <see cref="Path"/>
    /// </summary>
    [TestClass]
    public class PathTest
    {
        private readonly string m_Root;
        private readonly string m_BaseFolder;
        private readonly string m_SubFolder;
        private readonly string m_FileName;
        private readonly string m_BaseFileName;
        private readonly string m_Extension;

        public PathTest()
        {
            m_Root = @"C:\";
            m_BaseFolder = "Base_Folder";
            m_SubFolder = "Sub Folder";
            m_BaseFileName = "file";
            m_Extension = ".txt";
            m_FileName = m_BaseFileName + m_Extension;
        }

        [TestMethod]
        public void InitializePath()
        {
            void validateInitialization(Path path)
            {
                Assert.AreEqual(m_Root, path.GetRoot());
                Assert.AreEqual(2, path.GetFolders().Count);
                Assert.AreEqual(m_BaseFolder, path.GetFolders()[0]);
                Assert.AreEqual(m_SubFolder, path.GetFolders()[1]);
                Assert.AreEqual(m_FileName, path.GetFileName());
            }

            // Create a Path from a single string
            Assert.ThrowsException<ArgumentNullException>(() => new Path((string)null));
            Path path = new Path($@"{m_Root}{m_BaseFolder}\{m_SubFolder}\{m_FileName}");
            validateInitialization(path);

            // Create a Path from a single string, with indication of type (file or directory)
            Assert.ThrowsException<ArgumentNullException>(() => new Path(null, false));
            Assert.ThrowsException<ArgumentException>(() => new Path($@"{m_BaseFolder}\", true));
            path = new Path($@"{m_Root}{m_BaseFolder}\{m_SubFolder}\{m_FileName}", true);
            validateInitialization(path);

            // Create a Path from multiple string parts
            Assert.ThrowsException<ArgumentNullException>(() => new Path(m_Root, null, m_FileName));
            path = new Path(m_Root, m_BaseFolder, m_SubFolder, m_FileName);
            validateInitialization(path);

            // Create a Path from two other Paths
            Path path1 = new Path($@"{m_Root}{m_BaseFolder}", false);
            Path path2 = new Path($@"{m_SubFolder}\{m_FileName}", true);
            Assert.ThrowsException<ArgumentNullException>(() => new Path(path1, (Path)null));
            Assert.ThrowsException<ArgumentException>(() => new Path(path1, new Path(@"C:\")));
            Assert.ThrowsException<ArgumentException>(() => new Path(new Path("file"), path2));
            path = new Path(path1, path2);
            validateInitialization(path);

            // Create a Path from a Path and a string
            path1 = new Path($@"{m_Root}{m_BaseFolder}", false);
            string path2bis = $@"{m_SubFolder}\{m_FileName}";
            Assert.ThrowsException<ArgumentNullException>(() => new Path(path1, (string)null));
            Assert.ThrowsException<ArgumentException>(() => new Path(path1, @"C:\"));
            Assert.ThrowsException<ArgumentException>(() => new Path(new Path("file"), path2bis));
            path = new Path(path1, path2bis);
            validateInitialization(path);

            // Create a Path from a string and a Path
            string path1bis = $@"{m_Root}{m_BaseFolder}";
            path2 = new Path($@"{m_SubFolder}\{m_FileName}");
            Assert.ThrowsException<ArgumentNullException>(() => new Path(path1bis, (Path)null));
            Assert.ThrowsException<ArgumentException>(() => new Path(path1bis, new Path(@"C:\")));
            path = new Path(path1bis, path2);
            validateInitialization(path);
        }

        [TestMethod]
        public void AccessPathInformation()
        {
            Path path = new Path($@"{m_Root}{m_BaseFolder}\{m_SubFolder}\{m_FileName}");

            // Analyze path's basic characteristics
            Assert.IsTrue(path.IsRooted());
            Assert.IsFalse(path.IsDirectory());
            Assert.IsTrue(path.HasExtension());

            // Retrieve parts of the path
            Assert.AreEqual(m_Root, path.GetRoot());
            List<string> folders = path.GetFolders();
            Assert.IsTrue(folders.Count == 2 && folders[0] == m_BaseFolder && folders[1] == m_SubFolder);
            Assert.AreEqual(m_BaseFolder, path.GetRootFolderName());
            Assert.AreEqual(m_SubFolder, path.GetLastFolderName());
            Assert.AreEqual($@"{m_Root}{m_BaseFolder}\{m_SubFolder}", path.GetDirectory());
            Assert.AreEqual(m_FileName, path.GetFileName());
            Assert.AreEqual(m_BaseFileName, path.GetFileNameWithoutExtension());
            Assert.AreEqual(m_Extension, path.GetExtension());

            // Check properties related to the root
            Assert.IsFalse(path.IsDevice());
            Assert.IsFalse(path.IsUNCFormatted());
            Assert.IsTrue(path.IsFullyQualified());
        }

        [TestMethod]
        public void TransformPath()
        {
            Path path = new Path($@"\{m_BaseFolder}\{m_SubFolder}\{m_FileName}");

            // Change file
            string newFile = "new_file.json";
            path.ChangeFile(newFile);
            Assert.AreEqual(newFile, path.GetFileName());

            // Change extension
            string newExtension = ".png";
            path.ChangeExtension(newExtension);
            Assert.AreEqual(newExtension, path.GetExtension());
            Assert.AreEqual("new_file.png", path.GetFileName());

            // Remove file
            path.RemoveFile();
            Assert.AreEqual(string.Empty, path.GetFileName());
            Assert.IsTrue(path.IsDirectory());

            // Join to an additional relative path (version with string)
            string newFolder = "NewFolder";
            path.Join($@"{newFolder}\");
            Assert.AreEqual(3, path.GetFolders().Count);
            Assert.AreEqual(newFolder, path.GetLastFolderName());

            // Join to an additional relative path (version with Path instance)
            string newFolderBis = "NewFolderBis";
            string newFileBis = "new_file_bis.fbx";
            path.Join(new Path($@"..\{newFolderBis}\{newFileBis}"));
            Assert.AreEqual(3, path.GetFolders().Count);
            Assert.AreEqual(newFolderBis, path.GetLastFolderName());
            Assert.AreEqual(newFileBis, path.GetFileName());

            // Set to full root form
            Assert.AreEqual(@"\", path.GetRoot());
            path.SetFullPath();
            Assert.AreNotEqual(@"\", path.GetRoot());
            Assert.IsTrue(path.IsFullyQualified());

            // Set to relative root form
            string basePath = $@"\{m_BaseFolder}\";
            path.SetRelativePath(basePath);
            Assert.IsFalse(path.IsRooted());
            Assert.AreEqual(2, path.GetFolders().Count);
            Assert.AreEqual(m_SubFolder, path.GetRootFolderName());
        }

        [TestMethod]
        public void FormatPathRepresentation()
        {
            Path path = new Path($@"\\A\B", "C/D/E/");

            // Format with standard directory separators and an end separator
            string expectedFormat = System.IO.Path.DirectorySeparatorChar == '\\' ? @"\\A\B\C\D\E\" : @"//A/B/C/D/E/";
            Assert.AreEqual(expectedFormat, path.Format(PathSeparatorType.StdSeparator, false));

            // Format with alternative directory separators and an end separator
            expectedFormat = System.IO.Path.AltDirectorySeparatorChar == '\\' ? @"\\A\B\C\D\E\" : @"//A/B/C/D/E/";
            Assert.AreEqual(expectedFormat, path.Format(PathSeparatorType.AltSeparator, false));

            // Format with forward slash directory separators and an end separator
            Assert.AreEqual(@"//A/B/C/D/E/", path.Format(PathSeparatorType.ForwardSlash, false));

            // Format with backslash directory separators and an end separator
            Assert.AreEqual(@"\\A\B\C\D\E\", path.Format(PathSeparatorType.BackSlash, false));

            // Format with forward slash directory separators and no end separator
            Assert.AreEqual(@"//A/B/C/D/E", path.Format(PathSeparatorType.ForwardSlash, true));

            // Format with backslash directory separators and no end separator
            Assert.AreEqual(@"\\A\B\C\D\E", path.Format(PathSeparatorType.BackSlash, true));

            // Use ToString() method for standard formatting
            Assert.AreEqual(@"\\A\B\C\D\E\", path.ToString());
        }

        [TestMethod]
        public void ComparePaths()
        {
            Path path = new Path($@"{m_Root}{m_BaseFolder}\{m_SubFolder}\{m_FileName}");

            // When the compared path is the same as the current path -> Equals return true
            string samePath = $@"{m_Root}{m_BaseFolder}/OtherFolder/../{m_SubFolder}/./{m_FileName} ";
            Assert.IsTrue(path.Equals(new Path(samePath)));
            Assert.IsTrue(path.Equals(samePath));

            // When the compared path has different root -> Equals return false
            string differentRoot = $@"\\?\C:\{m_BaseFolder}\{m_SubFolder}\{m_FileName}";
            Assert.IsFalse(path.Equals(new Path(differentRoot)));

            // When the compared path has different folders -> Equals return false
            string differentFolders = $@"{m_Root}{m_BaseFolder}\OtherFolder\{m_FileName}";
            Assert.IsFalse(path.Equals(new Path(differentFolders)));

            // When the compared path has different filename -> Equals return false
            string differentFile = $@"{m_Root}{m_BaseFolder}\{m_SubFolder}\other_file";
            Assert.IsFalse(path.Equals(new Path(differentFile)));
        }
    }
}
