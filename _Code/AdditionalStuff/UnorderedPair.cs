using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public class UnorderedPair<T> {
        public T t; public T u;
        public UnorderedPair(T t, T u) { this.t = t; this.u = u; }


    }
    public class UnorderedPairComparer<T> : IEqualityComparer<UnorderedPair<T>> {
        /// <summary>
        /// Will return true if x and y pairs match, no matter the order of them
        /// </summary>
        public bool Equals(UnorderedPair<T> x, UnorderedPair<T> y) {
            return (x.t.Equals(y.t) && x.u.Equals(y.u)) ||
                    (x.t.Equals(y.u) && x.u.Equals(y.t));
        }

        /// <summary>
        /// Hash code is based only on the field values, and not the
        /// object instance. Therefore, two different instances of
        /// UnorderedPair with matching fields have the same code and equal each other
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(UnorderedPair<T> obj) {
            return obj.t.GetHashCode() + obj.t.GetHashCode();
        }
    }
}
