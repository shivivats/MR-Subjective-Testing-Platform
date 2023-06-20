using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper
{
	internal static class EnumerableExtensions
	{
#if !NET45
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> collection)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		{
			await foreach (var o in collection.ConfigureAwait(false))
			{
				if (o != null)
				{
					return o;
				}
			}

			return default(T);
		}
#endif
	}
}
