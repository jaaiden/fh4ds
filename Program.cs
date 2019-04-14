using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FH4TelemetryServer.DataStructs;

namespace FH4TelemetryServer
{
    class Program
    {
        private static int port = 9909;
        public static TelemetryServer server { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Forza Horizon 4 Telemetry Server");
            Console.WriteLine("developed by digital - https://dgtl.dev");
            Console.WriteLine();
            Console.Write("Port (9909): ");
            try
            {
                string portEntry = Console.ReadLine();
                if (portEntry != "")
                    int.TryParse(portEntry, out port);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.ToString()}");
            }

            // load vehicles from file
            VehicleDB.LoadVehicles();

            // run the app - this will run forever until force stopped
            server = new TelemetryServer(port);
            server.Start();

            // Wait for keypress to close
            Console.Read();
            VehicleDB.SaveVehiclesToFile();
        }
    }
}
