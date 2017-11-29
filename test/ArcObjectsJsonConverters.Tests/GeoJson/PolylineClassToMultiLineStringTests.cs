using System;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PolylineClassToMultiLineStringTests
    {
        private static readonly IArcObjectFactory Factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void TouchingPathsReturnsMultiLineString(GeoJsonConverter sut, ILine line, IPoint point, ISpatialReference spatialReference)
        {
            var path1 = (ISegmentCollection)Factory.CreateObject<Path>();
            path1.AddSegment((ISegment)line);

            var otherLine = (ILine) Factory.CreateObject<Line>();
            otherLine.FromPoint = line.ToPoint;
            otherLine.ToPoint = point;

            var path2 = (ISegmentCollection)Factory.CreateObject<Path>();
            path2.AddSegment((ISegment)otherLine);

            var polyline = (IGeometryCollection)Factory.CreateObject<Polyline>();
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
        public void NonTouchingPathsReturnsMultiLineString(GeoJsonConverter sut, ILine line, ILine otherLine, ISpatialReference spatialReference)
        {
            var path1 = (ISegmentCollection)Factory.CreateObject<Path>();
            path1.AddSegment((ISegment)line);

            var path2 = (ISegmentCollection)Factory.CreateObject<Path>();
            path2.AddSegment((ISegment)otherLine);

            var polyline = (IGeometryCollection)Factory.CreateObject<Polyline>();
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
        public void OnlyCurvesAreGeneralized(IPolyline polyline, ILine line, ILine otherLine, IPoint extensionPoint, IBezierCurve bezier)
        {
            var serializerSettings = new GeoJsonSerializerSettings();
            var sut = new GeoJsonConverter(serializerSettings);

            // Create {otherLine} that is an extension to {line}.
            // This segment must not be simplified during the serialization.
            line.QueryPoint(esriSegmentExtension.esriExtendAtTo, line.Length + line.Length / 2, false, extensionPoint);
            otherLine.PutCoords(line.ToPoint, extensionPoint);

            // Prepare the actual test value.
            polyline.SetEmpty();

            var segments1 = (ISegmentCollection)Factory.CreateObject<Path>();
            segments1.AddSegment((ISegment) line);
            segments1.AddSegment((ISegment) otherLine);

            var segments2 = (ISegmentCollection)Factory.CreateObject<Path>();
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

        public class SimplifyTrue
        {
            // TODO: The orientation of the polyline sometime changes. Investigate why.
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void NonTouchingPathsReturnsMultiLineString(GeoJsonSerializerSettings serializerSettings, ILine line, ILine otherLine, ISpatialReference spatialReference)
            {
                serializerSettings.Simplify = true;
                var sut = new GeoJsonConverter(serializerSettings);

                var path1 = (ISegmentCollection)Factory.CreateObject<Path>();
                path1.AddSegment((ISegment)line);

                var path2 = (ISegmentCollection)Factory.CreateObject<Path>();
                path2.AddSegment((ISegment)otherLine);

                var polyline = (IGeometryCollection)Factory.CreateObject<Polyline>();
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

        }
    }
}
