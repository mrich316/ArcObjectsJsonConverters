using System;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJsonToArcObjects
{
    public class PointToPointTests
    {
        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, typeof(IPoint))]
        [ArcObjectsConventions(32188, typeof(Point))]
        [ArcObjectsConventions(32188, typeof(PointClass))]
        public void PointReturnsPoint(Type type, GeometryGeoJsonConverter sut, double x, double y)
        {
            var geoJson = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [{x.ToJsonString()}, {y.ToJsonString()}]
}}";

            var actual = (IPoint)JsonConvert.DeserializeObject(geoJson, type, sut);

            Assert.Equal(x, actual.X);
            Assert.Equal(y, actual.Y);
        }

        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, typeof(IPoint))]
        [ArcObjectsConventions(32188, typeof(Point))]
        [ArcObjectsConventions(32188, typeof(PointClass))]
        public void PointReturnsPoint3D(Type type, GeometryGeoJsonConverter sut, double x, double y, double z)
        {
            var geoJson = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [{x.ToJsonString()}, {y.ToJsonString()}, {z.ToJsonString()}]
}}";

            var actual = (IPoint)JsonConvert.DeserializeObject(geoJson, type, sut);

            Assert.Equal(x, actual.X);
            Assert.Equal(y, actual.Y);
            Assert.Equal(z, actual.Z);
            Assert.True(((IZAware)actual).ZAware);
        }

        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, typeof(IPoint))]
        [ArcObjectsConventions(32188, typeof(Point))]
        [ArcObjectsConventions(32188, typeof(PointClass))]
        public void PointReturnsDefaultZValue(Type type, double x, double y, double defaultZValue)
        {
            var sut = new GeometryGeoJsonConverter(new GeoJsonSerializerSettings
            {
                DefaultZValue = defaultZValue,
                Dimensions = DimensionHandling.XYZ
            });

            var geoJson = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [{x.ToJsonString()}, {y.ToJsonString()}]
}}";

            var actual = (IPoint)JsonConvert.DeserializeObject(geoJson, type, sut);

            Assert.Equal(x, actual.X);
            Assert.Equal(y, actual.Y);
            Assert.Equal(defaultZValue, actual.Z);
            Assert.True(((IZAware)actual).ZAware);
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void InvalidObjectThrows(GeometryGeoJsonConverter sut)
        {
            var geoJson = @"[""wrong thing here""]";
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<IPoint>(geoJson, sut));
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void InvalidTypeThrows(GeometryGeoJsonConverter sut)
        {
            var geoJson = @"{
  ""type"": ""wrong"",
  ""coordinates"": ""wrong thing here""
}";

            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<IPoint>(geoJson, sut));
        }

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void InvalidCoordinatesThrows(GeometryGeoJsonConverter sut)
        {
            var geoJson = @"{
  ""type"": ""Point"",
  ""coordinates"": ""wrong thing here""
}";

            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<IPoint>(geoJson, sut));
        }

        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\"}")]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": []}")]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": null}")]
        public void NullCoordinatesReturnsEmptyPoint(string geoJson, GeometryGeoJsonConverter sut)
        {
            var point = JsonConvert.DeserializeObject<IPoint>(geoJson, sut);

            Assert.True(point.IsEmpty);
        }

        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": [\"a\", false]}")]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": [123]}")]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": [123, \"a\"]}")]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": [123, 123, \"a\"]}")]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": [\"a\", 123]}")]
        public void InvalidCoordinatesValueThrows(string geoJson, GeometryGeoJsonConverter sut)
        {
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<IPoint>(geoJson, sut));
        }

        [ArcObjectsTheory]
        [ArcObjectsConventions(32188, "{\"type\": \"Point\", \"coordinates\": [true, false]}")]
        public void BooleanCoordinatesValueReturns(string geoJson, GeometryGeoJsonConverter sut)
        {
            // Not sure what to do with boolean values.
            // --
            // The test is only there to document the current behavior.
            // If it breaks in the futur, we'll think how to manage it, but
            // it really is a dark and pretty limited edge case.
            var point = JsonConvert.DeserializeObject<IPoint>(geoJson, sut);
            Assert.Equal(1, point.X);
            Assert.Equal(0, point.Y);
        }
    }
}
