using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;
using Xunit;

namespace ArcObjectJsonConverters.Tests.GeoJsonToArcObjects
{
    public class LineStringToPointClassTests
    {
        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void Throws(GeometryGeoJsonConverter sut)
        {
            var geoJson = @"{
  ""type"": ""LineString"",
  ""coordinates"": [
    [100.0, 0.0],
    [101.0, 1.0]
  ]
}";
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<IPolyline>(geoJson, sut));
        }
    }
}