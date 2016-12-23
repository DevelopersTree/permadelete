using System;

namespace RudeFox.ApplicationManagement
{
    public class EntryPoint
    {
        // Note:
        // Rude Fox is a singleton application, meaning only one instance should be running at a time.
        // The implementation is based on the Microsoft's WPF samples
        // Link: https://github.com/Microsoft/WPF-Samples/tree/master/Application%20Management/SingleInstanceDetection

        [STAThread]
        public static void Main(string[] args)
        {
            var manager = new SingletonManager();
            manager.Run(args);
        }
    }
}
