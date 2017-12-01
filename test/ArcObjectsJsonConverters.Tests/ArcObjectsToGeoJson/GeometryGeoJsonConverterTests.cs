using System;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.ArcObjectsToGeoJson
{
    public class GeometryGeoJsonConverterTests
    {
        private static readonly IArcObjectFactory Factory = new ClientArcObjectFactory();

        [ArcObjectsFact]
        public void CtorThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>("serializerSettings", () => new TestGeometryGeoJsonConverter(null));
        }

        public class PrepareGeometry
        {
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void WithoutSideEffectsReturnsClone(IPolyline expected, ILine line, IBezierCurve curve)
            {
                var path1 = (ISegmentCollection)Factory.CreateObject<Path>();
                path1.AddSegment((ISegment) line);
                ((IGeometryCollection)expected).AddGeometry((IGeometry)path1);

                var path2 = (ISegmentCollection)Factory.CreateObject<Path>();
                path2.AddSegment((ISegment) curve);
                ((IGeometryCollection) expected).AddGeometry((IGeometry) path2);

                var sut = new TestGeometryGeoJsonConverter(new GeoJsonSerializerSettings
                {
                    SerializerHasSideEffects = false,
                    Simplify = true
                });
                var actual = sut.TestPrepareGeometry(expected);

                Assert.NotSame(expected, actual);
            }

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void WithSideEffectsReturnsSameInstance(IPolyline expected, ILine line, IBezierCurve curve)
            {
                var path1 = (ISegmentCollection)Factory.CreateObject<Path>();
                path1.AddSegment((ISegment)line);
                ((IGeometryCollection)expected).AddGeometry((IGeometry)path1);

                var path2 = (ISegmentCollection)Factory.CreateObject<Path>();
                path2.AddSegment((ISegment)curve);
                ((IGeometryCollection)expected).AddGeometry((IGeometry)path2);

                var sut = new TestGeometryGeoJsonConverter(new GeoJsonSerializerSettings
                {
                    SerializerHasSideEffects = true,
                    Simplify = true
                });
                var actual = sut.TestPrepareGeometry(expected);

                Assert.Same(expected, actual);
            }
        }

        private class TestGeometryGeoJsonConverter : GeometryGeoJsonConverter
        {
            public TestGeometryGeoJsonConverter(GeoJsonSerializerSettings serializerSettings)
                : base(serializerSettings)
            {
            }

            public IPolyline TestPrepareGeometry(IPolyline value)
            {
                return (IPolyline) PrepareGeometry(value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }
    }
}
