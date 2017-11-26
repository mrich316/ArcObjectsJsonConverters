using System;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PolylineClassToMultiLineStringTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NonTouchingPathsReturnsJson(PolylineGeoJsonConverter sut, ILine line, IPoint otherPoint, ISpatialReference spatialReference)
        {
            // Connect line with other point.
            var otherLine = (ILine)_factory.CreateObject<Line>();
            otherLine.FromPoint = line.ToPoint;
            otherLine.ToPoint = otherPoint;

            object missing = Type.Missing;

            var path = (ISegmentCollection)_factory.CreateObject<Path>();
            path.AddSegment((ISegment)line, missing, missing);
            path.AddSegment((ISegment)otherLine, missing, missing);

            var polyline = (IGeometryCollection)_factory.CreateObject<Polyline>();
            polyline.AddGeometry((IGeometry)path);

            ((IGeometry)polyline).SpatialReference = spatialReference;

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""MultiLineString"",
  ""coordinates"": [
    [
      {line.FromPoint.X.ToJsonString()},
      {line.FromPoint.Y.ToJsonString()}
    ],
    [
      {line.ToPoint.X.ToJsonString()},
      {line.ToPoint.Y.ToJsonString()}
    ],
    [
      {otherPoint.X.ToJsonString()},
      {otherPoint.Y.ToJsonString()}
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }
    }
}
