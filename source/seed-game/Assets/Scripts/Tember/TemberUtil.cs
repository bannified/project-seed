using System.Collections;
using System.Collections.Generic;
using System;


namespace Tember
{
    public class Util
    {
        /// <summary>
        /// Generic comparer class that allows for lambda expressions to be used over verbose Comparer classes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class FunctionalComparer<T> : IComparer<T>
        {
            private Func<T, T, int> comparer;
            public FunctionalComparer(Func<T, T, int> comparer)
            {
                this.comparer = comparer;
            }

            public static IComparer<T> Create(Func<T, T, int> comparer)
            {
                return new FunctionalComparer<T>(comparer);
            }
            public int Compare(T x, T y)
            {
                return comparer(x, y);
            }
        }
    }
}
