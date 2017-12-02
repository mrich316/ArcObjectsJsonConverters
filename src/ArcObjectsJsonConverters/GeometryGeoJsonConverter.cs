using System;
using System.Collections.Generic;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArcObjectConverters
{
    public class GeometryGeoJsonConverter : JsonConverter
    {
        private readonly GeoJsonSerializerSettings _serializerSettings;
        private static readonly JArray EmptyArray = new JArray();

        private readonly JsonLoadSettings _loadSettings = new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore,
            LineInfoHandling = LineInfoHandling.Load
        };

        public GeometryGeoJsonConverter()
            : this(new GeoJsonSerializerSettings())
        {
        }

        public GeometryGeoJsonConverter(GeoJsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null) throw new ArgumentNullException(nameof(serializerSettings));
            _serializerSettings = serializerSettings;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geometry = value as IGeometry;
            if (geometry == null || geometry.IsEmpty)
            {
                writer.WriteNull();
                return;
            }

            switch (geometry.GeometryType)
            {
                case esriGeometryType.esriGeometryPoint:
                    WritePointObject(writer, (IPoint) value, serializer);
                    break;

                case esriGeometryType.esriGeometryPolyline:
                    var polyline = (IPolyline) PrepareGeometry((IPolycurve) value);
                    WriteMultiLineStringObject(writer, polyline, serializer);
                    break;

                case esriGeometryType.esriGeometryMultipoint:
                    var multipoint = (IMultipoint) PrepareGeometry((IMultipoint) value);
                    WriteMultiPointObject(writer, multipoint, serializer);
                    break;

                default:
                    throw new JsonSerializationException($"{geometry.GeometryType} not supported by this implementation.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader, _loadSettings);

            // Type is mandatory.
            var type = json["type"];
            if (type == null || type.Type != JTokenType.String)
            {
                throw CreateJsonReaderException( type ?? json,
                    "GeoJSON property \"type\" is not found or its content is not a string.");
            }

            // Empty or missing coordinates is tolerated.
            var coordinates = json["coordinates"];
            if (coordinates == null || coordinates.Type == JTokenType.Null)
            {
                coordinates = EmptyArray;
            }
            else if (coordinates.Type != JTokenType.Array)
            {
                throw CreateJsonReaderException(coordinates, "GeoJSON property \"coordinates\" is not an array.");
            }

            switch (type.Value<string>())
            {
                case "Point":
                    return ReadJsonPoint(coordinates, objectType, existingValue, serializer);

                case "LineString":
                case "MultiLineString":
                case "MultiPoint":
                default:
                    throw new JsonSerializationException($"GeoJSON object of type \"{type}\" is not supported by this implementation.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(PointClass))
                   || objectType.IsAssignableFrom(typeof(PolylineClass))
                   || objectType.IsAssignableFrom(typeof(MultipointClass));
        }

        /// <summary>
        /// Prepare the geometry (or a copy of itself) to be serialized. Depending on <see cref="GeoJsonSerializerSettings"/>,
        /// the geometry might be altered, cloned and generalized by this function.
        ///  
        /// <see cref="IGeometry"/> operations like <see cref="IGeometry.Project(ISpatialReference)"/> can
        /// have side effets (altering the input object). If <c>true</c>, geometries will not be cloned,
        /// increasing performance, if <c>false</c>, no side effects will happen, at a cost of lower
        /// performance.
        /// </summary>
        protected virtual IGeometry PrepareGeometry(IPolycurve value)
        {
            if (value == null) return null;

            var hasNonLinearSegments = false;
            ((ISegmentCollection)value).HasNonLinearSegments(ref hasNonLinearSegments);

            var geometry = !_serializerSettings.SerializerHasSideEffects && _serializerSettings.Simplify && hasNonLinearSegments
                ? (IPolycurve)((IClone)value).Clone()
                : value;

            if (_serializerSettings.Simplify)
            {
                var topo = (ITopologicalOperator2)geometry;
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

        /// <summary>
        /// Prepare the geometry (or a copy of itself) to be serialized. Depending on <see cref="GeoJsonSerializerSettings"/>,
        /// the geometry might be altered, cloned and generalized by this function.
        ///  
        /// <see cref="IGeometry"/> operations like <see cref="IGeometry.Project(ISpatialReference)"/> can
        /// have side effets (altering the input object). If <c>true</c>, geometries will not be cloned,
        /// increasing performance, if <c>false</c>, no side effects will happen, at a cost of lower
        /// performance.
        /// </summary>
        protected virtual IGeometry PrepareGeometry(IMultipoint value)
        {
            if (value == null) return null;

            var geometry = !_serializerSettings.SerializerHasSideEffects && _serializerSettings.Simplify
                ? (IMultipoint)((IClone)value).Clone()
                : value;

            if (_serializerSettings.Simplify)
            {
                var topo = (ITopologicalOperator2)geometry;
                topo.IsKnownSimple_2 = false;
                topo.Simplify();
            }

            return geometry;
        }

        protected object ReadJsonPoint(JToken coordinates, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (objectType.IsAssignableFrom(typeof(PointClass)))
            {
                return ReadPositionArray((JArray) coordinates, (IPoint) existingValue, serializer);
            }

            IPointCollection pointCollection = null;

            if (objectType.IsAssignableFrom(typeof(MultipointClass)))
            {
                pointCollection = (IPointCollection) existingValue ?? new MultipointClass();
            }
            else if (objectType.IsAssignableFrom(typeof(PolylineClass)))
            {
                pointCollection = (IPointCollection) existingValue ?? new PolylineClass();
            }
            else
            {
                throw CreateJsonReaderException(
                    coordinates.Parent,
                    $"GeoJSON object of type \"Point\" cannot be deserialized to \"{objectType.FullName}\".");
            }

            var point = ReadPositionArray((JArray) coordinates, null, serializer);
            pointCollection.AddPoint(point);

            if (_serializerSettings.Simplify)
            {
                ((ITopologicalOperator) pointCollection).Simplify();
            }

            return pointCollection;
        }

        protected IPoint ReadPositionArray(JArray coordinates, IPoint existingPoint, JsonSerializer serializer)
        {
            var point = existingPoint ?? new PointClass();

            if (coordinates.Count == 0)
            {
                return point;
            }

            if (coordinates.Count < 2)
            {
                throw CreateJsonReaderException(coordinates,
                    "GeoJSON coordinates must contain at least two positions for a Point.");
            }

            try
            {
                point.X = coordinates[0].Value<double>();
            }
            catch (Exception pointException)
            {
                throw CreateJsonReaderException(coordinates[0], "Longitude (or Easting) could not be read.", pointException);
            }

            try
            {
                point.Y = coordinates[1].Value<double>();
            }
            catch (Exception pointException)
            {
                throw CreateJsonReaderException(coordinates[1], "Latitude (or Northing) could not be read.", pointException);
            }

            // TODO: Handle other dimensions: ie: M.

            if (coordinates.Count > 2)
            {
                try
                {
                    point.Z = coordinates[2].Value<double>();
                }
                catch (Exception pointException)
                {
                    throw CreateJsonReaderException(coordinates[2], "Altitude could not be read.", pointException);
                }

                ((IZAware) point).ZAware = true;
            }
            else if (_serializerSettings.Dimensions == DimensionHandling.XYZ)
            {
                point.Z = _serializerSettings.DefaultZValue;
                ((IZAware)point).ZAware = true;
            }

            return point;
        }

        protected void WritePositionArray(JsonWriter writer, IPoint value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.X, _serializerSettings.Precision));
            writer.WriteValue(Math.Round(value.Y, _serializerSettings.Precision));

            if (_serializerSettings.Dimensions == DimensionHandling.XYZ)
            {
                var zAware = (IZAware)value;
                var z = !zAware.ZAware || double.IsNaN(value.Z)
                    ? _serializerSettings.DefaultZValue
                    : Math.Round(value.Z, _serializerSettings.Precision);

                writer.WriteValue(z);
            }

            writer.WriteEndArray();
        }

        protected void WritePointObject(JsonWriter writer, IPoint point, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("Point");

            writer.WritePropertyName("coordinates");
            WritePositionArray(writer, point, serializer);

            writer.WriteEndObject();
        }

        protected void WriteMultiPointObject(JsonWriter writer, IMultipoint multiPoint, JsonSerializer serializer)
        {
            var pointCollection = (IPointCollection)multiPoint;
            var points = new List<IPoint>(pointCollection.PointCount);
            for (int i = 0, n = pointCollection.PointCount; i < n; i++)
            {
                var point = pointCollection.Point[i];
                if (point.IsEmpty) continue;

                points.Add(point);
            }

            WriteMultiPointObject(writer, points, serializer);
        }

        protected void WriteMultiPointObject(JsonWriter writer, List<IPoint> points, JsonSerializer serializer)
        {
            if (points.Count > 1)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("type");
                writer.WriteValue("MultiPoint");

                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();
                foreach (var point in points)
                {
                    WritePositionArray(writer, point, serializer);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            else if (points.Count == 1)
            {
                WritePointObject(writer, points[0], serializer);
            }
            else
            {
                writer.WriteNull();
            }
        }

        protected void WriteLineStringCoordinatesArray(JsonWriter writer, IPointCollection lineString, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0, n = lineString.PointCount; i < n; i++)
            {
                WritePositionArray(writer, lineString.Point[i], serializer);
            }

            writer.WriteEndArray();
        }

        protected void WriteLineStringObject(JsonWriter writer, IPointCollection lineString, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("LineString");

            writer.WritePropertyName("coordinates");
            WriteLineStringCoordinatesArray(writer, lineString, serializer);

            writer.WriteEndObject();
        }

        protected void WriteMultiLineStringObject(JsonWriter writer, IPolyline polyline, JsonSerializer serializer)
        {
            var collection = (IGeometryCollection)polyline;
            var paths = new List<IPointCollection>(collection.GeometryCount);
            var points = new List<IPoint>();

            for (int i = 0, n = collection.GeometryCount; i < n; i++)
            {
                var path = (IPath)collection.Geometry[i];
                var pathPoints = (IPointCollection)path;

                // Skip incomplete path (a single point) or zero-length segments.
                if (pathPoints.PointCount > 1 && path.Length > _serializerSettings.Tolerance)
                {
                    paths.Add(pathPoints);
                }
                // It could have two points, but at a distance lower than {_serializerSettings.Tolerance},
                // so we check > 0.
                else if (pathPoints.PointCount > 0)
                {
                    for (int j = 0, m = pathPoints.PointCount; j < m; j++)
                    {
                        // Take the first non-empty point.
                        var point = pathPoints.Point[j];
                        if (point.IsEmpty) continue;

                        points.Add(point);
                        break;
                    }
                }
            }

            if (paths.Count > 1)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("MultiLineString");

                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();

                foreach (var path in paths)
                {
                    WriteLineStringCoordinatesArray(writer, path, serializer);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else if (paths.Count == 1)
            {
                WriteLineStringObject(writer, paths[0], serializer);
            }
            else if (points.Count > 1)
            {
                WriteMultiPointObject(writer, points, serializer);
            }
            else if (points.Count == 1)
            {
                // Incomplete path (it's a single point)
                WritePointObject(writer, points[0], serializer);
            }
            else
            {
                writer.WriteNull();
            }
        }

        protected virtual JsonException CreateJsonReaderException(JToken token, string message, Exception innerException = null)
        {
            var lineInfo = (IJsonLineInfo) token;

            var exception = lineInfo == null || !lineInfo.HasLineInfo()
                ? new JsonReaderException(message, innerException)
                : new JsonReaderException(message, token.Path, lineInfo.LineNumber, lineInfo.LinePosition, innerException);

            return exception;
        }
    }
}
