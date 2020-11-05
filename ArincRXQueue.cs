using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOT
{
    public class ArincRXQueue
    {
   
            private ConcurrentDictionary<string, ConcurrentQueue<ArincRX>> _mailQueues = new ConcurrentDictionary<string, ConcurrentQueue<ArincRX>>();

            public void AddMailQueue(ArincRX message)
            {
                foreach (var variable in _mailQueues.Values)
                {
                    variable.Enqueue(message);
                }

            }

            public bool AddQueueToCollection(ConcurrentQueue<ArincRX> queue, string id)
            {
                return _mailQueues.TryAdd(id, queue);
            }

            public bool RemoveQueueToCollection(string id)
            {
                ConcurrentQueue<ArincRX> removeQueue;
                if (_mailQueues.ContainsKey(id))
                {
                    return _mailQueues.TryRemove(id, out removeQueue);
                }
                else
                {
                    return true;
                }
            }
        }
        public class ArincRX
        {
            public UInt32 data { get; set; }
            public Timestamp DateTime { get; set; }

        }

}
