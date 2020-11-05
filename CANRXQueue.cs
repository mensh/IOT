using System.Collections.Concurrent;
using static IOT.MCP2515;

namespace IOT
{
    public class CANRXQueue
    {
           private ConcurrentDictionary<string, ConcurrentQueue<CANMSG>> _mailQueues = new ConcurrentDictionary<string, ConcurrentQueue<CANMSG>>();

            public void AddMailQueue(CANMSG message)
            {
                foreach (var variable in _mailQueues.Values)
                {
                    variable.Enqueue(message);
                }

            }

            public bool AddQueueToCollection(ConcurrentQueue<CANMSG> queue, string id)
            {
                return _mailQueues.TryAdd(id, queue);
            }

            public bool RemoveQueueToCollection(string id)
            {
                ConcurrentQueue<CANMSG> removeQueue;
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
        
    
}