using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

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

        private static Dictionary<string, Vehicle> m_Vehicles = new Dictionary<string, Vehicle>();

        public static void AddVehicle(int Id, Vehicle VehicleData)
        {
            if (!m_Vehicles.Keys.Contains(Id.ToString()))
            {
                m_Vehicles.Add(Id.ToString(), VehicleData);
            }
        }

        public static void LoadVehicles()
        {
            if (!File.Exists("vehicles.json"))
                File.Create("vehicles.json");

            Dictionary<string, Vehicle> vroomVrooms = JsonConvert.DeserializeObject<Dictionary<string, Vehicle>>(File.ReadAllText("vehicles.json"));

            if (vroomVrooms != null)
            {
                foreach (KeyValuePair<string, Vehicle> item in vroomVrooms)
                {
                    AddVehicle(int.Parse(item.Key), item.Value);
                }
            }
        }

        public static void SaveVehiclesToFile()
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m_Vehicles));
            FileStream f = File.Open("vehicles.json", FileMode.Create);
            f.Write(jsonBytes, 0, jsonBytes.Length);
            f.Flush();
            f.Close();
        }

        public static Vehicle GetVehicle(int Id)
        {
            if (m_Vehicles.Keys.Contains(Id.ToString()))
                return m_Vehicles[Id.ToString()];
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
