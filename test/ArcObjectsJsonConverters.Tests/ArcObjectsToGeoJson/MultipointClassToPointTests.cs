using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.ArcObjectsToGeoJson
{
    public class MultipointClassToPointTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullReturnsNull(GeometryGeoJsonConverter sut)
        {
            var actual = JsonConvert.SerializeObject((MultipointClass)null, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyReturnsNull(IMultipoint multiPoint, GeometryGeoJsonConverter sut)
        {
            multiPoint.SetEmpty();

            var actual = JsonConvert.SerializeObject(multiPoint, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void SinglePointReturnsPoint(GeometryGeoJsonConverter sut, IMultipoint multiPoint, IPoint point)
        {
            multiPoint.SetEmpty();
            ((IPointCollection)multiPoint).AddPoint(point);

            var actual = JsonConvert.SerializeObject(multiPoint, sut);
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
        public void EmptyPointsAreRemoved(GeoJsonSerializerSettings serializerSettings, IMultipoint multiPoint, IPoint point, IPoint emptyPoint)
        {
            emptyPoint.SetEmpty();
            serializerSettings.Simplify = true;
            var sut = new GeometryGeoJsonConverter(serializerSettings);

            multiPoint.SetEmpty();
            ((IPointCollection)multiPoint).AddPoint(emptyPoint);
            ((IPointCollection)multiPoint).AddPoint(point);
            ((IPointCollection)multiPoint).AddPoint(emptyPoint);

            var actual = JsonConvert.SerializeObject(multiPoint, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()}
  ]
}}";
            JsonAssert.Equal(expected, actual);
        }

        public class SimplifyTrue
        {
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void MultiSamePointReturnsUniquePoint(GeoJsonSerializerSettings serializerSettings, IMultipoint multiPoint, IPoint point)
            {
                serializerSettings.Simplify = true;
                var sut = new GeometryGeoJsonConverter(serializerSettings);

                multiPoint.SetEmpty();
                ((IPointCollection)multiPoint).AddPoint(point);
                ((IPointCollection)multiPoint).AddPoint(point);
                ((IPointCollection)multiPoint).AddPoint(point);

                var actual = JsonConvert.SerializeObject(multiPoint, sut);
                var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()}
  ]
}}";
                JsonAssert.Equal(expected, actual);
            }
        }
    }
}
