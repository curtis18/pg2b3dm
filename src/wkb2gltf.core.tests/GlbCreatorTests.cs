﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using NUnit.Framework;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using Wkb2Gltf.Extensions;
using Wkx;
using System.Linq;

namespace Wkb2Gltf.Tests;

public class GlbCreatorTests
{

    [Test]
    public void CreateGltfWithAttributesTest()
    {
        // arrange
        var p0 = new Point(0, 0, 0);
        var p1 = new Point(1, 1, 0);
        var p2 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p0, p1, p2, 0);
        var triangles = new List<Triangle>() { triangle1 };

        var attributes = new Dictionary<string, List<object>>();
        attributes.Add("id", new List<object>() { "1" });
        attributes.Add("id1", new List<object>() { 1 });
        attributes.Add("id2", new List<object>() { (uint)1 });
        attributes.Add("id3", new List<object>() { 1.1 });

        // act
        var bytes = TileCreator.GetTile(attributes, new List<List<Triangle>>() { triangles }, createGltf: true);
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "gltf_withattributes.glb");
        File.WriteAllBytes(fileName, bytes);


        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGlbWithDefaultColor() { 

        // arrange
        var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
        var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
        var polyhedralsurface = ((PolyhedralSurface)g);
        var triangles = GeometryProcessor.GetTriangles(polyhedralsurface, 100);

        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));
    }

    [Test]
    public void CreateGlbWithSingleColor()
    {
        // arrange
        var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
        var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
        var polyhedralsurface = ((PolyhedralSurface)g);
        var triangles = GeometryProcessor.GetTriangles(polyhedralsurface, 100, null);
        
        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building_single_color.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
        var model = ModelRoot.Load(fileName);
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(1));

    }

    [Test]
    public void CreateGlbWithShader()
    {
        // arrange
        var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
        var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
        var polyhedralsurface = ((PolyhedralSurface)g);
        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();


        metallicRoughness.BaseColors = (from geo in polyhedralsurface.Geometries
                                        let random = new Random()
                                        let color = string.Format("#{0:X6}", random.Next(0x1000000))
                                        select color).ToList();


        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;

        // act
        var triangles = GeometryProcessor.GetTriangles(polyhedralsurface, 100, shaderColors);
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ams_building_multiple_colors.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert (each triangle becomes a primitive because colors
        var model = ModelRoot.Load(fileName);

        // there are 8 small triangles (area) that are removed.
        Assert.That(model.LogicalMeshes[0].Primitives.Count + 8, Is.EqualTo(triangles.Count));
    }

    [Test]
    public void CreateGlbWithWrongNumberOfColorsGivesArgumentOfRangeException()
    {
        // arrange
        var buildingWkb = File.OpenRead(@"testfixtures/ams_building.wkb");
        var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
        var polyhedralsurface = ((PolyhedralSurface)g);

        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();
        metallicRoughness.BaseColors = (from geo in polyhedralsurface.Geometries
                                        let random = new Random()
                                        let color = String.Format("#{0:X6}", random.Next(0x1000000))
                                        select color).ToList();

        // accidentally remove 1:
        metallicRoughness.BaseColors.RemoveAt(metallicRoughness.BaseColors.Count-1);

        var specularGlosiness = new PbrSpecularGlossinessColors();
        specularGlosiness.DiffuseColors = metallicRoughness.BaseColors;

        shaderColors.PbrMetallicRoughnessColors = metallicRoughness;
        shaderColors.PbrSpecularGlossinessColors = specularGlosiness;

        // act
        try {
            var triangles = GeometryProcessor.GetTriangles(polyhedralsurface, 100, shaderColors);
        }
        catch(Exception ex){
            // assert
            Assert.That(ex != null, Is.True);
            Assert.That(ex is ArgumentOutOfRangeException, Is.True);
            Assert.That(ex.Message.Contains("Diffuse, BaseColor"), Is.True);
        }
    }

    [Test]
    public void ColorTest()
    {
        var p1 = new Point(0, 0, 0);
        var p2 = new Point(1, 1, 0);
        var p3 = new Point(1, 0, 0);

        var triangle1 = new Triangle(p1, p2, p3, 100);

        p1 = new Point(5, 5, 0);
        p2 = new Point(6, 6, 0);
        p3 = new Point(6, 5, 0);

        var triangle2 = new Triangle(p1, p2, p3, 100);

        var materialGreen = new MaterialBuilder().
            WithDoubleSide(true).
            WithMetallicRoughnessShader().
            WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(0, 1, 0, 1));

        var materialWhite = new MaterialBuilder().
            WithDoubleSide(true).
            WithMetallicRoughnessShader().
            WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 1, 1, 1));

        var mesh = new MeshBuilder<VertexPositionNormal>("mesh");
        DrawTriangle(triangle1, materialWhite, mesh);
        DrawTriangle(triangle2, materialGreen, mesh);
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
        
        // act
        var model = scene.ToGltf2();

        // assert
        Assert.That(model.LogicalMeshes[0].Primitives.Count, Is.EqualTo(2));
    }

    [Test]
    public static void CreateGlbForNonTriangulatedGeometry()
    {
        // arrange
        var wkt = "POLYHEDRALSURFACE Z (((0 0 0, 0 1 0, 1 1 0, 1 0 0, 0 0 0)),((0 0 0, 0 1 0, 0 1 1, 0 0 1, 0 0 0)), ((0 0 0, 1 0 0, 1 0 1, 0 0 1, 0 0 0)), ((1 1 1, 1 0 1, 0 0 1, 0 1 1, 1 1 1)),((1 1 1, 1 0 1, 1 0 0, 1 1 0, 1 1 1)),((1 1 1, 1 1 0, 0 1 0, 0 1 1, 1 1 1)))";
        var g = Geometry.Deserialize<WktSerializer>(wkt);

        // act
        var triangles = GeometryProcessor.GetTriangles(g, 100);

        // assert
        Assert.That(((PolyhedralSurface)g).Geometries.Count, Is.EqualTo(6));
        Assert.That(triangles.Count, Is.EqualTo(12));
    }

    [Test]
    public static void CreateGlbForSimpleBuilding()
    {
        // arrange
        var buildingDelawareWkt = "POLYHEDRALSURFACE Z (((1237196.52254261 -4794569.11324542 4006730.36853675,1237205.09930114 -4794565.00723136 4006732.61840877,1237198.22281801 -4794557.02527831 4006744.21497578,1237196.52254261 -4794569.11324542 4006730.36853675)),((1237198.22281801 -4794557.02527831 4006744.21497578,1237189.64607418 -4794561.13128501 4006741.96510802,1237196.52254261 -4794569.11324542 4006730.36853675,1237198.22281801 -4794557.02527831 4006744.21497578)),((1237199.14544946 -4794579.27792655 4006738.92021596,1237207.72222617 -4794575.17190377 4006741.17009276,1237200.84572844 -4794567.18993371 4006752.76668446,1237199.14544946 -4794579.27792655 4006738.92021596)),((1237200.84572844 -4794567.18993371 4006752.76668446,1237192.26896643 -4794571.29594914 4006750.51681191,1237199.14544946 -4794579.27792655 4006738.92021596,1237200.84572844 -4794567.18993371 4006752.76668446)),((1237205.09930114 -4794565.00723136 4006732.61840877,1237196.52254261 -4794569.11324542 4006730.36853675,1237207.72222617 -4794575.17190377 4006741.17009276,1237205.09930114 -4794565.00723136 4006732.61840877)),((1237207.72222617 -4794575.17190377 4006741.17009276,1237199.14544946 -4794579.27792655 4006738.92021596,1237196.52254261 -4794569.11324542 4006730.36853675,1237207.72222617 -4794575.17190377 4006741.17009276)),((1237196.52254261 -4794569.11324542 4006730.36853675,1237189.64607418 -4794561.13128501 4006741.96510802,1237199.14544946 -4794579.27792655 4006738.92021596,1237196.52254261 -4794569.11324542 4006730.36853675)),((1237199.14544946 -4794579.27792655 4006738.92021596,1237192.26896643 -4794571.29594914 4006750.51681191,1237189.64607418 -4794561.13128501 4006741.96510802,1237199.14544946 -4794579.27792655 4006738.92021596)),((1237189.64607418 -4794561.13128501 4006741.96510802,1237198.22281801 -4794557.02527831 4006744.21497578,1237192.26896643 -4794571.29594914 4006750.51681191,1237189.64607418 -4794561.13128501 4006741.96510802)),((1237192.26896643 -4794571.29594914 4006750.51681191,1237200.84572844 -4794567.18993371 4006752.76668446,1237198.22281801 -4794557.02527831 4006744.21497578,1237192.26896643 -4794571.29594914 4006750.51681191)),((1237198.22281801 -4794557.02527831 4006744.21497578,1237205.09930114 -4794565.00723136 4006732.61840877,1237200.84572844 -4794567.18993371 4006752.76668446,1237198.22281801 -4794557.02527831 4006744.21497578)),((1237200.84572844 -4794567.18993371 4006752.76668446,1237207.72222617 -4794575.17190377 4006741.17009276,1237205.09930114 -4794565.00723136 4006732.61840877,1237200.84572844 -4794567.18993371 4006752.76668446)))";
        var colors = new List<string>() {"#385E0F","#385E0F", "#FF0000", "#FF0000", "#EEC900","#EEC900","#EEC900","#EEC900","#EEC900","#EEC900","#EEC900","#EEC900"};
        var g = Geometry.Deserialize<WktSerializer>(buildingDelawareWkt);
        var polyhedralsurface = ((PolyhedralSurface)g);
        var center = polyhedralsurface.GetCenter();

        var shaderColors = new ShaderColors();
        var metallicRoughness = new PbrMetallicRoughnessColors();
        metallicRoughness.BaseColors = colors;

        var triangles = GeometryProcessor.GetTriangles(polyhedralsurface, 100, shaderColors);
        CheckNormal(triangles[2], center);
        Assert.That(triangles.Count == 12, Is.True);

        // act
        var bytes = GlbCreator.GetGlb(new List<List<Triangle>>() { triangles });
        var fileName = Path.Combine(TestContext.CurrentContext.WorkDirectory, "simle_building.glb");
        File.WriteAllBytes(fileName, bytes);

        // assert
    }

    private static void CheckNormal(Triangle t, Point center)
    {
        // arrange
        var normal = t.GetNormal();
        var p0 = t.ToVectors().Item1;
        var vertexDistance = (p0- center.ToVector()).Length();
        var withNormalDistance = (p0 + normal - center.ToVector()).Length();

        // assert
        Assert.That(withNormalDistance > vertexDistance, Is.True);
    }

    private static void DrawTriangle(Triangle triangle, MaterialBuilder material, MeshBuilder<VertexPositionNormal> mesh)
    {
        var normal = triangle.GetNormal();

        var prim = mesh.UsePrimitive(material);

        prim.AddTriangle(
            new VertexPositionNormal((float)triangle.GetP0().X, (float)triangle.GetP0().Y, (float)triangle.GetP0().Z, normal.X, normal.Y, normal.Z),
            new VertexPositionNormal((float)triangle.GetP1().X, (float)triangle.GetP1().Y, (float)triangle.GetP1().Z, normal.X, normal.Y, normal.Z),
            new VertexPositionNormal((float)triangle.GetP2().X, (float)triangle.GetP2().Y, (float)triangle.GetP2().Z, normal.X, normal.Y, normal.Z)
            );

    }

}
