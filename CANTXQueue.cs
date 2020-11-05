using System.Collections.Concurrent;
using System.Collections.Generic;
using static IOT.MCP2515;

namespace IOT
{
    public class CANTXQueue
    {
        public ConcurrentDictionary<string, CANMSG> dictionary = new ConcurrentDictionary<string, CANMSG>();

        public bool AddToCollection(CANMSG queue, string id)
        {
            return dictionary.TryAdd(id, queue);
        }


        public ICollection<CANMSG> Values()
        {
            return dictionary.Values;
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool RemoveItem(string id)
        {
            CANMSG removeQueue;
            if (dictionary.ContainsKey(id))
            {
                return dictionary.TryRemove(id, out removeQueue);
            }
            else
            {
                return true;
            }
        }

        public bool UpdateItem(string id, CANMSG newvalue)
        {

            if (dictionary.ContainsKey(id))
            {
                CANMSG compValue;
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

    }
}