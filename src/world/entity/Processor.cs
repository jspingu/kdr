namespace KDR;

public abstract class Processor : EntityComponent
{
    public abstract void Process(float delta);
}
