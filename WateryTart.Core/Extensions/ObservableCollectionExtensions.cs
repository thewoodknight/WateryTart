using System;
using System.Collections.ObjectModel;
using System.Linq;
using WateryTart.Core.Extensions;

namespace WateryTart.Core.Extensions;

public static class ObservableCollectionExtensions
{
    extension<T>(ObservableCollection<T> collection)
    {
        ///https://stackoverflow.com/questions/5118513/removeall-for-observablecollections
        public void RemoveAll(Func<T, bool> condition)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }

        public int Remove(Func<T, bool> condition)
        {
            var itemsToRemove = collection.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                collection.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
    }
}