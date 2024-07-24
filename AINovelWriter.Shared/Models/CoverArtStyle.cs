using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models;

public enum CoverArtStyle
{
	[Description("A highly realistic and detailed style, mimicking real-life photography.")]
	[Display("Photo-realistic")]
	PhotoRealistic,

	[Description("A versatile and modern style created using digital tools and software.")]
	[Display("Digital Art")]
	DigitalArt,

	[Description("A traditional and classic style, using oil paints for rich textures and colors.")]
	[Display("Oil Painting")]
	OilPainting,

	[Description("A fluid and expressive style, using watercolors for soft and translucent effects.")]
	[Display("Watercolor")]
	Watercolor,

	[Description("A non-representational and conceptual style, emphasizing shapes and colors.")]
	[Display("Abstract")]
	Abstract,

	[Description("A hand-drawn or digitally rendered style, often used for illustrations and comics.")]
	[Display("Illustrated")]
	Illustrated,

	[Description("A clean and simple style, focusing on minimal elements and negative space.")]
	[Display("Minimalist")]
	Minimalist,

	[Description("A vivid and imaginative style, depicting fantastical and mythical scenes.")]
	[Display("Fantasy Art")]
	FantasyArt,

	[Description("A futuristic and speculative style, often featuring advanced technology and space themes.")]
	[Display("Sci-Fi Art")]
	SciFiArt,

	[Description("A dreamlike and unconventional style, blending reality with fantastical elements.")]
	[Display("Surreal")]
	Surreal
}
public class DisplayAttribute : Attribute
{
	public string DisplayName { get; set; }

	public DisplayAttribute(string displayName)
	{
		DisplayName = displayName;
	}
}
