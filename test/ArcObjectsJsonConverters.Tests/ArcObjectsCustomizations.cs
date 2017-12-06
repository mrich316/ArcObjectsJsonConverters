using System.Linq;
using ArcObjectConverters;
using ESRI.ArcGIS.Geometry;
using Ploeh.AutoFixture;

namespace ArcObjectJsonConverters.Tests
{
    public class ArcObjectsCustomizations : ICustomization
    {
        private readonly ISpatialReference _defaultSpatialReference;

        public ArcObjectsCustomizations(int wkid)
        {
            _defaultSpatialReference = new SpatialReferenceEnvironmentClass()
                .CreateProjectedCoordinateSystem(wkid);
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new GeoJsonSerializerSettings());

            fixture.Customize<ISpatialReference>(x => x
                .FromFactory(() => _defaultSpatialReference)
                .OmitAutoProperties());

            fixture.Customize<IPoint>(x => x
                .FromFactory(() => new PointClass() as IPoint)
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<IPolyline>(x => x
                .FromFactory(() => (IPolyline) new PolylineClass())
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<ILine>(x => x
                .FromFactory(() => new LineClass() as ILine)
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<IMultipoint>(x => x
                .FromFactory(() => new MultipointClass() as IMultipoint)
                .Without(w => w.SpatialReference)
                .Do(g => g.SpatialReference = _defaultSpatialReference));

            fixture.Customize<IBezierCurve>(x => x
                .FromFactory(() =>
                {
                    var bezier = (IBezierCurveGEN) new BezierCurveClass();
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