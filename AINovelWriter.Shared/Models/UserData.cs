using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace AINovelWriter.Shared.Models;

public class UserData
{
	public string id => UserName ?? "anon";
	public string? UserName { get; set; }
	public string? NickName { get; set; }
	public string? ImagePath { get; set; }
	[JsonIgnore]
	[Ignore]
	public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserName);
	public List<UserNovelData> SavedNovels { get; set; } = [];
}

public record UserNovelData(string NovelId, string Title)
{
	public string NovelId { get; set; } = NovelId;
}