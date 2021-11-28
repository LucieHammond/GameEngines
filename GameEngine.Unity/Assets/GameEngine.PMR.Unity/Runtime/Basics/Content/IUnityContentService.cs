using GameEngine.Core.Unity.System;
using GameEngine.PMR.Basics.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// Interface that exposes the operations provided by the UnityContentService
    /// </summary>
    public interface IUnityContentService : IContentService
    {
        /// <summary>
        /// Load, prepare and activate an asset bundle in order to make all its assets accessible afterwards
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle</param>
        /// <param name="loadAllAssets">Whether or not to load and cache all assets immediatly</param>
        /// <param name="checkDependencies">Whether or not to check and manage seamlessly the bundle dependencies</param>
        void ActivateAssetBundle(string bundleName, bool loadAllAssets, bool checkDependencies);

        /// <summary>
        /// Load, prepare and activate an asset bundle in order to make all its assets accessible afterwards (asynchronous)
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle</param>
        /// <param name="loadAllAssets">Whether or not to load and cache all assets immediatly</param>
        /// <param name="checkDependencies">Whether or not to check and manage seamlessly the bundle dependencies</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ActivateAssetBundleAsync(string bundleName, bool loadAllAssets, bool checkDependencies);

        /// <summary>
        /// Unload and deactivate an asset bundle when its assets are no longer needed
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle</param>
        /// <param name="unloadAllAssets"></param>
        void DeactivateAssetBundle(string bundleName, bool unloadAllAssets);

        /// <summary>
        /// Preload and cache a list of assets from a specific asset bundle
        /// </summary>
        /// <param name="assetNames">The list of assets names</param>
        /// <param name="bundleName">The name of the bundle containing those assets</param>
        /// <param name="withSubAssets">Whether or not to cache the subassets of the assets individually</param>
        void PreloadAssets(List<string> assetNames, string bundleName, bool withSubAssets);

        /// <summary>
        /// Unload all bundles and assets that are not used nor referenced elsewhere in code
        /// </summary>
        void CleanUnusedAssets();


        /// <summary>
        /// Retrieve and cache the content descriptor corresponding to the given name
        /// </summary>
        /// <typeparam name="TDescriptor">The type of the content descriptor</typeparam>
        /// <param name="name">The name that identifies the descriptor</param>
        /// <returns>A scriptable object of type TDescriptor</returns>
        TDescriptor GetContentDescriptor<TDescriptor>(string name) where TDescriptor : ContentDescriptor;

        /// <summary>
        /// Retrieve and cache all the content descriptors listed in the given collection
        /// </summary>
        /// <typeparam name="TDescriptor">The common type of the listed content descriptors</typeparam>
        /// <param name="collectionName">The name that identifies the collection</param>
        /// <returns>A collection of TDescriptor scriptable objects</returns>
        IEnumerable<TDescriptor> GetDescriptorCollection<TDescriptor>(string collectionName) where TDescriptor : ContentDescriptor;

        /// <summary>
        /// Clear the cache of content descriptors loaded into memory
        /// </summary>
        void ResetLoadedDescriptors();


        /// <summary>
        /// Retrieve and cache the content asset corresponding to the given name
        /// </summary>
        /// <typeparam name="TAsset">The type of the content asset</typeparam>
        /// <param name="name">The name that identifies the asset</param>
        /// <returns>An asset of type TAsset</returns>
        TAsset GetContentAsset<TAsset>(string name) where TAsset : Object;

        /// <summary>
        /// Clear the cache of content assets loaded into memory
        /// </summary>
        void ResetLoadedAssets();
    }
}
