namespace ArcObjectConverters
{
    public class GeoJsonSerializerSettings
    {
        /// <summary>
        /// Setting <see cref="ForceMultiGeometry"/> to  <c>true</c> will make the serializer's output
        /// only Multi* types for <see cref="ESRI.ArcGIS.Geometry.IPolyline"/>,
        /// <see cref="ESRI.ArcGIS.Geometry.IPolygon"/> and <see cref="ESRI.ArcGIS.Geometry.IMultipoint"/>, 
        /// even if they only contain a single part;
        /// <c>false</c> will ajust the serializer's output to the number of actual parts in the input geometries.
        /// So single-part geometries will be serialized as <c>Point</c>, <c>LineString</c> or <c>Polygon</c> and
        /// real multi-parts will be serialized to <c>MultiPoint</c>, <c>MultiLineString</c> or <c>MultiPolygon</c>.
        /// </summary>
        public bool ForceMultiGeometry { get; set; }

        public bool Simplify { get; set; }

        public int Precision { get; set; } = 6;

        public double Tolerance { get; set; } = 0.001;

        public DimensionHandling Dimensions { get; set; }

        /// <summary>
        /// Default <c>Z</c> value for json output with Z <see cref="Dimensions"/>. 
        /// </summary>
        public double DefaultZValue { get; set; } = 0.0;

        /// <summary>
        /// ADVANCED: Speed optimization, will skip cloning if the serializer can alter geometries before serialization. 
        /// </summary>
        public bool SerializerHasSideEffects { get; set; }
    }
}
