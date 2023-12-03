public class Entity
{
    public Entity Root;
    public Entity Parent;
    public readonly List<Entity> Children = new();

    readonly Dictionary<Type, EntityComponent> Components = new();

    public void ProcessCascading(float delta)
    {
        if(HasComponent<Processor>()) GetComponent<Processor>().Process(delta);

        foreach(Entity child in Children) child.ProcessCascading(delta);
    }

    public void RenderProcessCascading(Transform3 viewTransform)
    {
        if(HasComponent<Spatial>()) viewTransform = GetComponent<Spatial>().RenderProcess(viewTransform);

        foreach(Entity child in Children) child.RenderProcessCascading(viewTransform);
    }

    public void AddChild(Entity child)
    {
        Children.Add(child);
        child.Parent = this;

        if(Root is not null) child.OnTreeEnter(Root);
    }

    public void RemoveChild(Entity child)
    {
        Children.Remove(child);
        child.Parent = null;

        if(Root is not null) child.OnTreeExit();
    }

    public bool HasComponent<T>() where T : EntityComponent => Components.ContainsKey(typeof(T));

    public T GetComponent<T>() where T : EntityComponent
    {
        if(!HasComponent<T>()) throw new InvalidOperationException($"Entity does not contain {typeof(T)} component.");
        return (T)Components[typeof(T)];
    }

    public Entity SetComponent<T>(T component) where T : EntityComponent
    {
        if(Components.ContainsKey(typeof(T))) RemoveComponent<T>();
        
        Components.Add(typeof(T), component);
        component.ThisEntity = this;
        if(Root is not null) component.OnTreeEnter();

        return this;
    }

    public Entity RemoveComponent<T>() where T : EntityComponent
    {
        EntityComponent component = Components[typeof(T)];

        if(Root is not null) component.OnTreeExit();
        component.ThisEntity = null;
        Components.Remove(typeof(T));
        
        return this;
    }

    public void OnTreeEnter(Entity root)
    {
        Root = root;

        foreach(EntityComponent component in Components.Values) component.OnTreeEnter();
        foreach(Entity child in Children) child.OnTreeEnter(root);
    }

    public void OnTreeExit()
    {
        foreach(Entity child in Children) child.OnTreeExit();
        foreach(EntityComponent component in Components.Values) component.OnTreeExit();

        Root = null;
    }
}