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
	private const string SharedContainerName = "SharedNovels";
	private readonly Container _novelContainer = cosmosClient.GetContainer(DbName, NovelContainerName);
	private readonly Container _userContainer = cosmosClient.GetContainer(DbName, UserContainerName);
	public async Task<UserData?> GetUserProfile(string username)
	{
		try
		{
			var item = await _userContainer.ReadItemAsync<UserData>(username, new PartitionKey(username));
            var query = _novelContainer.GetItemLinqQueryable<NovelInfo>().Where(x => x.User == username);
            var feed = query.ToFeedIterator();
			item.Resource.SavedNovels.Clear();
            while (feed.HasMoreResults)
            {
                var results = await feed.ReadNextAsync();
				item.Resource.SavedNovels.AddRange(results.Select(x => new UserNovelData(x.id,x.Title, x.CreatedOn)));
            }
            return item;
		}
		catch
		{
			return null;
		}
		
	}
	public async Task<NovelInfo> GetUserNovel(string username, string novelId)
	{
        try
        {
            var item = await _novelContainer.ReadItemAsync<NovelInfo>(novelId, new PartitionKey(username));
            return item;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting item. Rehydrating user novels\n+++++++++++++++++++++\n"+ex.Message+"\n");
            var (foundItems, message) = await TryFindUserNovels(username);
            if (!foundItems)
                throw new Exception(message);
            else
            {
                Console.WriteLine(
                    $"Rehydrated user novels. Trying again\n+++++++++++++++++++++\n{message}\n+++++++++++++++++++++++\n");
                return new NovelInfo();
            }
        }

    }

    public async Task<(bool, string)> TryFindUserNovels(string username)
    {
		var user = await GetUserProfile(username);
		if (user is null) return (false, "User not found");
		var query = _novelContainer.GetItemLinqQueryable<NovelInfo>(true).Where(x => x.User == username);
		var feed = query.ToFeedIterator();
        var countAdded = 0;
        while (feed.HasMoreResults)
        {
			var items = await feed.ReadNextAsync();
            foreach (var item in items)
            {
				if (user.SavedNovels.Any(x => x.NovelId == item.id)) continue;
                if (user.SavedNovels.Any(x => x.Title == item.Title))
                {
					user.SavedNovels.First(x => x.Title == item.Title).NovelId = item.id;
					Console.WriteLine($"Updated {item.Title} with {item.id}");
					continue;
                }
				user.SavedNovels.Add(new UserNovelData(item.id, item.Title, item.CreatedOn));
				Console.WriteLine($"Added {item.Title} with {item.id}");
				countAdded++;
            }
        }
		await SaveUser(user);
		return (true, $"Added {countAdded} novels to user profile");
    }
	public async Task<(bool, string)> SaveUserNovel(NovelInfo novel, UserData userData)
    {
        if (string.IsNullOrEmpty(userData.UserName) && string.IsNullOrEmpty(novel.User))
            return (false, "User not found");
		if (string.IsNullOrEmpty(novel.User))
			novel.User = userData.UserName;
		if (userData.SavedNovels.Any(x => x.NovelId == novel.id)) 
			return await TryUpsertNovel(novel);
		userData.SavedNovels.Add(new UserNovelData(novel.id, novel.Title, novel.CreatedOn));
		await SaveUser(userData);

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
	public async Task<(bool, string)> DeleteUserNovel(UserData user, string novelId)
    {
        try
        {
			var username = user.UserName;
            await _novelContainer.DeleteItemAsync<NovelInfo>(novelId, new PartitionKey(username));
			user.SavedNovels.RemoveAll(x => x.NovelId == novelId);
			await SaveUser(user);
            return (true, "Novel deleted");
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }
}