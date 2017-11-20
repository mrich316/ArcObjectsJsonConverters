using System;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public class PointGeoJsonConverter : JsonConverter
    {
        private readonly int _coordinatesPrecision;
        private readonly bool _supportsSideEffects;

        public PointGeoJsonConverter()
            : this(6, false)
        {
        }

        /// <param name="coordinatesPrecision">Number of digits to keep during serialization</param>
        /// <param name="supportsSideEffects">
        /// <see cref="IGeometry"/> operations like <see cref="IGeometry.Project(ISpatialReference)"/> can
        /// have side effets (altering the input object). If <c>true</c>, geometries will not be cloned,
        /// increasing performance, if <c>false</c>, no side effects will happen, at a cost of lower
        /// performance.
        /// </param>
        public PointGeoJsonConverter(int coordinatesPrecision, bool supportsSideEffects)
        {
            if (coordinatesPrecision < 0) throw new ArgumentOutOfRangeException(nameof(coordinatesPrecision));

            _coordinatesPrecision = coordinatesPrecision;
            _supportsSideEffects = supportsSideEffects;
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geometry = (IPoint) value;

            if (geometry == null || geometry.IsEmpty)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartObject();

                writer.WritePropertyName("type");
                writer.WriteValue("Point");

                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();
                writer.WriteValue(Math.Round(geometry.X, _coordinatesPrecision));
                writer.WriteValue(Math.Round(geometry.Y, _coordinatesPrecision));

                if (((IZAware)geometry).ZAware)
                {
                    writer.WriteValue(Math.Round(geometry.Z, _coordinatesPrecision));
                }

                writer.WriteEndArray();

                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(PointClass) == objectType;
        }
    }
}
