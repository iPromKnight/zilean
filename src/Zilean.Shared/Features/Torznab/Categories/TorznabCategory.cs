namespace Zilean.Shared.Features.Torznab.Categories;

public class TorznabCategory(int id, string name)
{
    public int Id { get; } = id;
    public string Name { get; set; } = name;
    public List<TorznabCategory> SubCategories { get; private set; } = [];

    public bool Contains(TorznabCategory cat) =>
        Equals(this, cat) || SubCategories.Contains(cat);

    public JsonObject ToJson() =>
        new()
        {
            ["ID"] = Id,
            ["Name"] = Name
        };

    public override bool Equals(object? obj) => (obj as TorznabCategory)?.Id == Id;

    public override int GetHashCode() => Id;
    public TorznabCategory CopyWithoutSubCategories() => new(Id, Name);
}
