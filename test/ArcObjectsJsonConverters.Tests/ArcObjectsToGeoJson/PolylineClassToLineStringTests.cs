using ArcObjectConverters;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.ArcObjectsToGeoJson
{
    public class PolylineClassToLineStringTests
    {
        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyReturnsNull(GeometryGeoJsonConverter sut)
        {
            var polyline = (IPolyline) new PolylineClass();
            polyline.SetEmpty();

            var actual = JsonConvert.SerializeObject(polyline, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NullReturnsNull(GeometryGeoJsonConverter sut)
        {
            var actual = JsonConvert.SerializeObject((PolylineClass)null, sut);

            Assert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void PathWithSinglePointReturnsNull(GeometryGeoJsonConverter sut, IPoint point)
        {
            var polylineWithoutToPoint = (IPolyline)new PolylineClass();
            polylineWithoutToPoint.FromPoint = point;

            var actual = JsonConvert.SerializeObject(polylineWithoutToPoint, Formatting.Indented, sut);
            JsonAssert.Equal("null", actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void ManyPathWithSinglePointsReturnsMultiPoint(GeometryGeoJsonConverter sut, IPolyline polyline, IPoint[] points)
        {
            polyline.SetEmpty();

            var collection = (IGeometryCollection)polyline;

            foreach (var point in points)
            {
                var path = (IPointCollection)new PathClass();
                path.AddPoint(point);

                collection.AddGeometry((IGeometry)path);
            }

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = "null";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void LineReturnsLineString(GeometryGeoJsonConverter sut, ILine line, ISpatialReference spatialReference)
        {
            var polyline = (IGeometry) new PolylineClass();
            polyline.SpatialReference = spatialReference;

            var path = (ISegmentCollection) new PathClass();

            path.AddSegment((ISegment)line);

            ((IGeometryCollection)polyline).AddGeometry((IGeometry)path);

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
        public void OneCompletePathWithManyIncompletePathsReturnLinestring(GeometryGeoJsonConverter sut, IPolyline polyline, ILine line, IPoint fromPoint)
        {
            var emptyPath = (IPath) new PathClass();

            var incompleteLine = (ILine) new LineClass();
            incompleteLine.FromPoint = fromPoint;
            var incompletePath = (IPath) new PathClass();
            ((ISegmentCollection) incompletePath).AddSegment((ISegment) incompleteLine);

            // Add some incomplete lines.
            polyline.SetEmpty();
            var pathCol = (IGeometryCollection) polyline;
            pathCol.AddGeometry(emptyPath);
            pathCol.AddGeometry(incompletePath);

            // Add a complete line.
            var completePath = (IPath) new PathClass();
            ((ISegmentCollection) completePath).AddSegment((ISegment) line);
            pathCol.AddGeometry(completePath);

            // Add some more incomplete lines.
            pathCol.AddGeometry((IGeometry)((IClone)emptyPath).Clone());
            pathCol.AddGeometry((IGeometry)((IClone)incompletePath).Clone());

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
}}
";
            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void PathWithConnectedSegmentsReturnsLineString(GeometryGeoJsonConverter sut, ILine line, IPoint otherPoint, ISpatialReference spatialReference)
        {
            // Connect line with other point.
            var otherLine = (ILine) new LineClass();
            otherLine.FromPoint = line.ToPoint;
            otherLine.ToPoint = otherPoint;

            var path = (ISegmentCollection) new PathClass();
            path.AddSegment((ISegment)line);
            path.AddSegment((ISegment)otherLine);

            var polyline = (IGeometryCollection) new PolylineClass();
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



        public class SimplifyTrue
        {
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void NullFromPointReturnsNull(GeoJsonSerializerSettings serializerSettings, IPoint point)
            {
                serializerSettings.Simplify = true;

                var sut = new GeometryGeoJsonConverter(serializerSettings);

                var invalidPolyline = (IPolyline) new PolylineClass();
                invalidPolyline.ToPoint = point;

                var actual = JsonConvert.SerializeObject(invalidPolyline, Formatting.Indented, sut);

                Assert.Equal("null", actual);
            }

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void NullToPointReturnsNull(GeoJsonSerializerSettings serializerSettings, IPoint point)
            {
                serializerSettings.Simplify = true;

                var sut = new GeometryGeoJsonConverter(serializerSettings);

                var invalidPolyline = (IPolyline) new PolylineClass();
                invalidPolyline.FromPoint = point;

                var actual = JsonConvert.SerializeObject(invalidPolyline, Formatting.Indented, sut);

                Assert.Equal("null", actual);
            }

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void InvalidSegmentIsRemoved(GeoJsonSerializerSettings serializerSettings, ILine line,
                IPoint otherPoint, ISpatialReference spatialReference)
            {
                serializerSettings.Simplify = true;

                var sut = new GeometryGeoJsonConverter(serializerSettings);

                var otherLine = (ILine) new LineClass();
                otherLine.FromPoint = otherPoint;

                var path = (ISegmentCollection) new PathClass();
                path.AddSegment((ISegment) line);
                path.AddSegment((ISegment) otherLine);

                var polyline = (IGeometryCollection) new PolylineClass();
                polyline.AddGeometry((IGeometry) path);

                ((IGeometry) polyline).SpatialReference = spatialReference;

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
            public void OverlappedSegmentIsRemoved(GeoJsonSerializerSettings serializerSettings, ILine line,
                IPoint midPoint, IPoint extensionPoint, ISpatialReference spatialReference)
            {
                serializerSettings.Simplify = true;

                var sut = new GeometryGeoJsonConverter(serializerSettings);

                // Find the midpoint to create the FromPoint of the overlapped segment.
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, midPoint);

                // Extend pass the endpoint to create to ToPoint of the overlapped segment.
                line.QueryPoint(esriSegmentExtension.esriExtendAtTo, line.Length + line.Length / 2, false,
                    extensionPoint);

                var overlappedLine = (ILine) new LineClass();
                overlappedLine.FromPoint = midPoint;
                overlappedLine.ToPoint = extensionPoint;

                var path = (ISegmentCollection) new PathClass();
                path.AddSegment((ISegment) line);
                path.AddSegment((ISegment) overlappedLine);

                var polyline = (IGeometryCollection) new PolylineClass();
                polyline.AddGeometry((IGeometry) path);

                ((IGeometry) polyline).SpatialReference = spatialReference;

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

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void TouchingPathsReturnsLineString(GeoJsonSerializerSettings serializerSettings, ILine line, IPoint point, ISpatialReference spatialReference)
            {
                serializerSettings.Simplify = true;
                var sut = new GeometryGeoJsonConverter(serializerSettings);

                var path1 = (ISegmentCollection) new PathClass();
                path1.AddSegment((ISegment)line);

                var otherLine = (ILine) new LineClass();
                otherLine.FromPoint = line.ToPoint;
                otherLine.ToPoint = point;

                var path2 = (ISegmentCollection) new PathClass();
                path2.AddSegment((ISegment)otherLine);

                var polyline = (IGeometryCollection) new PolylineClass();
                polyline.AddGeometry((IGeometry)path1);
                polyline.AddGeometry((IGeometry)path2);

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
      {point.X.ToJsonString()},
      {point.Y.ToJsonString()}
    ]
  ]
}}";

                JsonAssert.Equal(expected, actual);
            }
        }
    }
}
