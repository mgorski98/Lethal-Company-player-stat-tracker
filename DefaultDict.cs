using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalCompanyStatTracker {
    public class DefaultDict<K,V> : Dictionary<K,V> {
        private Func<V> defaultValueSupplier;

        public DefaultDict(Func<V> defaultValueFunc) : base() {
            defaultValueSupplier = defaultValueFunc;
        }

        public DefaultDict(V defaultValue) : base() {
            defaultValueSupplier = () => defaultValue;
        }

        public void CopyFrom(Dictionary<K,V> other) {
            foreach (var pair in other) {
                this[pair.Key] = pair.Value;
            }
        }

        public new V this[K key] {
            get {
                V val;
                if (!TryGetValue(key, out val)) {
                    val = defaultValueSupplier.Invoke();
                    Add(key, val);
                }
                return val;
            }
            set => base[key] = value;
        }
    }
}
