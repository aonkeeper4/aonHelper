namespace Celeste.Mod.aonHelper.Entities.Controllers;

public class Controller : Entity
{
    protected Controller(Vector2 position) : base(position)
    {
        // fuck you
        Depth = int.MinValue;
        Active = Collidable = Visible = false;
        Components.LockMode = ComponentList.LockModes.Locked;
    }
    
    // no update for you
    public override sealed void Update() { }
    public override sealed void Render() { }
}
