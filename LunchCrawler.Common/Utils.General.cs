using System;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


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
	}
}
