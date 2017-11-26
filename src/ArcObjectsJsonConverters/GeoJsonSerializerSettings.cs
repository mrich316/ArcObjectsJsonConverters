﻿namespace ArcObjectConverters
{
    public class GeoJsonSerializerSettings
    {
        public bool Simplify { get; set; } = true;

        public int Precision { get; set; } = 6;

        public double Tolerance { get; set; } = 0.001;

        public DimensionHandling Dimensions { get; set; }

        /// <summary>
        /// Default <c>Z</c> value for json output with Z <see cref="Dimensions"/>. 
        /// </summary>
        public double DefaultZValue { get; set; } = 0.0;

        /// <summary>
        /// ADVANCED: Speed optimization, will skip cloning if the serializer can alter geometries before serialization. 
        /// </summary>
        public bool SerializerHasSideEffects { get; set; }
    }
}
