using System;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJsonToArcObjects
{
    public class LineStringToPolylineClassTests
    {
        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, typeof(IPolyline))]
        [ArcObjectsConventions(32188, typeof(Polyline))]
        [ArcObjectsConventions(32188, typeof(PolylineClass))]
        public void LineStringReturnsPolyline(Type type, GeometryGeoJsonConverter sut, double x1, double y1, double x2, double y2)
        {
            var geoJson = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [{x1.ToJsonString()}, {y1.ToJsonString()}],
    [{x2.ToJsonString()}, {y2.ToJsonString()}]
  ]
}}";

            var actual = (IPolyline)JsonConvert.DeserializeObject(geoJson, type, sut);

            Assert.Equal(x1, actual.FromPoint.X);
            Assert.Equal(y1, actual.FromPoint.Y);

            Assert.Equal(x2, actual.ToPoint.X);
            Assert.Equal(y2, actual.ToPoint.Y);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void SinglePositionArrayReturnsIncompletePolyline(GeometryGeoJsonConverter sut, double x1, double y1)
        {
            var geoJson = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [{x1.ToJsonString()}, {y1.ToJsonString()}]
  ]
}}";

            var actual = JsonConvert.DeserializeObject<IPolyline>(geoJson, sut);
            Assert.False(actual.IsEmpty);
            Assert.Equal(1, ((IPointCollection) actual).PointCount);
        }

        public class SimplifyTrue
        {
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void SinglePositionArrayReturnsEmpty(GeoJsonSerializerSettings serializerSettings, double x1, double y1)
            {
                serializerSettings.Simplify = true;
                var sut = new GeometryGeoJsonConverter(serializerSettings);

                var geoJson = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [{x1.ToJsonString()}, {y1.ToJsonString()}]
  ]
}}";

                var actual = JsonConvert.DeserializeObject<IPolyline>(geoJson, sut);
                Assert.True(actual.IsEmpty);
            }
        }
    }
}
