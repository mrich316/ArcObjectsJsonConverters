using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;

namespace ArcObjectJsonConverters.Tests
{
    public class ArcObjectsConventions : InlineAutoDataAttribute
    {
        public ArcObjectsConventions(int wkid, params object[] values)
            : base(CustomizeFixture(wkid), values)
        {
        }

        private static AutoDataAttribute CustomizeFixture(int wkid)
        {
            return new AutoDataAttribute(
                new Fixture()
                    .Customize(new ArcObjectsCustomizations(wkid)));
                    //.Customize(new AutoMoqCustomization());
        }
    }
}
