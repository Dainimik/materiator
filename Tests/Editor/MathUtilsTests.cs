using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Materiator
{
    public class MathUtilsTests
    {
        [Test]
        public void ShouldCalculateShiftedUVCoordsCorrectlyWithWithSquareAspectRatiosSameRectSize()
        {
            var cases = new[]
            {
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.25f, 0.25f),
                    new Rect(0f, 0f, 0.5f, 0.5f),
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
                    new Vector2(0.75f, 0.75f)),
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.125f, 0.125f),
                    new Rect(0.0625f, 0.0625f, 0.1875f, 0.1875f),
                    new Rect(0.375f, 0.75f, 0.1875f, 0.1875f),
                    new Vector2(0.4375f, 0.8125f))
            };

            foreach (var (coord, coordRect, destRect, expectation) in cases)
            {
                var result = MathUtils.Transform2DCoord(coord, destRect);

                Assert.AreEqual(expectation, result);
            }
        }
        [Test]
        public void ShouldCalculateShiftedUVCoordsCorrectlyWithWithNonSquareAspectRatiosSameRectSize()
        {
            var cases = new[]
            {
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.25f, 0.25f),
                    new Rect(0f, 0f, 0.375f, 0.5f),
                    new Rect(0.5f, 0.5f, 0.375f, 0.5f),
                    new Vector2(0.75f, 0.75f)),
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.125f, 0.125f),
                    new Rect(0.0625f, 0.0625f, 0.125f, 0.1875f),
                    new Rect(0.375f, 0.75f, 0.125f, 0.1875f),
                    new Vector2(0.4375f, 0.8125f)),
                 new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.125f, 0.25f),
                    new Rect(0.0625f, 0.0625f, 0.25f, 0.5f),
                    new Rect(0.5f, 0.4375f, 0.25f, 0.5f),
                    new Vector2(0.5625f, 0.625f))
            };

            foreach (var (coord, coordRect, destRect, expectation) in cases)
            {
                var result = MathUtils.Transform2DCoord(coord, destRect);

                Assert.AreEqual(expectation, result);
            }
        }
        [Test]
        public void ShouldCalculateShiftedUVCoordsCorrectlyWithSquareAspectRatiosDifferentRectSize()
        {
            var cases = new[]
            {
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.5f, 0.5f),
                    new Rect(0f, 0f, 1f, 1f),
                    new Rect(0f, 0f, 0.5f, 0.5f),
                    new Vector2(0.25f, 0.25f)),
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.1875f, 0.1875f),
                    new Rect(0.125f, 0.125f, 0.125f, 0.125f),
                    new Rect(0.0f, 0.0f, 0.25f, 0.25f),
                    new Vector2(0.125f, 0.125f))
            };

            foreach (var (coord, coordRect, destRect, expectation) in cases)
            {
                var result = MathUtils.Transform2DCoord(coord, destRect);

                Assert.AreEqual(expectation, result);
            }
        }
        [Test]
        public void ShouldCalculateShiftedUVCoordsCorrectlyWithNonSquareAspectRatioDifferentRectSize()
        {
            var cases = new[]
            {
                new Tuple<Vector2, Rect, Rect, Vector2>(
                    new Vector2(0.125f, 0.125f),
                    new Rect(0.0f, 0.0f, 0.1875f, 0.25f),
                    new Rect(0.0f, 0.0f, 0.09375f, 0.25f),
                    new Vector2(0.0625f, 0.125f))
            };    

            foreach (var (coord, coordRect, destRect, expectation) in cases)
            {
                var result = MathUtils.Transform2DCoord(coord, destRect);

                Assert.AreEqual(expectation, result);
            }
        }
    }
}
