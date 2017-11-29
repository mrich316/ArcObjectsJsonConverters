using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class MultipointClassToMultiPointTests
    {
        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyPointsAreRemoved(GeoJsonConverter sut, IMultipoint multiPoint, IPoint point1, IPoint point2, IPoint emptyPoint)
        {
            emptyPoint.SetEmpty();
            multiPoint.SetEmpty();
            ((IPointCollection)multiPoint).AddPoint(emptyPoint);
            ((IPointCollection)multiPoint).AddPoint(point1);
            ((IPointCollection)multiPoint).AddPoint(emptyPoint);
            ((IPointCollection)multiPoint).AddPoint(point2);
            ((IPointCollection)multiPoint).AddPoint(emptyPoint);

            var actual = JsonConvert.SerializeObject(multiPoint, sut);
            var expected = $@"{{
  ""type"": ""MultiPoint"",
  ""coordinates"": [
    [
      {point1.X.ToJsonString()},
      {point1.Y.ToJsonString()}
    ],
    [
      {point2.X.ToJsonString()},
      {point2.Y.ToJsonString()}
    ]
  ]
}}";
            JsonAssert.Equal(expected, actual);
        }
    }
}
