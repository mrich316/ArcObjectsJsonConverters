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

Before serializing a geometry to `json`, a converter will (if possible):
- serialize empty geometries as `null`
- remove lines with less than 2 points
- remove polygons with less than 4 points
- change non-simple geometries to simple ones
- auto-correct self-intersections and segment overlaps
- adjust ring orientations: counter clock-wise for exterior rings,
  clock-wise for holes
- set the geometry type to its "proper" value
- generalize paths/rings containing true curves when appropriate

At last, if nothing can be done to serialize the geometry, the converter will
complain by throwing an exception.

## Status

|Geometry  |Serialization|Deserialization|Notes|
-----------|------|------|---
Point      | done | todo | Needs more tests.
Polyline   | done | todo | Needs more tests, supports only single-part at the moment.
Polygon    | todo | todo |
MultiPoint | todo | todo |

A [nuget](https://nuget.org/) could be made when a geometry will support
serialization and deserialization.