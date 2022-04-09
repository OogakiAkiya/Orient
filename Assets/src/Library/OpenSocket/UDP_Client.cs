using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace OpenSocket
{

    class ClientState
    {
        public UdpClient socket = null;
        public IPEndPoint endPoint;
        private List<KeyValuePair<IPEndPoint, byte[]>> recvDataList = new List<KeyValuePair<IPEndPoint, byte[]>>();
        private System.Object lockObject = new System.Object();

        ~ClientState()
        {
            if (socket != null)
            {
                socket.Close();
            }
        }

        public KeyValuePair<IPEndPoint, byte[]> GetRecvData()
        {
            KeyValuePair<IPEndPoint, byte[]> returnByte;
            returnByte = recvDataList[0];

            lock (lockObject)
            {
                recvDataList.RemoveAt(0);
            }
            return returnByte;
        }

        public void AddRecvData(IPEndPoint _iPEndPoint, byte[] _data)
        {
            lock (lockObject)
            {
                int count = 0;
                while (true)
                {
                    int size = System.BitConverter.ToInt32(_data, count);
                    if (_data.Length - count < size) return;
                    KeyValuePair<IPEndPoint, byte[]> addData = new KeyValuePair<IPEndPoint, byte[]>(_iPEndPoint, new byte[size - sizeof(int)]);
                    Array.Copy(_data, (count + sizeof(int)), addData.Value, 0, addData.Value.Length);
                    recvDataList.Add(addData);
                    count += size;
                    if (_data.Length - count < sizeof(int)) return;
                }
            }
        }

        public int GetRecvDataSize()
        {
            int count = 0;
            lock (lockObject)
            {
                count = recvDataList.Count;
            }
            return count;
        }
    }


    class UDP_Client
    {

        public ClientState server { get; private set; } = new ClientState();

        int port = 12343;
        int sendPort = 12344;
        uint sequence = 0;

        public void Init(int _sendPort)
        {
            sendPort = _sendPort;
            server.socket = new UdpClient();
            server.socket.BeginReceive(new AsyncCallback(ReceiveCallback), server);

        }

        public void Init(int _port, int _sendPort)
        {
            port = _port;
            sendPort = _sendPort;
            //System.Net.IPEndPoint localEP =new System.Net.IPEndPoint(IPAddress.Any, port);
            server.socket = new UdpClient();
            //server.socket = new UdpClient(localEP);

            server.socket.BeginReceive(new AsyncCallback(ReceiveCallback), server);
        }

        //本来ならsendPortはportに変わる
        public void Send(KeyValuePair<IPEndPoint, byte[]> _data)
        {

            List<byte> sendData = new List<byte>();
            sendData.AddRange(BitConverter.GetBytes(sequence));
            sendData.AddRange(_data.Value);
            //sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _data.Key.Address.ToString(), sendPort);
            server.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _data.Key.Address.ToString(), sendPort);

            CountUPSequence();

        }

        public void Send(byte[] _data, string _IP, int _port)
        {

            List<byte> sendData = new List<byte>();
            sendData.AddRange(BitConverter.GetBytes(sequence));
            sendData.AddRange(_data);
            //sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _IP, _port);
            server.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _IP, _port);
            CountUPSequence();

        }

        private void CountUPSequence()
        {
            sequence++;
            if (sequence > 4200000000)
            {
                sequence = 0;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            ClientState client = (ClientState)ar.AsyncState;
            byte[] decodeData = CompressionWrapper.Decode(client.socket.EndReceive(ar, ref client.endPoint));
            client.AddRecvData(client.endPoint, decodeData);
            client.socket.BeginReceive(ReceiveCallback, client);
        }

    }
}