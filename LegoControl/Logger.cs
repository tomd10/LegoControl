namespace LegoControl
{
    public static class Logger
    {
        public static string messages = "";

        public static void AddAndDisplay(string msg, Severity s)
        {
            string mess = DateTime.Now.ToString();
            if (s == Severity.Error) { mess = mess + " ERR "; }
            else if (s == Severity.Information) { mess = mess + " INF "; }
            else if (s == Severity.Message) { mess = mess + " MSG "; }
            mess = mess + msg;

            Console.WriteLine(mess);
            messages = messages + mess + "\r\n";
        }

        public static void LogSongs(List<string> songs)
        {
            string msg = "Loaded " + songs.Count + " songs: ";
            foreach (string s in songs) 
            {
                msg = msg + s + ", ";
            }
            msg = msg.Substring(0, msg.Length - 2);

            AddAndDisplay(msg, Severity.Information);
        }
    }

    public enum Severity
    {
        Error,
        Information,
        Message
    }
}
