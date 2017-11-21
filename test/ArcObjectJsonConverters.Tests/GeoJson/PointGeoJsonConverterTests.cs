using System;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Xunit2;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PointGeoJsonConverterTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsFact]
        public void EmptyGeometry_ReturnsNullJson()
        {
            var sut = new PointGeoJsonConverter();

            var point = (IPoint)_factory.CreateObject<Point>();
            point.SetEmpty();

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, AutoData]
        public void WithNanCoords_ReturnsNullJson(double x)
        {
            var sut = new PointGeoJsonConverter();

            var point = (IPoint)_factory.CreateObject<Point>();
            point.PutCoords(x, double.NaN);

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, AutoData]
        public void WithInfinityCoords_ReturnsNullJson(double x)
        {
            var sut = new PointGeoJsonConverter();

            var point = (IPoint)_factory.CreateObject<Point>();
            point.PutCoords(x, double.PositiveInfinity);

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsFact]
        public void NullGeometry_ReturnsNullJson()
        {
            var sut = new PointGeoJsonConverter();

            var actual = JsonConvert.SerializeObject((PointClass)null, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, AutoData]
        public void Point2D_ReturnsGeoJson(double x, double y)
        {
            var sut = new PointGeoJsonConverter();

            var point = (IPoint)_factory.CreateObject<Point>();
            point.PutCoords(x, y);

            var actual = JsonConvert.SerializeObject(point, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {x.ToJsonString()},
    {y.ToJsonString()}
  ]
}}";

            Assert.Equal(expected, actual);
        }

        [ArcObjectsTheory, AutoData]
        public void Point3D_ReturnsGeoJson(double x, double y, double z)
        {
            var sut = new PointGeoJsonConverter();

            var point = (IPoint)_factory.CreateObject<Point>();
            point.PutCoords(x, y);

            ((IZAware)point).ZAware = true;
            point.Z = z;

            var actual = JsonConvert.SerializeObject(point, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {x.ToJsonString()},
    {y.ToJsonString()},
    {z.ToJsonString()}
  ]
}}";

            Assert.Equal(expected, actual);
        }

    }
}
