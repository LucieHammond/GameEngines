using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Unity.Basics.Content;
using GameEngine.PMR.UnityTests.Runtime.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.PMR.UnityTests
{
    /// <summary>
    /// API tests for the <see cref="DescriptorContentService"/> class implementing the <see cref="IDescriptorContentService"/> interface
    /// </summary>
    public class DescriptorContentServiceTest
    {
        private DescriptorContentService m_ContentService;

        private readonly string[] m_ContentNames;
        private readonly string m_CollectionName;
        private readonly Dictionary<string, string> m_ExpectedContent;

        public DescriptorContentServiceTest()
        {
            m_ContentService = new DescriptorContentService();
            m_ContentService.ConfigurationService = new DescriptorsConfigurationStub();

            // These are the content values written in the content assets to be loaded
            m_ContentNames = new string[] { "ContentDescriptor1", "ContentDescriptor2", "ContentDescriptor3" };
            m_CollectionName = "CollectionDescriptor";
            m_ExpectedContent = new Dictionary<string, string>()
            {
                { "ContentDescriptor1", "My First Content" },
                { "ContentDescriptor2", "My Second Content" },
                { "ContentDescriptor3", "My Third Content" }
            };
        }

        [SetUp]
        public void Initialize()
        {
            m_ContentService.BaseInitialize();
        }

        [TearDown]
        public void Cleanup()
        {
            m_ContentService.BaseUnload();
        }

        [Test]
        public void GetContentDescriptorTest()
        {
            // If the descriptor name is invalid -> return null
            Assert.IsNull(m_ContentService.GetContentDescriptor<TestDescriptor>("Unknown"));

            // If the given type is incorrect -> return null
            Assert.IsNull(m_ContentService.GetContentDescriptor<OtherDescriptor>(m_ContentNames[0]));

            // If the request is valid -> correct descriptor is returned and cached
            TestDescriptor first = m_ContentService.GetContentDescriptor<TestDescriptor>(m_ContentNames[0]);
            Assert.IsNotNull(first);
            Assert.AreEqual(m_ContentNames[0], first.ContentId);
            Assert.AreEqual(m_ExpectedContent[m_ContentNames[0]], first.ContentValue);

            // If requested again -> the same content object is returned
            TestDescriptor sameFirst = m_ContentService.GetContentDescriptor<TestDescriptor>(m_ContentNames[0]);
            Assert.AreEqual(first, sameFirst);

            // If requested again but with incorrect type -> return null
            Assert.IsNull(m_ContentService.GetContentDescriptor<OtherDescriptor>(m_ContentNames[0]));
        }

        [Test]
        public void GetDescriptorCollectionTest()
        {
            // If the collection name is invalid -> return null
            Assert.IsNull(m_ContentService.GetDescriptorCollection<TestDescriptor>("UnknownCollection"));

            // If the request is valid -> correct data is returned and cached
            List<TestDescriptor> collection = m_ContentService.GetDescriptorCollection<TestDescriptor>(m_CollectionName).ToList();
            Assert.IsNotNull(collection);
            Assert.AreEqual(m_ExpectedContent.Count, collection.Count);
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.AreEqual(m_ContentNames[i], collection[i].ContentId);
                Assert.AreEqual(m_ExpectedContent[m_ContentNames[i]], collection[i].ContentValue);
            }

            // If requested again -> the same content objects are returned
            List<TestDescriptor> sameCollection = m_ContentService.GetDescriptorCollection<TestDescriptor>(m_CollectionName).ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.AreEqual(collection[i], sameCollection[i]);
            }
        }

        [Test]
        public void ResetLoadedDescriptorsTest()
        {
            // Request collection -> descriptors are loaded and cached into memory
            List<TestDescriptor> cachedCollection = m_ContentService.GetDescriptorCollection<TestDescriptor>(m_CollectionName).ToList();
            Assert.AreEqual(cachedCollection.Count, m_ContentService.DescriptorCacheSize);

            // Request descriptor content -> return cached object
            TestDescriptor second = m_ContentService.GetContentDescriptor<TestDescriptor>(m_ContentNames[1]);
            Assert.AreEqual(cachedCollection[1], second);
            Assert.AreEqual(cachedCollection.Count, m_ContentService.DescriptorCacheSize);

            // Reset loaded descriptors
            m_ContentService.ResetLoadedDescriptors();
            Assert.AreEqual(0, m_ContentService.DescriptorCacheSize);

            // Request same descriptor -> descriptor is loaded again
            m_ContentService.GetContentDescriptor<TestDescriptor>(m_ContentNames[1]); ;
            Assert.AreEqual(1, m_ContentService.DescriptorCacheSize);
        }
    }

    #region Dependencies
    public class DescriptorsConfigurationStub : IConfigurationService
    {
        private UnityContentConfiguration m_Configuration;

        public DescriptorsConfigurationStub()
        {
            m_Configuration = new UnityContentConfiguration()
            {
                EnableContentData = false,
                EnableContentAssets = false,

                EnableContentDescriptors = true,
                DescriptorContentPath = "Assets/GameEngine.PMR.Unity/Tests/Runtime/Bundles/",
                DescriptorBundleName = "descriptors"
            };
        }

        public TConfig GetConfiguration<TConfig>(string configId) where TConfig : class
        {
            if (configId == UnityContentConfiguration.CONFIG_ID)
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
}
