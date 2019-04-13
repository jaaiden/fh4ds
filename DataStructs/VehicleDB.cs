using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FH4TelemetryServer.DataStructs
{
    public static class VehicleDB
    {

        public struct Vehicle
        {
            public string Make { get; set; }
            public string Model { get; set; }
            public int Year { get; set; }

            public override string ToString()
            {
                return $"{Year} {Make} {Model}";
            }
        }

        private static Dictionary<int, Vehicle> vehicles = new Dictionary<int, Vehicle>();

        public static void AddVehicle(int Id, Vehicle VehicleData)
        {

        }

        public static Vehicle GetVehicle(int Id)
        {
            if (vehicles.Keys.Contains(Id))
                return vehicles[Id];
            else
            {
                return new Vehicle {
                    Make = "Unknown",
                    Model = "Vehicle",
                    Year = DateTime.Now.Year
                };
            }
        }
    }
}
