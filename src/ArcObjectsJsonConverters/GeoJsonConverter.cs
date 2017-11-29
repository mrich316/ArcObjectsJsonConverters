using System;
using System.Collections.Generic;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters
{
    public class GeoJsonConverter : JsonConverter
    {
        private readonly GeoJsonSerializerSettings _serializerSettings;

        private static readonly List<Type> SupportedTypes = new List<Type>
        {
            typeof(IGeometry),
            typeof(PointClass),
            typeof(IPoint),
            typeof(IPolyline),
            typeof(PolylineClass),
            typeof(IMultipoint),
            typeof(MultipointClass)
        };

        public GeoJsonConverter()
            : this(new GeoJsonSerializerSettings())
        {
        }

        public GeoJsonConverter(GeoJsonSerializerSettings serializerSettings)
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
                    throw new NotImplementedException($"{geometry.GeometryType} is not implemented.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return SupportedTypes.Contains(objectType);
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
    }
}
