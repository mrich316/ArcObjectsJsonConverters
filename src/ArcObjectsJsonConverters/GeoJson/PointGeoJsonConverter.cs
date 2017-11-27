using System;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public class PointGeoJsonConverter : BaseGeoJsonConverter
    {
        public PointGeoJsonConverter()
        {
        }

        public PointGeoJsonConverter(GeoJsonSerializerSettings serializerSettings) 
            : base(serializerSettings)
        {
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geometry = (IPoint) PrepareGeometry(value);

            if (geometry == null || geometry.IsEmpty)
            {
                writer.WriteNull();
            }
            else
            {
                WritePointObject(writer, geometry, serializer);
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
