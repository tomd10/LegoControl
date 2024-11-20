using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LegoControl
{
    public class SingletonService
    {  
        //Connection info
        public const int port = 42069;                      //Robot port
        public const int localPort = 42070;                 //Local port
        public const string localIPString = "172.27.138.104";    //Local IP
        
        public UdpClient udpClient = new UdpClient();
        IPEndPoint e = new IPEndPoint(IPAddress.Parse(localIPString), localPort);
        UdpClient u = new UdpClient(new IPEndPoint(IPAddress.Parse(localIPString), localPort));

        //Counters
        public int receivedPackets = 0;
        public int sentPackets = 0;
        
        //Webpage variables
        public IPAddress IPAddress = null;
        public double distance = -1;

        Random rnd = new Random();
        public SingletonService()
        {
            UdpState s = new UdpState();
            s.e = e;
            s.u = u;

            Logger.AddAndDisplay("Listening on UDP/" + localIPString + ":" + localPort.ToString(), Severity.Information);
            u.BeginReceive(new AsyncCallback(ReceiveCallback), s);
        }
        public void SendCommand(string command)
        {
            if (IPAddress != null)
            {
                sentPackets++;
                Logger.AddAndDisplay("sent src " + localIPString + ":" + localPort + " dst " + IPAddress.ToString() + ":" + port + " data: '" + command +"'", Severity.Information);
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
            Logger.AddAndDisplay("re'd src " + (IPAddress != null ? IPAddress.ToString() : "0.0.0.0") + ":" + port + " dst " + localIPString + ":" + localPort + " data: '" + receiveString +"'", Severity.Information);
            receivedPackets++;

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
                        Logger.AddAndDisplay("Connection with" + (IPAddress != null ? IPAddress.ToString() : "0.0.0.0") + " OK!", Severity.Information);
                    }
                }
                if (parameters[1] == "SENSOR" && parameters[2] == "1" && double.TryParse(parameters[3].Replace('.', ','), out distance))
                {
                    distance = Math.Round(Convert.ToDouble(parameters[3].Replace('.', ',')), 4);
                }
                if (parameters[1] == "MESSAGE" && parameters.Length == 4)
                {
                    if (parameters[2] == "0")
                    {
                        Logger.AddAndDisplay(parameters[3], Severity.Error);
                    }
                    else
                    {
                        Logger.AddAndDisplay(parameters[3], Severity.Message);
                    }
                }
            }
            ///------------------------------------------------------------

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

            SendCommand(Commands.PingRequestCommand(pingIdentifier.ToString()));
            
        }

        public void Up()
        {
            SendCommand(Commands.RideCommand(0, 100, 100));
        }

        public void Down()
        {
            SendCommand(Commands.RideCommand(0, -100, -100));
        }

        public void Left()
        {
            SendCommand(Commands.RideCommand(0, 50, 100));
        }

        public void Right()
        {
            SendCommand(Commands.RideCommand(0, 100, 50));
        }

        public void Stop()
        {
            SendCommand(Commands.Stop);
        }

        public void Joystick (int x, int y, int dim)
        {
            int cartX = x - dim/2;
            int cartY = dim/2 - y;
            //Console.WriteLine(cartX + " " + cartY);

            double speed = 100 * (2 * (double)cartY / (double)dim);
            if (speed > 100) speed = 100;
            if (speed < -100) speed = -100;

            SendCommand(Commands.RideCommand(0, (int)speed, (int)speed));
        }
    }
}
