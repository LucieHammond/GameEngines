using GameEngine.Core.Logger;
using GameEngine.Core.Serialization.Text;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// A service that centralizes and regulates access to the game content that can be loaded from files
    /// </summary>
    [RuleAccess(typeof(IContentService))]
    public class ContentService : GameRule, IContentService
    {
        /// <summary>
        /// Id for referencing content configuration
        /// </summary>
        public const string CONTENT_CONFIG_ID = "Content";
        
        private const string TAG = "ContentService";
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
        /// Create an instance of ContentService
        /// </summary>
        public ContentService()
        {
            m_LoadedData = new Dictionary<string, ContentData>();
            m_DataCollections = new Dictionary<string, IEnumerable<string>>();
        }

        /// <summary>
        /// Setup the ContentService with the given configuration
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        protected void SetupFromConfig(ContentConfiguration configuration)
        {
            m_ContentPath = configuration.FileContentPath;
            m_Serializer = ObjectSerializer.CreateSerializer(
                configuration.FileContentFormat,
                EncodingUtils.CreateEncoding(configuration.FileEncodingType));

            string manifestPath = PathUtils.Join(m_ContentPath, MANIFEST_FILE);
            m_ContentManifest = LoadFileData<ContentManifest>(manifestPath);
        }

        #region GameRule cycle
        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize()
        {
            ContentConfiguration config = ConfigurationService.GetConfiguration<ContentConfiguration>(CONTENT_CONFIG_ID);
            SetupFromConfig(config);

            MarkInitialized();
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

        #region IContentService API
        /// <summary>
        /// Retrieve and cache the content data corresponding to the given identifier
        /// </summary>
        /// <typeparam name="TData">The type of the content</typeparam>
        /// <param name="name">The name that identifies the content</param>
        /// <returns>An object of type TData</returns>
        public TData GetContentData<TData>(string name) where TData : ContentData
        {
            return GetContent("data", name, ref m_LoadedData,
                (dataName) => LoadContentData<TData>(dataName));
        }

        /// <summary>
        /// Retrieve and cache all the content data listed in the given collection
        /// </summary>
        /// <typeparam name="TData">The common type of the content</typeparam>
        /// <param name="collectionName">The name that identifies the collection</param>
        /// <returns>A collection of TData objects</returns>
        public IEnumerable<TData> GetDataCollection<TData>(string collectionName) where TData : ContentData
        {
            return GetContentCollection("data", collectionName, ref m_LoadedData, ref m_DataCollections,
                (dataName) => LoadContentData<TData>(dataName),
                (collection) => LoadContentData<List<string>>(collection));
        }

        /// <summary>
        /// Clear the cache of contents loaded into memory
        /// </summary>
        public void ResetLoadedContent()
        {
            Log.Debug(TAG, $"Clearing all content data");
            m_LoadedData.Clear();
            m_DataCollections.Clear();
        }
        #endregion

        #region Content operations
        /// <summary>
        /// Expose a general procedure for getting and caching a specific content of a certain type given a certain loading method
        /// </summary>
        /// <typeparam name="T">The type of the content to retrieve</typeparam>
        /// <typeparam name="TBase">A base type from which the content type must be derived</typeparam>
        /// <param name="contentCategory">A string describing the category of content that is handled</param>
        /// <param name="name">The name of the content to retrieve</param>
        /// <param name="loadedContents">The cache structure storing all loaded contents of this category</param>
        /// <param name="loadContent">The method to use for loading the content if not in cache</param>
        /// <returns>The content object (or null in case an error occured)</returns>
        protected T GetContent<T, TBase>(
            string contentCategory,
            string name,
            ref Dictionary<string, TBase> loadedContents, 
            Func<string, T> loadContent)

            where T : class, TBase
        {
            if (loadedContents.ContainsKey(name))
            {
                if (loadedContents[name] is T)
                {
                    return (T)loadedContents[name];
                }
                else
                {
                    Log.Error(TAG, $"Invalid content type: The '{name}' {contentCategory} does not match type {typeof(T)}");
                    return null;
                }
            }

            Log.Debug(TAG, $"Loading content {contentCategory} '{name}'");
            T content = loadContent(name);
            if (content != null)
            {
                loadedContents.Add(name, content);
                return content;
            }
            else
            {
                Log.Error(TAG, $"Loading error: Failed to load the '{name}' content {contentCategory}");
                return null;
            }
        }

        /// <summary>
        /// Expose a general procedure for getting and caching a specific collection of content of a certain type given a certain loading method
        /// </summary>
        /// <typeparam name="T">The type of the content to retrieve</typeparam>
        /// <typeparam name="TBase">A base type from which the content type must be derived</typeparam>
        /// <param name="contentCategory">A string describing the category of content that is handled</param>
        /// <param name="collectionName">The name of the content collection to retrieve</param>
        /// <param name="loadedContents">The cache structure storing all loaded contents of this category</param>
        /// <param name="contentCollection">The cache structure storing all known collections of this category</param>
        /// <param name="loadContent">The method to use for loading the content if not in cache</param>
        /// <param name="loadCollection">The method to use for loading the collection if not in cache</param>
        /// <returns>The collection of content objects (or null in case an error occured)</returns>
        protected IEnumerable<T> GetContentCollection<T, TBase>(
            string contentCategory,
            string collectionName,
            ref Dictionary<string, TBase> loadedContents,
            ref Dictionary<string, IEnumerable<string>> contentCollection,
            Func<string, T> loadContent,
            Func<string, List<string>> loadCollection)

            where T : class, TBase
        {
            List<T> contentList = new List<T>();
            if (contentCollection.ContainsKey(collectionName))
            {
                foreach (string contentName in contentCollection[collectionName])
                    contentList.Add(GetContent(contentCategory, contentName, ref loadedContents, loadContent));
                return contentList;
            }

            Log.Debug(TAG, $"Loading content {contentCategory} collection '{collectionName}'");
            List<string> collection = loadCollection(collectionName);
            if (collection != null)
            {
                contentCollection.Add(collectionName, collection);
                foreach (string contentName in collection)
                    contentList.Add(GetContent(contentCategory, contentName, ref loadedContents, loadContent));
                return contentList;
            }
            else
            {
                Log.Error(TAG, $"Loading error: Failed to load the '{collectionName}' collection of {contentCategory}");
                return null;
            }
        }
        #endregion

        #region private
        private T LoadContentData<T>(string contentName) where T : class
        {
            if (m_ContentManifest.FileNames.ContainsKey(contentName))
            {
                return LoadFileData<T>(m_ContentManifest.FileNames[contentName]);
            }
            else
            {
                Log.Error(TAG, $"Invalid content name: Cannot find content {contentName} in the content manifest");
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
                return m_Serializer.DeserializeFromBytes<T>(fileData);
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
