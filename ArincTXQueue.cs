using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOT
{
    public class ArincTXQueue
    {
        public ConcurrentDictionary<string, ArincTX> dictionary = new ConcurrentDictionary<string, ArincTX>();

        public bool AddToCollection(ArincTX queue, string id)
        {
            return dictionary.TryAdd(id, queue);
        }


        public ICollection<ArincTX> Values()
        {
            return dictionary.Values;
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool RemoveItem(string id)
        {
            ArincTX removeQueue;
            if (dictionary.ContainsKey(id))
            {
                return dictionary.TryRemove(id, out removeQueue);
            }
            else
            {
                return true;
            }
        }

        public bool UpdateItem(string id, ArincTX newvalue)
        {

            if (dictionary.ContainsKey(id))
            {
                ArincTX compValue;
                if (dictionary.TryGetValue(id, out compValue))
                {
                    return dictionary.TryUpdate(id, newvalue, compValue);
                }
                return false;
            }
            else
            {
                return false;
            }
        }


        public int Count()
        {
            return dictionary.Count;
        }

        public bool Search(string id)
        {
            if (dictionary.ContainsKey(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public class ArincTX
        {
            public string name;
            public byte channel;
            public byte address;
            public byte low;
            public byte hi;
            public byte sup;
            public bool enable;
            public string id;
            public int freq;
            public int numberpacket;
            public bool isNonContinue = false;
            public long time;
        }
    }

}

