namespace LegoControl
{
    public static class Commands
    {
        public static string PingRequest = "@@@LEGOCTRL#PING#0";
        public static string Stop = "@@@LEGOCTRL#STOP";
        public static string Ride = "@@@LEGOCTRL#RIDE";
        public static string Mute = "@@@LEGOCTRL#MUTE";
        public static string Song = "@@@LEGOCTRL#SONG";
        public static string Volume = "@@@LEGOCTRL#VOLUME";
        public static string Tone = "@@@LEGOCTRL#TONE";
        public static string Joystick = "@@@LEGOCTRL#JOYSTICK";
        public static string SongCommand(string songName)
        {
            return Song + "#" + songName;
        }
        public static string RideCommand(int time, int speedL, int speedR)
        {
            return Ride+"#"+time+"#"+speedL+"#"+speedR;
        }

        public static string PingRequestCommand(string data)
        {
            return PingRequest + "#" + data;
        }

        public static string VolumeCommand(int volume)
        {
            return Volume + "#" + volume;
        }

        public static string JoystickCommand(int x, int y)
        {
            return Joystick + "#" + x + "#" + y;
        }

        public static string ToneCommand(int hz)
        {
            return Tone + "#" + hz;
        }
    }
}
