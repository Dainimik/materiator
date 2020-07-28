using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Materiator
{
    public class AtlasFactoryTests
    {
        #region Calculate Atlas Size
        [Test]
        public void ShouldCalculateAtlasSizeCorrectly()
        {
            var cases = new[]
            {
                new Tuple<int, Vector2Int, Vector2Int>(0, new Vector2Int(4, 4), new Vector2Int(4, 4)),
                new Tuple<int, Vector2Int, Vector2Int>(1, new Vector2Int(8, 8), new Vector2Int(8, 8)),
                new Tuple<int, Vector2Int, Vector2Int>(5, new Vector2Int(8, 8), new Vector2Int(32, 32))
            };

            foreach (var (number, entrySize, expectation) in cases)
            {
                var result = AtlasFactory.CalculateAtlasSize(number, entrySize);

                Assert.AreEqual(expectation, result);
            }
        }

        [Test]
        public void ShouldCalculateCorrectRectSizeWithSameDimensions()
        {
            var cases = new[]
            {
                new Tuple<int, Vector2Int, Rect>(10, new Vector2Int(4, 4), new Rect(0f, 0f, 0.25f, 0.25f)),
                new Tuple<int, Vector2Int, Rect>(8, new Vector2Int(8, 8), new Rect(0f, 0f, 0.25f, 0.25f))
            };

            foreach (var (number, rectSize, expectation) in cases)
            {
                var rects = AtlasFactory.CalculateRects(number, rectSize);

                foreach (var rect in rects)
                {
                    Assert.AreEqual(expectation.width, rect.width);
                    Assert.AreEqual(expectation.height, rect.height);
                }
            }
        }
        #endregion
    }
}
