using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LegoControl
{
    public class SingletonService
    {  
        
        public const int port = 42069;                      //Robot port
        public const int localPort = 42070;                 //Local port
        public const string localIPString = "172.27.138.104";    //Local IP
        
        public UdpClient udpClient = new UdpClient();
        IPEndPoint e = new IPEndPoint(IPAddress.Parse(localIPString), localPort);
        UdpClient u = new UdpClient(new IPEndPoint(IPAddress.Parse(localIPString), localPort));
        
        //Webpage variables
        public IPAddress IPAddress;
        public double distance = -1;

        Random rnd = new Random();
        public SingletonService()
        {
            UdpState s = new UdpState();
            s.e = e;
            s.u = u;

            Console.WriteLine("listening for messages on " + localIPString + ":" + localPort.ToString());
            u.BeginReceive(new AsyncCallback(ReceiveCallback), s);
        }
        public void SendCommand(string command)
        {
            if (IPAddress != null)
            {
                byte[] CMD = Encoding.ASCII.GetBytes(command);
                udpClient.Send(CMD, CMD.Length, IPAddress.ToString(), port);
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            //Evaluation of received data
            ///------------------------------------------------------------
            Console.WriteLine(receiveString);

            string[] cmds = receiveString.Split("@@@");
            foreach (string cmd in cmds)
            {
                if (cmd.Length <= 8) continue;
                if (!cmd.StartsWith("LEGOCTRL")) continue;

                string[] parameters = cmd.Split('#');

                if (parameters[1] == "PING" && parameters.Length == 4)
                {
                    if (awaitingPingReply == true && parameters[2] == "1" && parameters[3] == pingIdentifier.ToString())
                    {
                        awaitingPingReply = false;
                        message = message + "\r\nSpojení s robotem " + IPAddress.ToString() + " OK!";
                    }
                }
                if (parameters[1] == "SENSOR" && parameters[2] == "1" && double.TryParse(parameters[3], out distance))
                {
                    distance = Math.Round(Convert.ToDouble(parameters[3]), 4);
                }
            }
            ///------------------------------------------------------------
            Console.WriteLine($"Received: {receiveString}");

            UdpState s = new UdpState();
            s.e = e;
            s.u = u;

            u.BeginReceive(new AsyncCallback(ReceiveCallback), s);
        }

        private string message = "";
        public bool DisplayMessage (out string msg)
        {
            if (message == "")
            {
                msg = "";
                return false;
            }
            else
            {
                msg = message;
                message = "";
                return true;
            }
        }

        private bool awaitingPingReply = false;
        private int pingIdentifier = 0;
        public void Test()
        {
            if (awaitingPingReply) return;
            pingIdentifier = rnd.Next(0, 100);
            awaitingPingReply = true;

            SendCommand(Commands.PingRequest + pingIdentifier.ToString());
            
        }
    }
}
