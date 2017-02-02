// Written by: Marlon Grech
// Source: http://marlongrech.wordpress.com/2008/03/20/more-than-just-mvc-for-wpf/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete
{
    /// <summary>
    /// The multi dictionary is a dictionary that contains
    /// more than one value per key
    /// </summary>
    /// <typeparam name="T">The type of the key</typeparam>
    /// <typeparam name="K">The type of the list contents</typeparam>
    public class MultiDictionary<T, K> : Dictionary<T, List<K>>
    {

        #region Private Methods
        //checks if the key is already present
        private void EnsureKey(T key)
        {
            if (!ContainsKey(key))
            {
                this[key] = new List<K>(1);
            }
            else
            {
                if (this[key] == null)
                    this[key] = new List<K>(1);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a new value in the Values collection
        /// </summary>
        /// <param name="key">The key where to place the
        /// item in the value list</param>
        /// <param name="newItem">The new item to add</param>
        public void AddValue(T key, K newItem)
        {
            EnsureKey(key);
            this[key].Add(newItem);
        }


        /// <summary>
        /// Adds a list of values to append to the value collection
        /// </summary>
        /// <param name="key">The key where to place the item in the value list</param>
        /// <param name="newItems">The new items to add</param>
        public void AddValues(T key, IEnumerable<K> newItems)
        {
            EnsureKey(key);
            this[key].AddRange(newItems);
        }

        /// <summary>
        /// Removes a specific element from the dict
        /// If the value list is empty the key is removed from the dict
        /// </summary>
        /// <param name="key">The key from where to remove the value</param>
        /// <param name="value">The value to remove</param>
        /// <returns>Returns false if the key was not found</returns>
        public bool RemoveValue(T key, K value)
        {
            if (!ContainsKey(key))
                return false;

            this[key].Remove(value);

            if (this[key].Count == 0)
                this.Remove(key);

            return true;
        }

        /// <summary>
        /// Removes all items that match the prediacte
        /// If the value list is empty the key is removed from the dict
        /// </summary>
        /// <param name="key">The key from where to remove the value</param>
        /// <param name="match">The predicate to match the items</param>
        /// <returns>Returns false if the key was not found</returns>
        public bool RemoveAllValue(T key, Predicate<K> match)
        {
            if (!ContainsKey(key))
                return false;

            this[key].RemoveAll(match);

            if (this[key].Count == 0)
                this.Remove(key);

            return true;
        }
        #endregion

    }
}
