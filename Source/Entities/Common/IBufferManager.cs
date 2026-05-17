namespace Celeste.Mod.aonHelper.Entities.Common;

public interface IBufferManager<TBuffers>
{
    public static abstract void QueryBuffers(int depth, out TBuffers buffers);
}
