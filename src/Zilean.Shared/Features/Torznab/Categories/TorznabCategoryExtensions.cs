namespace Zilean.Shared.Features.Torznab.Categories;

public static class TorznabCategoryExtensions
{
    public static List<TorznabCategory> GetTorznabCategoryTree(this List<TorznabCategory> categories)
    {
        var sortedTree = categories
            .Select(c =>
        {
            var sortedSubCats = c.SubCategories.OrderBy(x => x.Id);
            var newCat = new TorznabCategory(c.Id, c.Name);
            newCat.SubCategories.AddRange(sortedSubCats);
            return newCat;
        }).OrderBy(x => x.Id >= 100000 ? "zzz" + x.Name : x.Id.ToString()).ToList();

        return sortedTree;
    }
}
