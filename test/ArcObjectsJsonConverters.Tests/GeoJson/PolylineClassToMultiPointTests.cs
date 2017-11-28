using System.Text;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PolylineClassToMultiPointTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void ManySinglePointsReturnsMultiPoint(GeoJsonConverter sut, IPolyline polyline, IPoint[] points)
        {
            polyline.SetEmpty();

            var collection = (IGeometryCollection) polyline;
            var sb = new StringBuilder();

            foreach (var point in points)
            {
                var path = (IPointCollection) _factory.CreateObject<Path>();
                path.AddPoint(point);

                collection.AddGeometry((IGeometry) path);

                sb.Append($"[{point.X.ToJsonString()}, {point.Y.ToJsonString()}],");
            }

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""MultiPoint"",
  ""coordinates"": [ {sb.ToString(0, sb.Length - 1)}
  ]
}}";

            JsonAssert.Equal(expected, actual);

        }
    }
}
