using GameEngine.Core.Logger;
using GameEngine.Core.Unity.System;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// A service that centralizes and regulates access to the game content descriptors that can be loaded from scriptable object assets
    /// </summary>
    [RuleAccess(typeof(IDescriptorContentService))]
    public class DescriptorContentService : GameRule, IDescriptorContentService
    {
        private const string TAG = "DescriptorContentService";

        /// <summary>
        /// Dependency to IConfigurationService
        /// </summary>
        [RuleDependency(RuleDependencySource.Service, Required = true)]
        public IConfigurationService ConfigurationService;

        private string m_DescriptorsPath;
        private string m_DescriptorsBundleName;

        private AssetBundle m_DescriptorsBundle;
        private Dictionary<string, ContentDescriptor> m_LoadedDescriptors;
        private Dictionary<string, IEnumerable<string>> m_DescriptorsCollections;

        /// <summary>
        /// The number of content descriptors loaded into memory
        /// </summary>
        public int DescriptorCacheSize => m_LoadedDescriptors.Count;

        /// <summary>
        /// Create an instance of DescriptorContentService
        /// </summary>
        public DescriptorContentService()
        {
            m_LoadedDescriptors = new Dictionary<string, ContentDescriptor>();
            m_DescriptorsCollections = new Dictionary<string, IEnumerable<string>>();
        }

        #region GameRule Cycle
        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize()
        {
            UnityContentConfiguration config = ConfigurationService
                .GetConfiguration<UnityContentConfiguration>(UnityContentConfiguration.CONFIG_ID);

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
            ResetLoadedDescriptors();
            m_DescriptorsBundle.Unload(true);
            MarkUnloaded();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update() { }
        #endregion

        #region IDescriptorContentService API
        /// <summary>
        /// Retrieve and cache the content descriptor corresponding to the given name
        /// </summary>
        /// <typeparam name="TDescriptor">The type of the content descriptor</typeparam>
        /// <param name="name">The name that identifies the descriptor</param>
        /// <returns>A scriptable object of type TDescriptor</returns>
        public TDescriptor GetContentDescriptor<TDescriptor>(string name) where TDescriptor : ContentDescriptor
        {
            if (m_LoadedDescriptors.ContainsKey(name))
            {
                if (m_LoadedDescriptors[name] is TDescriptor)
                {
                    return (TDescriptor)m_LoadedDescriptors[name];
                }
                else
                {
                    Log.Error(TAG, $"Invalid content type: The '{name}' descriptor does not match type {typeof(TDescriptor)}");
                    return null;
                }
            }

            Log.Debug(TAG, $"Loading content descriptor '{name}'");
            TDescriptor content = LoadDescriptor<TDescriptor>(name);
            if (content != null)
            {
                m_LoadedDescriptors.Add(name, content);
                return content;
            }

            return null;
        }

        /// <summary>
        /// Retrieve and cache all the content descriptors listed in the given collection
        /// </summary>
        /// <typeparam name="TDescriptor">The common type of the listed content descriptors</typeparam>
        /// <param name="collectionName">The name that identifies the collection</param>
        /// <returns>A collection of TDescriptor scriptable objects</returns>
        public IEnumerable<TDescriptor> GetDescriptorCollection<TDescriptor>(string collectionName) where TDescriptor : ContentDescriptor
        {
            if (m_DescriptorsCollections.ContainsKey(collectionName))
            {
                return m_DescriptorsCollections[collectionName]
                    .Select((descriptorName) => GetContentDescriptor<TDescriptor>(descriptorName));
            }

            Log.Debug(TAG, $"Loading content descriptor collection '{collectionName}'");
            List<string> collection = LoadCollection(collectionName);
            if (collection != null)
            {
                m_DescriptorsCollections.Add(collectionName, collection);
                return collection.Select((descriptorName) => GetContentDescriptor<TDescriptor>(descriptorName));
            }

            return null;
        }

        /// <summary>
        /// Clear the cache of content descriptors loaded into memory
        /// </summary>
        public void ResetLoadedDescriptors()
        {
            Log.Debug(TAG, $"Clearing all content descriptors");
            m_LoadedDescriptors.Clear();
            m_DescriptorsCollections.Clear();
        }
        #endregion

        #region private
        private bool SetupFromConfig(UnityContentConfiguration configuration)
        {
            if (!configuration.EnableContentDescriptors)
            {
                Log.Error(TAG, "Impossible to run service: configuration is disabled for ContentDescriptors");
                return false;
            }

            m_DescriptorsPath = configuration.DescriptorContentPath;
            m_DescriptorsBundleName = configuration.DescriptorBundleName;
            m_DescriptorsBundle = AssetBundle.LoadFromFile(PathUtils.Join(m_DescriptorsPath, m_DescriptorsBundleName));
            return m_DescriptorsBundle != null;
        }

        private TDescriptor LoadDescriptor<TDescriptor>(string descriptorName) where TDescriptor : ContentDescriptor
        {
            TDescriptor descriptor = m_DescriptorsBundle.LoadAsset<TDescriptor>(descriptorName);
            if (descriptor != null)
            {
                descriptor.ContentId = descriptor.name;
                if (descriptor.name != descriptorName)
                {
                    Log.Warning(TAG, $"The descriptor was requested and referenced by a name different from its real name.\n" +
                        $"(Ref = {descriptorName}, Real = {descriptor.name}. This could cause cached duplicates.");
                }
                return descriptor;
            }
            else
            {
                Log.Error(TAG, $"Invalid descriptor: Unable to load descriptor {descriptorName} from descriptor bundle");
                return null;
            }
        }

        private List<string> LoadCollection(string collectionName)
        {
            CollectionDescriptor collection = m_DescriptorsBundle.LoadAsset<CollectionDescriptor>(collectionName);
            if (collection == null)
            {
                Log.Error(TAG, $"Invalid collection: Unable to load collection {collectionName} from descriptor bundle");
                return null;
            }
            if (collection.name != collectionName)
            {
                Log.Warning(TAG, $"The collection was requested and referenced by a name different from its real name.\n" +
                    $"(Ref = {collectionName}, Real = {collection.name}. This could cause cached duplicates.");
            }
            return collection.Collection;
        }
        #endregion
    }
}
