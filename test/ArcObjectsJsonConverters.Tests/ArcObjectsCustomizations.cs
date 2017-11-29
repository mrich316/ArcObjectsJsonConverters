using System.Linq;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Ploeh.AutoFixture;
using VL.ArcObjectsApi;

namespace ArcObjectJsonConverters.Tests
{
    public class ArcObjectsCustomizations : ICustomization
    {
        private readonly ISpatialReference _defaultSpatialReference;
        private readonly IArcObjectFactory _factory;

        public ArcObjectsCustomizations(int wkid)
        {
            _factory = new ClientArcObjectFactory();

            _defaultSpatialReference = _factory.CreateObject<SpatialReferenceEnvironment>()
                .CreateProjectedCoordinateSystem(wkid);
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new GeoJsonSerializerSettings());

            fixture.Customize<ISpatialReference>(x => x
                .FromFactory(() => _defaultSpatialReference)
                .OmitAutoProperties());

            fixture.Customize<IPoint>(x => x
                .FromFactory(() => _factory.CreateObject<Point>() as IPoint)
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<IPolyline>(x => x
                .FromFactory(() => (IPolyline)_factory.CreateObject<Polyline>())
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<ILine>(x => x
                .FromFactory(() => _factory.CreateObject<Line>() as ILine)
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<IMultipoint>(x => x
                .FromFactory(() => _factory.CreateObject<Multipoint>() as IMultipoint)
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<IBezierCurve>(x => x
                .FromFactory(() =>
                {
                    var bezier = (IBezierCurveGEN) _factory.CreateObject<BezierCurve>();
                    var controlPoints = Enumerable.Range(0, 4)
                        .Select(i => fixture.Create<IPoint>())
                        .ToArray();

                    bezier.PutCoords(ref controlPoints);

                    return (IBezierCurve)bezier;
                })
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));
        }
    }
}