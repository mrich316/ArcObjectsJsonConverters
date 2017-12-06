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
                    WritePoint(writer, (IPoint) value, serializer);
                    break;

                case esriGeometryType.esriGeometryPolyline:
                    var polyline = (IPolyline)PrepareGeometry((IPolycurve)value);
                    WriteMultiLineString(writer, polyline, serializer);
                    break;

                case esriGeometryType.esriGeometryPolygon:
                    var polygon = (IPolygon)PrepareGeometry((IPolycurve)value);
                    WriteMultiPolygon(writer, polygon, serializer);
                    break;

                case esriGeometryType.esriGeometryMultipoint:
                    var multipoint = (IMultipoint) PrepareGeometry((IMultipoint) value);
                    WriteMultiPoint(writer, multipoint, serializer);
                    break;

                default:
                    throw new JsonSerializationException(
                        $"{geometry.GeometryType} not supported by this implementation.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var json = JObject.Load(reader, _loadSettings);

            // Type is mandatory.
            var type = json["type"];
            if (type == null || type.Type != JTokenType.String)
            {
                throw CreateJsonReaderException(type ?? json,
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

            IGeometry geometry;

            switch (type.Value<string>())
            {
                case "Point":
                    geometry = ReadPoint((JArray) coordinates, objectType, existingValue, serializer);
                    break;

                case "LineString":
                    geometry = ReadLineString((JArray) coordinates, objectType, existingValue, serializer);
                    break;

                case "Polygon":
                    geometry = ReadPolygon((JArray) coordinates, objectType, existingValue, serializer);
                    break;

                case "MultiLineString":
                    geometry = ReadMultiLineString((JArray) coordinates, objectType, existingValue, serializer);
                    break;

                case "MultiPolygon":
                    throw new JsonSerializationException(
                        $"GeoJSON object of type \"{type}\" is not supported by this implementation.");

                case "MultiPoint":
                    geometry = ReadMultiPoint((JArray)coordinates, objectType, existingValue, serializer);
                    break;

                default:
                    throw new JsonSerializationException(
                        $"GeoJSON object of type \"{type}\" is not supported by this implementation.");
            }

            if (_serializerSettings.Dimensions == DimensionHandling.XYZ)
            {
                ((IZAware) geometry).ZAware = true;
            }

            if (_serializerSettings.Simplify)
            {
                ((ITopologicalOperator) geometry).Simplify();
            }

            return geometry;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(PointClass))
                   || objectType.IsAssignableFrom(typeof(PolylineClass))
                   || objectType.IsAssignableFrom(typeof(PolygonClass))
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
            ((ISegmentCollection) value).HasNonLinearSegments(ref hasNonLinearSegments);

            var geometry = !_serializerSettings.SerializerHasSideEffects && _serializerSettings.Simplify &&
                           hasNonLinearSegments
                ? (IPolycurve) ((IClone) value).Clone()
                : value;

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
                ? (IMultipoint) ((IClone) value).Clone()
                : value;

            if (_serializerSettings.Simplify)
            {
                var topo = (ITopologicalOperator2) geometry;
                topo.IsKnownSimple_2 = false;
                topo.Simplify();
            }

            return geometry;
        }

        protected IGeometry ReadPolygon(JArray coordinates, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var polygon = (IPolygon) existingValue ?? new PolygonClass();

            using (var iterator = coordinates.GetEnumerator())
            {
                if (!iterator.MoveNext()) return polygon;

                var rings = (IGeometryCollection)polygon;

                var exteriorRing = (IRing) ReadPositionArray(iterator.Current, new RingClass(), serializer);
                if (!exteriorRing.IsExterior)
                {
                    exteriorRing.ReverseOrientation();
                }

                rings.AddGeometry(exteriorRing);

                while (iterator.MoveNext())
                {
                    var interiorRing = (IRing)ReadPositionArray(iterator.Current, new RingClass(), serializer);
                    if (interiorRing.IsExterior)
                    {
                        interiorRing.ReverseOrientation();
                    }

                    rings.AddGeometry(interiorRing);
                }
            }

            return polygon;
        }

        protected IGeometry ReadMultiLineString(JArray coordinates, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (!objectType.IsAssignableFrom(typeof(PolylineClass)))
            {
                throw CreateJsonReaderException(
                    coordinates.Parent,
                    $"GeoJSON object of type \"MultiLineString\" cannot be deserialized to \"{objectType.FullName}\".");
            }

            var polyline = (IPolyline) existingValue ?? new PolylineClass();

            foreach (var lineStringCoordinatesArray in coordinates)
            {
                var part = ReadPositionArray(lineStringCoordinatesArray, new PathClass(), serializer);
                ((IGeometryCollection) polyline).AddGeometry(part);
            }

            return polyline;
        }

        protected IGeometry ReadLineString(JArray coordinates, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (!objectType.IsAssignableFrom(typeof(PolylineClass)))
            {
                throw CreateJsonReaderException(
                    coordinates.Parent,
                    $"GeoJSON object of type \"LineString\" cannot be deserialized to \"{objectType.FullName}\".");
            }

            var polyline = (IPointCollection) existingValue ?? new PolylineClass();
            var geometry = ReadPositionArray(coordinates, polyline, serializer);

            return geometry;
        }

        protected IGeometry ReadPositionArray(JToken positions, IPointCollection points,
            JsonSerializer serializer)
        {
            if (positions.Type != JTokenType.Array)
            {
                throw CreateJsonReaderException(positions, "GeoJSON position array must be an array of number arrays.");
            }

            return ReadPositionArray((JArray) positions, points, serializer);
        }

        protected IGeometry ReadPositionArray(JArray positions, IPointCollection points,
            JsonSerializer serializer)
        {
            foreach (var numbers in positions)
            {
                var point = ReadPosition(numbers, new PointClass(), serializer);
                points.AddPoint(point);
            }

            return (IGeometry) points;
        }

        protected IGeometry ReadMultiPoint(JArray coordinates, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (!objectType.IsAssignableFrom(typeof(MultipointClass)))
            {
                throw CreateJsonReaderException(
                    coordinates.Parent,
                    $"GeoJSON object of type \"MultiPoint\" cannot be deserialized to \"{objectType.FullName}\".");
            }

            var pointCollection = (IPointCollection)existingValue ?? new MultipointClass();

            foreach (var position in coordinates)
            {
                var point = ReadPosition(position, new PointClass(), serializer);
                pointCollection.AddPoint(point);
            }

            return (IGeometry)pointCollection;
        }

        protected IGeometry ReadPoint(JArray coordinates, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (objectType.IsAssignableFrom(typeof(PointClass)))
            {
                return ReadPosition(coordinates, (IPoint) existingValue ?? new PointClass(), serializer);
            }

            IPointCollection pointCollection;

            if (objectType.IsAssignableFrom(typeof(MultipointClass)))
            {
                pointCollection = (IPointCollection) existingValue ?? new MultipointClass();
            }
            else if (objectType.IsAssignableFrom(typeof(PolylineClass)))
            {
                pointCollection = (IPointCollection) existingValue ?? new PolylineClass();
            }
            else if (objectType.IsAssignableFrom(typeof(PolygonClass)))
            {
                pointCollection = (IPointCollection) existingValue ?? new PolygonClass();
            }
            else
            {
                throw CreateJsonReaderException(
                    coordinates.Parent,
                    $"GeoJSON object of type \"Point\" cannot be deserialized to \"{objectType.FullName}\".");
            }

            var point = ReadPosition(coordinates, new PointClass(), serializer);
            pointCollection.AddPoint(point);

            return (IGeometry) pointCollection;
        }

        protected IPoint ReadPosition(JToken numbers, IPoint position, JsonSerializer serializer)
        {
            if (numbers.Type != JTokenType.Array)
            {
                throw CreateJsonReaderException(numbers, "GeoJSON position must be an array of numbers.");
            }

            return ReadPosition((JArray) numbers, position, serializer);
        }

        protected IPoint ReadPosition(JArray numbers, IPoint position, JsonSerializer serializer)
        {
            if (numbers.Count == 0)
            {
                return position;
            }

            if (numbers.Count < 2)
            {
                throw CreateJsonReaderException(numbers,
                    "GeoJSON position must contain at least two numbers for a Point.");
            }

            try
            {
                position.X = numbers[0].Value<double>();
            }
            catch (Exception pointException)
            {
                throw CreateJsonReaderException(numbers[0], "Longitude (or Easting) could not be read.",
                    pointException);
            }

            try
            {
                position.Y = numbers[1].Value<double>();
            }
            catch (Exception pointException)
            {
                throw CreateJsonReaderException(numbers[1], "Latitude (or Northing) could not be read.",
                    pointException);
            }

            // TODO: Handle other dimensions: ie: M.

            if (numbers.Count > 2)
            {
                try
                {
                    position.Z = numbers[2].Value<double>();
                    ((IZAware) position).ZAware = true;
                }
                catch (Exception pointException)
                {
                    throw CreateJsonReaderException(numbers[2], "Altitude could not be read.", pointException);
                }
            }
            else if (_serializerSettings.Dimensions == DimensionHandling.XYZ)
            {
                position.Z = _serializerSettings.DefaultZValue;
                ((IZAware) position).ZAware = true;
            }

            return position;
        }

        protected void WritePosition(JsonWriter writer, IPoint value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.X, _serializerSettings.Precision));
            writer.WriteValue(Math.Round(value.Y, _serializerSettings.Precision));

            if (_serializerSettings.Dimensions == DimensionHandling.XYZ)
            {
                var zAware = (IZAware) value;
                var z = !zAware.ZAware || double.IsNaN(value.Z)
                    ? _serializerSettings.DefaultZValue
                    : Math.Round(value.Z, _serializerSettings.Precision);

                writer.WriteValue(z);
            }

            writer.WriteEndArray();
        }

        protected void WritePoint(JsonWriter writer, IPoint point, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("Point");

            writer.WritePropertyName("coordinates");
            WritePosition(writer, point, serializer);

            writer.WriteEndObject();
        }

        protected void WriteMultiPoint(JsonWriter writer, IMultipoint multiPoint, JsonSerializer serializer)
        {
            var pointCollection = (IPointCollection) multiPoint;
            var points = new List<IPoint>(pointCollection.PointCount);
            for (int i = 0, n = pointCollection.PointCount; i < n; i++)
            {
                var point = pointCollection.Point[i];
                if (point.IsEmpty) continue;

                points.Add(point);
            }

            WriteMultiPoint(writer, points, serializer);
        }

        protected void WriteMultiPoint(JsonWriter writer, List<IPoint> points, JsonSerializer serializer)
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
                    WritePosition(writer, point, serializer);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            else if (points.Count == 1)
            {
                WritePoint(writer, points[0], serializer);
            }
            else
            {
                writer.WriteNull();
            }
        }

        protected void WritePositionArray(JsonWriter writer, IPointCollection lineString,
            JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0, n = lineString.PointCount; i < n; i++)
            {
                WritePosition(writer, lineString.Point[i], serializer);
            }

            writer.WriteEndArray();
        }

        protected void WriteMultiLineString(JsonWriter writer, IPolyline polyline, JsonSerializer serializer)
        {
            var collection = (IGeometryCollection) polyline;
            var paths = new List<IPointCollection>(collection.GeometryCount);

            for (int i = 0, n = collection.GeometryCount; i < n; i++)
            {
                var path = (IPath) collection.Geometry[i];
                var pathPoints = (IPointCollection) path;

                // Skip incomplete path (a single point) or zero-length segments.
                if (pathPoints.PointCount > 1 && path.Length > _serializerSettings.Tolerance)
                {
                    paths.Add(pathPoints);
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
                    WritePositionArray(writer, path, serializer);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else if (paths.Count == 1)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("type");
                writer.WriteValue("LineString");

                writer.WritePropertyName("coordinates");
                WritePositionArray(writer, paths[0], serializer);

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }

        protected void WriteMultiPolygon(JsonWriter writer, IPolygon polygon, JsonSerializer serializer)
        {
            var exteriorRingBag = (IGeometryCollection) ((IPolygon4) polygon).ExteriorRingBag;
            var exteriorRings = new List<IRing>();

            for (int i = 0, n = exteriorRingBag.GeometryCount; i < n; i++)
            {
                var ring = (IRing) exteriorRingBag.Geometry[i];
                var pointCollection = (IPointCollection)ring;

                if (ring.IsClosed && pointCollection.PointCount > 3 && ring.Length > _serializerSettings.Tolerance)
                {
                    exteriorRings.Add(ring);
                }
            }

            if (exteriorRings.Count > 1)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("MultiPolygon");

                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();

                foreach (var exteriorRing in exteriorRings)
                {
                    var interiorRings = GetInteriorRings((IPolygon4)polygon, exteriorRing);

                    WritePolygonCoordinates(writer, exteriorRing, interiorRings, serializer);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else if (exteriorRings.Count == 1)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue("Polygon");

                var exteriorRing = exteriorRings[0];
                var interiorRings = GetInteriorRings((IPolygon4) polygon, exteriorRing);

                writer.WritePropertyName("coordinates");
                WritePolygonCoordinates(writer, exteriorRing, interiorRings, serializer);

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }

        protected void WritePolygonCoordinates(JsonWriter writer, IRing exteriorRing, IEnumerable<IRing> interiorRings,
            JsonSerializer serializer)
        {
            writer.WriteStartArray();

            WriteLinearRing(writer, exteriorRing, serializer);

            // Serialize interior rings (holes), if present.
            foreach (var interiorRing in interiorRings)
            {
                WriteLinearRing(writer, interiorRing, serializer);
            }

            writer.WriteEndArray();
        }

        protected void WriteLinearRing(JsonWriter writer, IRing ring, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            var pointCollection = (IPointCollection)ring;

            // For ESRI, it's the left hand rule: exterior ring is CLOCKWISE.
            // Loop in reverse to match the right hand rule.
            var i = pointCollection.PointCount;
            while (i >= 0)
            {
                WritePosition(writer, pointCollection.Point[--i], serializer);
            }

            writer.WriteEndArray();
        }

        protected IEnumerable<IRing> GetInteriorRings(IPolygon4 polygon, IRing exteriorRing)
        {
            var interiorRings = (IGeometryCollection) polygon.InteriorRingBag[exteriorRing];

            for (int i = 0, n = interiorRings.GeometryCount; i < n; i++)
            {
                var ring = (IRing) interiorRings.Geometry[i];
                var pointCollection = (IPointCollection) ring;

                if (ring.IsClosed && pointCollection.PointCount > 3 && ring.Length > _serializerSettings.Tolerance)
                {
                    yield return ring;
                }
            }
        }

        protected virtual JsonException CreateJsonReaderException(JToken token, string message,
            Exception innerException = null)
        {
            var lineInfo = (IJsonLineInfo) token;

            var exception = lineInfo == null || !lineInfo.HasLineInfo()
                ? new JsonReaderException(message, innerException)
                : new JsonReaderException(message, token.Path, lineInfo.LineNumber, lineInfo.LinePosition,
                    innerException);

            return exception;
        }
    }
}