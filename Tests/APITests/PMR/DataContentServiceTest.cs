using GameEngine.Core.Serialization.Text;
using GameEngine.Core.System;
using GameEngine.Core.Utilities.Enums;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Basics.Content;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEnginesTest.APITests.PMR
{
    /// <summary>
    /// API tests for the <see cref="DataContentService"/> class implementing the <see cref="IDataContentService"/> interface
    /// </summary>
    [TestClass]
    public class DataContentServiceTest
    {
        private DataContentService m_ContentService;

        private string[] m_ContentNames;
        private string m_CollectionName;
        private Dictionary<string, string> m_ExpectedContent;

        public DataContentServiceTest()
        {
            m_ContentService = new DataContentService();
            m_ContentService.ConfigurationService = new DataConfigurationStub();

            // These are the content values written in the content files to be loaded
            m_ContentNames = new string[] { "first", "second", "third" };
            m_CollectionName = "collection";
            m_ExpectedContent = new Dictionary<string, string>()
            {
                { "first", "My First Content" },
                { "second", "My Second Content" },
                { "third", "My Third Content" }
            };
        }

        [TestInitialize]
        public void Initialize()
        {
            m_ContentService.BaseInitialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            m_ContentService.BaseUnload();
        }

        [TestMethod]
        public void GetContentDataTest()
        {
            // If the content name is not in manifest -> return null
            Assert.IsNull(m_ContentService.GetContentData<TestData>("unknown"));

            // If the request is valid -> correct data is returned and cached
            TestData firstData = m_ContentService.GetContentData<TestData>(m_ContentNames[0]);
            Assert.IsNotNull(firstData);
            Assert.AreEqual(m_ContentNames[0], firstData.ContentId);
            Assert.AreEqual(m_ExpectedContent[m_ContentNames[0]], firstData.ContentValue);

            // If the given type is incorrect after cache is made -> return null
            Assert.IsNull(m_ContentService.GetContentData<OtherData>(m_ContentNames[0]));

            // If requested again -> the same content object is returned
            TestData sameFirstData = m_ContentService.GetContentData<TestData>(m_ContentNames[0]);
            Assert.AreEqual(firstData, sameFirstData);
        }

        [TestMethod]
        public void GetDataCollectionTest()
        {
            // If the collection name is not in manifest -> return null
            Assert.IsNull(m_ContentService.GetDataCollection<TestData>("unknownCollection"));

            // If the request is valid -> correct data is returned and cached
            List<TestData> collectionData = m_ContentService.GetDataCollection<TestData>(m_CollectionName).ToList();
            Assert.IsNotNull(collectionData);
            Assert.AreEqual(m_ExpectedContent.Count, collectionData.Count);
            for(int i = 0; i < collectionData.Count; i++)
            {
                Assert.AreEqual(m_ContentNames[i], collectionData[i].ContentId);
                Assert.AreEqual(m_ExpectedContent[m_ContentNames[i]], collectionData[i].ContentValue);
            }

            // If requested again -> the same content objects are returned
            List<TestData> sameCollectionData = m_ContentService.GetDataCollection<TestData>(m_CollectionName).ToList();
            for (int i = 0; i < collectionData.Count; i++)
            {
                Assert.AreEqual(collectionData[i], sameCollectionData[i]);
            }
        }

        [TestMethod]
        public void ResetLoadedContentTest()
        {
            // Request collection -> data is loaded and cached into memory
            List<TestData> cachedCollection = m_ContentService.GetDataCollection<TestData>(m_CollectionName).ToList();
            Assert.AreEqual(cachedCollection.Count, m_ContentService.DataCacheSize);

            // Request data content -> return cached object
            TestData secondData = m_ContentService.GetContentData<TestData>(m_ContentNames[1]);
            Assert.AreEqual(cachedCollection[1], secondData);

            // Reset loaded content
            m_ContentService.ResetLoadedContent();
            Assert.AreEqual(0, m_ContentService.DataCacheSize);

            // Request data content again -> return new object
            TestData newSecondData = m_ContentService.GetContentData<TestData>(m_ContentNames[1]);
            Assert.AreNotEqual(cachedCollection[1], newSecondData);
            Assert.AreNotEqual(secondData, newSecondData);
        }
    }

    #region Dependencies
    public class DataConfigurationStub : IConfigurationService
    {
        private ContentConfiguration m_Configuration;

        public DataConfigurationStub()
        {
            m_Configuration = new ContentConfiguration()
            {
                EnableContentData = true,
                DataContentPath = $"{TestUtils.GetResourcesPath()}/Content/",
                FileContentFormat = SerializerFormat.Json,
                FileEncodingType = EncodingType.UTF8
            };
        }

        public TConfig GetConfiguration<TConfig>(string configId) where TConfig : class
        {
            if (configId == ContentConfiguration.CONFIG_ID)
            {
                return m_Configuration as TConfig;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool ValidateConfiguration<TConfig>(string configId)
        {
            return true;
        }
    }
    #endregion

    #region Models
    public class TestData : ContentData
    {
        public string ContentValue;
    }

    public class OtherData : ContentData
    {
    }
    #endregion
}
