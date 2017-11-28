using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PolylineClassToPointTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void SinglePointReturnsPoint(GeoJsonSerializerSettings serializerSettings, IPoint point)
        {
            serializerSettings.Simplify = false;

            var polylineWithoutToPoint = (IPolyline) _factory.CreateObject<Polyline>();
            polylineWithoutToPoint.FromPoint = point;

            var sut = new GeoJsonConverter(serializerSettings);
            var actual = JsonConvert.SerializeObject(polylineWithoutToPoint, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""Point"",
  ""coordinates"": [
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()}
  ]
}}";

            JsonAssert.Equal(expected, actual);

        }
    }
}
