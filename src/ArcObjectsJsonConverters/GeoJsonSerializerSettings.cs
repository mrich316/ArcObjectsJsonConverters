using ESRI.ArcGIS.Geometry;

namespace ArcObjectConverters
{
    public class GeoJsonSerializerSettings
    {
        public int Precision { get; set; } = 6;

        public DimensionHandling Dimensions { get; set; }

        /// <summary>
        /// ADVANCED: Speed optimization, will skip cloning if the serializer can alter geometries before serialization. 
        /// </summary>
        public bool SerializerHasSideEffects { get; set; }
    }
}
