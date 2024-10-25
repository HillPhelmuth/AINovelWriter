using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AINovelWriter.Shared.Models;

public enum NovelGenre
{
    None,
    [SubGenre("High Fantasy, Low Fantasy, Dark Fantasy, Urban Fantasy, Sword and Sorcery")]
    [Display("Fantasy")]
    Fantasy,
    [SubGenre("Hard Sci-Fi, Soft Sci-Fi, Space Opera, Cyberpunk, Dystopian")]
    [Display("Science Fiction")]
    ScienceFiction,
    [SubGenre("Cozy Mystery, Hard-Boiled Mystery, Police Procedural, Noir, Historical Mystery")]
    [Display("Mystery")]
    Mystery,
    [SubGenre("Contemporary Romance, Historical Romance, Paranormal Romance, Romantic Suspense, Erotic Romance")]
    [Display("Romance")]
    Romance,
    [SubGenre("Psychological Thriller, Crime Thriller, Legal Thriller, Espionage Thriller, Horror Thriller")]
    [Display("Thriller")]
    Thriller,
    [SubGenre("Alternative History, Military History, Saga, Biographical Fiction")]
    [Display("Historical Fiction")]
    HistoricalFiction,
    [SubGenre("Gothic Horror, Cosmic Horror, Slasher, Paranormal Horror, Body Horror")]
    [Display("Horror")]
    Horror,
    [SubGenre("Contemporary YA, Fantasy YA, Sci-Fi YA, Romance YA, Dystopian YA")]
    [Display("Young Adult")]
    YoungAdult,
    [SubGenre("Modernist Literature, Postmodern Literature, Magical Realism, Experimental Fiction")]
    [Display("Literary Fiction")]
    LiteraryFiction,
    [SubGenre("Post-Apocalyptic, Apocalyptic, Political Dystopia, Social Dystopia")]
    [Display("Dystopian")]
    Dystopian,
    [SubGenre("Action-Adventure, Historical Adventure, Swashbuckler, Survival")]
    [Display("Adventure")]
    Adventure,
    [SubGenre("Detective Fiction, Legal Thriller, Gangster Fiction, Noir, Hard-Boiled")]
    [Display("Crime")]
    Crime,
    [SubGenre("Fabulism, Surrealism, Magic Realism")]
    [Display("Magical Realism")]
    MagicalRealism,
    [SubGenre("Slice of Life, Social Commentary, Domestic Fiction, Coming-of-Age")]
    [Display("Contemporary")]
    Contemporary,
    [SubGenre("Classic Western, Contemporary Western, Weird Western, Space Western")]
    [Display("Western")]
    Western
}
public enum GenreCategory
{
   
    [Description("Fiction exploring diverse themes and settings, focusing on imaginative narratives.")]
    [GenreType(typeof(ThemeFiction))]
    [Display("Themes Fiction")]
    ThemeFiction,

    [Description("Distinct styles and approaches to storytelling within the realm of fiction.")]
    [GenreType(typeof(GenreFiction))]
    [Display("Genre Fiction")]
    GenreFiction,
    [Description("A mix of crime and mystery, often with suspenseful and thrilling narratives.")]
    [GenreType(typeof(CrimeAndThrillers))]
    [Display("Crime and Thrillers")]
    CrimeAndThrillers,

    [Description("Fiction with magical, supernatural, or fantastical elements.")]
    [GenreType(typeof(Fantasy))]
    [Display("Fantasy")]
    Fantasy,

    [Description("Stories grounded in science or speculative scientific advancements.")]
    [GenreType(typeof(ScienceFiction))]
    [Display("Science Fiction")]
    ScienceFiction,

    [Description("Exciting, adventurous tales often involving daring exploits and escapades.")]
    [GenreType(typeof(ActionAndAdventure))]
    [Display("Action and Adventure")]
    ActionAndAdventure
}

public enum ThemeFiction
{
    [Description("Fiction that delves into the absurd, often highlighting the illogical aspects of human existence.")]
    [Display("Absurdist Fiction")]
    AbsurdistFiction,

    [Description("Stories that center around animals, either as main characters or as central themes.")]
    [Display("Animal Fiction")]
    AnimalFiction,

    [Description("Fiction that explores the intricacies of city life and urban experiences.")]
    [Display("City Life Fiction")]
    CityLifeFiction,

    [Description("Stories that follow characters through the challenges and growth of adolescence.")]
    [Display("Coming-of-Age Fiction")]
    ComingOfAgeFiction,

    [Description("Fiction that focuses on the dynamics and relationships within families.")]
    [Display("Family Life Fiction")]
    FamilyLifeFiction,

    [Description("Multi-generational sagas centered around the history and evolution of a family.")]
    [Display("Family Saga Fiction")]
    FamilySagaFiction,

    [Description("Light-hearted fiction intended to uplift and provide a feel-good experience.")]
    [Display("Feel-Good Fiction")]
    FeelGoodFiction,

    [Description("Stories centered on the deep bonds and trials of friendship.")]
    [Display("Friendship Fiction")]
    FriendshipFiction,

    [Description("Stories themed around holidays, often imbued with festive spirit.")]
    [Display("Holiday Fiction")]
    HolidayFiction,

    [Description("Fiction that explores LGBTQ+ themes, characters, and experiences.")]
    [Display("LGBTQ+ Genre Fiction")]
    LGBTQPlusGenreFiction,

    [Description("Stories centered around medical themes, hospitals, or the healthcare industry.")]
    [Display("Medical Fiction")]
    MedicalFiction,

    [Description("Fiction that captures the essence of life in small towns or rural settings.")]
    [Display("Small Town and Rural Fiction")]
    SmallTownAndRuralFiction,

    [Description("Stories deeply rooted in the traditions, culture, and atmosphere of the American South.")]
    [Display("Southern Fiction")]
    SouthernFiction,

    [Description("Fiction that revolves around sports, athletes, and competitive events.")]
    [Display("Sports Fiction")]
    SportsFiction,

    [Description("Urban-centered stories, often focusing on the complexities of modern city life.")]
    [Display("Urban Life Fiction")]
    FictionUrbanLife,

    [Description("Tales of life in the American West, often with cowboys, outlaws, and frontier life.")]
    [Display("Westerns")]
    Westerns
}

public enum GenreFiction
{
    [Description("Narratives based on or inspired by real-life biographies.")]
    [Display("Biographical Fiction")]
    BiographicalFiction,

    [Description("Narratives told through letters, diary entries, or other forms of written communication.")]
    [Display("Epistolary Fiction")]
    EpistolaryFiction,

    [Description("Fiction that combines elements of gaming and literature, often with RPG elements.")]
    [Display("GameLit and LitRPG Fiction")]
    GameLitAndLitRPGFiction,

    [Description("Dark, mysterious tales with a focus on the macabre and the supernatural.")]
    [Display("Gothic Fiction")]
    GothicFiction,

    [Description("Fiction set in or inspired by historical events or periods.")]
    [Display("Historical Fiction")]
    HistoricalFiction,

    [Description("Literature meant to evoke fear, horror, or suspenseful dread.")]
    [Display("Horror Literature and Fiction")]
    HorrorLiteratureAndFiction,

    [Description("A blend of various genres, creating unique and often experimental fiction.")]
    [Display("Mashup Fiction")]
    MashupFiction,

    [Description("Fiction that delves into spiritual, existential, or philosophical themes.")]
    [Display("Metaphysical and Visionary Fiction")]
    MetaphysicalAndVisionaryFiction,

    [Description("Stories with a focus on political themes, ideologies, and conflicts.")]
    [Display("Political Fiction")]
    PoliticalFiction,

    [Description("Fiction that explores the intricacies of the human mind and psychological states.")]
    [Display("Psychological Fiction")]
    PsychologicalFiction,

    [Description("Literature with strong religious or spiritual themes.")]
    [Display("Religious Literature and Fiction")]
    ReligiousLiteratureAndFiction,

    [Description("Fiction that uses humor and irony to critique or satirize aspects of society.")]
    [Display("Satirical Fiction")]
    FictionSatire,

    [Description("Stories centered around seafaring adventures and life at sea.")]
    [Display("Sea Stories")]
    SeaStories,

    [Description("Fiction based on wars, both historical and speculative.")]
    [Display("War Fiction")]
    WarFiction
}
public enum CrimeAndThrillers
{
    [Description("Thrillers focused on crime investigations, often involving detectives and criminal minds.")]
    [Display("Crime Thrillers")]
    CrimeThrillers,

    [Description("Domestic-centered thrillers with a focus on family, relationships, and home life.")]
    [Display("Domestic Thrillers")]
    DomesticThrillers,

    [Description("Thrillers with a financial twist, involving corporate espionage, banking, or the stock market.")]
    [Display("Financial Thrillers")]
    FinancialThrillers,

    [Description("Historical settings provide the backdrop for these suspenseful thrillers.")]
    [Display("Historical Thrillers")]
    HistoricalThrillers,

    [Description("Thrillers centered around legal battles, courtrooms, and justice.")]
    [Display("Legal Thrillers")]
    LegalThrillers,

    [Description("Medical-themed thrillers often set in hospitals or involving health crises.")]
    [Display("Medical Thrillers")]
    MedicalThrillers,

    [Description("Military-focused thrillers involving armed forces, war strategies, and battlefield suspense.")]
    [Display("Military Thrillers")]
    MilitaryThrillers,

    [Description("Psychologically intense thrillers exploring mental states and mind games.")]
    [Display("Psychological Thrillers")]
    PsychologicalThrillers,

    [Description("Thrillers involving spies, espionage, and political intrigue.")]
    [Display("Spies and Political Thrillers")]
    SpiesAndPoliticalThrillers,

    [Description("Supernatural elements add a chilling twist to these thrillers.")]
    [Display("Supernatural Thrillers")]
    SupernaturalThrillers,

    [Description("High-stakes thrillers with intense suspense and action-packed narratives.")]
    [Display("Suspense Thrillers")]
    SuspenseThrillers,

    [Description("Thrillers focused on technology, cyber warfare, and futuristic scenarios.")]
    [Display("Techno Thrillers")]
    Technothrillers
}
public enum Fantasy
{
    [Description("Fantasy with epic battles, legendary heroes, and grand quests.")]
    [Display("High Fantasy")]
    HighFantasy,

    [Description("Fantasy with a more grounded, realistic tone and setting.")]
    [Display("Low Fantasy")]
    LowFantasy,

    [Description("Dark, often grim fantasy with elements of horror or moral ambiguity.")]
    [Display("Dark Fantasy")]
    DarkFantasy,

    [Description("Fantasy set in modern urban environments, blending the magical with the mundane.")]
    [Display("Urban Fantasy")]
    UrbanFantasy,

    [Description("Heroic fantasy with elements of swordplay, magic, and adventure.")]
    [Display("Sword and Sorcery")]
    SwordAndSorcery
}
public enum ScienceFiction
{
    [Description("Sci-fi with a focus on scientific accuracy and realistic technology.")]
    [Display("Hard Sci-Fi")]
    HardSciFi,

    [Description("Sci-fi with a more imaginative or speculative approach, often with less focus on scientific realism.")]
    [Display("Soft Sci-Fi")]
    SoftSciFi,

    [Description("Epic space adventures often involving starships, intergalactic battles, and large-scale conflicts.")]
    [Display("Space Opera")]
    SpaceOpera,

    [Description("Sci-fi exploring dystopian futures, often with themes of societal collapse or authoritarian regimes.")]
    [Display("Dystopian")]
    Dystopian,

    [Description("A subgenre of sci-fi centered on futuristic, high-tech societies, often with a focus on hacking and AI.")]
    [Display("Cyberpunk")]
    Cyberpunk,
    [Description("High tech, near future espionage that blends the elements of espionage thrillers with science fiction concepts.")]
    SpyFi
}
public enum ActionAndAdventure
{
    [Description("Classic tales of adventure, often with daring heroes and exotic settings.")]
    [Display("Classic Action and Adventure")]
    ClassicActionAndAdventure,
    [Description("Modern stories of espionage, intrigue, and high-tech action.")]
    [Display("Modern Espionage Action")]
    ModernEspionageAction,
    [Description("Adventure stories set in fantastical worlds with elements of magic and myth.")]
    [Display("Fantasy Action and Adventure")]
    FantasyActionAndAdventure,

    [Description("Action-packed fiction with a focus on male protagonists and traditional masculine themes.")]
    [Display("Men's Adventure Fiction")]
    MensAdventureFiction,

    [Description("Thrilling adventures with elements of mystery, suspense, and high-stakes action.")]
    [Display("Mystery, Thriller, and Suspense Action Fiction")]
    MysteryThrillerAndSuspenseActionFiction,

    [Description("Adventure stories with a romantic subplot, often with high-stakes drama and excitement.")]
    [Display("Romantic Action and Adventure")]
    RomanticActionAndAdventure,

    [Description("Sci-fi adventures with a focus on exploration, technology, and futuristic escapades.")]
    [Display("Science Fiction Adventures")]
    ScienceFictionAdventures,

    [Description("Superhero-themed adventures with larger-than-life characters and epic battles.")]
    [Display("Superhero Action Adventure")]
    SuperHeroActionAdventure,

    [Description("Adventure stories centered around the high seas, with sailors, pirates, and naval battles.")]
    [Display("Sea Adventures Fiction")]
    SeaAdventuresFiction,

    [Description("Short stories with fast-paced action and adventurous plots.")]
    [Display("Action and Adventure Short Stories")]
    ActionAndAdventureShortStories,

    [Description("Military-themed adventures with a focus on war, combat, and military strategies.")]
    [Display("War and Military Action Fiction")]
    WarAndMilitaryActionFiction,

    [Description("Adventure fiction featuring strong female protagonists and empowering narratives.")]
    [Display("Women's Adventure Fiction")]
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
				Name = category.GetDisplayName(),
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
				Name = genre.GetDisplayName(),
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
