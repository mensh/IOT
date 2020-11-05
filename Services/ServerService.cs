using Grpc.Core;
using IOT;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOT.MCP2515;

namespace IOT.Services
{
    public class ServerService : ServerForecast.ServerForecastBase
    {



        private readonly ArincRXQueue _messageQueueRepository;
        private readonly ArincTXQueue _txdict;
        private CANTXQueue _CANTXQueue;
        private CANRXQueue _CANRXQueue;

        public ServerService(ArincRXQueue messageQueueRepository, ArincTXQueue tXDict, CANTXQueue CANTXQueue, CANRXQueue CANRXQueue)
        {
            _messageQueueRepository = messageQueueRepository;
            _txdict = tXDict;
            _CANTXQueue = CANTXQueue;
            _CANRXQueue = CANRXQueue;
        }


        public override async Task RXCANStream(RXRequest request, IServerStreamWriter<CANmsg> responseStream, ServerCallContext context)
        {
            var id = request.Id;
            var queue = new ConcurrentQueue<CANMSG>();
            while (_CANRXQueue.AddQueueToCollection(queue, id) == false)
            {
                await Task.Delay(1);
            }

            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (queue.Count > 1)
                {
                    if (queue.TryDequeue(out var messageRx))
                    {

                        if (messageRx == null) continue;
                        await responseStream.WriteAsync(new CANmsg()
                        {
                            CANID = messageRx.CANID,
                            DataLength = (uint)messageRx.DataLength,
                            IsExtended = messageRx.IsExtended,
                            IsRemote = messageRx.IsRemote,
                            Data = { Google.Protobuf.ByteString.CopyFrom(messageRx.data) }
                        });
                    }
                }
                else
                {
                    await Task.Yield();
                }

            }

            await DisconnectRxStream(id);
        }

        public override async Task RXStream(RXRequest request, IServerStreamWriter<RXReply> responseStream, ServerCallContext context)
        {

            var id = request.Id;
            var queue = new ConcurrentQueue<ArincRX>();
            while (_messageQueueRepository.AddQueueToCollection(queue, id) == false)
            {
                await Task.Delay(1);
            }
            Console.WriteLine("New Client RX Stream " + id);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (queue.Count > 1)
                {
                    if (queue.TryDequeue(out var messageRx))
                    {

                        if (messageRx == null) continue;
                        await responseStream.WriteAsync(new RXReply()
                        {
                            Message = messageRx.data,
                            Time = messageRx.DateTime
                        });
                    }
                }
                else
                {
                    await Task.Yield();
                }

            }

            await DisconnectRxStream(id);

        }


        private async Task DisconnectRxStream(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            while (_messageQueueRepository.RemoveQueueToCollection(id) == false)
            {
                await Task.Delay(1);
            }
            Console.WriteLine("Remove Client RX Stream " + id);
        }

        public override async Task<ConfigReply> SayHello(ConfigRequest request, ServerCallContext context)
        {

            if (request.Hello == "DisconnectRX")
            {
                Console.WriteLine("DisconnectRX with Client Id " + request.Id);
                await DisconnectRxStream(request.Id);
                return await Task.FromResult(new ConfigReply { Message = "DisconnectRX_OK" });
            }


            if (request.Hello == "PingRX")
            {
                Console.WriteLine("PingRX with Client Id " + request.Id);
                return await Task.FromResult(new ConfigReply { Message = "PingRX_OK" });
            }

            if (request.Hello == "imClient")
            {
                Console.WriteLine("imClient with Client Id " + request.Id);
                return await Task.FromResult(new ConfigReply { Message = "imClient_OK" });
            }


            if (request.ClearTX == true)
            {
                Console.WriteLine("ClearTX with Client Id " + request.Id);
                _txdict.Clear();
                return await Task.FromResult(new ConfigReply { Message = "ClearTX_OK" });
            }

            if (!string.IsNullOrEmpty(request.RXSpeed) && !string.IsNullOrEmpty(request.TXSpeed))
            {
                if (request.RXSpeed == "100kHz")
                {
                    Console.WriteLine("RX 100kHz with Client Id " + request.Id);
                    await Holt.SetRX100kHZAsync();

                }

                if (request.RXSpeed == "12.5kHz")
                {
                    Console.WriteLine("RX 12.5kHz with Client Id " + request.Id);
                    await Holt.SetRX12kHZAsync();

                }


                if (request.TXSpeed == "100kHz")
                {
                    Console.WriteLine("TX 100kHz with Client Id " + request.Id);
                    await Holt.SetTX100kHZAsync();

                }

                if (request.TXSpeed == "12.5kHz")
                {
                    Console.WriteLine("TX 12.5kHz with Client Id " + request.Id);
                    await Holt.SetTX12kHZAsync();

                }

                return await Task.FromResult(new ConfigReply { Message = "TX =" + request.TXSpeed + " RX = " + request.RXSpeed });
            }




            return await Task.FromResult(new ConfigReply { Message = "Ping_OK" });
        }

        public override async Task<TXReply> TXStream(TXRequest request, ServerCallContext context)
        {

            if (request.Removeitem == true)
            {
                _txdict.RemoveItem(request.Id);
                return await Task.FromResult(new TXReply() { Message = "Removeitem_OK" });
            }


            var tx = new ArincTXQueue.ArincTX();
            tx.name = request.Name;
            tx.address = (byte)request.Address;
            tx.low = (byte)request.Low;
            tx.hi = (byte)request.Hi;
            tx.sup = (byte)request.Sup;
            tx.channel = (byte)request.Channel;
            tx.id = request.Id;
            tx.freq = (int)request.Freq;
            tx.enable = request.Enable;
            tx.numberpacket = (int)request.Numberpacket;

            if (tx.numberpacket > 0)
            {
                tx.isNonContinue = true;
            }

            if (!string.IsNullOrEmpty(tx.id))
            {
                if (tx.freq == 0)
                {
                    tx.freq = 1000;
                }
                tx.enable = true;

                if (_txdict.Search(tx.id) == false)
                {
                    Console.WriteLine("No find Item in TXCollection");
                    _txdict.AddToCollection(tx, tx.id);
                    return await Task.FromResult(new TXReply() { Message = "Add_OK" });
                }
                else
                {
                    Console.WriteLine("Found Item in TXCollection");
                    _txdict.UpdateItem(tx.id, tx);
                    return await Task.FromResult(new TXReply() { Message = "Update_OK" });

                }
            }


            return await Task.FromResult(new TXReply() { Message = "Error" });
        }
    }
}
