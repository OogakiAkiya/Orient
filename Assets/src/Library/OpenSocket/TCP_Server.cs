using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace OpenSocket
{
    class Tcp_Server_Socket
    {
        public const int BUFSIZE = 2048;
        object lockObj = new object();
        public Socket socket { get; }
        public byte[] ReceiveBuffer;
        public List<byte> recvTempDataList = new List<byte>();
        public List<byte[]> recvDataList = new List<byte[]>();
        public bool deleteFlg { get; private set; } = false;
        public bool threadDeleteFlg { get; private set; } = false;

        public Tcp_Server_Socket(Socket _socket)
        {
            socket = _socket;
            ReceiveBuffer = new byte[BUFSIZE];
        }
        public byte[] GetRecvDataList()
        {
            byte[] returnData;
            lock (lockObj)
            {
                returnData = recvDataList[0];
                recvDataList.RemoveAt(0);
            }
            return returnData;
        }

        public void AddRecvDateList(byte[] _data)
        {
            lock (lockObj)
            {
                recvDataList.Add(_data);
            }
        }

        public int RecvDataListCount()
        {
            int count;
            lock (lockObj)
            {
                count = recvDataList.Count;
            }
            return count;
        }


        public async Task Send(byte[] _sendData, int _dataSize)
        {
            await Task.Run(() =>
            {

                int sendSize = -1;

                byte[] sendHeader;

            //header設定
            sendHeader = BitConverter.GetBytes(_dataSize);
            //sendBytes作成
            byte[] sendBytes = new byte[sendHeader.Length + _dataSize];
                sendHeader.CopyTo(sendBytes, 0);
                _sendData.CopyTo(sendBytes, sendHeader.Length);

                try
                {
                    if (socket.Connected)
                    {
                        sendSize = socket.Send(sendBytes, sendBytes.Length, 0);
                    }
                    else
                    {
                        Console.WriteLine("sendError");
                    }
                }
                catch (System.ObjectDisposedException)
                {
                //閉じた時
                /*
                System.Console.WriteLine("閉じました。");
                this.OnDeleteFlg();
                */
                }
                catch (SocketException e)
                {
                //Console.WriteLine("ソケットが切断されています。");
                //Console.WriteLine(e);
            }
            });

        }

        public async Task Send(List<byte[]> _sendDataList)
        {
            await Task.Run(() =>
            {
                List<byte> sendData = new List<byte>();
                foreach (var data in _sendDataList)
                {
                    byte[] sendHeader;

                //header設定
                sendHeader = BitConverter.GetBytes((int)data.Length);
                //sendBytes作成
                byte[] addData = new byte[sendHeader.Length + data.Length];
                    sendHeader.CopyTo(addData, 0);
                    data.CopyTo(addData, sendHeader.Length);

                    sendData.AddRange(addData);
                }

                int sendSize = -1;


                try
                {
                    if (socket.Connected)
                    {
                        sendSize = socket.Send(sendData.ToArray(), sendData.ToArray().Length, 0);
                    }
                    else
                    {
                        Console.WriteLine("sendError");
                    }
                }
                catch (System.ObjectDisposedException)
                {
                //閉じた時

                //System.Console.WriteLine("閉じました。");
                //this.OnDeleteFlg();

            }
                catch (SocketException e)
                {
                //Console.WriteLine("ソケットが切断されています。");
                //Console.WriteLine(e);
            }

            });

        }

        public void OnDeleteFlg()
        {
            lock (lockObj)
            {
                deleteFlg = true;
            }
        }
        public void OnThreadDeleteFlg()
        {
            lock (lockObj)
            {
                deleteFlg = true;
            }
        }

    }





    class TCP_Server
    {
        public List<Tcp_Server_Socket> clientList { get; } = new List<Tcp_Server_Socket>();
        private List<Tcp_Server_Socket> addClientList = new List<Tcp_Server_Socket>();
        private readonly System.Object lockObj = new System.Object();
        Socket listener;

        public void BeginUpdate()
        {
            //addClientListのための排他制御
            lock (lockObj)
            {
                if (addClientList.Count > 0)
                {
                    clientList.AddRange(addClientList);
                    addClientList = new List<Tcp_Server_Socket>();
                }
            }
        }

        public void EndUpdate()
        {
            if (clientList.Count <= 0) return;

            List<Tcp_Server_Socket> deleteList = new List<Tcp_Server_Socket>();
            foreach (Tcp_Server_Socket client in clientList)
            {
                if (client.deleteFlg == true)
                {
                    deleteList.Add(client);
                }
            }
            foreach (Tcp_Server_Socket client in deleteList)
            {
                clientList.Remove(client);
            }
        }
        public void Init(string _ip, int _port)
        {
            //ListenするIPアドレス
            IPAddress ipAdd = IPAddress.Parse(_ip);

            //ホスト名からIPアドレスを取得する時は、次のようにする
            //string host = "localhost";
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.GetHostEntry(host).AddressList[0];

            //TcpListenerオブジェクトを作成する
            IPEndPoint ipe = new IPEndPoint(ipAdd, _port);
            listener = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Bind(ipe);
            listener.Listen(1000);

        }

        public async Task StartAccept()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Tcp_Server_Socket temp = new Tcp_Server_Socket(listener.Accept());
                    lock (lockObj)
                    {
                        addClientList.Add(temp);
                    //Console.WriteLine("接続あり");
                }
                    StartReceive(temp);
                }
            });
        }


        //===========================================================================
        //recv関係
        //===========================================================================
        //データ受信スタート
        private static void StartReceive(Tcp_Server_Socket _server)
        {
            //非同期受信を開始
            _server.socket.BeginReceive(_server.ReceiveBuffer,
                    0,
                    _server.ReceiveBuffer.Length,
                    System.Net.Sockets.SocketFlags.None,
                    new System.AsyncCallback(ReceiveDataCallback),
                    _server);
        }


        //BeginReceiveのコールバック
        private static void ReceiveDataCallback(System.IAsyncResult ar)
        {
            //状態オブジェクトの取得
            Tcp_Server_Socket server = (Tcp_Server_Socket)ar.AsyncState;

            //読み込んだ長さを取得
            int len = 0;
            try
            {
                if (server.socket.Connected) len = server.socket.EndReceive(ar);
            }
            catch (System.ObjectDisposedException)
            {
                //閉じた時
                System.Console.WriteLine("閉じました。");
                server.OnDeleteFlg();
                return;
            }

            //切断されたか調べる
            if (len <= 0)
            {
                System.Console.WriteLine("切断されました。");
                server.socket.Close();
                server.OnDeleteFlg();
                return;
            }

            //受信したデータを蓄積する
            System.Array.Resize(ref server.ReceiveBuffer, len);
            server.recvTempDataList.AddRange(server.ReceiveBuffer);

            //受信用配列のサイズを元に戻す
            System.Array.Resize(ref server.ReceiveBuffer, Tcp_Server_Socket.BUFSIZE);

            //データの整形
            while (server.recvTempDataList.Count > sizeof(int))
            {
                int byteSize = (int)server.recvTempDataList[0];
                if (server.recvTempDataList.Count >= byteSize + sizeof(int))
                {
                    byte[] addData;
                    addData = server.recvTempDataList.GetRange(sizeof(int), byteSize).ToArray();
                    server.AddRecvDateList(addData);
                    server.recvTempDataList.RemoveRange(0, sizeof(int) + byteSize);
                }
                else
                {
                    break;
                }
            }

            //System.Threading.Thread.Sleep(100);
            if (server.deleteFlg || !server.socket.Connected)
            {
                server.OnDeleteFlg();
                return;
            }
            //再び受信開始
            server.socket.BeginReceive(server.ReceiveBuffer,
                    0,
                    server.ReceiveBuffer.Length,
                    System.Net.Sockets.SocketFlags.None,
                    new System.AsyncCallback(ReceiveDataCallback),
                    server);
        }

    }
}