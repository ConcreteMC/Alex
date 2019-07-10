using System;
using System.Collections.Generic;

namespace Alex.API.Utils
{
    public static class LinqExtensions
    {
	    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
		    Func<TSource, TKey> selector)
	    {
		    return source.MinBy(selector, null);
	    }

	    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
		    Func<TSource, TKey> selector, IComparer<TKey> comparer)
	    {
		    try
		    {
			    if (source == null) throw new ArgumentNullException("source");
			    if (selector == null) throw new ArgumentNullException("selector");
			    comparer = comparer ?? Comparer<TKey>.Default;

			    using (var sourceIterator = source.GetEnumerator())
			    {
				    if (!sourceIterator.MoveNext())
				    {
					    return default(TSource);
                        throw new InvalidOperationException("Sequence contains no elements");
				    }

				    var min = sourceIterator.Current;
				    var minKey = selector(min);
				    while (sourceIterator.MoveNext())
				    {
					    var candidate = sourceIterator.Current;
					    var candidateProjected = selector(candidate);
					    if (comparer.Compare(candidateProjected, minKey) < 0)
					    {
						    min = candidate;
						    minKey = candidateProjected;
					    }
				    }

				    return min;
			    }
		    }
		    catch
		    {
			    return default(TSource);
		    }
	    }
    }
}
