using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Util.Extensions {
    public static class IEnumerableExtensions {
        public static T RandomElement<T>(this IEnumerable<T> collection) {
            if (collection.Count() == 0) return default(T);

            var index = UnityEngine.Random.Range(0, collection.Count());
            return collection.ElementAt(index);
        }

        public static T RandomElement<T>(this IEnumerable<T> collection, Func<T, bool> predicate) {
            collection = collection.Where(predicate);
            if (collection.Count() == 0) return default(T);

            var index = UnityEngine.Random.Range(0, collection.Count());
            return collection.ElementAt(index);
        }

        public static double WeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight) {
            double weightedValueSum = records.Sum(record => value(record) * weight(record));
            double weightSum = records.Sum(record => weight(record));

            if (weightSum != 0)
                return weightedValueSum / weightSum;
            else
                throw new DivideByZeroException("Weighted mean with 0 combined weight.");
        }
    }
}
