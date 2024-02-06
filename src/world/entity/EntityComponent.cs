namespace KDR;

public abstract class EntityComponent
{
    public Entity ComposingEntity;

    public virtual void OnTreeEnter() {}
    public virtual void OnTreeExit() {}
}
