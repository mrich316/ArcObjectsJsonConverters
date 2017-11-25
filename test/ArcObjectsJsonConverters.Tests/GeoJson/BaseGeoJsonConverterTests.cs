using System;
using ArcObjectConverters;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class BaseGeoJsonConverterTests
    {
        [ArcObjectsFact]
        public void CtorThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>("serializerSettings", () => new TestGeoJsonConverter(null));
        }

        public class GetOrCloneGeometry
        {
            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void WithoutSideEffectsReturnSameInstance(IPoint expected)
            {
                var sut = new TestGeoJsonConverter(new GeoJsonSerializerSettings {SerializerHasSideEffects = false});

                var actual = sut.TestGetOrCloneGeometry<IPoint>(expected);

                Assert.NotEqual(expected, actual);
            }

            [ArcObjectsTheory, ArcObjectsConventions(32188)]
            public void WithSideEffectsReturnSameInstance(IPoint expected)
            {
                var sut = new TestGeoJsonConverter(new GeoJsonSerializerSettings { SerializerHasSideEffects = true });

                var actual = sut.TestGetOrCloneGeometry<IPoint>(expected);

                Assert.Same(expected, actual);
            }
        }

        private class TestGeoJsonConverter : BaseGeoJsonConverter
        {
            public TestGeoJsonConverter(GeoJsonSerializerSettings serializerSettings)
                : base(serializerSettings)
            {
            }

            public T TestGetOrCloneGeometry<T>(object value)
            {
                return GetOrCloneGeometry<T>(value);
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
