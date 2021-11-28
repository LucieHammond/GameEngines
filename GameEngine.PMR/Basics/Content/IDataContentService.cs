using GameEngine.Core.System;
using System.Collections.Generic;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// Interface that exposes the operations provided by the ContentService
    /// </summary>
    public interface IDataContentService
    {
        /// <summary>
        /// Retrieve and cache the content data corresponding to the given name
        /// </summary>
        /// <typeparam name="TData">The type of the content data</typeparam>
        /// <param name="name">The name that identifies the data</param>
        /// <returns>An object of type TData</returns>
        TData GetContentData<TData>(string name) where TData : ContentData;

        /// <summary>
        /// Retrieve and cache all the content data listed in the given collection
        /// </summary>
        /// <typeparam name="TData">The common type of the listed content data</typeparam>
        /// <param name="collectionName">The name that identifies the collection</param>
        /// <returns>A collection of TData objects</returns>
        IEnumerable<TData> GetDataCollection<TData>(string collectionName) where TData : ContentData;

        /// <summary>
        /// Clear the cache of content data loaded into memory
        /// </summary>
        void ResetLoadedContent();
    }
}
