# pg2b3dm
 
 ![Build status](https://github.com/Geodan/pg2b3dm/actions/workflows/main.yml/badge.svg)[![Nuget](https://img.shields.io/nuget/vpre/pg2b3dm)](https://www.nuget.org/packages/pg2b3dm)
 [![Join the chat at https://discord.gg/gGCka4Nd](https://img.shields.io/discord/1013017110814932993?color=%237289DA&label=pg2b3dm&logo=discord&logoColor=white)](https://discord.gg/uSKvUwPgmG)

 Tool for converting 3D geometries from PostGIS to [3D Tiles](https://github.com/AnalyticalGraphicsInc/3d-tiles). The generated 
 3D Tiles can be visualized in Cesium JS, Cesium for Unreal, Cesium for Unity3D, Cesium for Omniverse, Mapbox GL JS v3 beta (experimental) or other 3D Tiles client viewers.

![image](https://user-images.githubusercontent.com/538812/227500590-bebe59b6-5697-462d-9ebd-b40fe9a2dc2b.png)

Features:

- 3D Tiles 1.1 Implicit tiling;

- 3D Tiles extensions EXT_Mesh_Features and EXT_Structural_Metadata; 

- Valid glTF 2.0 files;

- Shading PbrMetallicRoughness and PbrSpecularGlossiness;

- Query parameter support;

- Cesium: LOD support and Outlines support (using CESIUM_primitive_outline);

- Triangulation of input geometries MultiPolygon/PolyhedralSurface;

- Docker support.

Resulting tilesets are validated against 3D Tiles Validator (https://github.com/CesiumGS/3d-tiles-validator).

To run this tool there must be a PostGIS table available containing polyhedralsurface/multipolygon geometries. 

Tileset.json and b3dm tiles are by default created in the 'output/content' subdirectory (or specify output directory with   -o, --output).

## Getting started

Convert 3D Data (Multipolygon Z) to 3D Tiles

- Download Geopackage from https://3dbag.nl/, for example https://3dbag.nl/nl/download?tid=7-480-624

Result: 7-480-624.gpkg (18 MB)

- Import in PostGIS database, convert to Cesium coordinates

```
$ ogr2ogr -f PostgreSQL pg:"host=localhost user=postgres password=postgres" -t_srs epsg:4978 7-480-624.gpkg lod22_3d
```

- Convert to 3D Tiles using pg2b3dm

```
$ pg2b3dm -h localhost -U postgres -c geom -d postgres -t lod22_3d -a identificatie
```
- Load 3D Tiles in Cesium viewer, example result see https://geodan.github.io/pg2b3dm/sample_data/3dbag/tienhoven/  

Older getting started documents:

1] See [getting started](getting_started.md) for a tutorial how to convert a 2D shapefile of buildings with height attribute to 3D Tiles and visualize in CesiumJS/Cesium for Unreal/Unity3D.

2] For a dataprocessing workflow from CityGML to 3D Tiles using GDAL, PostGIS and FME see [dataprocessing/dataprocessing_citygml](dataprocessing/dataprocessing_citygml.md).

## Demo

![Alt Text](demo_pg2b3dm.gif)

## Live Sample viewers

- 3D Bag by tudelftnl - 10 million Dutch buildings in 3D Tiles https://3dbag.nl/ 

![image](https://user-images.githubusercontent.com/538812/194698535-5b324133-bdf1-4d8c-8d53-37555a6f7b5b.png)

- FOSS4G presentations

Presentation at FOSS4G 2021: A fast web 3D viewer for 11 million buildings https://www.youtube.com/watch?v=1_JM2Xf5mDk

Presentation at FOSS4G 2019: 3D geodata in the MapBox GL JS viewer with 3D Tiles https://www.youtube.com/watch?v=HXQJbyEnC9w

- Texel - 3D Terrain, subsurface and buildings in MapBox GL JS: http://beta.geodan.nl/mapbox3d

![texel](https://user-images.githubusercontent.com/538812/77528003-74f6d900-6e8d-11ea-968e-5c510b6a1ad3.png)

- GeoTop Subsurface in MapBox GL JS: https://geodan.github.io/pg2b3dm/sample_data/geotop/mapbox/

- Amsterdam Buildings in MapBox GL JS: https://geodan.github.io/pg2b3dm/sample_data/amsterdam/mapbox/

- Amsterdam Buildings in Cesium: https://geodan.github.io/pg2b3dm/sample_data/amsterdam/cesium/

- Dover - Delaware buildings in MapBox GL JS: https://geodan.github.io/pg2b3dm/sample_data/delaware/mapbox/

- Dover - Delaware buildings in Cesium: https://geodan.github.io/pg2b3dm/sample_data/delaware/cesium/

- Duisburg buidings converted from CityGML in MapBox GL JS - https://geodan.github.io/pg2b3dm/sample_data/duisburg/mapbox/#15.62/51.430166/6.782675/0/45

## Command line options

All parameters are optional, except the -t --table option. 

If --username and/or --dbname are not specified the current username is used as default.

```
  -U, --username                  Database user

  -h, --host                      (Default: localhost) Database host

  -d, --dbname                    Database name

  -c, --column                    (Default: geom) Geometry column

  -t, --table                     Required. Database table, include database schema if needed

  -p, --port                      (Default: 5432) Database port

  -o, --output                    (Default: output) Output path

  -a, --attributecolumns          (Default: '') Attribute columns

  -q, --query                     (Default: '') Query parameter

  --copyright                     (Default: '') glTF asset copyright

  --default_color                 (Default: #FFFFFF) Default color

  --default_metallic_roughness    (Default: #008000) Default metallic roughness

  --double_sided                  (Default: true) Default double sided

  --create_gltf                   (Default: true) Create glTF files

  --max_features_per_tile         (Default: 1000) maximum features per tile (Cesium)

  -l, --lodcolumn                 (Default: '') LOD column (Cesium)

  -g, --geometricerrors           (Default: 2000,0) Geometric errors (Cesium)

  --shaderscolumn                 (Default: '') shaders column (Cesium)

  --use_implicit_tiling           (Default: true) use 1.1 implicit tiling (Cesium)

  --boundingvolume_heights        (Default: 0,100) Tile boundingVolume heights (min, max) in meters (Cesium)

  --add_outlines                  (Default: false) Add outlines (Cesium)

  -r, --refinement                (Default: REPLACE) Refinement ADD/REPLACE (Cesium)

  --min_zoom                      (Default: 15) Minimum zoom level (Mapbox)

  --max_zoom                      (Default: 15) Maximum zoom level (Mapbox)

  --help                          Display this help screen.

  --version                       Display version information.
```

Sample command for running pg2b3dm:

```
-h localhost -U postgres -c geom_triangle --shaderscolumn shaders -t delaware_buildings -d postgres -g 100,0 
```

## Installation


Prerequisite: .NET 6.0 SDK is installed https://dotnet.microsoft.com/download/dotnet/6.0

```
$ dotnet tool install -g pg2b3dm
```

Or update

```
$ dotnet tool update -g pg2b3dm
```

To run:

```
$ pg2b3dm
```

## Styling

For styling see [styling 3D Tiles](styling.md) 

## Geometries

Input geometries must be of type PolyhedralSurface or Multipolygon (with z values). When the geometry is not triangulated, pg2b3dm will perform
triangulation.

For large datasets create a spatial index on the geometry column:

```
psql> CREATE INDEX ON the_table USING gist(st_centroid(st_envelope(geom_triangle)));
```

When there the spatial index is not present the following warning is shown.

![image](https://user-images.githubusercontent.com/538812/261248327-c29b4520-a374-4441-83bf-2b60e8313c65.png)

## Query parameter

The -q --query will be added to the 'where' part of all queries. 

Samples:

Attribute query:

```
-q "ogc_fid=118768"
```

Spatial query:

```
-q "ST_Intersects(wkb_geometry, 'SRID=4326;POLYGON((-75.56996406 39.207228824,-75.56996406 39.2074420320001,-75.5696300339999 39.2074420320001,-75.5696300339999 39.207228824,-75.56996406 39.207228824))'::geometry)"
```

Make sure to check the indexes when using large tables.

## Attributes

With the -a attributecolumns parameter multiple columns with attributes can be specified. The attribute information is stored in the b3dm batch table. 
Multiple columns must be comma separated:

Sample:  --attributescolumns col1,col2

Attribute columns can be of any type.

## Cesium support

For Cesium support (tiling schema, LODS, outlines) see [Cesium notes](cesium_notes.md) 

## Mapbox support

For Mapbox support see [Mapbox notes](mapbox_notes.md) 

## QGIS support

In QGIS 3.34 support for 3D Tiles is added see https://cesium.com/blog/2023/11/07/qgis-now-supports-3d-tiles/

To create 3D Tiles for QGIS use parameters '--create_gltf false --use_implicit_tiling false' as 3D Tiles 1.1 features are not supported yet. 

## Run from Docker

Docker image: https://hub.docker.com/repository/docker/geodan/pg2b3dm

Tags used (https://hub.docker.com/repository/docker/geodan/pg2b3dm/tags): 

- {version}: specific version

- latest: is build automatically after push to master


### Building Dockers

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src
$ docker build -t geodan/pg2b3dm .
```

Test feature branch:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ git checkout {name_of_feature_branch}
$ cd pg2b3dm/src
$ docker build -t geodan/pg2b3dm:{name_of_feature_branch} .
```

### Running

Sample on Linux:

```
$ docker run -v $(pwd)/output:/app/output -it geodan/pg2b3dm -h my_host -U my_user -d my_database -t my_schema.my_table
```

## Run from source

Requirement: Install .NET 6.0 SDK

https://dotnet.microsoft.com/download/dotnet/6.0

Installation guide see https://docs.microsoft.com/en-us/dotnet/core/install/

To run the app:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet run -- -h my_host -U my_user -d my_database -t my_schema.my_table
```

To create an self-contained executable '~/bin/pg2b3dm' for Linux:

```
$ git clone https://github.com/Geodan/pg2b3dm.git
$ cd pg2b3dm/src/pg2b3dm
$ dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
$ cp ./bin/Release/net6.0/linux-x64/publish/pg2b3dm ~/bin
$ ~/bin/pg2b3dm
```

Alternative options for parameter -r in dotnet publish: 'osx-x64' (Mac), 'win-x64' (Windows)

## Debugging in Visual Studio Code

```
$ git clone https://github.com/Geodan/pg2b3dm.git

$ cd src

$ code .
```

In Visual Studio Code, open .vscode/launch.json and adjust the 'args' parameter to your environment

```
"args": ["-h" ,"my_host", "-U" ,"my_user", "-d", "my_database", "-t", "my_table"],            
```

Press F5 to start debugging.

## Dependencies

- b3dm-tile (https://github.com/bertt/b3dm-tile-cs) - for generating b3dm files;

- CommandLineParser (https://github.com/commandlineparser/commandline) for parsing command line options;

- Npgsql (https://www.npgsql.org/) - for access to PostgreSQL;

- SharpGLTF (https://github.com/vpenades/SharpGLTF) for generating glTF;

- Subtree (https://github.com/bertt/subtree) - for subtree file handling

- Triangulator (https://github.com/bertt/triangulator) - for triangulating geometries

- Wkx (https://github.com/cschwarz/wkx-sharp) - for geometry handling.

## History

2023-11-13: release 1.8.5, fix for dataset with geometries on 1 location

2023-10-25: release 1.8.4, add -r --refinement option

2023-10-17: release 1.8.3, tileset.json asset version from 1.0 to 1.1, database connection timeout removed

2023-10-04: release 1.8.2, use humanizer with resources 

2023-09-26: release 1.8.1, updating triangulator 

2023-09-22: release 1.8, adding 3D Tiles 1.1 Metadata support (EXT_Mesh_Features / EXT_Structural_Metadata). Options added: create_gltf (default true), double_sided (default true)

2023-08-29: release 1.7.1, improve spatial index check

2023-08-29: release 1.7.0, add triangulator - runs only when geometry is not triangulated

2023-08-29: release 1.6.3, add support for MultiPolygonZ

2023-08-17: release 1.6.2, add check for spatial index

2023-08-16: release 1.6.1, translate b3dm's to center of tile for Mapbox GL JS v3

2023-08-16: release 1.6.0, add experimental support for Mapbox GL JS v3

2023-06-20: release 1.5.5, fix issue when only 1 level is generated

2023-04-06: release 1.5.3, fix disappearing features

2023-04-04: release 1.5.2, fix query parameter

2023-03-27: release 1.5.1, add outlines support for multiple shaders

2023-03-15: release 1.5.0, adding options 'add_outlines' (default false) and 'default_color' (#FFFFFF)

2023-02-16: release 1.4.3, fix for implicit tiling - missing b3dm's on high z-levels

2023-02-02: release 1.4.2, fix subtree files generation

2023-02-01: release 1.4.1, fix global tool

2023-02-01: release 1.4, adding tree of subtree files support

2023-01-10: release 1.3, adding LOD support

2022-12-13: release 1.2.3, fixing parameter use_implicit_tiling

2022-08-30: release 1.2.2, fixing initial boundingbox issue

2022-08-29: release 1.2.1 

- Fixing debug boundingVolumes and query parameter;

- Option 'use_implicit_tiling' default value changed from False to True;

2022-08-24: release 1.1: adding parameters sql_command_timeout (default: 30 seconds) and boundingvolume_heights (default: 0,100)

2022-08-23: release 1.0

Use a quadtree tiling method by default, fix skewed bounding volumes in Cesium.

MapBox GL JS support is discontinued at the moment.

Breaking changes:

- removed: parameter -i, --idcolumn

- removed: parameter -e, --extenttile

- renamed: parameter implicit_tiling_max_features to max_features_per_tile

2022-08-09: release 0.16, fixing materials (MetallicRoughness and SpecularGlossiness)

2022-08-09: release 0.15, use 1 geometric error for implicit tiling

2022-07-20: release 0.14, adding 3D Tiles 1.1 implicit tiling option

2022-07-05: release 0.13, adding glTF asset copyright

2022-01-24: release 0.12, to .NET 6, fixing decimal symbols regional settings on Windows

2021-10-27: release 0.11.2, fixing non latin characters issue in batch table

2021-09-30: release 0.11, adding multiple attribute columns support. 0.11.1 contains bug fix for batch table length 

2020-11-17: release 0.10, adding shader support PbrMetallicRoughness and PbrSpecularGlossiness + to .NET 5.0

2020-06-18: release 0.9.4, adding query parameter support (-q --query)

2020-05-07: release 0.9.3, rewriting tiling method 

2019-11-18: release 0.8 adding -f, --featurespertile and -e, --extenttile options

2019-10-02: release 0.7 adding id column option (default 'id')

2019-09-02: release 0.6 adding batching option on single column (-a option)

2019-08-21: release 0.5.1 with fix for non trusted Postgres connection

2019-08-20: release 0.5 adds support for multiple colors

2019-08-15: release 0.4.4 improving roof colors

2019-08-15: release 0.4.3 change degenerated triangles detection + removal

2019-08-14: release 0.4.2 fixing roof colors + filter very small triangles (<0.01)

2019-08-13: release 0.4.1 with fix for roof colors (option -r)

2019-08-12: release 0.4 adding roof color column option (-r)

2019-08-01: release 0.3.3 with 2 colors

2019-07-09: release 0.3 using library SharpGLTF

2019-06-01: release 0.2.1 with some small fixes

2019-06-01: initial release 0.2

2019-05-01: initial release 0.1
