using System.ComponentModel;

namespace AINovelWriter.Shared.Models;

public static class EnumHelpers
{
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
    public static string GetOpenAIModelName(this Enum value, bool isAzure = false)
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
}