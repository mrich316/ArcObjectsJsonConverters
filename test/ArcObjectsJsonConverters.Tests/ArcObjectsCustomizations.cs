using System.Collections.Generic;
using ArcObjectConverters;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
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

        }
    }
}