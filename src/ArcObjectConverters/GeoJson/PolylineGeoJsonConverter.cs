using System;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public class PolylineGeoJsonConverter : JsonConverter
    {
        private readonly int _coordinatesPrecision;
        private readonly double _maxAllowedOffset;
        private readonly bool _supportsSideEffects;

        public PolylineGeoJsonConverter()
            : this(6, 0.1, false)
        {
        }

        /// <param name="coordinatesPrecision">Number of digits to keep during serialization</param>
        /// <param name="maxAllowedOffset">GeoJson does not support curves. Geometries will be generalized.
        /// </param>
        /// <param name="supportsSideEffects">
        /// <see cref="IGeometry"/> operations like <see cref="IGeometry.Project(ISpatialReference)"/> can
        /// have side effets (altering the input object). If <c>true</c>, geometries will not be cloned,
        /// increasing performance, if <c>false</c>, no side effects will happen, at a cost of lower
        /// performance.
        /// </param>
        public PolylineGeoJsonConverter(int coordinatesPrecision, double maxAllowedOffset, bool supportsSideEffects)
        {
            if (coordinatesPrecision < 0) throw new ArgumentOutOfRangeException(nameof(coordinatesPrecision));
            if (maxAllowedOffset < 0) throw new ArgumentOutOfRangeException(nameof(maxAllowedOffset));

            _coordinatesPrecision = coordinatesPrecision;
            _maxAllowedOffset = maxAllowedOffset;
            _supportsSideEffects = supportsSideEffects;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geometry = _supportsSideEffects
                ? (IPolyline) value
                : (IPolyline) ((IClone) value).Clone();

            if (geometry == null)
            {
                writer.WriteNull();
                return;
            }

            // The GeoJson spec does not support true curves.
            geometry.Generalize(_maxAllowedOffset);

            // Make sure the geometry is "valid".
            ((ITopologicalOperator2) geometry).IsKnownSimple_2 = false;
            geometry.SimplifyNetwork();

            if (geometry.IsEmpty)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            var collection = (IGeometryCollection) geometry;

            writer.WritePropertyName("type");
            writer.WriteValue(collection.GeometryCount > 1
                ? "MultiLineString"
                : "LineString");

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

    }
}
