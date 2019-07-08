using System.Collections.Generic;
using System.Linq;
namespace Blish_HUD {
    public static class DictionaryExtension
    {
        /// <summary>
        /// Merges an array of dictionaries into another dictionary, resolving dublicates if demanded.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="main">The dictionary to merge into.</param>
        /// <param name="update">If values of existing keys should be updated.</param>
        /// <param name="dictionaries">The array of dictionaries to merge.</param>
        public static void MergeLeft<TKey, TValue>(
            this Dictionary<TKey, TValue> main, bool update = false,
            params Dictionary<TKey, TValue>[] dictionaries)
        {
            foreach (var dictionary in dictionaries)
            {
                foreach (var item in dictionary)
                {
                    if (!main.ContainsKey(item.Key) || update)
                    {
                        main[item.Key] = item.Value;
                    }
                }
            }
        }
        /// <summary>
        /// Merges dictionaries into another dictionary and returns a new dictionary from the result.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="main">The dictionary to merge into.</param>
        /// <param name="dictionaries">The array of dictionaries to merge.</param>
        /// <returns>A new dictionary from the merge result.</returns>
        public static T MergeLeft<T, TKey, TValue>(
            this T main,
            params Dictionary<TKey, TValue>[] dictionaries)
            where T : IDictionary<TKey, TValue>, new()
        {
            T new_dictionary = new T();
            foreach (IDictionary<TKey, TValue> src in
                (new List<IDictionary<TKey, TValue>> { main }).Concat(dictionaries)) {
                foreach (KeyValuePair<TKey, TValue> p in src)
                {
                    new_dictionary[p.Key] = p.Value;
                }
            }
            return new_dictionary;
        }
    }
}
