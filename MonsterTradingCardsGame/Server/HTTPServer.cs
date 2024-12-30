using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    internal class HTTPServer
    {
        private readonly TcpListener _listener;

        public HTTPServer(IPAddress ip, int port)
        {
            _listener = new TcpListener(ip, port);
        }

        public async Task Start()
        {
            _listener.Start();
            while (true) 
            {
                var client = _listener.AcceptTcpClient();
                var requestHandler = new RequestHandler();
                using (var networkStream = client.GetStream())
                {
                    await requestHandler.HandleRequestAsync(networkStream);
                }




            }
        }

        public void Stop()
        {

        }

        

    }

}
