using ArcObjectConverters;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Xunit2;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PointClassToPointTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullReturnsNull(IPoint point, PointGeoJsonConverter sut)
        {
            var actual = JsonConvert.SerializeObject((PointClass)null, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyReturnsNull(IPoint point, PointGeoJsonConverter sut)
        {
            point.SetEmpty();

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NanXReturnsNull(IPoint point, PointGeoJsonConverter sut)
        {
            point.X = double.NaN;

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NanYReturnsNull(IPoint point, PointGeoJsonConverter sut)
        {
            point.Y = double.NaN;

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NanZReturnsDefaultZValue(IPoint point, double defaultZValue)
        {
            ((IZAware)point).ZAware = false;
            point.Z = double.NaN;

            var sut = new PointGeoJsonConverter(new GeoJsonSerializerSettings
            {
                Dimensions = DimensionHandling.XYZ,
                DefaultZValue = defaultZValue
            });

            var actual = JsonConvert.SerializeObject(point, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()},
    {defaultZValue.ToJsonString()}
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory(Skip = "Too many branching to test +/- Infinity, should we really check that ?"), ArcObjectsConventions(32188)]
        public void InfinityCoordsReturnsNull(IPoint point)
        {
            var sut = new PointGeoJsonConverter();

            point.Y = double.PositiveInfinity;

            var actual = JsonConvert.SerializeObject(point, sut);

            JsonAssert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void Point2DReturnsJson(IPoint point)
        {
            ((IZAware) point).ZAware = false;

            var sut = new PointGeoJsonConverter();

            var actual = JsonConvert.SerializeObject(point, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()}
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void Point3DReturnsJson(IPoint point)
        {
            var settings = new GeoJsonSerializerSettings
            {
                Dimensions = DimensionHandling.XYZ
            };
            var sut = new PointGeoJsonConverter(settings);

            ((IZAware)point).ZAware = true;

            var actual = JsonConvert.SerializeObject(point, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()},
    {point.Z.ToJsonString()}
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

    }
}
