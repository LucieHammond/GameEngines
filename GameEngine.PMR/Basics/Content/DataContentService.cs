using GameEngine.Core.Logger;
using GameEngine.Core.Serialization.Text;
using GameEngine.Core.System;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// A service that centralizes and regulates access to the game content data that can be loaded from files
    /// </summary>
    [RuleAccess(typeof(IDataContentService))]
    public class DataContentService : GameRule, IDataContentService
    {
        private const string TAG = "DataContentService";
        private const string MANIFEST_FILE = "manifest.json";

        /// <summary>
        /// Dependency to IConfigurationService
        /// </summary>
        [RuleDependency(RuleDependencySource.Service, Required = true)]
        public IConfigurationService ConfigurationService;

        private string m_ContentPath;
        private ContentManifest m_ContentManifest;
        private ObjectSerializer m_Serializer;

        private Dictionary<string, ContentData> m_LoadedData;
        private Dictionary<string, IEnumerable<string>> m_DataCollections;

        /// <summary>
        /// The number of content data loaded into memory
        /// </summary>
        public int DataCacheSize => m_LoadedData.Count;

        /// <summary>
        /// Create an instance of DataContentService
        /// </summary>
        public DataContentService()
        {
            m_LoadedData = new Dictionary<string, ContentData>();
            m_DataCollections = new Dictionary<string, IEnumerable<string>>();
        }

        #region GameRule cycle
        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize()
        {
            ContentConfiguration config = ConfigurationService.GetConfiguration<ContentConfiguration>(ContentConfiguration.CONFIG_ID);

            if (SetupFromConfig(config))
                MarkInitialized();
            else
                MarkError();
        }

        /// <summary>
        /// Unload
        /// </summary>
        protected override void Unload()
        {
            ResetLoadedContent();

            MarkUnloaded();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update() { }
        #endregion

        #region IDataContentService API
        /// <summary>
        /// Retrieve and cache the content data corresponding to the given identifier
        /// </summary>
        /// <typeparam name="TData">The type of the content</typeparam>
        /// <param name="name">The name that identifies the content</param>
        /// <returns>An object of type TData</returns>
        public TData GetContentData<TData>(string name) where TData : ContentData
        {
            if (m_LoadedData.ContainsKey(name))
            {
                if (m_LoadedData[name] is TData)
                {
                    return (TData)m_LoadedData[name];
                }
                else
                {
                    Log.Error(TAG, $"Invalid content type: The '{name}' data does not match type {typeof(TData)}");
                    return null;
                }
            }

            Log.Debug(TAG, $"Loading content data '{name}'");
            TData content = LoadContentData<TData>(name);
            if (content != null)
            {
                m_LoadedData.Add(name, content);
                return content;
            }

            return null;
        }

        /// <summary>
        /// Retrieve and cache all the content data listed in the given collection
        /// </summary>
        /// <typeparam name="TData">The common type of the content</typeparam>
        /// <param name="collectionName">The name that identifies the collection</param>
        /// <returns>A collection of TData objects</returns>
        public IEnumerable<TData> GetDataCollection<TData>(string collectionName) where TData : ContentData
        {
            if (m_DataCollections.ContainsKey(collectionName))
            {
                return m_DataCollections[collectionName].Select((dataName) => GetContentData<TData>(dataName));
            }

            Log.Debug(TAG, $"Loading content data collection '{collectionName}'");
            List<string> collection = LoadCollectionData(collectionName);
            if (collection != null)
            {
                m_DataCollections.Add(collectionName, collection);
                return collection.Select((dataName) => GetContentData<TData>(dataName));
            }

            return null;
        }

        /// <summary>
        /// Clear the cache of content data loaded into memory
        /// </summary>
        public void ResetLoadedContent()
        {
            Log.Debug(TAG, $"Clearing all content data");
            m_LoadedData.Clear();
            m_DataCollections.Clear();
        }
        #endregion

        #region private
        private bool SetupFromConfig(ContentConfiguration configuration)
        {
            if (!configuration.EnableContentData)
            {
                Log.Error(TAG, "Impossible to run service: configuration is disabled for ContentData");
                return false;
            }

            m_ContentPath = configuration.DataContentPath;
            m_Serializer = ObjectSerializer.CreateSerializer(
                configuration.FileContentFormat,
                EncodingUtils.CreateEncoding(configuration.FileEncodingType));
            m_ContentManifest = LoadFileData<ContentManifest>(MANIFEST_FILE);
            return m_ContentManifest != null;
        }

        private TData LoadContentData<TData>(string contentName) where TData : ContentData
        {
            if (m_ContentManifest.FileNames.ContainsKey(contentName))
            {
                TData data = LoadFileData<TData>(m_ContentManifest.FileNames[contentName]);
                if (data == null)
                    return null;

                data.ContentId = contentName;
                return data;
            }
            else
            {
                Log.Error(TAG, $"Invalid content name: Cannot find content {contentName} in the content manifest");
                return null;
            }
        }

        private List<string> LoadCollectionData(string collectionName)
        {
            if (m_ContentManifest.FileNames.ContainsKey(collectionName))
            {
                return LoadFileData<List<string>>(m_ContentManifest.FileNames[collectionName]);
            }
            else
            {
                Log.Error(TAG, $"Invalid collection name: Cannot find collection {collectionName} in the content manifest");
                return null;
            }
        }

        private T LoadFileData<T>(string fileName) where T : class
        {
            byte[] fileData = ReadFileData(fileName);
            if (fileData == null)
                return null;

            if (fileData.Length > 0)
            {
                try
                {
                    return m_Serializer.DeserializeFromBytes<T>(fileData);
                }
                catch (Exception exception)
                {
                    Log.Error(TAG, $"Format error: The file {fileName} could not be deserialized correctly");
                    Log.Exception(TAG, exception);
                    return null;
                }
            }
            else
            {
                Log.Error(TAG, $"Empty content file: The file {fileName} does not contain any content");
                return null;
            }
        }

        private byte[] ReadFileData(string fileName)
        {
            string contentPath = PathUtils.Join(m_ContentPath, fileName);
            if (File.Exists(contentPath))
            {
                return File.ReadAllBytes(contentPath);
            }
            else
            {
                Log.Error(TAG, $"Missing content file: Cannot find file {fileName} in the content folder");
                return null;
            }
        }
        #endregion
    }
}
