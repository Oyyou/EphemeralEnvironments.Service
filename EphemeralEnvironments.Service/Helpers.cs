using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EphemeralEnvironments.Service
{
    public static class Helpers
    {
        public static void Log(string value)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {value}");
        }

        public static void Retry(Action action, TimeSpan delay, int maxAttempts)
        {
            int attempts = 0;

            do
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");

                    attempts++;
                    if (attempts < maxAttempts)
                    {
                        Log($"Retrying in {delay.TotalSeconds} seconds...");
                        Thread.Sleep(delay);
                    }
                    else
                    {
                        Log($"Max attempts reached. Giving up.");
                        throw;
                    }
                }
            } while (true);
        }

    }
}
