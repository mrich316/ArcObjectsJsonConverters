namespace ArcObjectConverters
{
    /// <summary>
    /// Dimensions to serialize.
    /// </summary>
    public enum DimensionHandling
    {
        /// <summary>
        /// Serialize longitude and latitude.
        /// </summary>
        XY = 0,

        /// <summary>
        /// Serialize longitude, latitude and altitude.
        /// </summary>
        XYZ
    }
}