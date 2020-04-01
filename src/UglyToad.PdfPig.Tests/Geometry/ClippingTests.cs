

namespace UglyToad.PdfPig.Tests.Geometry
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Geometry;
    using Xunit;

    public class ClippingTests
    {
        private static readonly DoubleComparer DoubleComparer = new DoubleComparer(3);
        private static readonly PointComparer PointComparer = new PointComparer(DoubleComparer);

        #region data
        public static IEnumerable<object[]> SutherlandHodgmanData => new[]
        {
            new object[]
            {
                new PdfPoint[]
                {
                    // with duplicate points!
                    new PdfPoint(18.52, 61.771),
                    new PdfPoint(19.094148, 55.003606),
                    new PdfPoint(20.756384, 48.583888),
                    new PdfPoint(23.4163959999999, 42.5977419999999),
                    new PdfPoint(26.9838719999999, 37.131064),
                    new PdfPoint(31.3685, 32.26975),
                    new PdfPoint(36.479968, 28.099696),
                    new PdfPoint(42.2279639999999, 24.706798),
                    new PdfPoint(48.522176, 22.176952),
                    new PdfPoint(55.272292, 20.596054),
                    new PdfPoint(62.388, 20.05),
                    new PdfPoint(62.388, 20.05),
                    new PdfPoint(69.503465, 20.596054),
                    new PdfPoint(76.25344, 22.176952),
                    new PdfPoint(82.5475949999999, 24.7067979999999),
                    new PdfPoint(88.2956, 28.099696),
                    new PdfPoint(93.407125, 32.26975),
                    new PdfPoint(97.79184, 37.131064),
                    new PdfPoint(101.359414999999, 42.597742),
                    new PdfPoint(104.01952, 48.583888),
                    new PdfPoint(105.681825, 55.003606),
                    new PdfPoint(106.256, 61.771),
                    new PdfPoint(106.256, 61.771),
                    new PdfPoint(105.681825, 68.538095),
                    new PdfPoint(104.01952, 74.95752),
                    new PdfPoint(101.359414999999, 80.9433849999999),
                    new PdfPoint(97.79184, 86.4098),
                    new PdfPoint(93.407125, 91.2708749999999),
                    new PdfPoint(88.2956, 95.44072),
                    new PdfPoint(82.547595, 98.8334449999999),
                    new PdfPoint(76.25344, 101.36316),
                    new PdfPoint(69.503465, 102.943975),
                    new PdfPoint(62.388, 103.49),
                    new PdfPoint(62.388, 103.49),
                    new PdfPoint(55.272292, 102.943975),
                    new PdfPoint(48.522176, 101.36316),
                    new PdfPoint(42.2279639999999, 98.8334449999999),
                    new PdfPoint(36.479968, 95.44072),
                    new PdfPoint(31.3685, 91.2708749999999),
                    new PdfPoint(26.983872, 86.4098),
                    new PdfPoint(23.416396, 80.9433849999999),
                    new PdfPoint(20.756384, 74.95752),
                    new PdfPoint(19.094148, 68.538095),
                    new PdfPoint(18.52, 61.771)
                },
                new PdfPoint[]
                {
                    new PdfPoint(0, 0),
                    new PdfPoint(423, 0),
                    new PdfPoint(423, 270),
                    new PdfPoint(0, 270),
                    new PdfPoint(0, 0)
                },
                new PdfPoint[]
                {
                    new PdfPoint(18.52, 61.771),
                    new PdfPoint(19.094148, 55.003606),
                    new PdfPoint(20.756384, 48.583888),
                    new PdfPoint(23.4163959999999, 42.5977419999999),
                    new PdfPoint(26.9838719999999, 37.131064),
                    new PdfPoint(31.3685, 32.26975),
                    new PdfPoint(36.479968, 28.099696),
                    new PdfPoint(42.2279639999999, 24.706798),
                    new PdfPoint(48.522176, 22.176952),
                    new PdfPoint(55.272292, 20.596054),
                    new PdfPoint(62.388, 20.05),
                    new PdfPoint(69.503465, 20.596054),
                    new PdfPoint(76.25344, 22.176952),
                    new PdfPoint(82.5475949999999, 24.7067979999999),
                    new PdfPoint(88.2956, 28.099696),
                    new PdfPoint(93.407125, 32.26975),
                    new PdfPoint(97.79184, 37.131064),
                    new PdfPoint(101.359414999999, 42.597742),
                    new PdfPoint(104.01952, 48.583888),
                    new PdfPoint(105.681825, 55.003606),
                    new PdfPoint(106.256, 61.771),
                    new PdfPoint(105.681825, 68.538095),
                    new PdfPoint(104.01952, 74.95752),
                    new PdfPoint(101.359414999999, 80.9433849999999),
                    new PdfPoint(97.79184, 86.4098),
                    new PdfPoint(93.407125, 91.2708749999999),
                    new PdfPoint(88.2956, 95.44072),
                    new PdfPoint(82.547595, 98.8334449999999),
                    new PdfPoint(76.25344, 101.36316),
                    new PdfPoint(69.503465, 102.943975),
                    new PdfPoint(62.388, 103.49),
                    new PdfPoint(55.272292, 102.943975),
                    new PdfPoint(48.522176, 101.36316),
                    new PdfPoint(42.2279639999999, 98.8334449999999),
                    new PdfPoint(36.479968, 95.44072),
                    new PdfPoint(31.3685, 91.2708749999999),
                    new PdfPoint(26.983872, 86.4098),
                    new PdfPoint(23.416396, 80.9433849999999),
                    new PdfPoint(20.756384, 74.95752),
                    new PdfPoint(19.094148, 68.538095),
                    new PdfPoint(18.52, 61.771)
                }
            }
        };
        #endregion

        [Theory]
        [MemberData(nameof(SutherlandHodgmanData))]
        public void SutherlandHodgman(PdfPoint[] clipping, PdfPoint[] polygon, PdfPoint[] expected)
        {
            var computed = ClippingOld.SutherlandHodgman(clipping, polygon);

            Assert.Equal(expected.Length, computed.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, computed, PointComparer);
            }
        }
    }
}
