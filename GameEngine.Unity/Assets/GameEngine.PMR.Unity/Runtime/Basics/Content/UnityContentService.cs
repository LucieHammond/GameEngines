using GameEngine.Core.Logger;
using GameEngine.Core.Unity.System;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Basics.Content;
using GameEngine.PMR.Rules.Dependencies;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// A service that centralizes and regulates access to the game content, especially Unity assets that can be loaded from asset bundles
    /// </summary>
    [RuleAccess(typeof(IUnityContentService))]
    public class UnityContentService : ContentService, IUnityContentService
    {
        private const string TAG = "ContentService";

        private string m_DescriptorsPath;
        private string m_DescriptorsBundleName;

        private AssetBundle m_DescriptorsBundle;
        private Dictionary<string, ContentDescriptor> m_LoadedDescriptors;
        private Dictionary<string, IEnumerable<string>> m_DescriptorsCollections;

        private string m_AssetBundlesPath;
        private AssetBundleManifest m_AssetBundlesManifest;

        private Dictionary<string, AssetBundle> m_LoadedBundles;
        private List<string> m_ActiveAssetBundles;
        private Dictionary<string, Object> m_LoadedAssets;

        /// <summary>
        /// Create an instance of UnityContentService
        /// </summary>
        public UnityContentService()
        {
            m_LoadedBundles = new Dictionary<string, AssetBundle>();
            m_ActiveAssetBundles = new List<string>();
            m_LoadedDescriptors = new Dictionary<string, ContentDescriptor>();
            m_DescriptorsCollections = new Dictionary<string, IEnumerable<string>>();
            m_LoadedAssets = new Dictionary<string, Object>();
        }

        /// <summary>
        /// Setup the UnityContentService with the given configuration
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        protected bool SetupFromConfig(UnityContentConfiguration configuration)
        {
            if (!base.SetupFromConfig(configuration))
                return false;

            if (!configuration.EnableContentDescriptors)
            {
                Log.Error(TAG, "Impossible to run service: configuration is disabled for ContentDescriptors");
                return false;
            }

            m_DescriptorsPath = configuration.DescriptorContentPath;
            m_DescriptorsBundleName = configuration.DescriptorBundleName;
            m_DescriptorsBundle = AssetBundle.LoadFromFile(PathUtils.Join(m_DescriptorsPath, m_DescriptorsBundleName));

            if (!configuration.EnableContentAssets)
            {
                Log.Error(TAG, "Impossible to run service: configuration is disabled for ContentAssets");
                return false;
            }

            m_AssetBundlesPath = configuration.AssetContentPath;
            string mainBundleName = PathUtils.GetFolders(PathUtils.Normalize(m_AssetBundlesPath, false)).Last();
            AssetBundle mainBundle = AssetBundle.LoadFromFile(PathUtils.Join(m_AssetBundlesPath, mainBundleName));
            m_AssetBundlesManifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            return m_DescriptorsBundle != null && m_AssetBundlesManifest != null;
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
            ResetLoadedContent();
            ResetLoadedDescriptors();
            ResetLoadedAssets();

            MarkUnloaded();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update() { }
        #endregion

        #region IUnityContentService API
        /// <summary>
        /// Load, prepare and activate an asset bundle in order to make all its assets accessible afterwards
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle</param>
        /// <param name="loadAllAssets">Whether or not to load and cache all assets immediatly</param>
        /// <param name="checkDependencies">Whether or not to check and manage seamlessly the bundle dependencies</param>
        public void ActivateAssetBundle(string bundleName, bool loadAllAssets, bool checkDependencies)
        {
            if (m_ActiveAssetBundles.Contains(bundleName))
            {
                Log.Warning(TAG, $"Redundant operation: Asset bundle {bundleName} is already active");
                return;
            }

            Log.Debug(TAG, $"Activate asset bundle {bundleName} (synchronous load)");
            if (checkDependencies)
            {
                string[] dependencies = m_AssetBundlesManifest.GetAllDependencies(bundleName);
                foreach (string requiredBundle in dependencies)
                {
                    if (!m_LoadedBundles.ContainsKey(requiredBundle))
                    {
                        Log.Warning(TAG, $"Resolving missing dependency: Loading '{requiredBundle}' required for '{bundleName}'");
                        LoadAssetBundle(requiredBundle);
                    }
                }
            }

            if (!m_LoadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                bundle = LoadAssetBundle(bundleName);
            }

            if (loadAllAssets && bundle != null)
            {
                Object[] bundleAssets = bundle.LoadAllAssets();
                foreach (Object asset in bundleAssets)
                {
                    m_LoadedAssets.Add(asset.name, asset);
                }
            }

            m_ActiveAssetBundles.Add(bundleName);
        }

        /// <summary>
        /// Load, prepare and activate an asset bundle in order to make all its assets accessible afterwards (asynchronous)
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle</param>
        /// <param name="loadAllAssets">Whether or not to load and cache all assets immediatly</param>
        /// <param name="checkDependencies">Whether or not to check and manage seamlessly the bundle dependencies</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ActivateAssetBundleAsync(string bundleName, bool loadAllAssets, bool checkDependencies)
        {
            if (m_ActiveAssetBundles.Contains(bundleName))
            {
                Log.Warning(TAG, $"Redundant operation: Asset bundle {bundleName} is already active");
                return;
            }

            Log.Debug(TAG, $"Activate asset bundle {bundleName} (asynchronous load)");
            if (checkDependencies)
            {
                string[] dependencies = m_AssetBundlesManifest.GetAllDependencies(bundleName);
                foreach (string requiredBundle in dependencies)
                {
                    if (!m_LoadedBundles.ContainsKey(requiredBundle))
                    {
                        Log.Warning(TAG, $"Resolving missing dependency: Loading '{requiredBundle}' required for '{bundleName}'");
                        await LoadAssetBundleAsync(requiredBundle);
                    }
                }
            }

            if (!m_LoadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                bundle = await LoadAssetBundleAsync(bundleName);
            }

            if (loadAllAssets && bundle != null)
            {
                TaskCompletionSource<Object[]> taskSource = new TaskCompletionSource<Object[]>();
                AssetBundleRequest request = bundle.LoadAllAssetsAsync();
                request.completed += (operation) => taskSource.SetResult(request.allAssets);

                Object[] bundleAssets = await taskSource.Task;
                foreach (Object asset in bundleAssets)
                {
                    m_LoadedAssets.Add(asset.name, asset);
                }
            }

            m_ActiveAssetBundles.Add(bundleName);
        }

        /// <summary>
        /// Unload and deactivate an asset bundle when its assets are no longer needed
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle</param>
        /// <param name="unloadAllAssets"></param>
        public void DeactivateAssetBundle(string bundleName, bool unloadAllAssets)
        {
            if (!m_ActiveAssetBundles.Contains(bundleName))
            {
                Log.Warning(TAG, $"Unnecessary operation: Asset bundle {bundleName} is already not active");
                return;
            }

            Log.Debug(TAG, $"Deactivate asset bundle {bundleName}");
            AssetBundle bundle = m_LoadedBundles[bundleName];

            IEnumerable<string> bundleAssets = bundle.GetAllAssetNames().Select(GetStandardAssetName);
            IEnumerable<string> assetsToRemove = m_LoadedAssets.Keys.Where((asset) => bundleAssets.Contains(asset.ToLowerInvariant()));
            foreach (string asset in assetsToRemove.ToList())
            {
                m_LoadedAssets.Remove(asset);
            }

            bundle.Unload(unloadAllAssets);
            m_LoadedBundles.Remove(bundleName);
            m_ActiveAssetBundles.Remove(bundleName);
        }

        /// <summary>
        /// Preload and cache a list of assets from a specific asset bundle
        /// </summary>
        /// <param name="assetNames">The list of assets names</param>
        /// <param name="bundleName">The name of the bundle containing those assets</param>
        /// <param name="withSubAssets">Whether or not to cache the subassets of the assets individually</param>
        public void PreloadAssets(List<string> assetNames, string bundleName, bool withSubAssets)
        {
            if (!m_ActiveAssetBundles.Contains(bundleName))
            {
                Log.Warning(TAG, $"Invalid operation: Asset bundle {bundleName} is not currently available for preloading assets");
                return;
            }

            Log.Debug(TAG, $"Preload assets from bundle {bundleName} ({assetNames.Count})");
            AssetBundle bundle = m_LoadedBundles[bundleName];
            if (!withSubAssets)
            {
                foreach (string assetName in assetNames)
                {
                    Object asset = bundle.LoadAsset(assetName);
                    m_LoadedAssets[asset.name] = asset;
                }
            }
            else
            {
                foreach (string assetName in assetNames)
                {
                    Object[] assets = bundle.LoadAssetWithSubAssets(assetName);
                    foreach (Object asset in assets)
                        m_LoadedAssets[asset.name] = asset;
                }
            }
        }

        /// <summary>
        /// Unload all bundles and assets that are not used nor referenced elsewhere in code
        /// </summary>
        public void CleanUnusedAssets()
        {
            Log.Debug(TAG, $"Clean unused assets and bundles");
            IEnumerable<string> inactiveBundles = m_LoadedBundles.Keys.Except(m_ActiveAssetBundles);
            if (inactiveBundles.Count() > 0)
            {
                IEnumerable<string> requiredBundles = new List<string>();
                foreach (string activeBundle in m_ActiveAssetBundles)
                {
                    requiredBundles = requiredBundles.Union(m_AssetBundlesManifest.GetAllDependencies(activeBundle));
                }

                IEnumerable<string> unusedBundles = inactiveBundles.Except(requiredBundles);
                foreach (string bundleName in unusedBundles.ToList())
                {
                    m_LoadedBundles[bundleName].Unload(true);
                    m_LoadedBundles.Remove(bundleName);
                }
            }

            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Retrieve and cache the content descriptor corresponding to the given name
        /// </summary>
        /// <typeparam name="TDescriptor">The type of the content descriptor</typeparam>
        /// <param name="name">The name that identifies the descriptor</param>
        /// <returns>A scriptable object of type TDescriptor</returns>
        public TDescriptor GetContentDescriptor<TDescriptor>(string name) where TDescriptor : ContentDescriptor
        {
            return GetContent("descriptor", name, ref m_LoadedDescriptors,
                (descriptorName) => LoadDescriptor<TDescriptor>(descriptorName));
        }

        /// <summary>
        /// Retrieve and cache all the content descriptors listed in the given collection
        /// </summary>
        /// <typeparam name="TDescriptor">The common type of the listed content descriptors</typeparam>
        /// <param name="collectionName">The name that identifies the collection</param>
        /// <returns>A collection of TDescriptor scriptable objects</returns>
        public IEnumerable<TDescriptor> GetDescriptorCollection<TDescriptor>(string collectionName) where TDescriptor : ContentDescriptor
        {
            return GetContentCollection("descriptor", collectionName, ref m_LoadedDescriptors, ref m_DescriptorsCollections,
                (descriptorName) => LoadDescriptor<TDescriptor>(descriptorName),
                (collection) => LoadCollection(collection));
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

        /// <summary>
        /// Retrieve and cache the content asset corresponding to the given name
        /// </summary>
        /// <typeparam name="TAsset">The type of the content asset</typeparam>
        /// <param name="name">The name that identifies the asset</param>
        /// <returns>An asset of type TAsset</returns>
        public TAsset GetContentAsset<TAsset>(string name) where TAsset : Object
        {
            return GetContent("asset", name, ref m_LoadedAssets,
                (assetName) => LoadAsset<TAsset>(assetName));
        }

        /// <summary>
        /// Clear the cache of content assets loaded into memory
        /// </summary>
        public void ResetLoadedAssets()
        {
            Log.Debug(TAG, $"Clearing all content assets");
            m_LoadedAssets.Clear();
        }
        #endregion

        #region private
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

        private AssetBundle LoadAssetBundle(string bundleName)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(PathUtils.Join(m_AssetBundlesPath, bundleName));
            if (bundle != null)
                m_LoadedBundles.Add(bundleName, bundle);
            else
                Log.Error(TAG, $"Loading error: Fail to load the '{bundleName}' bundle from the bundle folder");

            return bundle;
        }

        private async Task<AssetBundle> LoadAssetBundleAsync(string bundleName)
        {
            TaskCompletionSource<AssetBundle> taskSource = new TaskCompletionSource<AssetBundle>();
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(PathUtils.Join(m_AssetBundlesPath, bundleName));
            request.completed += (operation) => taskSource.SetResult(request.assetBundle);

            AssetBundle bundle = await taskSource.Task;
            if (bundle != null)
                m_LoadedBundles.Add(bundleName, bundle);
            else
                Log.Error(TAG, $"Loading error: Fail to load the '{bundleName}' bundle from the bundle folder");

            return bundle;
        }

        private TAsset LoadAsset<TAsset>(string assetName) where TAsset : Object
        {
            foreach (string bundleName in m_ActiveAssetBundles)
            {
                if (m_LoadedBundles[bundleName].GetAllAssetNames()
                    .Any((assetPath) => GetStandardAssetName(assetPath) == assetName.ToLowerInvariant()))
                {
                    TAsset asset = m_LoadedBundles[bundleName].LoadAsset<TAsset>(assetName);
                    if (asset == null)
                    {
                        Log.Error(TAG, $"Loading asset error: Failed to load asset '{assetName}' from its bundle");
                        return null;
                    }
                    if (asset.name != assetName)
                    {
                        Log.Warning(TAG, $"The asset was requested and referenced by a name different from its real name.\n" +
                            $"(Ref = {assetName}, Real = {asset.name}. This could cause cached duplicates.");
                    }
                    return asset;
                }
            }

            Log.Error(TAG, $"Asset not found: Cannot find asset '{assetName}' in any active asset bundle");
            return null;
        }

        private string GetStandardAssetName(string assetPath)
        {
            return PathUtils.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        }
        #endregion
    }
}
