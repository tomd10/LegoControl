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

        public IPAddress localIP = null;
        public UdpClient udpClient = new UdpClient();
        IPEndPoint e;
        UdpClient u;

        //Counters
        public int receivedPackets = 0;
        public int sentPackets = 0;

        //Webpage variables
        public IPAddress IPAddress = null;
        public bool connected = false;
        public double distance = -1;
        public List<string> songs;

        Random rnd = new Random();
        public SingletonService()
        {

        }

        public bool SaveLocalIP(string IPLocal)
        {
            if (localIP != null) return true;
            UdpState s = new UdpState();
            s.e = e;
            s.u = u;
            try
            {
                localIP = IPAddress.Parse(IPLocal);
            }
            catch
            {
                return false;
            }

            localIP = IPAddress.Parse(IPLocal);
            e = new IPEndPoint(localIP, localPort);
            u = new UdpClient(new IPEndPoint(localIP, localPort));
            Logger.AddAndDisplay("Listening on UDP/" + localIP.ToString() + ":" + localPort.ToString(), Severity.Information);
            u.BeginReceive(new AsyncCallback(ReceiveCallback), s);
            return true;
        }
        public void SendCommand(string command)
        {
            if ((localIP != null && IPAddress != null && (connected || command.StartsWith(Commands.PingRequest))))
            {
                sentPackets++;
                Logger.AddAndDisplay("sent src " + localIP.ToString() + ":" + localPort + " dst " + IPAddress.ToString() + ":" + port + " data: '" + command + "'", Severity.Information);
                byte[] CMD = Encoding.ASCII.GetBytes(command);
                udpClient.Send(CMD, CMD.Length, IPAddress.ToString(), port);
            }
            else
            {
                Logger.AddAndDisplay("tried sending, but no conn! " + command, Severity.Information);
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
            Logger.AddAndDisplay("re'd src " + (IPAddress != null ? IPAddress.ToString() : "0.0.0.0") + ":" + port + " dst " + (localIP != null ? localIP.ToString() : "0.0.0.0") + ":" + localPort + " data: '" + receiveString + "'", Severity.Information);
            receivedPackets++;

            string[] cmds = receiveString.Split("@@@");
            foreach (string cmd in cmds)
            {
                if (cmd.Length <= 8) continue;
                if (!cmd.StartsWith("LEGOCTRL")) continue;

                string[] parameters = cmd.Split('#');

                if (parameters[1] == "PING" && parameters.Length >= 4)
                {
                    if (awaitingPingReply == true && parameters[2] == "1" && parameters[3] == pingIdentifier.ToString())
                    {
                        awaitingPingReply = false;
                        Logger.AddAndDisplay("Connection with" + (IPAddress != null ? IPAddress.ToString() : "0.0.0.0") + " OK!", Severity.Information);
                        connected = true;
                        songs = new List<string>();
                        for (int i = 4; i < parameters.Length; i = i + 2)
                        {
                            if (parameters[i] == "SONG")
                            {
                                songs.Add(parameters[i + 1]);
                            }
                        }

                        Logger.LogSongs(songs);
                    }
                }
                if (parameters[1] == "SENSOR" && parameters[2] == "4" && double.TryParse(parameters[3].Replace('.', ','), out distance))
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
        /*
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
        */

        private bool awaitingPingReply = false;
        private int pingIdentifier = 0;
        public bool Test(string robotIP)
        {
            try
            {
                this.IPAddress = IPAddress.Parse(robotIP);
                connected = false;
                pingIdentifier = rnd.Next(0, 100);
                awaitingPingReply = true;

                SendCommand(Commands.PingRequestCommand(pingIdentifier.ToString()));
                return true;
            }
            catch
            {
                return false;
            }
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
            SendCommand(Commands.RideCommand(0, -100, 100));
        }

        public void Right()
        {
            SendCommand(Commands.RideCommand(0, 100, -100));
        }

        public void Stop()
        {
            SendCommand(Commands.Stop);
        }

        public void Play(string songName)
        {
            SendCommand(Commands.SongCommand(songName));
        }

        public void Mute()
        {
            SendCommand(Commands.Mute);
        }

        public void Joystick(int x, int y, int dim)
        {
            int cartX = x - dim / 2;
            int cartY = dim / 2 - y;
            //Console.WriteLine(cartX + " " + cartY);

            int normY = (int)(100 * (2 * (double)cartY / (double)dim));
            int normX = (int)(100 * (2 * (double)cartX / (double)dim));

            //Console.WriteLine("[" + normX + " ; " + normY + "]");

            if (normX > 100) normX = 100;
            if (normX < -100) normX = -100;
            if (normY > 100) normY = 100;
            if (normY < -100) normY = -100;

            SendCommand(Commands.JoystickCommand(normX, normY));
            //if (speed > 100) speed = 100;
            //if (speed < -100) speed = -100;

            //SendCommand(Commands.RideCommand(0, (int)speed, (int)speed));
        }

        public void SetVolume(int volume)
        {
            SendCommand(Commands.VolumeCommand(volume));
        }

        public void PlayTone(int frequency)
        {
            if (frequency >= 20 && frequency <= 20000)
            {
                SendCommand(Commands.ToneCommand(frequency));
            }
        }
    }
}
