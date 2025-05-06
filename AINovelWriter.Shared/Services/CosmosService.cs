using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AINovelWriter.Shared.Models;
using Microsoft.Azure.Cosmos.Linq;

namespace AINovelWriter.Shared.Services;

/// <summary>
/// Provides methods for interacting with Azure Cosmos DB containers for user novels, user profiles, and shared novels.
/// </summary>
public class CosmosService(CosmosClient cosmosClient)
{
    private const string DbName = "UserNovels";
    private const string NovelContainerName = "SavedNovels";
    private const string UserContainerName = "Users";
    private const string SharedContainerName = "SharedNovels";
    private readonly Container _novelContainer = cosmosClient.GetContainer(DbName, NovelContainerName);
    private readonly Container _userContainer = cosmosClient.GetContainer(DbName, UserContainerName);
    private readonly Container _sharedContainer = cosmosClient.GetContainer(DbName, SharedContainerName);

    /// <summary>
    /// Shares a user's novel by copying it to the shared novels container.
    /// </summary>
    /// <param name="username">The username of the novel's owner.</param>
    /// <param name="novelId">The ID of the novel to share.</param>
    /// <returns>A tuple indicating success and a message.</returns>
    public async Task<(bool, string)> ShareNovel(SharedNovelInfo sharedNovel)
    {
        try
        {
            // Fetch the novel from the user's container
            //var response = await _novelContainer.ReadItemAsync<NovelInfo>(sharedNovel.id, new PartitionKey(sharedNovel.SharedBy));
            
            // Optionally, you could set a flag or modify the novel to indicate it's shared
            // For now, just upsert as-is
            var result = await _sharedContainer.UpsertItemAsync(sharedNovel, new PartitionKey(sharedNovel.SharedBy));
            var message = result.StatusCode == System.Net.HttpStatusCode.Created ? "Novel shared (created)" : "Novel shared (replaced)";
            return (true, message);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to share novel: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all shared novels from the shared novels container.
    /// </summary>
    /// <returns>A list of shared <see cref="SharedNovelInfo"/> objects.</returns>
    public async Task<List<SharedNovelInfo>> GetAllSharedNovels()
    {
        var sharedNovels = new List<SharedNovelInfo>();
        try
        {
            var query = _sharedContainer.GetItemLinqQueryable<SharedNovelInfo>(allowSynchronousQueryExecution: false);
            var feed = query.ToFeedIterator();
            while (feed.HasMoreResults)
            {
                var results = await feed.ReadNextAsync();
                sharedNovels.AddRange(results);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\u001b[31mError fetching shared novels: {ex.Message}\u001b[0m");
        }
        return sharedNovels;
    }

    /// <summary>
    /// Removes a shared novel from the shared container.
    /// Only novels shared by the current user can be removed.
    /// </summary>
    /// <param name="username">The username of the user who shared the novel.</param>
    /// <param name="sharedNovelId">The ID of the shared novel to remove.</param>
    /// <returns>A tuple indicating success and a message.</returns>
    public async Task<(bool, string)> RemoveSharedNovel(string username, string sharedNovelId)
    {
        try
        {
            // First retrieve the shared novel to verify ownership
            var response = await _sharedContainer.ReadItemAsync<SharedNovelInfo>(sharedNovelId, new PartitionKey(username));
            var sharedNovel = response.Resource;

            // Verify that the current user is the one who shared it
            if (sharedNovel.SharedBy != username)
            {
                return (false, "You can only remove novels that you've shared");
            }

            // Delete the item from the shared container
            await _sharedContainer.DeleteItemAsync<SharedNovelInfo>(sharedNovelId, new PartitionKey(username));

            return (true, "Shared novel was successfully removed");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (false, "Shared novel not found");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to remove shared novel: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves the user profile for the specified username, including their saved novels.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>The <see cref="UserData"/> profile, or null if not found.</returns>
    public async Task<UserData?> GetUserProfile(string username)
    {
        try
        {
            Console.WriteLine($"\u001b[32mGetting user profile for {username}\u001b[0m");
            UserData item = await _userContainer.ReadItemAsync<UserData>(username, new PartitionKey(username));
            var sharedIds = item.SavedNovels.Where(x => x.IsShared).Select(x => x.NovelId).ToList();
            var query = _novelContainer.GetItemLinqQueryable<NovelInfo>().Where(x => x.User == username);
            var feed = query.ToFeedIterator();
            item.SavedNovels.Clear();
            while (feed.HasMoreResults)
            {
                var results = await feed.ReadNextAsync();
                if (results.Count == 0) Console.WriteLine($"\u001b[31mNovels not found for {username}\u001b[0m");
                item.SavedNovels.AddRange(results.Select(x => new UserNovelData(x.id, x.Title, x.CreatedOn){IsShared = sharedIds.Contains(x.id)}));
            }
            return item;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\u001b[31mUser not found. Creating new user profile for {username}\n\nError:{ex}\u001b[0m");
            return null;
        }
    }

    /// <summary>
    /// Retrieves a specific novel for a user by novel ID.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="novelId">The ID of the novel to retrieve.</param>
    /// <returns>The <see cref="NovelInfo"/> object.</returns>
    /// <exception cref="Exception">Thrown if the novel cannot be found or rehydrated.</exception>
    public async Task<NovelInfo> GetUserNovel(string username, string novelId)
    {
        try
        {
            var item = await _novelContainer.ReadItemAsync<NovelInfo>(novelId, new PartitionKey(username));
            return item;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting item. Rehydrating user novels\n+++++++++++++++++++++\n" + ex.Message + "\n");
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

    /// <summary>
    /// Attempts to find and update the user's saved novels list by querying the novels container.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>A tuple indicating success and a message.</returns>
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

    /// <summary>
    /// Saves a user's novel and updates their profile with the new novel if necessary.
    /// </summary>
    /// <param name="novel">The novel to save.</param>
    /// <param name="userData">The user profile data.</param>
    /// <returns>A tuple indicating success and a message.</returns>
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

    /// <summary>
    /// Saves or updates the user profile in the users container.
    /// </summary>
    /// <param name="user">The user profile to save.</param>
    public async Task SaveUser(UserData user)
    {
        await _userContainer.UpsertItemAsync(user, new PartitionKey(user.UserName));
    }

    /// <summary>
    /// Attempts to upsert (insert or update) a novel in the novels container.
    /// </summary>
    /// <param name="novel">The novel to upsert.</param>
    /// <returns>A tuple indicating success and a message.</returns>
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

    /// <summary>
    /// Deletes a user's novel from the novels container and updates their profile.
    /// </summary>
    /// <param name="user">The user profile.</param>
    /// <param name="novelId">The ID of the novel to delete.</param>
    /// <returns>A tuple indicating success and a message.</returns>
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
