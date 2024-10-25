using AINovelWriter.Shared.Models;

namespace AINovelWriter.Web.Components;

public partial class GenreSelect
{
    private List<GenreCategoryItem> GenreCategories => Enum.GetValues<GenreCategory>()
        .Select(e => new GenreCategoryItem { Category = e, Description = e.GetDescription()})
        .ToList();
    private class GenreItem(string category, string name, string description)
    {
        public string Category { get; set; } = category;
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
    }
    
    private class GenreCategoryItem
    {
        public string Description { get; set; } = "";
        public GenreCategory Category { get; set; }
        public List<GenreItem> Items => GetItems();

        private List<GenreItem> GetItems()
        {
            var enumType = Category switch
            {
                GenreCategory.GenreFiction => typeof(GenreFiction),
                GenreCategory.ThemeFiction => typeof(ThemeFiction),
                GenreCategory.CrimeAndThrillers => typeof(CrimeAndThrillers),
                GenreCategory.Fantasy => typeof(Fantasy),
                GenreCategory.ScienceFiction => typeof(ScienceFiction),
                GenreCategory.ActionAndAdventure => typeof(ActionAndAdventure),
                _ => throw new ArgumentException("Invalid GenreCategory")
            };
            return Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new GenreItem(Category.ToString(), e.ToString(), e.GetDescription()))
                .ToList();
        }
    }
}