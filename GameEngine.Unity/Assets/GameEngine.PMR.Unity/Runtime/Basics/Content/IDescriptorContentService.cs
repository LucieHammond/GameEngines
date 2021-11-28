using GameEngine.Core.Unity.System;
using System.Collections.Generic;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// Interface that exposes the operations provided by the DescriptorContentService
    /// </summary>
    public interface IDescriptorContentService
    {
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
    }
}
