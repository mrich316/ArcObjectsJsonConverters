using System;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public class PolylineGeoJsonConverter : BaseGeoJsonConverter
    {
        private readonly GeoJsonSerializerSettings _serializerSettings;

        public PolylineGeoJsonConverter()
            :this(new GeoJsonSerializerSettings())
        {
        }

        public PolylineGeoJsonConverter(GeoJsonSerializerSettings serializerSettings) 
            : base(serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geometry = (IPolyline) PrepareGeometry(value);

            if (geometry == null || geometry.IsEmpty)
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
                    WriteLineStringArray(writer, (IPointCollection) collection.Geometry[i], serializer);
                }

                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("type");

                var points = (IPointCollection) geometry;
                if (points.PointCount == 1)
                {
                    writer.WriteValue("Point");

                    writer.WritePropertyName("coordinates");
                    WritePositionArray(writer, points.Point[0], serializer);
                }
                else
                {
                    writer.WriteValue("LineString");

                    writer.WritePropertyName("coordinates");
                    WriteLineStringArray(writer, points, serializer);
                }
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

        protected override object PrepareGeometry(object value)
        {
            if (value == null) return null;

            var hasNonLinearSegments = false;
            ((ISegmentCollection) value).HasNonLinearSegments(ref hasNonLinearSegments);

            var geometry = !_serializerSettings.SerializerHasSideEffects && _serializerSettings.Simplify && hasNonLinearSegments
                ? (IPolyline) ((IClone) value).Clone()
                : (IPolyline) value;

            if (_serializerSettings.Simplify)
            {
                var topo = (ITopologicalOperator2) geometry;
                topo.IsKnownSimple_2 = false;
                topo.Simplify();

                geometry.Generalize(_serializerSettings.Tolerance);
            }
            else if (hasNonLinearSegments)
            {
                // TODO: Simplify = false: we should not generalize the geometry, just its curved parts.
                // We should do this, because Generalize might return only a subset of points if they
                // fit in the tolerance given.
                geometry.Generalize(_serializerSettings.Tolerance);
            }

            return geometry;
        }

        private void WriteLineStringArray(JsonWriter writer, IPointCollection lineString, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0, n = lineString.PointCount; i < n; i++)
            {
                WritePositionArray(writer, lineString.Point[i], serializer);
            }

            writer.WriteEndArray();
        }

    }
}
