using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.ArcObjectsToGeoJson
{
    public class MultipointClassToMultiPointTests
    {
        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void EmptyPointsAreRemoved(GeometryGeoJsonConverter sut, IMultipoint multiPoint, IPoint point1, IPoint point2, IPoint emptyPoint)
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
            public void PointReturnsMultiPoint(IMultipoint multiPoint, IPoint point)
            {
                multiPoint.SetEmpty();
                ((IPointCollection) multiPoint).AddPoint(point);

                var actual = JsonConvert.SerializeObject(multiPoint, Formatting.Indented, _sut);
                var expected = $@"{{
  ""type"": ""MultiPoint"",
  ""coordinates"": [[
    {point.X.ToJsonString()},
    {point.Y.ToJsonString()}
  ]]
}}";

                JsonAssert.Equal(expected, actual);
            }
        }
    }
}
