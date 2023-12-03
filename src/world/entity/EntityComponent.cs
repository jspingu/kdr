public abstract class EntityComponent
{
    public Entity ThisEntity;

    public virtual void OnTreeEnter() {}
    public virtual void OnTreeExit() {}
}