namespace AINovelWriter.Shared.Models;

public static class CollectionExtensions
{
    public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (chunkSize <= 0) throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));

        var result = new List<List<T>>();
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            result.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
        }
        return result;
    }
    public static void Shuffle<T>(this List<T> list, Random? rng = null)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));

        rng ??= new Random();

        var count = list.Count;
        for (var i = count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]); // Swap elements
        }
    }
}