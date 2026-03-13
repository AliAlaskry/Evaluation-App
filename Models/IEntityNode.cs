public interface IEntityNode
{
    public string ConfigEntityId { get; }

    public ConfigEntity ConfigEntity { get; }

    public EntityBaseConfig BaseConfig { get; }

    public RootEntityConfig? RootConfig { get; }

    public ValueEntityConfig? ValueConfig { get; }

    public AddonsEntityConfig? AddonsConfig { get; }

    bool HasChilds { get; }

    IReadOnlyList<IEntityNode>? ReadonlyChilds { get; }

    double Score { get; }
    int Value { get; set; }
}