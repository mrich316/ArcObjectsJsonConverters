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
            : this(GeoJsonDefaults.CoordinatesPrecision, 0.2, false)
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

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var geometry = _supportsSideEffects
                ? (IPolyline) value
                : (IPolyline) ((IClone) value).Clone();

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
                    WriteCoordinatesArray(writer, (IPointCollection)collection.Geometry[i], serializer);
                }

                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("type");
                writer.WriteValue("LineString");

                writer.WritePropertyName("coordinates");
                WriteCoordinatesArray(writer, (IPointCollection) geometry, serializer);
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

        private void WriteCoordinatesArray(JsonWriter writer, IPointCollection collection, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0, n = collection.PointCount; i < n; i++)
            {
                var point = collection.Point[i];

                writer.WriteStartArray();
                writer.WriteValue(Math.Round(point.X, _coordinatesPrecision));
                writer.WriteValue(Math.Round(point.Y, _coordinatesPrecision));

                if (((IZAware)point).ZAware)
                {
                    writer.WriteValue(Math.Round(point.Z, _coordinatesPrecision));
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

    }
}
