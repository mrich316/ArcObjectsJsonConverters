using System;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJsonToArcObjects
{
    public class PointToPolygonClassTests
    {
        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, typeof(IPolygon))]
        [ArcObjectsConventions(32188, typeof(Polygon))]
        [ArcObjectsConventions(32188, typeof(PolygonClass))]
        public void PointReturnsPolygon(Type type, GeometryGeoJsonConverter sut, double x, double y)
        {
            var geoJson = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [{x.ToJsonString()}, {y.ToJsonString()}]
}}";

            var actual = (IPointCollection)JsonConvert.DeserializeObject(geoJson, type, sut);

            Assert.Equal(1, actual.PointCount);
            Assert.Equal(x, actual.Point[0].X);
            Assert.Equal(y, actual.Point[0].Y);
        }

        public class SimplifyTrue
        {
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void PointReturnsEmpty(GeoJsonSerializerSettings serializerSettings, double x, double y)
            {
                serializerSettings.Simplify = true;
                var sut = new GeometryGeoJsonConverter(serializerSettings);

                var geoJson = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [{x.ToJsonString()}, {y.ToJsonString()}]
}}";

                var actual = JsonConvert.DeserializeObject<IPolygon>(geoJson, sut);

                Assert.True(actual.IsEmpty);
            }
        }
    }
}
