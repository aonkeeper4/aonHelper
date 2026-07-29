using System.Runtime.InteropServices;

namespace Celeste.Mod.aonHelper.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct SwirlDisplacementVertex(
    Vector3 position, Color color, Vector2 texCoord,
    float minSwirlRadius, float maxSwirlRadius, float minSwirlSpeed, float maxSwirlSpeed)
    : IVertexType
{
    public Vector3 Position = position;
    public Color Color = color;
    public Vector2 TextureCoordinate = texCoord;
    public Vector4 SwirlConfiguration = new(minSwirlRadius, maxSwirlRadius, minSwirlSpeed, maxSwirlSpeed);
    
    // hmm this is 40 bytes . is that too big :disappointed_relieved:
    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(sizeof(float) * 3 + 4 + sizeof(float) * 2, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
    );
    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}