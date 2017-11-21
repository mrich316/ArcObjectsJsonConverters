# Documentation for ArcObjectsJsonConverters

## Gotchas

This library uses [`ITopologicalOperator.Simplify()`](http://desktop.arcgis.com/en/arcobjects/latest/net/webframe.htm#ITopologicalOperator_Simplify.htm)
to auto-correct invalid shapes. This implies that all geometries
must have its spatial reference set. If its not set, coordinates
shifting might happen.

This sample shows the shifting:

```csharp
using System;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace SimplifyAltersPoint
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            var license = new AoInitializeClass();
            license.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            // set x == 64, we will compare that
            // value after calling Simplify().
            var x = 64d;
            var pt1 = new PointClass();
            pt1.PutCoords(x, 12);

            var pt2 = new PointClass();
            pt2.PutCoords(2 * x, 12);

            var line = new PolylineClass();
            line.FromPoint = pt1;
            line.ToPoint = pt2;

            var topo = (ITopologicalOperator2) line;
            topo.IsKnownSimple_2 = false;
            topo.Simplify();

            if (line.FromPoint.X != x)
            {
                // The coordinates have shifted after calling Simplify()...
                // line.FromPoint.X == 64.0000019073486
                throw new Exception($"{line.FromPoint.X} != {x}");
            }
        }
    }
}
```

Setting a spatial reference stabilizes the `Simplify()` function
and returns the expected behavior.

```csharp
...
var sr = new SpatialReferenceEnvironmentClass()
    .CreateProjectedCoordinateSystem(32188);
var line = new PolylineClass();
line.SpatialReference = sr
...
```