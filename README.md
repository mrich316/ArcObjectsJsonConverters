# Opinionated Json Converters for ESRI ArcObjects

## *This project is under development...*

This library aims to augment interoperability between ESRI
ArcObjects and other projects. It provides custom `JsonConverters`
for [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
to export `IGeometry` objects to different `json` formats.

## ArcObjects: The start line

ArcObjects are COM components with parameterless constructors, they cannot
enforce strict format rules nor object constraints. Therefore, a geometry might be
incomplete, incoherent or simply empty depending of where and how it was created.
Also, `IPolygon` or `IPolyline` (but strangely not `IPoint`) can always
become multi-parts by adding geometries to its collection without altering
its geometry type. More over, `IMultiPoint` can be empty or contain a single point.

## The opinions to a finish line

The converters work hard to make the serialization go as smooth and predictable
as possible and always assume the worst case scenario: a half baked geometry.

Before serializing a geometry to `json`, a converter could (depending on
`GeoJsonSerializerSettings`):
- serialize empty geometries as `null`
- remove paths with less than 2 points
- remove rings with less than 4 points
- change non-simple geometries to simple ones
- auto-correct self-intersections and segment overlaps
- adjust ring orientations: counter clock-wise for exterior rings,
  clock-wise for holes
- downgrade the geometry type to its "proper" value (see conversion table below)
- generalize paths/rings containing true curves when appropriate

At last, if nothing can be done to serialize the geometry, the converter will
complain by throwing an exception.

## Conversion Table for ArcObjects Geometries to GeoJson

### Definitions

- `invalid`: denotes a state not appropriate for the type being serialized.
   For example:

  - a path with a single point
  - a path or ring length smaller than `Tolerance`
  - a ring with less than 4 coordinates
  - an unclosed ring
  - a polygon without an exterior ring
  - an empty geometry part

### Setting `ForceMulti`

Using `ForceMulti=false` will make the serializer ajust to input
geometries.  If a `Multipoint`, `Polyline` or `Polygon` contains a
single part, the serializer will write a GeoJSON type of `Point`, `LineString` or `Polygon`.

If `ForceMulti=true`, all ArcObjects types will always be serialized as `Multi*`,
even if they contain a single part, because ArcObjects does not make a distinction
between single and multi-parts (excepting `Point`).

### Setting `Simplify`

Using `Simplify=true` will use `ITopologicalOperator.Simplify()` before
serializing a geometry. `Simplify` may remove duplicate geometry parts
and may reorient segments in paths and rings.

| ArcObjects | GeoJSON         | ForceMulti | Simplify | ForceMulti + Simplify |
-------------|:---------------:|:----------:|:--------:|:---------------------:
empty or invalid geometry | `null` | `null`   | `null`     | `null`
`Point`      | `P`             | `P`          | `P`        | `P`
`Multipoint` (single point)              | `P`  | `MP` | `P` | `MP`
`Multipoint` (a point + invalid points`*`)  | `P`  | `MP` | `P` | `MP`
`Multipoint` (multiple different points) | `MP` | `MP` | `MP`| `MP`
`Multipoint` (copies of the same point)  | `MP` | `MP` | `P` | `MP`
`Polyline` (single path)                 | `L`  | `ML` | `L` | `ML`
`Polyline` (duplicated path)             | `ML` | `ML` | `L`| `ML`
`Polyline` (multi path)                  | `ML` | `ML` | `ML`| `ML`
`Polyline` (single path + invalid paths`*`) | `L`  | `ML` | `L` | `ML`
`Polyline` (only invalid paths`*`) | `null` | `null` | `null` | `null`
`Polygon` (single exterior ring)         | `P`  | `MP` | `P` | `MP`
`Polygon` (duplicate exterior ring)      | `MP` | `MP` | `P`| `MP`
`Polygon` (many exterior ring)           | `MP` | `MP` | `MP`| `MP`
`Polygon` (exterior ring + invalid exterior ring`*`) | `P`  | `MP` | `P` | `MP`
`Polygon` (exterior ring + invalid interior ring`**`) | `P`  | `MP` | `P` | `MP`
`Polygon` (only invalid rings)  | `null` | `null` | `null` | `null`
`Polygon` (only interior rings) | `null` | `null` | `null` | `null`

| Legend |  |
:-------:|---
P  | Point
MP | MultiPoint
L  | LineString
ML | MultiLineString
P  | Polygon
MP | MultiPolygon

Notes:
- `*` Invalid parts are always removed.
- `**` Polygon serialized without a hole

## Status

|Geometry    |Serialization|Deserialization|Notes|
-------------|------|------|---
`Point`      | partial | partial |
`Polyline`   | partial | partial | When true curves are present, the geometry is always generalized, even with `Simplify=false`. This will eventually be adjusted to only generalize the curved segments.
`Polygon`    | todo | todo |
`MultiPoint` | partial | todo |

A [nuget](https://nuget.org/) could be made when a geometry will support
serialization and deserialization.