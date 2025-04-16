using System.ComponentModel;
using System.Text;

namespace AINovelWriter.Shared.Models;

public static class EnumHelpers
{
    // a generic GetAttribute method for enums
    public static TAttribute GetAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), false);
            return attributes is { Length: > 0 } ? (TAttribute)attributes[0] : null;
        }
        return null;
    }
    public static IEnumerable<string> GetModelProvidors(this AIModel model)
    {
        var type = model.GetType();
        var memberInfo = type.GetMember(model.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(ModelProvidorAttribute), false);
            return attributes.Select(attr => ((ModelProvidorAttribute)attr).Providor);
        }
        return [];
    }
    public static string GetAIModelName(this Enum value, bool isAzure = false)
    {
        var fi = value.GetType().GetField(value.ToString());
        if (isAzure)
        {
            var attribs = (AzureOpenAIModelAttribute[])fi.GetCustomAttributes(typeof(AzureOpenAIModelAttribute), false);
            return attribs is { Length: > 0 } ? attribs[0].Model : string.Empty;
        }
        var attributes = (ModelNameAttribute[])fi.GetCustomAttributes(typeof(ModelNameAttribute), false);

        return attributes is { Length: > 0 } ? attributes[0].Model : string.Empty;
    }
    public static Dictionary<TEnum, string> GetEnumsWithDescriptions<TEnum>() where TEnum : Enum
    {
        var enumDict = Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
            .ToDictionary(t => t, t => t.GetDescription());
        return enumDict;
    }
    public static List<ValueTuple<TEnum, string, string>> GetEnumsWithDisplayAndDescriptions<TEnum>() where TEnum : Enum
	{
		var enumList = Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
			.Select(t => (t, t.GetDisplayName(), t.GetDescription()))
			.ToList();
		return enumList;
	}

    public static string GetPromptContext(this ReviewContext context)
    {
        var sb = new StringBuilder();
        var description = context.GetDescription();
        var prompt = context.GetPromptText();
        sb.AppendLine($"**Context Description:** {description}");
        sb.AppendLine($"**Context specific instruction:** {prompt}");
        return sb.ToString();
    }
    public static string GetDescription(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes is { Length: > 0 } ? attributes[0].Description : value.ToString();
    }
    public static string GetDisplayName(this Enum value)
	{
		var fi = value.GetType().GetField(value.ToString());
		var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

		return attributes is { Length: > 0 } ? attributes[0].DisplayName : value.ToString();
	}
    public static string GetPromptText(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        var attributes = (PromptAttribute[])fi.GetCustomAttributes(typeof(PromptAttribute), false);

        return attributes is { Length: > 0 } ? attributes[0].PromptText : string.Empty;
    }
    public static List<string> GetSubGenreList(this NovelGenre genre)
    {
        var fi = genre.GetType().GetField(genre.ToString());
        var attributes = (SubGenreAttribute[])fi.GetCustomAttributes(typeof(SubGenreAttribute), false);
        if (attributes is { Length: 0 })
        {
            return [];
        }
        var subGenres = attributes[0].SubGenres;
        return subGenres.Split(',').Select(x => x.Trim()).ToList();
    }
}