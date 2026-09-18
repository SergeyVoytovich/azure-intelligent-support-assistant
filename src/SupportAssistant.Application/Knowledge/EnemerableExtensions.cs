namespace SupportAssistant.Application.Knowledge;

public static class EnemerableExtensions
{
    private static bool IsIndexIn<T>(this IList<T> source, int index)
        => index >= 0 && index <= source.Count - 1;

    private static T? ByIndexOrDefault<T>(this IList<T> source, int index)
        => source.IsIndexIn(index) ?  source[index] : default;

    public static T? Preview<T>(this IList<T> source, T current)
        => source.ByIndexOrDefault(source.IndexOf(current) - 1);

    public static T? Next<T>(this IList<T> source, T current)
        => source.ByIndexOrDefault(source.IndexOf(current) + 1);


    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, IEnumerable<T> append)
        => append.Aggregate(source, (current, item) => current.Append(item));

    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, IEnumerable<T> append)
        => append.Aggregate(source, (current, item) => current.Prepend(item));
}
