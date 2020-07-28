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
                new Tuple<int, Vector2Int>(0, new Vector2Int(4, 4)),
                new Tuple<int, Vector2Int>(1, new Vector2Int(4, 4)),
                new Tuple<int, Vector2Int>(5, new Vector2Int(16, 16))
            };

            foreach (var (number, expectation) in cases)
            {
                var result = AtlasFactory.CalculateAtlasSize(number);

                Assert.AreEqual(expectation, result);
            }
        }
        #endregion
    }
}
