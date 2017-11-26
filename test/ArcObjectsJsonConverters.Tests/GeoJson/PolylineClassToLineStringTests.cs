using System;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PolylineClassToLineStringTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyReturnsNull(PolylineGeoJsonConverter sut)
        {
            var polyline = (IPolyline)_factory.CreateObject<Polyline>();
            polyline.SetEmpty();

            var actual = JsonConvert.SerializeObject(polyline, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullReturnsNull(PolylineGeoJsonConverter sut)
        {
            var actual = JsonConvert.SerializeObject((PolylineClass)null, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullFromPointReturnsNull(PolylineGeoJsonConverter sut, IPoint point)
        {
            var invalidPolyline = (IPolyline)_factory.CreateObject<Polyline>();
            invalidPolyline.ToPoint = point;

            var actual = JsonConvert.SerializeObject(invalidPolyline, Formatting.Indented, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullToPointReturnsNull(PolylineGeoJsonConverter sut, IPoint point)
        {
            var invalidPolyline = (IPolyline)_factory.CreateObject<Polyline>();
            invalidPolyline.FromPoint = point;

            var actual = JsonConvert.SerializeObject(invalidPolyline, Formatting.Indented, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void LineReturnsJson(PolylineGeoJsonConverter sut, ILine line, ISpatialReference spatialReference)
        {
            var polyline = (IGeometry)_factory.CreateObject<Polyline>();
            polyline.SpatialReference = spatialReference;

            var path = (ISegmentCollection)_factory.CreateObject<Path>();

            object missing = Type.Missing;
            path.AddSegment((ISegment) line, missing, missing);

            ((IGeometryCollection) polyline).AddGeometry((IGeometry) path, missing, missing);

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [
      {line.FromPoint.X.ToJsonString()},
      {line.FromPoint.Y.ToJsonString()}
    ],
    [
      {line.ToPoint.X.ToJsonString()},
      {line.ToPoint.Y.ToJsonString()}
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void PathWithConnectedSegmentsReturnsJson(PolylineGeoJsonConverter sut, ILine line, IPoint otherPoint, ISpatialReference spatialReference)
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
  ""type"": ""LineString"",
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

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void InvalidSegmentIsRemoved(PolylineGeoJsonConverter sut, ILine line, IPoint otherPoint, ISpatialReference spatialReference)
        {
            var otherLine = (ILine)_factory.CreateObject<Line>();
            otherLine.FromPoint = otherPoint;

            object missing = Type.Missing;

            var path = (ISegmentCollection)_factory.CreateObject<Path>();
            path.AddSegment((ISegment)line, missing, missing);
            path.AddSegment((ISegment)otherLine, missing, missing);

            var polyline = (IGeometryCollection)_factory.CreateObject<Polyline>();
            polyline.AddGeometry((IGeometry)path);

            ((IGeometry)polyline).SpatialReference = spatialReference;

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [
      {line.FromPoint.X.ToJsonString()},
      {line.FromPoint.Y.ToJsonString()}
    ],
    [
      {line.ToPoint.X.ToJsonString()},
      {line.ToPoint.Y.ToJsonString()}
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void OverlappedSegmentIsRemoved(PolylineGeoJsonConverter sut, ILine line, IPoint midPoint, IPoint extensionPoint, ISpatialReference spatialReference)
        {
            // Find the midpoint to create the FromPoint of the overlapped segment.
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, midPoint);

            // Extend pass the endpoint to create to ToPoint of the overlapped segment.
            line.QueryPoint(esriSegmentExtension.esriExtendAtTo, line.Length + line.Length / 2, false, extensionPoint);

            var overlappedLine = (ILine)_factory.CreateObject<Line>();
            overlappedLine.FromPoint = midPoint;
            overlappedLine.ToPoint = extensionPoint;

            object missing = Type.Missing;

            var path = (ISegmentCollection)_factory.CreateObject<Path>();
            path.AddSegment((ISegment)line, missing, missing);
            path.AddSegment((ISegment)overlappedLine, missing, missing);

            var polyline = (IGeometryCollection)_factory.CreateObject<Polyline>();
            polyline.AddGeometry((IGeometry)path);

            ((IGeometry)polyline).SpatialReference = spatialReference;

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [
      {line.FromPoint.X.ToJsonString()},
      {line.FromPoint.Y.ToJsonString()}
    ],
    [
      {extensionPoint.X.ToJsonString()},
      {extensionPoint.Y.ToJsonString()}
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }
    }
}
