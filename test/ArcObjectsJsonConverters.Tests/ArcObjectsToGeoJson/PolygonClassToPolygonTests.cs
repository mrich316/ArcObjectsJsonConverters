using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.ArcObjectsToGeoJson
{
    public class PolygonClassToPolygonTests
    {
        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyReturnsNull(GeometryGeoJsonConverter sut)
        {
            var polygon = new PolygonClass();
            polygon.SetEmpty();

            var actual = JsonConvert.SerializeObject(polygon, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullReturnsNull(GeometryGeoJsonConverter sut)
        {
            var actual = JsonConvert.SerializeObject((PolygonClass)null, sut);

            Assert.Equal("null", actual);
        }


        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void PolygonReturnsPolygon(GeometryGeoJsonConverter sut, ILine line, IPoint point, ISpatialReference spatialReference)
        {
            var polygon = (IGeometry) new PolygonClass();
            polygon.SpatialReference = spatialReference;

            var ring = (IPointCollection) new RingClass();
            ring.AddPoint(line.FromPoint);
            ring.AddPoint(line.ToPoint);
            ring.AddPoint(point);
            ring.AddPoint(line.FromPoint);

            ((IGeometryCollection) polygon).AddGeometry((IGeometry) ring);

            var actual = JsonConvert.SerializeObject(polygon, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""Polygon"",
  ""coordinates"": [
    [{line.FromPoint.X.ToJsonString()}, {line.FromPoint.Y.ToJsonString()}],
    [{line.ToPoint.X.ToJsonString()}, {line.ToPoint.Y.ToJsonString()}],
    [{point.X.ToJsonString()}, {point.Y.ToJsonString()}],
    [{line.FromPoint.X.ToJsonString()}, {line.FromPoint.Y.ToJsonString()}]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }


    }
}
