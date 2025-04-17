using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

// todo:
// add more special  indication to warp edges
namespace Celeste.Mod.aonHelper.Entities.DarkerMatter;

[Tracked]
public class DarkerMatterRenderer : Entity
{
    private class Edge
    {
        public DarkerMatter Parent;

        public bool Visible;

        public Vector2 A, B;

        public Vector2 Min, Max;

        public bool Warp;

        public Edge(DarkerMatter parent, Vector2 a, Vector2 b)
        {
            Parent = parent;
            Visible = false;
            A = a;
            B = b;
            Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            Warp = false;
        }

        public bool InView(ref Rectangle view)
        {
            return view.Left < Parent.X + Max.X && view.Right > Parent.X + Min.X && view.Top < Parent.Y + Max.Y && view.Bottom > Parent.Y + Min.Y;
        }
    }

    private List<DarkerMatter> list = new();

    private List<Edge> edges = new();

    private VertexPositionColor[] edgeVerts;

    private VirtualMap<bool> tiles;

    private Rectangle levelTileBounds;

    private uint edgeSeed;

    private bool dirty;

    private float totalTime;

    private List<DarkerMatter> barriers = new();

    private DarkerMatterController controller;

    public DarkerMatterRenderer() : base()
    {
        Tag = Tags.Global | Tags.TransitionUpdate;
        Depth = -10100;
    }

    public void Track(DarkerMatter block, Level level, DarkerMatterController darkMatterController)
    {
        controller = darkMatterController;
        list.Add(block);
        if (tiles == null)
        {
            levelTileBounds = level.TileBounds;
            tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, emptyValue: false);
        }

        for (int i = (int)block.X / 8; i < ((int)block.X + block.Width) / 8; i++)
        {
            for (int j = (int)block.Y / 8; j < ((int)block.Y + block.Height) / 8; j++)
            {
                tiles[i - levelTileBounds.X, j - levelTileBounds.Y] = !(block.WrapHorizontal || block.WrapVertical);
            }
        }

        foreach (DarkerMatter barrier in level.Tracker.GetEntities<DarkerMatter>().Cast<DarkerMatter>())
        {
            barriers.Add(barrier);
        }
        dirty = true;
    }

    public void Untrack(DarkerMatter block)
    {
        list.Remove(block);
        if (list.Count <= 0)
        {
            tiles = null;
        }
        else
        {
            for (int i = (int)block.X / 8; i < block.Right / 8f; i++)
            {
                for (int j = (int)block.Y / 8; j < block.Bottom / 8f; j++)
                {
                    tiles[i - levelTileBounds.X, j - levelTileBounds.Y] = false;
                }
            }
        }
        dirty = false;
    }

    public override void Update()
    {
        Depth = -8000;
        totalTime += Engine.DeltaTime;
        if (dirty)
        {
            RebuildEdges();
        }
        if (SceneAs<Level>() is not null)
        {
            ToggleEdges();
        }
        if (Scene.OnInterval(0.1f))
        {
            edgeSeed = (uint)Calc.Random.Next();
        }
    }

    public Color ColorCycle(Level level, int offset)
    {
        float time = (float)((totalTime + offset) % 10);
        int timeInt = (int)time;
        return level is not null
            ? Color.Lerp(controller.DarkerMatterColors[timeInt % controller.DarkerMatterColors.Length], controller.DarkerMatterColors[(timeInt + 1) % controller.DarkerMatterColors.Length], time % 1f)
            : default;
    }

    public Color WarpColorCycle(Level level, int offset)
    {
        float time = (float)((totalTime + offset) % 10);
        int timeInt = (int)time;
        return level is not null
            ? Color.Lerp(controller.DarkerMatterWarpColors[timeInt % controller.DarkerMatterWarpColors.Length], controller.DarkerMatterWarpColors[(timeInt + 1) % controller.DarkerMatterWarpColors.Length], time % 1f)
            : default;
    }

    private void ToggleEdges(bool immediate = false)
    {
        Camera camera = SceneAs<Level>().Camera;
        Rectangle view = new((int)camera.Left - 4, (int)camera.Top - 4, (int)(camera.Right - camera.Left) + 8, (int)(camera.Bottom - camera.Top) + 8);
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i] is not null)
            {
                if (immediate)
                {
                    edges[i].Visible = edges[i].InView(ref view);
                }
                else if (!edges[i].Visible && Scene.OnInterval(0.05f, i * 0.01f) && edges[i].InView(ref view))
                {
                    edges[i].Visible = true;
                }
                else if (edges[i].Visible && Scene.OnInterval(0.25f, i * 0.01f) && !edges[i].InView(ref view))
                {
                    edges[i].Visible = false;
                }
            }
        }

        if (barriers.Count > 0)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i] is not null && edges[i].InView(ref view))
                {
                    if ((edges[i].A.X == 0) && (edges[i].B.X == 0))
                    {
                        edges[i].Visible = true;
                        edges[i].Warp = edges[i].Parent.EdgeTypes[0] == DarkerMatter.EdgeType.Warp;
                    }
                    else if ((edges[i].A.X == edges[i].Parent.Width) && (edges[i].B.X == edges[i].Parent.Width))
                    {
                        edges[i].Visible = true;
                        edges[i].Warp = edges[i].Parent.EdgeTypes[1] == DarkerMatter.EdgeType.Warp;
                    }
                    else if ((edges[i].A.Y == 0) && (edges[i].B.Y == 0))
                    {
                        edges[i].Visible = true;
                        edges[i].Warp = edges[i].Parent.EdgeTypes[2] == DarkerMatter.EdgeType.Warp;
                    }
                    else if ((edges[i].A.Y == edges[i].Parent.Height) && (edges[i].B.Y == edges[i].Parent.Height))
                    {
                        edges[i].Visible = true;
                        edges[i].Warp = edges[i].Parent.EdgeTypes[3] == DarkerMatter.EdgeType.Warp;
                    }
                    else
                    {
                        edges[i] = null;
                    }
                }
            }
        }
    }

    private void RebuildEdges()
    {
        dirty = false;
        edges.Clear();
        if (list.Count <= 0)
        {
            return;
        }

        Point[] array = [
            new(0, -1),
            new(0, 1),
            new(-1, 0),
            new(1, 0)
        ];
        foreach (DarkerMatter item in list)
        {
            for (int i = (int)item.X / 8; i < (item.X + item.Width) / 8f; i++)
            {
                for (int j = (int)item.Y / 8; j < (item.Y + item.Height) / 8f; j++)
                {
                    Point[] array2 = array;
                    for (int k = 0; k < array2.Length; k++)
                    {
                        Point point = array2[k];
                        Point point2 = new(-point.Y, point.X);
                        if (Inside(item, i + point.X, j + point.Y) || (Inside(item, i - point2.X, j - point2.Y) && !Inside(item, i + point.X - point2.X, j + point.Y - point2.Y)))
                        {
                            continue;
                        }
                        Point point3 = new(i, j);
                        Point point4 = new(i + point2.X, j + point2.Y);
                        Vector2 value = new Vector2(4f) + new Vector2(point.X - point2.X, point.Y - point2.Y) * 4f;
                        int num = 1;
                        while (Inside(item, point4.X, point4.Y) && !Inside(item, point4.X + point.X, point4.Y + point.Y))
                        {
                            point4.X += point2.X;
                            point4.Y += point2.Y;
                            num++;
                            if (num > 8)
                            {
                                Vector2 a = new Vector2(point3.X, point3.Y) * 8f + value - item.Position;
                                Vector2 b = new Vector2(point4.X, point4.Y) * 8f + value - item.Position;
                                edges.Add(new Edge(item, a, b));
                                num = 0;
                                point3 = point4;
                            }
                        }
                        if (num > 0)
                        {
                            Vector2 a = new Vector2(point3.X, point3.Y) * 8f + value - item.Position;
                            Vector2 b = new Vector2(point4.X, point4.Y) * 8f + value - item.Position;
                            edges.Add(new Edge(item, a, b));
                        }
                    }
                }
            }
        }

        edgeVerts ??= new VertexPositionColor[1024];
    }


    private bool Inside(DarkerMatter block, int tx, int ty)
    {
        return (block.WrapHorizontal || block.WrapVertical) ? block.Collider.Bounds.Contains(tx * 8, ty * 8) : tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
    }

    public override void Render()
    {

        if (list.Count <= 0)
        {
            return;
        }

        Level level = SceneAs<Level>();
        Camera camera = level.Camera;

        if (edges.Count <= 0)
        {
            return;
        }
        int index = 0;
        foreach (Edge edge in edges)
        {
            if (edge is not null)
            {
                if (edge.Visible)
                {
                    DrawSimpleLightning(ref index, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, edge.Warp ? WarpColorCycle(level, 0) : ColorCycle(level, 0), 1f);
                    DrawSimpleLightning(ref index, ref edgeVerts, edgeSeed + 1, edge.Parent.Position, edge.A, edge.B, edge.Warp ? WarpColorCycle(level, 5) : ColorCycle(level, 5), 1f);
                }
            }
        }

        if (index > 0)
        {
            GameplayRenderer.End();
            GFX.DrawVertices(camera.Matrix, edgeVerts, index);
            GameplayRenderer.Begin();
        }
    }

    private static void DrawSimpleLightning(ref int index, ref VertexPositionColor[] verts, uint seed, Vector2 pos, Vector2 a, Vector2 b, Color color, float thickness = 1f)
    {
        seed += (uint)(a.GetHashCode() + b.GetHashCode());
        a += pos;
        b += pos;
        float num = (b - a).Length();
        Vector2 vector = (b - a) / num;
        Vector2 vector2 = vector.TurnRight();
        a += vector2;
        b += vector2;
        Vector2 vector3 = a;
        int num2 = (PseudoRand(ref seed) % 2u != 0) ? 1 : (-1);
        float num3 = PseudoRandRange(ref seed, 0f, (float)Math.PI * 2f);
        float num4 = 0f;
        float num5 = index + ((b - a).Length() / 4f + 1f) * 6f;
        while (num5 >= verts.Length)
        {
            Array.Resize(ref verts, verts.Length * 2);
        }
        for (int i = index; i < num5; i++)
        {
            verts[i].Color = color;
        }
        do
        {
            float num6 = PseudoRandRange(ref seed, 0f, 4f);
            num3 += 0.1f;
            num4 += 4f + num6;
            Vector2 vector4 = a + vector * num4;
            if (num4 < num)
            {
                vector4 += num2 * vector2 * num6 - vector2;
            }
            else
            {
                vector4 = b;
            }
            verts[index++].Position = new Vector3(vector3 - vector2 * thickness, 0f);
            verts[index++].Position = new Vector3(vector4 - vector2 * thickness, 0f);
            verts[index++].Position = new Vector3(vector4 + vector2 * thickness, 0f);
            verts[index++].Position = new Vector3(vector3 - vector2 * thickness, 0f);
            verts[index++].Position = new Vector3(vector4 + vector2 * thickness, 0f);
            verts[index++].Position = new Vector3(vector3, 0f);
            vector3 = vector4;
            num2 = -num2;
        }
        while (num4 < num);
    }

    private static uint PseudoRand(ref uint seed)
    {
        seed ^= seed << 13;
        seed ^= seed >> 17;
        return seed;
    }

    public static float PseudoRandRange(ref uint seed, float min, float max)
    {
        return min + (PseudoRand(ref seed) & 0x3FFu) / 1024f * (max - min);
    }
}