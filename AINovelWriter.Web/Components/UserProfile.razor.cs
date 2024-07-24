using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AINovelWriter.Web.Components
{
	public partial class UserProfile 
	{
		[Inject]
		private CosmosService CosmosService { get; set; } = default!;
		[Inject]
		private DialogService Dialog { get; set; } = default!;
		private async Task SelectNovel(UserNovelData userNovelData)
		{
			var novel = await CosmosService.GetUserNovel(AppState.UserData.UserName, userNovelData.NovelId);
			AppState.NovelInfo = novel;
			Dialog.Close();
		}
	}
}
