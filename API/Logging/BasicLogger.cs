namespace API.Logging
{
    public class BasicLogger : IBasicLogger
    {
        public void Log(string message, string type)
        {
            if(type == "ERROR") 
            {
                Console.WriteLine("Error: " + message);
            }
            else
            {
                Console.WriteLine("INFO: " + message);
            }
        }
    }
}
