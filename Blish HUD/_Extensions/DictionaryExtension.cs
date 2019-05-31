using System;
using System.Collections.Generic;
namespace Blish_HUD {
    public static class DictionaryExtension
    {
        /// <summary>
        /// Merges an array of dictionaries into another dictionary, resolving dublicates.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="main">The dictionary to merge into.</param>
        /// <param name="update">If values of existing keys should be updated.</param>
        /// <param name="dictionaries">The array of dictionaries to merge.</param>
        public static void MergeIn<TKey, TValue>(
            this Dictionary<TKey, TValue> main, bool update = false,
            params Dictionary<TKey, TValue>[] dictionaries)
        {
            foreach (var dictionary in dictionaries)
            {
                foreach (var item in dictionary)
                {
                    if (!main.ContainsKey(item.Key) && !update)
                    {
                        main[item.Key] = item.Value;
                    }
                }
            }
        }
    }
}
