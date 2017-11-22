using System;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public class PolylineGeoJsonConverter : BaseGeoJsonConverter
    {
        private readonly double _maxAllowedOffset;

        public PolylineGeoJsonConverter()
            : this(0.1, new GeoJsonSerializerSettings())
        {
        }

        /// <param name="maxAllowedOffset">GeoJson does not support curves. Geometries will be generalized.
        /// </param>
        public PolylineGeoJsonConverter(double maxAllowedOffset)
            : this(maxAllowedOffset, new GeoJsonSerializerSettings())
        {
            if (maxAllowedOffset < 0) throw new ArgumentOutOfRangeException(nameof(maxAllowedOffset));

            _maxAllowedOffset = maxAllowedOffset;
        }

        protected PolylineGeoJsonConverter(double maxAllowedOffset, GeoJsonSerializerSettings serializerSettings) 
            : base(serializerSettings)
        {
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var geometry = GetOrCloneGeometry<IPolyline>(value);

            var topoOperator = (ITopologicalOperator2) geometry;

            // Make sure the geometry is "valid".
            // Helps to generalize invalid shapes by first cleaning/correcting them.
            topoOperator.IsKnownSimple_2 = false;
            topoOperator.Simplify();

            // The GeoJson spec does not support true curves.
            geometry.Generalize(_maxAllowedOffset);

            // Make sure the geometry is "valid" after generalize.
            topoOperator.IsKnownSimple_2 = false;
            topoOperator.Simplify();

            if (geometry.IsEmpty)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            var collection = (IGeometryCollection) geometry;

            if (collection.GeometryCount > 1)
            {
                writer.WritePropertyName("type");
                writer.WriteValue("MultiLineString");

                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();

                for (int i = 0, n = collection.GeometryCount; i < n; i++)
                {
                    WriteLineStringArray(writer, (IPointCollection)collection.Geometry[i], serializer);
                }

                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("type");
                writer.WriteValue("LineString");

                writer.WritePropertyName("coordinates");
                WriteLineStringArray(writer, (IPointCollection) geometry, serializer);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(PolylineClass) == objectType;
        }

        private void WriteLineStringArray(JsonWriter writer, IPointCollection collection, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0, n = collection.PointCount; i < n; i++)
            {
                WritePositionArray(writer, collection.Point[i], serializer);
            }

            writer.WriteEndArray();
        }

    }
}
