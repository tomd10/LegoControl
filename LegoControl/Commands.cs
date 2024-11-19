namespace LegoControl
{
    public static class Commands
    {
        public static string PingRequest = "@@@LEGOCTRL#PING#0#";
        public static string Up = "@@@LEGOCTRL#FWD#0#100";
        public static string Down = "@@@LEGOCTRL#BWD#0#100";
        public static string Left = "@@@LEGOCTRL#LEFT#0#50";
        public static string Right = "@@@LEGOCTRL#RIGHT#0#50";
        public static string Stop = "@@@LEGOCTRL#STOP";
    }
}
