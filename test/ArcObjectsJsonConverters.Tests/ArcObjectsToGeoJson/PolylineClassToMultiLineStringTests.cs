using System;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.ArcObjectsToGeoJson
{
    public class PolylineClassToMultiLineStringTests
    {
        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void TouchingPathsReturnsMultiLineString(GeometryGeoJsonConverter sut, ILine line, IPoint point, ISpatialReference spatialReference)
        {
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
  ""type"": ""MultiLineString"",
  ""coordinates"": [
    [
      [
        {line.FromPoint.X.ToJsonString()},
        {line.FromPoint.Y.ToJsonString()}
      ],
      [
        {line.ToPoint.X.ToJsonString()},
        {line.ToPoint.Y.ToJsonString()}
      ]
    ],
    [
      [
        {otherLine.FromPoint.X.ToJsonString()},
        {otherLine.FromPoint.Y.ToJsonString()}
      ],
      [
        {otherLine.ToPoint.X.ToJsonString()},
        {otherLine.ToPoint.Y.ToJsonString()}
      ]
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NonTouchingPathsReturnsMultiLineString(GeometryGeoJsonConverter sut, ILine line, ILine otherLine, ISpatialReference spatialReference)
        {
            var path1 = (ISegmentCollection) new PathClass();
            path1.AddSegment((ISegment)line);

            var path2 = (ISegmentCollection) new PathClass();
            path2.AddSegment((ISegment)otherLine);

            var polyline = (IGeometryCollection) new PolylineClass();
            polyline.AddGeometry((IGeometry)path1);
            polyline.AddGeometry((IGeometry)path2);

            ((IGeometry)polyline).SpatialReference = spatialReference;

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""MultiLineString"",
  ""coordinates"": [
    [
      [
        {line.FromPoint.X.ToJsonString()},
        {line.FromPoint.Y.ToJsonString()}
      ],
      [
        {line.ToPoint.X.ToJsonString()},
        {line.ToPoint.Y.ToJsonString()}
      ]
    ],
    [
      [
        {otherLine.FromPoint.X.ToJsonString()},
        {otherLine.FromPoint.Y.ToJsonString()}
      ],
      [
        {otherLine.ToPoint.X.ToJsonString()},
        {otherLine.ToPoint.Y.ToJsonString()}
      ]
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }

        [ArcObjectsFact, ArcObjectsConventions(32188)]
        public void CurvesInEveryPathsAreGeneralized()
        {
            throw new NotImplementedException();
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void OnlyCurvesAreGeneralized(GeometryGeoJsonConverter sut, IPolyline polyline, ILine line, ILine otherLine, IPoint extensionPoint, IBezierCurve bezier)
        {
            // Create {otherLine} that is an extension to {line}.
            // This segment must not be simplified during the serialization.
            line.QueryPoint(esriSegmentExtension.esriExtendAtTo, line.Length + line.Length / 2, false, extensionPoint);
            otherLine.PutCoords(line.ToPoint, extensionPoint);

            // Prepare the actual test value.
            polyline.SetEmpty();

            var segments1 = (ISegmentCollection) new PathClass();
            segments1.AddSegment((ISegment) line);
            segments1.AddSegment((ISegment) otherLine);

            var segments2 = (ISegmentCollection) new PathClass();
            segments2.AddSegment((ISegment) bezier);

            ((IGeometryCollection)polyline).AddGeometry((IGeometry)segments1);
            ((IGeometryCollection)polyline).AddGeometry((IGeometry)segments2);

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);

            // It must contain the "mid" point between line.FromPoint and otherLine.ToPoint.
            // If it is missing, the serialization merged the two segments even tho Simplify=false.
            JsonAssert.Contains($@"
[
  {otherLine.FromPoint.X.ToJsonString()},
  {otherLine.FromPoint.Y.ToJsonString()}
]", actual);
        }

        public class ForceMultiGeometryTrue
        {
            private readonly GeometryGeoJsonConverter _sut;

            public ForceMultiGeometryTrue()
            {
                _sut = new GeometryGeoJsonConverter(new GeoJsonSerializerSettings
                {
                    ForceMultiGeometry = true
                });
            }

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void SinglePathReturnsMultiLineString(IPolyline polyline, ILine line, ISpatialReference spatialReference)
            {
                polyline.SetEmpty();
                polyline.SpatialReference = spatialReference;
                ((ISegmentCollection) polyline).AddSegment((ISegment) line);

                var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, _sut);
                var expected = $@"{{
  ""type"": ""MultiLineString"",
  ""coordinates"": [
    [
      [
        {line.FromPoint.X.ToJsonString()},
        {line.FromPoint.Y.ToJsonString()}
      ],
      [
        {line.ToPoint.X.ToJsonString()},
        {line.ToPoint.Y.ToJsonString()}
      ]
    ]
  ]
}}";

                JsonAssert.Equal(expected, actual);
            }
        }

        public class SimplifyTrue
        {
            private readonly GeometryGeoJsonConverter _sut;

            public SimplifyTrue()
            {
                _sut = new GeometryGeoJsonConverter(new GeoJsonSerializerSettings
                {
                    Simplify = true
                });
            }

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void TouchingPathsReturnsLineString(ILine line, IPoint point, ISpatialReference spatialReference)
            {
                var path1 = (ISegmentCollection)new PathClass();
                path1.AddSegment((ISegment)line);

                var otherLine = (ILine)new LineClass();
                otherLine.FromPoint = line.ToPoint;
                otherLine.ToPoint = point;

                var path2 = (ISegmentCollection)new PathClass();
                path2.AddSegment((ISegment)otherLine);

                var polyline = (IGeometryCollection)new PolylineClass();
                polyline.AddGeometry((IGeometry)path1);
                polyline.AddGeometry((IGeometry)path2);

                ((IGeometry)polyline).SpatialReference = spatialReference;

                var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, _sut);
                var expected = $@"{{
  ""type"": ""LineString"",
  ""coordinates"": [
    [
      {line.FromPoint.X.ToJsonString()},
      {line.FromPoint.Y.ToJsonString()}
    ],
    [
      {otherLine.FromPoint.X.ToJsonString()},
      {otherLine.FromPoint.Y.ToJsonString()}
    ],
    [
      {otherLine.ToPoint.X.ToJsonString()},
      {otherLine.ToPoint.Y.ToJsonString()}
    ]
  ]
}}";

                JsonAssert.Equal(expected, actual);
            }
            
            // TODO: The orientation of the polyline sometime changes. Investigate why.
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void NonTouchingPathsReturnsMultiLineString(IPolyline polyline, ILine line, ILine otherLine)
            {
                var path1 = (ISegmentCollection) new PathClass();
                path1.AddSegment((ISegment)line);

                var path2 = (ISegmentCollection) new PathClass();
                path2.AddSegment((ISegment)otherLine);

                polyline.SetEmpty();
                ((IGeometryCollection) polyline).AddGeometry((IGeometry) path1);
                ((IGeometryCollection)polyline).AddGeometry((IGeometry)path2);

                var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, _sut);
                var expected = $@"{{
  ""type"": ""MultiLineString"",
  ""coordinates"": [
    [
      [
        {line.FromPoint.X.ToJsonString()},
        {line.FromPoint.Y.ToJsonString()}
      ],
      [
        {line.ToPoint.X.ToJsonString()},
        {line.ToPoint.Y.ToJsonString()}
      ]
    ],
    [
      [
        {otherLine.FromPoint.X.ToJsonString()},
        {otherLine.FromPoint.Y.ToJsonString()}
      ],
      [
        {otherLine.ToPoint.X.ToJsonString()},
        {otherLine.ToPoint.Y.ToJsonString()}
      ]
    ]
  ]
}}";

                JsonAssert.Equal(expected, actual);
            }
        }
    }
}
