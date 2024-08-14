using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AINovelWriter.Shared.Models;

public enum NovelGenre
{
	None,
	[SubGenre("High Fantasy, Low Fantasy, Dark Fantasy, Urban Fantasy, Sword and Sorcery")]
	Fantasy,
	[SubGenre("Hard Sci-Fi, Soft Sci-Fi, Space Opera, Cyberpunk, Dystopian")]
	ScienceFiction,
	[SubGenre("Cozy Mystery, Hard-Boiled Mystery, Police Procedural, Noir, Historical Mystery")]
	Mystery,
	[SubGenre("Contemporary Romance, Historical Romance, Paranormal Romance, Romantic Suspense, Erotic Romance")]
	Romance,
	[SubGenre("Psychological Thriller, Crime Thriller, Legal Thriller, Espionage Thriller, Horror Thriller")]
	Thriller,
	[SubGenre("Alternative History, Military History, Saga, Biographical Fiction")]
	HistoricalFiction,
	[SubGenre("Gothic Horror, Cosmic Horror, Slasher, Paranormal Horror, Body Horror")]
	Horror,
	[SubGenre("Contemporary YA, Fantasy YA, Sci-Fi YA, Romance YA, Dystopian YA")]
	YoungAdult,
	[SubGenre("Modernist Literature, Postmodern Literature, Magical Realism, Experimental Fiction")]
	LiteraryFiction,
	[SubGenre("Post-Apocalyptic, Apocalyptic, Political Dystopia, Social Dystopia")]
	Dystopian,
	[SubGenre("Action-Adventure, Historical Adventure, Swashbuckler, Survival")]
	Adventure,
	[SubGenre("Detective Fiction, Legal Thriller, Gangster Fiction, Noir, Hard-Boiled")]
	Crime,
	[SubGenre("Fabulism, Surrealism, Magic Realism")]
	MagicalRealism,
	[SubGenre("Slice of Life, Social Commentary, Domestic Fiction, Coming-of-Age")]
	Contemporary,
	[SubGenre("Classic Western, Contemporary Western, Weird Western, Space Western")]
	Western
}
public enum GenreCategory
{
	[Description("Various forms of literature focusing on imaginative storytelling.")]
	[GenreType(typeof(LiteratureAndFiction))]
	LiteratureAndFiction,

	[Description("A mix of crime and mystery, often with suspenseful and thrilling narratives.")]
	[GenreType(typeof(CrimeAndThrillers))]
	CrimeAndThrillers,

	[Description("Fiction with magical, supernatural, or fantastical elements.")]
	[GenreType(typeof(Fantasy))]
	Fantasy,

	[Description("Stories grounded in science or speculative scientific advancements.")]
	[GenreType(typeof(ScienceFiction))]
	ScienceFiction,

	[Description("Exciting, adventurous tales often involving daring exploits and escapades.")]
	[GenreType(typeof(ActionAndAdventure))]
	ActionAndAdventure
}
public enum LiteratureAndFiction
{
	[Description("Fiction that delves into the absurd, often highlighting the illogical aspects of human existence.")]
	AbsurdistFiction,

	[Description("Stories that center around animals, either as main characters or as central themes.")]
	AnimalFiction,

	[Description("Narratives based on or inspired by real-life biographies.")]
	BiographicalFiction,

	[Description("Fiction that explores the intricacies of city life and urban experiences.")]
	CityLifeFiction,

	[Description("Stories that follow characters through the challenges and growth of adolescence.")]
	ComingOfAgeFiction,

	[Description("Narratives told through letters, diary entries, or other forms of written communication.")]
	EpistolaryFiction,

	[Description("Fiction that focuses on the dynamics and relationships within families.")]
	FamilyLifeFiction,

	[Description("Multi-generational sagas centered around the history and evolution of a family.")]
	FamilySagaFiction,

	[Description("Light-hearted fiction intended to uplift and provide a feel-good experience.")]
	FeelGoodFiction,

	[Description("Stories centered on the deep bonds and trials of friendship.")]
	FriendshipFiction,

	[Description("Fiction that combines elements of gaming and literature, often with RPG elements.")]
	GameLitAndLitRPGFiction,

	[Description("Dark, mysterious tales with a focus on the macabre and the supernatural.")]
	GothicFiction,

	[Description("Fiction set in or inspired by historical events or periods.")]
	HistoricalFiction,

	[Description("Stories themed around holidays, often imbued with festive spirit.")]
	HolidayFiction,

	[Description("Literature meant to evoke fear, horror, or suspenseful dread.")]
	HorrorLiteratureAndFiction,

	[Description("Fiction that explores LGBTQ+ themes, characters, and experiences.")]
	LGBTQPlusGenreFiction,

	[Description("A blend of various genres, creating unique and often experimental fiction.")]
	MashupFiction,

	[Description("Stories centered around medical themes, hospitals, or the healthcare industry.")]
	MedicalFiction,

	[Description("Fiction that delves into spiritual, existential, or philosophical themes.")]
	MetaphysicalAndVisionaryFiction,

	[Description("Stories with a focus on political themes, ideologies, and conflicts.")]
	PoliticalFiction,

	[Description("Fiction that explores the intricacies of the human mind and psychological states.")]
	PsychologicalFiction,

	[Description("Literature with strong religious or spiritual themes.")]
	ReligiousLiteratureAndFiction,

	[Description("Fiction that uses humor and irony to critique or satirize aspects of society.")]
	FictionSatire,

	[Description("Stories centered around seafaring adventures and life at sea.")]
	SeaStories,

	[Description("Fiction that captures the essence of life in small towns or rural settings.")]
	SmallTownAndRuralFiction,

	[Description("Stories deeply rooted in the traditions, culture, and atmosphere of the American South.")]
	SouthernFiction,

	[Description("Fiction that revolves around sports, athletes, and competitive events.")]
	SportsFiction,

	[Description("Urban-centered stories, often focusing on the complexities of modern city life.")]
	FictionUrbanLife,

	[Description("Fiction based on wars, both historical and speculative.")]
	WarFiction,

	[Description("Tales of life in the American West, often with cowboys, outlaws, and frontier life.")]
	Westerns
}
public enum CrimeAndThrillers
{
	[Description("Thrillers focused on crime investigations, often involving detectives and criminal minds.")]
	CrimeThrillers,

	[Description("Domestic-centered thrillers with a focus on family, relationships, and home life.")]
	DomesticThrillers,

	[Description("Thrillers with a financial twist, involving corporate espionage, banking, or the stock market.")]
	FinancialThrillers,

	[Description("Historical settings provide the backdrop for these suspenseful thrillers.")]
	HistoricalThrillers,

	[Description("Thrillers centered around legal battles, courtrooms, and justice.")]
	LegalThrillers,

	[Description("Medical-themed thrillers often set in hospitals or involving health crises.")]
	MedicalThrillers,

	[Description("Military-focused thrillers involving armed forces, war strategies, and battlefield suspense.")]
	MilitaryThrillers,

	[Description("Psychologically intense thrillers exploring mental states and mind games.")]
	PsychologicalThrillers,

	[Description("Thrillers involving spies, espionage, and political intrigue.")]
	SpiesAndPoliticalThrillers,

	[Description("Supernatural elements add a chilling twist to these thrillers.")]
	SupernaturalThrillers,

	[Description("High-stakes thrillers with intense suspense and action-packed narratives.")]
	SuspenseThrillers,

	[Description("Thrillers focused on technology, cyber warfare, and futuristic scenarios.")]
	Technothrillers
}
public enum Fantasy
{
	[Description("Fantasy with epic battles, legendary heroes, and grand quests.")]
	HighFantasy,

	[Description("Fantasy with a more grounded, realistic tone and setting.")]
	LowFantasy,

	[Description("Dark, often grim fantasy with elements of horror or moral ambiguity.")]
	DarkFantasy,

	[Description("Fantasy set in modern urban environments, blending the magical with the mundane.")]
	UrbanFantasy,

	[Description("Heroic fantasy with elements of swordplay, magic, and adventure.")]
	SwordAndSorcery
}
public enum ScienceFiction
{
	[Description("Sci-fi with a focus on scientific accuracy and realistic technology.")]
	HardSciFi,

	[Description("Sci-fi with a more imaginative or speculative approach, often with less focus on scientific realism.")]
	SoftSciFi,

	[Description("Epic space adventures often involving starships, intergalactic battles, and large-scale conflicts.")]
	SpaceOpera,

	[Description("Sci-fi exploring dystopian futures, often with themes of societal collapse or authoritarian regimes.")]
	Dystopian,

	[Description("A subgenre of sci-fi centered on futuristic, high-tech societies, often with a focus on hacking and AI.")]
	Cyberpunk
}
public enum ActionAndAdventure
{
	[Description("Classic tales of adventure, often with daring heroes and exotic settings.")]
	ClassicActionAndAdventure,

	[Description("Adventure stories set in fantastical worlds with elements of magic and myth.")]
	FantasyActionAndAdventure,

	[Description("Action-packed fiction with a focus on male protagonists and traditional masculine themes.")]
	MensAdventureFiction,

	[Description("Thrilling adventures with elements of mystery, suspense, and high-stakes action.")]
	MysteryThrillerAndSuspenseActionFiction,

	[Description("Adventure stories with a romantic subplot, often with high-stakes drama and excitement.")]
	RomanticActionAndAdventure,

	[Description("Sci-fi adventures with a focus on exploration, technology, and futuristic escapades.")]
	ScienceFictionAdventures,

	[Description("Adventure stories centered around the high seas, with sailors, pirates, and naval battles.")]
	SeaAdventuresFiction,

	[Description("Short stories with fast-paced action and adventurous plots.")]
	ActionAndAdventureShortStories,

	[Description("Military-themed adventures with a focus on war, combat, and military strategies.")]
	WarAndMilitaryActionFiction,

	[Description("Adventure fiction featuring strong female protagonists and empowering narratives.")]
	WomensAdventureFiction
}

public class GenreForm
{
	public GenreCategory GenreCategory { get; set; }

}
public static class GenreExtensionMethods
{
	// An extention method to get a list of sub-genres from GenreType attribute for a given genre category
	public static List<T> GetSubGenres<T>(this GenreCategory genreCategory) where T : Enum
	{
		var subGenres = genreCategory.GetAttribute<GenreTypeAttribute>()?.GenreType;
		return subGenres == null ? [] : Enum.GetValues(subGenres).Cast<T>().ToList();
	}
	//{
	//    var genreType = genreCategory.GetAttribute<GenreTypeAttribute>()?.GenreType;
	//    return genreType == null ? [] : [.. Enum.GetNames(genreType)];
	//}

}


public class SubGenreAttribute(string subGenres) : Attribute
{
	public string SubGenres { get; set; } = subGenres;
}
public class GenreTypeAttribute(Type genreType) : Attribute
{
	public Type GenreType { get; set; } = genreType;
}



public class GenreConverter
{
	public static List<GenreCategoryItem> ToCategoryItems()
	{
		var genreCategories = Enum.GetValues(typeof(GenreCategory))
			.Cast<GenreCategory>()
			.Select(category => new GenreCategoryItem
			{
				Category = category,
				Name = category.ToString(),
				Description = category.GetDescription(),
				Genres = GetGenresForCategory(category).ToList()
			});

		return genreCategories.ToList();
		//return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
	}

	private static IEnumerable<Genre> GetGenresForCategory(GenreCategory category)
	{
		var genreType = GetGenreType(category);
		if (genreType == null)
		{
			return Enumerable.Empty<Genre>();
		}

		return Enum.GetValues(genreType)
			.Cast<Enum>()
			.Select(genre => new Genre
			{
				Name = genre.ToString(),
				Description = genre.GetDescription()
			});
	}

	private static Type GetGenreType(GenreCategory category)
	{
		var genreType = category.GetAttribute<GenreTypeAttribute>()?.GenreType;
		return genreType;
	}
}


public class GenreCategoryItem
{
	public GenreCategory Category { get; set; }
	[JsonPropertyName("Name")]
	public string Name { get; set; }

	[JsonPropertyName("Description")]
	public string Description { get; set; }

	[JsonPropertyName("Genres")]
	public List<Genre> Genres { get; set; }
}

public class Genre
{
	[JsonPropertyName("Name")]
	public string Name { get; set; }

	[JsonPropertyName("Description")]
	public string Description { get; set; }
	public override string ToString()
    {
        return $"{Name} - {Description}";
    }
}