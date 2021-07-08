﻿using GameEngine.Core.Descriptors;
using GameEngine.Core.Pools;
using GameEngine.Core.Pools.Managers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameEngine.Core.Test
{
    /// <summary>
    /// Component tests for the TexturePoolManager class
    /// <see cref="TexturePoolManager"/>
    /// </summary>
    public class TexturePoolTest
    {
        private readonly TexturePoolManager m_PoolManager;
        private PoolDescriptor<TextureDescriptor> m_PoolDescriptor;

        public TexturePoolTest()
        {
            m_PoolManager = new TexturePoolManager();
        }

        [SetUp]
        public void Initialize()
        {
            m_PoolDescriptor = new PoolDescriptor<TextureDescriptor>()
            {
                PoolId = "texture_test_pool",
                InitialSize = 10,
                IsExtensible = true,
                ObjectDescriptor = new TextureDescriptor()
                {
                    Width = 64,
                    Height = 64,
                    Format = TextureFormat.DXT1,
                    UseMipMap = true,
                    Linear = false,
                    FilterMode = FilterMode.Bilinear,
                    WrapMode = TextureWrapMode.Repeat,
                    AnisoLevel = 1
                }
            };
        }

        [TearDown]
        public void CleanUp()
        {
            m_PoolManager.Clear();
        }

        [UnityTest]
        public IEnumerator CreateAndDestroyTextures()
        {
            // Create pool and get pooled texture
            m_PoolManager.CreatePool(m_PoolDescriptor);
            Texture2D texture = m_PoolManager.GetObjectFromPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The returned texture is correctly configured
            Assert.IsTrue(texture != null);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.Width, texture.width);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.Height, texture.height);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.Format, texture.format);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.FilterMode, texture.filterMode);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.WrapMode, texture.wrapMode);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.AnisoLevel, texture.anisoLevel);

            // Release pooled texture and destroy pool
            m_PoolManager.ReleaseObjectToPool(m_PoolDescriptor.PoolId, texture);
            m_PoolManager.DestroyPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The previously used texture is null in Unity
            Assert.IsTrue(texture == null);
        }

        [Test]
        public void LoadImageOnRequestedTexture()
        {
            m_PoolManager.CreatePool(m_PoolDescriptor);
            byte[] pngImageData = GetTestImageData(out int imageWidth, out int imageHeight);
            Texture2D texture = m_PoolManager.LoadImageOnPooledTexture(m_PoolDescriptor.PoolId, pngImageData);

            // The returned texture is not null
            Assert.IsTrue(texture != null);

            // It has image size
            Assert.AreEqual(imageWidth, texture.width);
            Assert.AreEqual(imageHeight, texture.height);

            // The pixels correspond to the loaded image
            Color backgroundColor = new Color(1.0f, 0.99215686274f, 1.0f);
            Assert.AreEqual(backgroundColor, texture.GetPixel(0, 0));
            Assert.AreEqual(backgroundColor, texture.GetPixel(imageWidth - 1, 0));
            Assert.AreEqual(backgroundColor, texture.GetPixel(0, imageHeight - 1));
            Assert.AreEqual(backgroundColor, texture.GetPixel(imageWidth - 1, imageHeight - 1));
        }

        private byte[] GetTestImageData(out int width, out int height)
        {
            width = 64;
            height = 64;

            // This is a Unity logo
            return new byte[] {
                0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,
                0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x40,0x08,0x00,0x00,0x00,0x00,0x8F,0x02,0x2E,
                0x02,0x00,0x00,0x01,0x57,0x49,0x44,0x41,0x54,0x78,0x01,0xA5,0x57,0xD1,0xAD,0xC4,
                0x30,0x08,0x83,0x81,0x32,0x4A,0x66,0xC9,0x36,0x99,0x85,0x45,0xBC,0x4E,0x74,0xBD,
                0x8F,0x9E,0x5B,0xD4,0xE8,0xF1,0x6A,0x7F,0xDD,0x29,0xB2,0x55,0x0C,0x24,0x60,0xEB,
                0x0D,0x30,0xE7,0xF9,0xF3,0x85,0x40,0x74,0x3F,0xF0,0x52,0x00,0xC3,0x0F,0xBC,0x14,
                0xC0,0xF4,0x0B,0xF0,0x3F,0x01,0x44,0xF3,0x3B,0x3A,0x05,0x8A,0x41,0x67,0x14,0x05,
                0x18,0x74,0x06,0x4A,0x02,0xBE,0x47,0x54,0x04,0x86,0xEF,0xD1,0x0A,0x02,0xF0,0x84,
                0xD9,0x9D,0x28,0x08,0xDC,0x9C,0x1F,0x48,0x21,0xE1,0x4F,0x01,0xDC,0xC9,0x07,0xC2,
                0x2F,0x98,0x49,0x60,0xE7,0x60,0xC7,0xCE,0xD3,0x9D,0x00,0x22,0x02,0x07,0xFA,0x41,
                0x8E,0x27,0x4F,0x31,0x37,0x02,0xF9,0xC3,0xF1,0x7C,0xD2,0x16,0x2E,0xE7,0xB6,0xE5,
                0xB7,0x9D,0xA7,0xBF,0x50,0x06,0x05,0x4A,0x7C,0xD0,0x3B,0x4A,0x2D,0x2B,0xF3,0x97,
                0x93,0x35,0x77,0x02,0xB8,0x3A,0x9C,0x30,0x2F,0x81,0x83,0xD5,0x6C,0x55,0xFE,0xBA,
                0x7D,0x19,0x5B,0xDA,0xAA,0xFC,0xCE,0x0F,0xE0,0xBF,0x53,0xA0,0xC0,0x07,0x8D,0xFF,
                0x82,0x89,0xB4,0x1A,0x7F,0xE5,0xA3,0x5F,0x46,0xAC,0xC6,0x0F,0xBA,0x96,0x1C,0xB1,
                0x12,0x7F,0xE5,0x33,0x26,0xD2,0x4A,0xFC,0x41,0x07,0xB3,0x09,0x56,0xE1,0xE3,0xA1,
                0xB8,0xCE,0x3C,0x5A,0x81,0xBF,0xDA,0x43,0x73,0x75,0xA6,0x71,0xDB,0x7F,0x0F,0x29,
                0x24,0x82,0x95,0x08,0xAF,0x21,0xC9,0x9E,0xBD,0x50,0xE6,0x47,0x12,0x38,0xEF,0x03,
                0x78,0x11,0x2B,0x61,0xB4,0xA5,0x0B,0xE8,0x21,0xE8,0x26,0xEA,0x69,0xAC,0x17,0x12,
                0x0F,0x73,0x21,0x29,0xA5,0x2C,0x37,0x93,0xDE,0xCE,0xFA,0x85,0xA2,0x5F,0x69,0xFA,
                0xA5,0xAA,0x5F,0xEB,0xFA,0xC3,0xA2,0x3F,0x6D,0xFA,0xE3,0xAA,0x3F,0xEF,0xFA,0x80,
                0xA1,0x8F,0x38,0x04,0xE2,0x8B,0xD7,0x43,0x96,0x3E,0xE6,0xE9,0x83,0x26,0xE1,0xC2,
                0xA8,0x2B,0x0C,0xDB,0xC2,0xB8,0x2F,0x2C,0x1C,0xC2,0xCA,0x23,0x2D,0x5D,0xFA,0xDA,
                0xA7,0x2F,0x9E,0xFA,0xEA,0xAB,0x2F,0xDF,0xF2,0xFA,0xFF,0x01,0x1A,0x18,0x53,0x83,
                0xC1,0x4E,0x14,0x1B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82,
            };
        }
    }
}
