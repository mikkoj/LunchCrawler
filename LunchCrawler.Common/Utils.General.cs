using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


namespace LunchCrawler.Common
{
	public static partial class Utils
	{
        public static bool In<T>(this T source, params T[] list)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (!typeof(T).IsValueType && source == null) throw new ArgumentNullException("source");
            return list.Contains(source);
        }

        /// <summary>
        /// A cleaner way to do string.Format.
        /// </summary>
        public static string With(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        /// <summary>
        /// Creates a deep-clone of any given object.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">Object to be deep-cloned.</param>
        /// <returns>A deep-clone of the object.</returns>
        public static T DeepClone<T>(this T obj) where T : new()
        {
            if (!typeof(T).IsSerializable && !(typeof(T) is ISerializable))
            {
                throw new InvalidOperationException("A serializable Type is required");
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        public static IEnumerable<T> MergeContents<T>(this IEnumerable<T> source, IEnumerable<T> other, Func<T, T, T> mergeop)
        {
            if (mergeop == null)
                throw new ArgumentNullException("mergeop");

            if (source == null || source.Count() == 0)
                return other;
            else if (other == null || other.Count() == 0)
                return source;

            return source.SelectMany(item => other.Select(item2 => mergeop(item, item2)));
        }

        public static IEnumerable<TAcc> Scan<T, TAcc>(this IEnumerable<T> source, TAcc seed, Func<TAcc, T, TAcc> transformation)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (transformation == null)
                throw new ArgumentNullException("transformation");

            using (var i = source.GetEnumerator())
            {
                var newseed = seed;
                while (i.MoveNext())
                {
                    newseed = transformation(newseed, i.Current);
                    yield return newseed;
                }
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        public static string ParseInnerException(this Exception ex)
        {
            if (ex == null || string.IsNullOrEmpty(ex.Message))
            {
                return string.Empty;
            }

            var error = new StringBuilder(ex.Message);
            if (ex.InnerException != null)
            {
                error.Append("\n" + ParseInnerException(ex.InnerException));
            }

            return error.ToString();
        }
	}
}
