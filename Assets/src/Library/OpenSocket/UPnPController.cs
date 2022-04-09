using System.IO;
using System.Net;
using System.Text;
using System;
using System.Net.Sockets;
using UnityEngine;

namespace OpenSocket
{
    class UPnPController : MonoBehaviour
    {
        [SerializeField] public uint recvPort = 12343;
        private string selfIP = "192.168.179.2";
        private string hostIP = "192.168.179.1";

        private void Start()
        {
            selfIP = CheckSelfIP();
            //CheckPort();
            if (selfIP != "") AddPort();

        }

        private void OnDestroy()
        {
            if (this.isActiveAndEnabled)
            {
                if (selfIP != "") DeletePort();
            }


        }

        private string CheckSelfIP()
        {
            try
            {
                //ホスト名を取得
                string hostname = System.Net.Dns.GetHostName();

                //ホスト名からIPアドレスを取得
                System.Net.IPAddress[] addr_arr = System.Net.Dns.GetHostAddresses(hostname);

                //探す
                foreach (System.Net.IPAddress addr in addr_arr)
                {
                    string addr_str = addr.ToString();

                    //IPv4 && localhostでない
                    if (addr_str.IndexOf(".") > 0 && !addr_str.StartsWith("127."))
                    {
                        return addr_str;
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
            return "";

        }

        private void SerchHostIP(string _str)
        {
            //HostIPの取得
            if (_str.Contains("http://"))
            {
                var temp = _str.Replace("http://", "");
                hostIP = temp.Substring(0, temp.IndexOf(":"));
                return;
            }
            if (_str.Contains("https://"))
            {
                var temp = _str.Replace("http://", "");
                hostIP = temp.Substring(0, temp.IndexOf(":"));
            }

        }

        private void AddPort()
        {
            int port = 0;
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1500);

            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            string requestString = "M-SEARCH * HTTP/1.1\r\n" +
                                   "HOST: 239.255.255.250:1900\r\n" +
                                   "ST: upnp:rootdevice\r\n" +
                                   "MAN: \"ssdp:discover\"\r\n" +
                                   "MX: 3\r\n" +
                                   "\r\n";
            byte[] requestByte = Encoding.ASCII.GetBytes(requestString);
            client.SendTo(requestByte, requestByte.Length, SocketFlags.None, endPoint);

            EndPoint endPoint2 = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseByte = new byte[1024];
            client.ReceiveFrom(responseByte, ref endPoint2);
            var responseString = Encoding.ASCII.GetString(responseByte);
            FileController.GetInstance().Write("UPnP", responseString);
            string location = "";
            string[] parts = responseString.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (part.ToLower().StartsWith("location"))
                {
                    location = part.Substring(part.IndexOf(':') + 1);
                    string port_s = location.Substring(location.LastIndexOf(':') + 1);
                    port = int.Parse(port_s.Substring(0, port_s.IndexOf('/')));
                    location = location.Trim();

                    //IPの取得
                    break;
                }
            }
            SerchHostIP(location);
            FileController.GetInstance().Write("UPnP", location);

            string controlUrl;
            string urn = "urn:schemas-upnp-org:service:WANPPPConnection:1";
            using (WebClient webClient = new WebClient())
            {
                var st = webClient.DownloadString(location);
                FileController.GetInstance().Write("UPnP", "XML==" + st);

                int serviceIndex = st.IndexOf(urn);
                if (serviceIndex == -1)
                {
                    urn = "urn:schemas-upnp-org:service:WANIPConnection:1";
                    serviceIndex = st.IndexOf(urn);
                }
                controlUrl = st.Substring(serviceIndex);
                string tag1 = "<controlURL>";
                string tag2 = "</controlURL>";
                controlUrl = controlUrl.Substring(controlUrl.IndexOf(tag1) + tag1.Length);
                controlUrl = controlUrl.Substring(0, controlUrl.IndexOf(tag2));
            }

            string bodyString =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        "<s:Envelope " + "xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " + "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
        " <s:Body>" +
        "  <u:AddPortMapping xmlns:u=\"" + urn + "\">" +
        "   <NewRemoteHost></NewRemoteHost>" +
        "   <NewExternalPort>" + recvPort + "</NewExternalPort>" +
        "   <NewProtocol>" + "UDP" + "</NewProtocol>" +
        "   <NewInternalPort>" + recvPort + "</NewInternalPort>" +
        "   <NewInternalClient>" + selfIP + "</NewInternalClient>" +
        "   <NewEnabled>1</NewEnabled>" +
        "   <NewPortMappingDescription>" + "GAME" + "</NewPortMappingDescription>" +
        "   <NewLeaseDuration>0</NewLeaseDuration>" +
        "  </u:AddPortMapping>" +
        " </s:Body>" +
        "</s:Envelope>";


            byte[] bodyByte = UTF8Encoding.ASCII.GetBytes(bodyString);
            string headString = "POST " + controlUrl + " HTTP/1.1\r\n" +
                                "HOST: " + hostIP + ":" + port + "\r\n" +
                                "CONTENT-LENGTH: " + bodyByte.Length + "\r\n" +
                                "CONTENT-TYPE: text/xml; charset=\"utf-8\"" + "\r\n" +
                                "SOAPACTION: \"" + urn + "#" + "AddPortMapping" + "\"\r\n" +
                                "\r\n";
            byte[] headByte = Encoding.ASCII.GetBytes(headString);
            byte[] tempByte = new byte[headByte.Length + bodyByte.Length];
            headByte.CopyTo(tempByte, 0);
            bodyByte.CopyTo(tempByte, headByte.Length);

            SOAPSend(tempByte, port);
        }


        private void SOAPSend(byte[] _data, int _port)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(hostIP), _port);                        //IPはルーター
            client.Connect(endPoint);
            client.Send(_data, _data.Length, SocketFlags.None);

            byte[] responseByte = new byte[1024];
            MemoryStream memoryStream = new MemoryStream();
            while (true)
            {
                int responseSize = client.Receive(responseByte, responseByte.Length, SocketFlags.None);
                if (responseSize == 0) break;
                memoryStream.Write(responseByte, 0, responseSize);
                break;
            }

            string responseString = Encoding.ASCII.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            memoryStream.Close();

            client.Shutdown(SocketShutdown.Both);
            client.Close();


        }
        public void DeletePort()
        {
            int port = 0;
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1500);

            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            string requestString = "M-SEARCH * HTTP/1.1\r\n" +
                                   "HOST: 239.255.255.250:1900\r\n" +
                                   "ST: upnp:rootdevice\r\n" +
                                   "MAN: \"ssdp:discover\"\r\n" +
                                   "MX: 3\r\n" +
                                   "\r\n";
            byte[] requestByte = Encoding.ASCII.GetBytes(requestString);
            client.SendTo(requestByte, requestByte.Length, SocketFlags.None, endPoint);

            EndPoint endPoint2 = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseByte = new byte[1024];
            client.ReceiveFrom(responseByte, ref endPoint2);
            var responseString = Encoding.ASCII.GetString(responseByte);
            FileController.GetInstance().Write("UPnP", responseString);

            string location = "";
            string[] parts = responseString.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (part.ToLower().StartsWith("location"))
                {
                    location = part.Substring(part.IndexOf(':') + 1);
                    string port_s = location.Substring(location.LastIndexOf(':') + 1);
                    port = int.Parse(port_s.Substring(0, port_s.IndexOf('/')));
                    location = location.Trim();

                    //IPの取得
                    break;
                }
            }
            FileController.GetInstance().Write("UPnP", location);
            SerchHostIP(location);

            string controlUrl;
            string urn = "urn:schemas-upnp-org:service:WANPPPConnection:1";
            using (WebClient webClient = new WebClient())
            {
                var st = webClient.DownloadString(location);

                FileController.GetInstance().Write("UPnP", "XML=" + st);

                int serviceIndex = st.IndexOf(urn);
                if (serviceIndex == -1)
                {
                    urn = "urn:schemas-upnp-org:service:WANIPConnection:1";
                    serviceIndex = st.IndexOf(urn);
                }
                controlUrl = st.Substring(serviceIndex);
                string tag1 = "<controlURL>";
                string tag2 = "</controlURL>";
                controlUrl = controlUrl.Substring(controlUrl.IndexOf(tag1) + tag1.Length);
                controlUrl = controlUrl.Substring(0, controlUrl.IndexOf(tag2));
            }


            string bodyString =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<s:Envelope " + "xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " + "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            " <s:Body>" +
            "  <u:DeletePortMapping xmlns:u=\"" + urn + "\">" +
            "   <NewRemoteHost></NewRemoteHost>" +
            "   <NewExternalPort>" + recvPort + "</NewExternalPort>" +
            "   <NewProtocol>" + "UDP" + "</NewProtocol>" +
            "  </u:DeletePortMapping>" +
            " </s:Body>" +
            "</s:Envelope>";


            byte[] bodyByte = UTF8Encoding.ASCII.GetBytes(bodyString);
            string headString = "POST " + controlUrl + " HTTP/1.1\r\n" +
                                "HOST: " + hostIP + ":" + port + "\r\n" +
                                "CONTENT-LENGTH: " + bodyByte.Length + "\r\n" +
                                "CONTENT-TYPE: text/xml; charset=\"utf-8\"" + "\r\n" +
                                "SOAPACTION: \"" + urn + "#" + "DeletePortMapping" + "\"\r\n" +
                                "\r\n";
            byte[] headByte = Encoding.ASCII.GetBytes(headString);
            byte[] tempByte = new byte[headByte.Length + bodyByte.Length];
            headByte.CopyTo(tempByte, 0);
            bodyByte.CopyTo(tempByte, headByte.Length);

            SOAPSend(tempByte, port);


        }

        public void CheckPort()
        {
            int port = 0;
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1500);

            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            string requestString = "M-SEARCH * HTTP/1.1\r\n" +
                                   "HOST: 239.255.255.250:1900\r\n" +
                                   "ST: upnp:rootdevice\r\n" +
                                   "MAN: \"ssdp:discover\"\r\n" +
                                   "MX: 3\r\n" +
                                   "\r\n";
            byte[] requestByte = Encoding.ASCII.GetBytes(requestString);
            client.SendTo(requestByte, requestByte.Length, SocketFlags.None, endPoint);

            EndPoint endPoint2 = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseByte = new byte[1024];
            client.ReceiveFrom(responseByte, ref endPoint2);
            var responseString = Encoding.ASCII.GetString(responseByte);
            //FileController.GetInstance().Write("UPnP", responseString);

            string location = "";
            string[] parts = responseString.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (part.ToLower().StartsWith("location"))
                {
                    location = part.Substring(part.IndexOf(':') + 1);
                    string port_s = location.Substring(location.LastIndexOf(':') + 1);
                    port = int.Parse(port_s.Substring(0, port_s.IndexOf('/')));
                    location = location.Trim();

                    //IPの取得
                    break;
                }
            }
            //FileController.GetInstance().Write("UPnP", location);
            SerchHostIP(location);

            string controlUrl;
            string urn = "urn:schemas-upnp-org:service:WANPPPConnection:1";
            using (WebClient webClient = new WebClient())
            {
                var st = webClient.DownloadString(location);

                //FileController.GetInstance().Write("UPnP", "XML=" + st);

                int serviceIndex = st.IndexOf(urn);
                if (serviceIndex == -1)
                {
                    urn = "urn:schemas-upnp-org:service:WANIPConnection:1";
                    serviceIndex = st.IndexOf(urn);
                }
                controlUrl = st.Substring(serviceIndex);
                string tag1 = "<controlURL>";
                string tag2 = "</controlURL>";
                controlUrl = controlUrl.Substring(controlUrl.IndexOf(tag1) + tag1.Length);
                controlUrl = controlUrl.Substring(0, controlUrl.IndexOf(tag2));
            }

            string bodyString =
                bodyString =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<s:Envelope " + "xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " + "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                    " <s:Body>" +
                    "  <u:GetGenericPortMappingEntry xmlns:u=\"" + urn + "\">" +
                    "   <NewPortMappingIndex>" + 0 + "</NewPortMappingIndex>" +
                    "  </u:GetGenericPortMappingEntry>" +
                    " </s:Body>" +
                    "</s:Envelope>";


            byte[] bodyByte = UTF8Encoding.ASCII.GetBytes(bodyString);
            string headString = "POST " + controlUrl + " HTTP/1.1\r\n" +
                                "HOST: " + hostIP + ":" + port + "\r\n" +
                                "CONTENT-LENGTH: " + bodyByte.Length + "\r\n" +
                                "CONTENT-TYPE: text/xml; charset=\"utf-8\"" + "\r\n" +
                                "SOAPACTION: \"" + urn + "#" + "DeletePortMapping" + "\"\r\n" +
                                "\r\n";
            byte[] headByte = Encoding.ASCII.GetBytes(headString);
            byte[] tempByte = new byte[headByte.Length + bodyByte.Length];
            headByte.CopyTo(tempByte, 0);
            bodyByte.CopyTo(tempByte, headByte.Length);

            SOAPSend(tempByte, port);

        }

    }
}