using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using FH4TelemetryServer.DataStructs;

namespace FH4TelemetryServer
{

    public enum EDrivetrainType
    {
        FWD = 0,
        RWD = 1,
        AWD = 2
    }

    public enum ECarClass
    {
        E = 0,
        D = 1,
        C = 2,
        B = 3,
        A = 4,
        S1 = 5,
        S2 = 6,
        X = 7
    }

    public struct TelemetryData
    {
        public int IsRaceOn { get; set; } // 1 when race is on. = 0 when in menus/race stopped
        public uint TimestampMS { get; set; } // Can overflow to 0 eventually

        public float EngineMaxRPM { get; set; }
        public float EngineIdleRPM { get; set; }
        public float EngineCurrentRPM { get; set; }

        // In the car's local space; X = right, Y = up, Z = forward
        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }
        public float AccelerationZ { get; set; }

        // In the car's local space; X = right, Y = up, Z = forward
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float VelocityZ { get; set; }

        // In the car's local space; X = pitch, Y = yaw, Z = roll
        public float AngularVelocityX { get; set; }
        public float AngularVelocityY { get; set; }
        public float AngularVelocityZ { get; set; }

        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        // Suspension travel normalized: 0.0f = max stretch; 1.0 = max compression
        public float NormalizedSuspensionTravelFrontLeft { get; set; }
        public float NormalizedSuspensionTravelFrontRight { get; set; }
        public float NormalizedSuspensionTravelRearLeft { get; set; }
        public float NormalizedSuspensionTravelRearRight { get; set; }

        // Tire normalized slip ratio, = 0 means 100% grip and |ratio| > 1.0 means loss of grip.
        public float TireSlipRatioFrontLeft { get; set; }
        public float TireSlipRatioFrontRight { get; set; }
        public float TireSlipRatioRearLeft { get; set; }
        public float TireSlipRatioRearRight { get; set; }

        // Wheel rotation speed radians/sec.
        public float WheelRotationSpeedFrontLeft { get; set; }
        public float WheelRotationSpeedFrontRight { get; set; }
        public float WheelRotationSpeedRearLeft { get; set; }
        public float WheelRotationSpeedRearRight { get; set; }

        // 1 when wheel is on rumble strip, 0 when off.
        public int WheelOnRumbleStripFrontLeft { get; set; }
        public int WheelOnRumbleStripFrontRight { get; set; }
        public int WheelOnRumbleStripRearLeft { get; set; }
        public int WheelOnRumbleStripRearRight { get; set; }

        // From 0 to 1, where 1 is the deepest puddle
        public float WheelInPuddleDepthFrontLeft { get; set; }
        public float WheelInPuddleDepthFrontRight { get; set; }
        public float WheelInPuddleDepthRearLeft { get; set; }
        public float WheelInPuddleDepthRearRight { get; set; }

        // Non-dimensional surface rumble values passed to controller force feedback
        public float SurfaceRumbleFrontLeft { get; set; }
        public float SurfaceRumbleFrontRight { get; set; }
        public float SurfaceRumbleRearLeft { get; set; }
        public float SurfaceRumbleRearRight { get; set; }

        // Tire normalized slip angle, = 0 means 100% grip and |angle| > 1.0 means loss of grip.
        public float TireSlipAngleFrontLeft { get; set; }
        public float TireSlipAngleFrontRight { get; set; }
        public float TireSlipAngleRearLeft { get; set; }
        public float TireSlipAngleRearRight { get; set; }

        // Tire normalized combined slip, = 0 means 100% grip and |slip| > 1.0 means loss of grip.
        public float TireCombinedSlipFrontLeft { get; set; }
        public float TireCombinedSlipFrontRight { get; set; }
        public float TireCombinedSlipRearLeft { get; set; }
        public float TireCombinedSlipRearRight { get; set; }

        // Actual suspension travel in meters
        public float SuspensionTravelFrontLeft { get; set; }
        public float SuspensionTravelFrontRight { get; set; }
        public float SuspensionTravelRearLeft { get; set; }
        public float SuspensionTravelRearRight { get; set; }

        public int CarOrdinal { get; set; } // Unique ID of the car make/model
        public ECarClass CarClass { get; set; } // Between 0 (D class) and 7 (X class) - inclusive
        public int CarPI { get; set; } // 100-999 - inclusive
        public EDrivetrainType DrivetrainType { get; set; } // FWD/RWD/AWD
        public int NumCylinders { get; set; } // Number of cylinders in the engine

        // Position (meters)
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public float Speed { get; set; } // Meters per second
        public float Power { get; set; } // Watts
        public float Torque { get; set; } // Newton meter

        public float TireTempFrontLeft { get; set; }
        public float TireTempFrontRight { get; set; }
        public float TireTempRearLeft { get; set; }
        public float TireTempRearRight { get; set; }

        public float Boost { get; set; }
        public float Fuel { get; set; }
        public float DistanceTraveled { get; set; }
        public float BestLap { get; set; }
        public float LastLap { get; set; }
        public float CurrentLap { get; set; }
        public float CurrentRaceTime { get; set; }

        public UInt16 LapNumber { get; set; }
        public byte RacePosition { get; set; }

        public byte Accel { get; set; }
        public byte Brake { get; set; }
        public byte Clutch { get; set; }
        public byte Handbrake { get; set; }
        public byte Gear { get; set; }
        public sbyte Steer { get; set; }

        public sbyte NormalizedDrivingLine { get; set; }
        public sbyte NormalizedAIBrakeDifference { get; set; }
    }

    public class TelemetryServer
    {

        private UdpClient udpClient;
        private IPEndPoint ep;

        public TelemetryData LastUpdate { get; private set; }

        public TelemetryServer(int port = 9909)
        {
            try
            {
                ep = new IPEndPoint(IPAddress.Loopback, port);
                udpClient = new UdpClient(ep);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.ToString()}");
            }
        }

        public void Start()
        {
            Console.WriteLine($"Listening on {ep.Address.ToString()}:{ep.Port}");
            udpClient.BeginReceive(new AsyncCallback(UdpReceive), null);
        }

        private void UdpReceive(IAsyncResult ar)
        {
            byte[] data = udpClient.EndReceive(ar, ref ep);
            LastUpdate = CreateDataStruct(data);
            PrintData();
            Console.Read();
            udpClient.BeginReceive(new AsyncCallback(UdpReceive), null);
        }

        private void PrintData()
        {
            // First clear the console
            Console.Clear();

            // Print car ID and name (if it exists)
            Console.WriteLine($"ID: {LastUpdate.CarOrdinal}\t{VehicleDB.GetVehicle(LastUpdate.CarOrdinal).ToString()} [ {LastUpdate.CarClass.ToString()} | {LastUpdate.CarPI} ]");
        }

        private TelemetryData CreateDataStruct(byte[] data)
        {
            TelemetryData td = new TelemetryData();

            td.IsRaceOn = BitConverter.ToInt32(data, 0);
            td.TimestampMS = BitConverter.ToUInt32(data, 4);

            td.EngineMaxRPM = BitConverter.ToSingle(data, 8);
            td.EngineIdleRPM = BitConverter.ToSingle(data, 12);
            td.EngineCurrentRPM = BitConverter.ToSingle(data, 16);

            td.AccelerationX = BitConverter.ToSingle(data, 20);
            td.AccelerationY = BitConverter.ToSingle(data, 24);
            td.AccelerationZ = BitConverter.ToSingle(data, 28);

            td.VelocityX = BitConverter.ToSingle(data, 32);
            td.VelocityY = BitConverter.ToSingle(data, 36);
            td.VelocityZ = BitConverter.ToSingle(data, 40);

            td.AngularVelocityX = BitConverter.ToSingle(data, 44);
            td.AngularVelocityY = BitConverter.ToSingle(data, 48);
            td.AngularVelocityZ = BitConverter.ToSingle(data, 52);

            td.Yaw = BitConverter.ToSingle(data, 56);
            td.Pitch = BitConverter.ToSingle(data, 60);
            td.Roll = BitConverter.ToSingle(data, 64);

            td.NormalizedSuspensionTravelFrontLeft = BitConverter.ToSingle(data, 68);
            td.NormalizedSuspensionTravelFrontRight = BitConverter.ToSingle(data, 72);
            td.NormalizedSuspensionTravelRearLeft = BitConverter.ToSingle(data, 76);
            td.NormalizedSuspensionTravelRearRight = BitConverter.ToSingle(data, 80);

            td.TireSlipRatioFrontLeft = BitConverter.ToSingle(data, 84);
            td.TireSlipRatioFrontRight = BitConverter.ToSingle(data, 88);
            td.TireSlipRatioRearLeft = BitConverter.ToSingle(data, 92);
            td.TireSlipRatioRearRight = BitConverter.ToSingle(data, 96);

            td.WheelRotationSpeedFrontLeft = BitConverter.ToSingle(data, 100);
            td.WheelRotationSpeedFrontRight = BitConverter.ToSingle(data, 104);
            td.WheelRotationSpeedRearLeft = BitConverter.ToSingle(data, 108);
            td.WheelRotationSpeedRearRight = BitConverter.ToSingle(data, 112);

            td.WheelOnRumbleStripFrontLeft = BitConverter.ToInt32(data, 116);
            td.WheelOnRumbleStripFrontRight = BitConverter.ToInt32(data, 120);
            td.WheelOnRumbleStripRearLeft = BitConverter.ToInt32(data, 124);
            td.WheelOnRumbleStripRearRight = BitConverter.ToInt32(data, 128);

            td.WheelInPuddleDepthFrontLeft = BitConverter.ToSingle(data, 132);
            td.WheelInPuddleDepthFrontRight = BitConverter.ToSingle(data, 136);
            td.WheelInPuddleDepthRearLeft = BitConverter.ToSingle(data, 140);
            td.WheelInPuddleDepthRearRight = BitConverter.ToSingle(data, 144);

            td.SurfaceRumbleFrontLeft = BitConverter.ToSingle(data, 148);
            td.SurfaceRumbleFrontRight = BitConverter.ToSingle(data, 152);
            td.SurfaceRumbleRearLeft = BitConverter.ToSingle(data, 156);
            td.SurfaceRumbleRearRight = BitConverter.ToSingle(data, 160);

            td.TireSlipAngleFrontLeft = BitConverter.ToSingle(data, 164);
            td.TireSlipAngleFrontRight = BitConverter.ToSingle(data, 168);
            td.TireSlipAngleRearLeft = BitConverter.ToSingle(data, 172);
            td.TireSlipAngleRearRight = BitConverter.ToSingle(data, 176);

            td.TireCombinedSlipFrontLeft = BitConverter.ToSingle(data, 180);
            td.TireCombinedSlipFrontRight = BitConverter.ToSingle(data, 184);
            td.TireCombinedSlipRearLeft = BitConverter.ToSingle(data, 188);
            td.TireCombinedSlipRearRight = BitConverter.ToSingle(data, 192);

            td.SuspensionTravelFrontLeft = BitConverter.ToSingle(data, 196);
            td.SuspensionTravelFrontRight = BitConverter.ToSingle(data, 200);
            td.SuspensionTravelRearLeft = BitConverter.ToSingle(data, 204);
            td.SuspensionTravelRearRight = BitConverter.ToSingle(data, 208);

            td.CarOrdinal = BitConverter.ToInt32(data, 212);

            td.CarClass = (ECarClass)BitConverter.ToInt32(data, 216);
            td.CarPI = BitConverter.ToInt32(data, 220);
            td.DrivetrainType = (EDrivetrainType)BitConverter.ToInt32(data, 224);
            td.NumCylinders = BitConverter.ToInt32(data, 228);

            td.PositionX = BitConverter.ToSingle(data, 232);
            td.PositionY = BitConverter.ToSingle(data, 236);
            td.PositionZ = BitConverter.ToSingle(data, 240);

            td.Speed = BitConverter.ToSingle(data, 244);
            td.Power = BitConverter.ToSingle(data, 248);
            td.Torque = BitConverter.ToSingle(data, 252);

            td.TireTempFrontLeft = BitConverter.ToSingle(data, 256);
            td.TireTempFrontRight = BitConverter.ToSingle(data, 260);
            td.TireTempRearLeft = BitConverter.ToSingle(data, 264);
            td.TireTempRearRight = BitConverter.ToSingle(data, 268);

            td.Boost = BitConverter.ToSingle(data, 272);
            td.Fuel = BitConverter.ToSingle(data, 276);
            td.DistanceTraveled = BitConverter.ToSingle(data, 280);

            td.BestLap = BitConverter.ToSingle(data, 284);
            td.LastLap = BitConverter.ToSingle(data, 288);
            td.CurrentLap = BitConverter.ToSingle(data, 292);
            td.CurrentRaceTime = BitConverter.ToSingle(data, 296);
            td.LapNumber = BitConverter.ToUInt16(data, 300);

            td.RacePosition = data[302];
            td.Accel = data[303];
            td.Brake = data[304];
            td.Clutch = data[305];
            td.Handbrake = data[306];
            td.Gear = data[307];
            td.Steer = (sbyte)data[308];
            td.NormalizedDrivingLine = (sbyte)data[309];
            td.NormalizedAIBrakeDifference = (sbyte)data[310];

            return td;
        }
    }
}
