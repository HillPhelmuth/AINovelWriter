using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AINovelWriter.Shared.Models;
using Microsoft.Azure.Cosmos.Linq;

namespace AINovelWriter.Shared.Services;

public class CosmosService(CosmosClient cosmosClient)
{
	private const string DbName = "UserNovels";
	private const string NovelContainerName = "SavedNovels";
	private const string UserContainerName = "Users";
	private readonly Container _novelContainer = cosmosClient.GetContainer(DbName, NovelContainerName);
	private readonly Container _userContainer = cosmosClient.GetContainer(DbName, UserContainerName);
	public async Task<UserData?> GetUserProfile(string username)
	{
		var item = await _userContainer.ReadItemAsync<UserData>(username, new PartitionKey(username));
		return item;
	}
	public async Task<NovelInfo> GetUserNovel(string username, string novelId)
	{
		var item = await _novelContainer.ReadItemAsync<NovelInfo>(novelId, new PartitionKey(username));
		if (item is not null) return item;
		throw new Exception("Novel not found");
	}
	public async Task<(bool, string)> SaveUserNovel(NovelInfo novel, UserData userData)
	{
		if (userData.SavedNovels.All(x => x.NovelId != novel.id))
		{
			userData.SavedNovels.Add(new UserNovelData(novel.id, novel.Title));
			await SaveUser(userData);
		}

		return await TryUpsertNovel(novel);

	}
	public async Task SaveUser(UserData user)
	{
		await _userContainer.UpsertItemAsync(user, new PartitionKey(user.UserName));
	}
	public async Task<(bool, string)> TryUpsertNovel(NovelInfo novel)
	{
		try
		{
			var result = await _novelContainer.UpsertItemAsync(novel, new PartitionKey(novel.User));
			var message = result.StatusCode == System.Net.HttpStatusCode.Created ? "Item Created" : "Item Replaced";
			return (true, $"Upsert successful. {message}");
		}
		catch (Exception e)
		{
			return (false, e.Message);
		}
	}
}