using GameEngine.Core.Logger;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// A service that centralizes and regulates access to the game content assets that can be loaded from asset bundles
    /// </summary>
    [RuleAccess(typeof(IAssetContentService))]
    public class AssetContentService : GameRule, IAssetContentService
    {
        private const string TAG = "AssetContentService";

        /// <summary>
        /// Dependency to IConfigurationService
        /// </summary>
        [RuleDependency(RuleDependencySource.Service, Required = true)]
        public IConfigurationService ConfigurationService;

        private string m_AssetBundlesPath;
        private AssetBundleManifest m_AssetBundlesManifest;

        private Dictionary<string, AssetBundle> m_LoadedBundles;
        private List<string> m_ActiveAssetBundles;
        private Dictionary<string, Object> m_LoadedAssets;

        /// <summary>
        /// The number of content assets loaded into memory
        /// </summary>
        public int AssetCacheSize => m_LoadedAssets.Count;

        /// <summary>
        /// Create an instance of AssetContentService
        /// </summary>
        public AssetContentService()
        {
            m_LoadedBundles = new Dictionary<string, AssetBundle>();
            m_ActiveAssetBundles = new List<string>();
            m_LoadedAssets = new Dictionary<string, Object>();
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
            ResetLoadedAssets();
            m_LoadedBundles.Clear();
            m_ActiveAssetBundles.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
            MarkUnloaded();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update() { }
        #endregion

        #region IAssetContentService API
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
                    foreach(Object asset in assets)
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
        /// Retrieve and cache the content asset corresponding to the given name
        /// </summary>
        /// <typeparam name="TAsset">The type of the content asset</typeparam>
        /// <param name="name">The name that identifies the asset</param>
        /// <returns>An asset of type TAsset</returns>
        public TAsset GetContentAsset<TAsset>(string name) where TAsset : Object
        {
            if (m_LoadedAssets.ContainsKey(name))
            {
                if (m_LoadedAssets[name] is TAsset)
                {
                    return (TAsset)m_LoadedAssets[name];
                }
                else
                {
                    Log.Error(TAG, $"Invalid content type: The '{name}' asset does not match type {typeof(TAsset)}");
                    return null;
                }
            }

            Log.Debug(TAG, $"Loading content asset '{name}'");
            TAsset content = LoadAsset<TAsset>(name);
            if (content != null)
            {
                m_LoadedAssets.Add(name, content);
                return content;
            }

            return null;
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
        private bool SetupFromConfig(UnityContentConfiguration configuration)
        {
            if (!configuration.EnableContentAssets)
            {
                Log.Error(TAG, "Impossible to run service: configuration is disabled for ContentAssets");
                return false;
            }

            m_AssetBundlesPath = configuration.AssetContentPath;
            string mainBundleName = PathUtils.GetFolders(PathUtils.Normalize(m_AssetBundlesPath, false)).Last();
            AssetBundle mainBundle = AssetBundle.LoadFromFile(PathUtils.Join(m_AssetBundlesPath, mainBundleName));
            m_AssetBundlesManifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            return m_AssetBundlesManifest != null;
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
