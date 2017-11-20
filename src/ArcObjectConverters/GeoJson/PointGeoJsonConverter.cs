using System;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public class PointGeoJsonConverter : JsonConverter
    {
        private readonly int _coordinatesPrecision;

        public PointGeoJsonConverter()
            : this(GeoJsonDefaults.CoordinatesPrecision)
        {
        }

        /// <param name="coordinatesPrecision">Number of digits to keep during serialization</param>
        public PointGeoJsonConverter(int coordinatesPrecision)
        {
            if (coordinatesPrecision < 0) throw new ArgumentOutOfRangeException(nameof(coordinatesPrecision));

            _coordinatesPrecision = coordinatesPrecision;
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
