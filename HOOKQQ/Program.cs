using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.Resources;
using TouchSocket.Http.WebSockets;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace HOOKQQ
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TouchSocket();
            //STASocket();


            Console.ReadKey();
        }
        private static void STASocket()
        {
            var ws = new WebSocket("ws://127.0.0.1:5800");


            ws.OnOpen += Ws_OnOpen;
            ws.OnMessage += Ws_OnMessage;

            ws.ConnectAsync();
            ws.OnError += Ws_OnError;
            ws.OnClose += Ws_OnClose;
            //ws.Send("BALUS");
        }

        private static void Ws_OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }

        private static void Ws_OnError(object sender, ErrorEventArgs e)
        {

        }

        private static void Ws_OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("连接服务端");
        }

        private static void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var ws = (WebSocket)sender;
            Console.WriteLine(e.Data);
            if (e.Data != null)
            {
                if (e.IsText)
                {
                    Console.WriteLine(e.Data.ToString());
                    if (e.Data.ToString().Contains("414725048"))
                    {
                        Send("/send_private_msg", "{\"user_id\":414725048 ,\"message\":\"480325208 \"}");
                    }
                }
                if (e.IsBinary)
                {

                }
            }

        }

        private static void Send(string url, string msg)
        {
            url = "ws://127.0.0.1:5800" + url;
            using (var ws = new WebSocket(url))
            {
                //ws.OnMessage += (sender, e) =>Console.WriteLine("Laputa says: " + e.Data);
                
                ws.Connect();
                ws.Send(msg);
                //Console.ReadKey(true);
            }
        }

        private static void TouchSocket()
        {
            var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig().SetRemoteIPHost("ws://127.0.0.1:5800")
                .ConfigureContainer(a =>
                {
                    a.AddFileLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocketHeartbeat()//使用心跳插件
                    .SetTick(TimeSpan.FromSeconds(1));//每5秒ping一次。
                }));
            client.Connect();
            client.Received = (c, e) =>
            {
                switch (e.DataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        break;
                    case WSDataType.Text:
                        Console.WriteLine(e.DataFrame.ToText());
                        if (e.DataFrame.ToText().ToString().Contains("414725048"))
                        {
                            string msg = "{\"user_id\":414725048 ,\"message\":\"480325208 \"}";
                            TouchSendHttp("/send_private_msg", msg);

                            //socket模式暂时没搞定
                            // string msg1 = "{\"action\": \"send_private_msg\", \"params\": {\"user_id\": 414725048,\"message\": \"hello\"},\"echo\": \"123456\" }";
                            //TouchSendSocket("/send_private_msg", msg1);
                            //client.SendAsync(msg1).Wait();  
                        }
                        
                        break;
                    case WSDataType.Binary:
                        byte[] by = e.DataFrame.PayloadData.Buffer;
                        break;
                    case WSDataType.Close:
                        break;
                    case WSDataType.Ping:
                        break;
                    case WSDataType.Pong:
                        break;
                    default:
                        break;
                }

                return null;
            };
        }
        private static void TouchSendHttp(string api,string msg)
        {
            HttpClient client = new HttpClient();

            client.Setup(new TouchSocketConfig().SetRemoteIPHost(new IPHost("http://localhost:5700")));
            client.Connect();//先做连接
            HttpRequest request = new HttpRequest();
            request
                .InitHeaders()
                .SetUrl(api)
                .SetHost(client.RemoteIPHost.Host)
                .SetContent(msg)
                .AsPost();
            var respose = client.Request(request, millisecondsTimeout: 1000 * 10);
            Console.WriteLine(respose.GetBody());
        }
        private static void TouchSendSocket(string api, string msg)
        {
            var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig().SetRemoteIPHost("ws://127.0.0.1:5800"+api)
                .ConfigureContainer(a =>
                {
                    a.AddFileLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocketHeartbeat()//使用心跳插件
                    .SetTick(TimeSpan.FromSeconds(1));//每5秒ping一次。
                }));
            client.Connect();
            client.Send(msg);
            client.Received = (c, e) =>
            {
                switch (e.DataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        break;
                    case WSDataType.Text:
                        Console.WriteLine(e.DataFrame.ToText());
                        break;
                    case WSDataType.Binary:
                        byte[] by = e.DataFrame.PayloadData.Buffer;
                        break;
                    case WSDataType.Close:
                        break;
                    case WSDataType.Ping:
                        break;
                    case WSDataType.Pong:
                        break;
                    default:
                        break;
                }

                return null;
            };
            client.Close(); 
        }
    }
}
