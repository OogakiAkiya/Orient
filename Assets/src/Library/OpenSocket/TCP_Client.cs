using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace OpenSocket
{
    class TCP_Client
    {
        private List<byte> recvTempDataList = new List<byte>();
        private List<byte[]> recvDataList = new List<byte[]>();
        //ソケット作成
        private TcpClient tcp = null;
        private NetworkStream ns;
        private System.Object lockObject = new System.Object();
        private bool deleteFlg = false;

        public TCP_Client() { }

        ~TCP_Client() { tcp.Close(); }

        public bool TryConnect(string _ipAddr, int _port)
        {
            try
            {
                int timeout = 1000;
                tcp = new TcpClient();
                Task task = tcp.ConnectAsync(_ipAddr, _port);
                if (!task.Wait(timeout))
                {
                    tcp.Close();
                    //throw new SocketException(10060);
                    return false;
                }
            }
            catch (SocketException e)
            {
                tcp.Close();
                return false;
            }
            catch (AggregateException ae)
            {
                tcp.Close();
                return false;
            }
            tcp.Close();
            return true;
        }

        public void Init(string _hostname, int _port)
        {
            tcp = new System.Net.Sockets.TcpClient(_hostname, _port);

            //通信設定
            ns = tcp.GetStream();
        }

        public async Task Send(byte[] _sendData, int _dataSize)
        {
            byte[] sendHeader;
            //header設定
            sendHeader = BitConverter.GetBytes(_dataSize);
            //sendBytes作成
            byte[] sendBytes = new byte[sendHeader.Length + _dataSize];
            sendHeader.CopyTo(sendBytes, 0);
            _sendData.CopyTo(sendBytes, sendHeader.Length);

            await Task.Run(() =>
            {
            //send
            ns.Write(sendBytes, 0, sendBytes.Length);
            });
        }

        public void Update()
        {
            Routine();
        }

        public int RecvDataSize()
        {
            return recvDataList.Count;
        }

        public async void StartRecvThread()
        {
            await Task.Run(() =>
            {
                this.Recv();
            });
        }

        public byte[] GetRecvData()
        {
            byte[] returnData;
            returnData = recvDataList[0];
            recvDataList.RemoveAt(0);
            return returnData;
        }


        private void Close()
        {
            deleteFlg = true;
            ns.Close();
            tcp.Close();
        }

        private void Routine()
        {
            lock (lockObject)
            {
                while (recvTempDataList.Count > sizeof(int))
                {
                    int byteSize = (int)recvTempDataList[0];
                    Console.WriteLine("Routine size={0}\n", byteSize);
                    if (recvTempDataList.Count >= byteSize + sizeof(int))
                    {
                        byte[] addData;
                        addData = recvTempDataList.GetRange(sizeof(int), byteSize).ToArray();
                        recvDataList.Add(addData);
                        recvTempDataList.RemoveRange(0, sizeof(int) + byteSize);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }



        private void Recv()
        {
            while (true)
            {
                if (deleteFlg) { break; }
                byte[] resBytes = new byte[1024];
                int resSize = -1;
                resSize = ns.Read(resBytes, 0, resBytes.Length);
                if (resSize == 0)
                {
                    //Console.WriteLine("サーバーが切断しました。");
                    deleteFlg = true;
                }
                else if (resSize > 0)
                {
                    Array.Resize(ref resBytes, resSize);
                    lock (lockObject)
                    {
                        recvTempDataList.AddRange(resBytes);
                    }
                    //Console.WriteLine("recvSize={0},recvDataListSize={1}\n", resSize, recvTempDataList.Count);
                }
            }
        }

    }
}