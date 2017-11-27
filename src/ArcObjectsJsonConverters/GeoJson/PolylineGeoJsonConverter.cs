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

            var collection = (IGeometryCollection) geometry;

            if (collection.GeometryCount > 1)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("MultiLineString");

                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();

                for (int i = 0, n = collection.GeometryCount; i < n; i++)
                {
                    var points = (IPointCollection) collection.Geometry[i];

                    // Skip incomplete path (a single point)
                    if (points.PointCount > 1)
                    {
                        WriteLineStringCoordinatesArray(writer, points, serializer);
                    }
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else
            {
                var points = (IPointCollection) geometry;

                if (points.PointCount == 1)
                {
                    // Incomplete path (it's a single point)
                    WritePointObject(writer, points.Point[0], serializer);
                }
                else
                {
                    WriteLineStringObject(writer, points, serializer);
                }
            }
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
                // TODO: When Simplify = false: we should not generalize the entire geometry, just its curved parts.
                // We should do this, because Generalize will return only a subset of points if they
                // fit in the tolerance given (no matter if it's a line segment or a curve).
                geometry.Generalize(_serializerSettings.Tolerance);
            }

            return geometry;
        }
    }
}
