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
    public class PolylineGeoJsonConveterTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsFact]
        public void EmptyGeometry_ReturnsNullJson()
        {
            var sut = new PolylineGeoJsonConverter();

            var point = (IPolyline)_factory.CreateObject<Polyline>();
            point.SetEmpty();

            var actual = JsonConvert.SerializeObject(point, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsFact]
        public void NullGeometry_ReturnsNullJson()
        {
            var sut = new PolylineGeoJsonConverter();

            var actual = JsonConvert.SerializeObject((PolylineClass)null, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, AutoData]
        public void WithSinglePoint2D_ReturnsNullJson(double x, double y)
        {
            var sut = new PolylineGeoJsonConverter();

            var point = (IPoint)_factory.CreateObject<Point>();
            point.PutCoords(x, y);

            var invalidPolyline = (IPolyline) _factory.CreateObject<Polyline>();
            invalidPolyline.FromPoint = point;

            var actual = JsonConvert.SerializeObject(invalidPolyline, Formatting.Indented, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, AutoData]
        public void Line_ReturnsGeoJson(double x1, double y1, double x2, double y2)
        {
            var sut = new PolylineGeoJsonConverter();

            var point1 = (IPoint)_factory.CreateObject<Point>();
            point1.PutCoords(x1, y1);

            var point2 = (IPoint)_factory.CreateObject<Point>();
            point2.PutCoords(x2, y2);

            var line = (IPolyline)_factory.CreateObject<Polyline>();
            line.FromPoint = point1;
            line.ToPoint = point2;

            var actual = JsonConvert.SerializeObject(line, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [
      {x1.ToJsonString()},
      {y1.ToJsonString()}
    ],
    [
      {x2.ToJsonString()},
      {y2.ToJsonString()}
    ]
  ]
}}";

            Assert.Equal(expected, actual);
        }
    }
}
