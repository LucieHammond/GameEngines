using GameEngine.PMR.Basics.Configuration;
using GameEngine.PMR.Unity.Basics.Content;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameEngine.PMR.UnityTests
{
    /// <summary>
    /// API tests for the <see cref="AssetContentService"/> class implementing the <see cref="IAssetContentService"/> interface
    /// </summary>
    public class AssetContentServiceTest
    {
        private AssetContentService m_ContentService;

        private struct PrefabContent { public string Name; public string Material; }
        private struct MaterialContent { public string Name; public Color Color; }
        private struct SpriteContent { public string Name; public int NbSubSprites; }

        private readonly string m_PrefabsBundle;
        private readonly string m_MaterialsBundle;
        private readonly string m_SpritesBundle;

        private readonly PrefabContent m_Prefab;
        private readonly MaterialContent[] m_Materials;
        private readonly SpriteContent m_Sprite;

        public AssetContentServiceTest()
        {
            m_ContentService = new AssetContentService();
            m_ContentService.ConfigurationService = new AssetsConfigurationStub();

            // These are the created test asset bundles
            m_PrefabsBundle = "main";
            m_MaterialsBundle = "materials";
            m_SpritesBundle = "sprites";

            // These are the asset contents that can be loaded from thoses bundles
            m_Prefab = new PrefabContent() { Name = "ContentPrefab", Material = "ContentMaterial1" };
            m_Materials = new MaterialContent[2]
            {
                new MaterialContent() { Name = "ContentMaterial1", Color = Color.blue},
                new MaterialContent() { Name = "ContentMaterial2", Color = Color.magenta},
            };
            m_Sprite = new SpriteContent() { Name = "ContentSprite", NbSubSprites = 4 };
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
        public void ActivateAssetBundleTest()
        {
            // Before bundle activation, test asset can't be loaded
            Assert.IsNull(m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name));

            // Activate asset bundle with no asset load and no dependency check
            m_ContentService.ActivateAssetBundle(m_PrefabsBundle, false, false);
            Assert.AreEqual(0, m_ContentService.AssetCacheSize);
            GameObject prefab1 = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Assert.IsTrue(IsPrefabCorrect(prefab1));
            Assert.IsNull(prefab1.GetComponent<Renderer>().sharedMaterial);
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);

            // Activate asset bundle with all assets loaded
            m_ContentService.ActivateAssetBundle(m_PrefabsBundle, true, false);
            Assert.AreEqual(1, m_ContentService.AssetCacheSize);
            GameObject prefab2 = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Assert.IsTrue(IsPrefabCorrect(prefab2));
            Assert.IsNull(prefab2.GetComponent<Renderer>().sharedMaterial);
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);

            // Activate asset bundle with dependency check
            m_ContentService.ActivateAssetBundle(m_PrefabsBundle, false, true);
            Assert.AreEqual(0, m_ContentService.AssetCacheSize);
            GameObject prefab3 = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Assert.IsTrue(IsPrefabCorrect(prefab3));
            Assert.IsNotNull(prefab3.GetComponent<Renderer>().sharedMaterial);
            Assert.AreEqual(m_Prefab.Material, prefab3.GetComponent<Renderer>().sharedMaterial.name);
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);
        }

        [UnityTest]
        public IEnumerator ActivateAssetBundleAsyncTest()
        {
            // Before bundle activation, test asset can't be loaded
            Assert.IsNull(m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name));

            // Activate asset bundle asynchronously with no asset load and no dependency check
            yield return RunAsyncTask(m_ContentService.ActivateAssetBundleAsync(m_PrefabsBundle, false, false));
            Assert.AreEqual(0, m_ContentService.AssetCacheSize);
            GameObject prefab1 = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Assert.IsTrue(IsPrefabCorrect(prefab1));
            Assert.IsNull(prefab1.GetComponent<Renderer>().sharedMaterial);
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);

            // Activate asset bundle asynchronously with all assets loaded
            yield return RunAsyncTask(m_ContentService.ActivateAssetBundleAsync(m_PrefabsBundle, true, false));
            Assert.AreEqual(1, m_ContentService.AssetCacheSize);
            GameObject prefab2 = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Assert.IsTrue(IsPrefabCorrect(prefab2));
            Assert.IsNull(prefab2.GetComponent<Renderer>().sharedMaterial);
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);

            // Activate asset bundle asynchronously with dependency check
            yield return RunAsyncTask(m_ContentService.ActivateAssetBundleAsync(m_PrefabsBundle, false, true));
            Assert.AreEqual(0, m_ContentService.AssetCacheSize);
            GameObject prefab3 = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Assert.IsTrue(IsPrefabCorrect(prefab3));
            Assert.IsNotNull(prefab3.GetComponent<Renderer>().sharedMaterial);
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);
        }

        [Test]
        public void DeactivateAssetBundleTest()
        {
            // Get asset from asset bundle
            m_ContentService.ActivateAssetBundle(m_MaterialsBundle, true, false);
            Material material1 = m_ContentService.GetContentAsset<Material>(m_Materials[0].Name);
            Assert.IsNotNull(material1);

            // Deactivate asset bundle with all assets unloaded
            m_ContentService.DeactivateAssetBundle(m_MaterialsBundle, true);
            Assert.IsTrue(material1 == null);
            Assert.IsNull(m_ContentService.GetContentAsset<Material>(m_Materials[0].Name));

            // Get asset from asset bundle
            m_ContentService.ActivateAssetBundle(m_MaterialsBundle, true, false);
            Material material2 = m_ContentService.GetContentAsset<Material>(m_Materials[1].Name);
            Assert.IsNotNull(material2);

            // Deactivate materials bundle with assets not unloaded
            m_ContentService.DeactivateAssetBundle(m_MaterialsBundle, false);
            Assert.IsFalse(material2 == null);
            Assert.IsNull(m_ContentService.GetContentAsset<Material>(m_Materials[1].Name));
        }

        [Test]
        public void PreloadAssetsTest()
        {
            m_ContentService.ActivateAssetBundle(m_SpritesBundle, false, false);
            Assert.AreEqual(0, m_ContentService.AssetCacheSize);

            // Preload all assets without subassets
            m_ContentService.PreloadAssets(new List<string> { m_Sprite.Name }, m_SpritesBundle, false);
            Assert.AreEqual(1, m_ContentService.AssetCacheSize);
            Assert.IsNotNull(m_ContentService.GetContentAsset<Texture2D>(m_Sprite.Name));
            Assert.IsNull(m_ContentService.GetContentAsset<Sprite>($"{m_Sprite.Name}_0"));

            // Preload all assets with subassets
            m_ContentService.PreloadAssets(new List<string> { m_Sprite.Name }, m_SpritesBundle, true);
            Assert.AreEqual(1 + m_Sprite.NbSubSprites, m_ContentService.AssetCacheSize);
            Assert.IsNotNull(m_ContentService.GetContentAsset<Sprite>($"{m_Sprite.Name}_0"));
        }

        [Test]
        public void CleanUnusedAssetsTest()
        {
            // Load the prefab bundle and its dependencies (material bundle)
            m_ContentService.ActivateAssetBundle(m_PrefabsBundle, true, true);

            // Get the prefab asset and the linked material asset -> both are non null
            GameObject prefab = m_ContentService.GetContentAsset<GameObject>(m_Prefab.Name);
            Material material = prefab.GetComponent<Renderer>().sharedMaterial;
            Assert.IsFalse(prefab == null);
            Assert.IsFalse(material == null);

            // Unload the prefab bundle with all its assets -> prefab is null but not material
            m_ContentService.DeactivateAssetBundle(m_PrefabsBundle, true);
            Assert.IsTrue(prefab == null);
            Assert.IsFalse(material == null);

            // Clean unused assets -> material asset is null
            m_ContentService.CleanUnusedAssets();
            Assert.IsTrue(material == null);
        }

        [Test]
        public void GetContentAssetTest()
        {
            m_ContentService.ActivateAssetBundle(m_MaterialsBundle, false, false);

            // If the asset name is invalid -> return null
            Assert.IsNull(m_ContentService.GetContentAsset<Material>("Unknown"));

            // If the given type is incorrect -> return null
            Assert.IsNull(m_ContentService.GetContentAsset<Texture>(m_Materials[0].Name));

            // If the request is valid -> correct asset is returned and cached
            Material material1 = m_ContentService.GetContentAsset<Material>(m_Materials[0].Name);
            Assert.IsNotNull(material1);
            Assert.AreEqual(m_Materials[0].Name, material1.name);
            Assert.AreEqual(m_Materials[0].Color, material1.color);

            // If requested again -> the same content object is returned
            Material sameMaterial = m_ContentService.GetContentAsset<Material>(m_Materials[0].Name);
            Assert.AreEqual(material1, sameMaterial);

            // If requested again but with incorrect type -> return null
            Assert.IsNull(m_ContentService.GetContentAsset<Texture>(m_Materials[0].Name));
        }

        [Test]
        public void ResetLoadedAssetsTest()
        {
            // Activate asset bundle and load all its assets
            m_ContentService.ActivateAssetBundle(m_MaterialsBundle, true, false);
            Assert.AreEqual(m_Materials.Length, m_ContentService.AssetCacheSize);

            // Reset loaded assets
            m_ContentService.ResetLoadedAssets();
            Assert.AreEqual(0, m_ContentService.AssetCacheSize);

            // Request an asset -> asset is loaded again
            m_ContentService.GetContentAsset<Material>(m_Materials[1].Name); ;
            Assert.AreEqual(1, m_ContentService.AssetCacheSize);
        }

        private bool IsPrefabCorrect(GameObject prefab)
        {
            return prefab != null
                && prefab.GetComponent<Renderer>() != null
                && prefab.GetComponent<Collider>() != null;
        }

        private IEnumerator RunAsyncTask(Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }

            yield return null;
        }
    }

    #region Dependencies
    public class AssetsConfigurationStub : IConfigurationService
    {
        private UnityContentConfiguration m_Configuration;

        public AssetsConfigurationStub()
        {
            m_Configuration = new UnityContentConfiguration()
            {
                EnableContentData = false,
                EnableContentDescriptors = false,

                EnableContentAssets = true,
                AssetContentPath = "Assets/GameEngine.PMR.Unity/Tests/Runtime/Bundles/"
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
