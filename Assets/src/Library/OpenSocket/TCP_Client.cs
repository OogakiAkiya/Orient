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
        private TcpClient socket = null;
        private NetworkStream ns;
        private System.Object lockObject = new System.Object();
        private bool deleteFlg = false;
        private static readonly int HEADERSIZE = sizeof(int);
        private static readonly string ENDMARKER = "\r\n";
        private static readonly int TCP_BUFFERSIZE = 2048;


        public TCP_Client() { }

        ~TCP_Client() { socket.Close(); }

        public bool TryConnect(string _hostname, int _port,int _timeout = 1000)
        {
            try
            {
                socket = new TcpClient();
                Task task = socket.ConnectAsync(_hostname, _port);
                if (!task.Wait(_timeout))
                {
                    socket.Close();
                    FileController file = FileController.GetInstance();
                    file.Write("error", "Warning:Connection Faild Timeout");
                    return false;
                }
            }
            catch (SocketException e)
            {
                socket.Close();
                FileController file = FileController.GetInstance();
                file.Write("error",e.Message);
                return false;
            }
            catch (AggregateException ae)
            {
                socket.Close();
                FileController file = FileController.GetInstance();
                file.Write("error", ae.Message);
                return false;
            }
            socket.Close();
            return true;
        }

        public void Init(string _hostname, int _destPort)
        {
            try
            {
                socket = new System.Net.Sockets.TcpClient(_hostname, _destPort);
            } catch (System.Exception e) {
                FileController file = FileController.GetInstance();
                file.Write("error", e.Message);
            }

            //通信設定
            ns = socket.GetStream();
        }

        public async Task Send(byte[] _sendData, int _dataSize)
        {
            //header設定
            byte[] sendHeader = BitConverter.GetBytes(_dataSize);
            //fotter設定
            byte[] sendFooter = Convert.ToStringUTF8Byte(ENDMARKER);

            //sendBytes作成
            byte[] sendBytes = new byte[sendHeader.Length + _dataSize + sendFooter.Length];
            sendHeader.CopyTo(sendBytes, 0);
            _sendData.CopyTo(sendBytes, sendHeader.Length);
            sendFooter.CopyTo(sendBytes, sendHeader.Length + _dataSize);
            
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
            socket.Close();
        }

        private void Routine()
        {
            lock (lockObject)
            {
                while (recvTempDataList.Count > HEADERSIZE)
                {
                    try
                    {
                        //先頭パケット解析
                        int byteSize = (int)recvTempDataList[0];
                        if (byteSize < 0 || byteSize > TCP_BUFFERSIZE - HEADERSIZE - ENDMARKER.Length)
                        {
                            recvTempDataList.Clear();
                            return;
                        }

                        //エンドマーカーの値が正常かチェック
                        if (!ENDMARKER.Equals(Convert.StringUTF8ByteConversion(recvTempDataList.ToArray(), HEADERSIZE + byteSize)))
                        {
                            recvTempDataList.Clear();
                            return;
                        }

                        if (recvTempDataList.Count >= byteSize + HEADERSIZE + ENDMARKER.Length)
                        {
                            byte[] addData;
                            addData = recvTempDataList.GetRange(HEADERSIZE, byteSize).ToArray();
                            recvDataList.Add(addData);
                            recvTempDataList.RemoveRange(0, HEADERSIZE + byteSize + ENDMARKER.Length);
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        recvTempDataList.Clear();
                        FileController.GetInstance().Write("Exception.txt", e.Message);
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