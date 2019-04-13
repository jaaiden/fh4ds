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
        public int CarClass { get; set; } // Between 0 (D class) and 7 (X class) - inclusive
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
                ep = new IPEndPoint(IPAddress.Any, port);
                udpClient = new UdpClient(ep);
                udpClient.BeginReceive(UdpReceive, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.ToString()}");
            }
        }

        private void UdpReceive(IAsyncResult ar)
        {
            byte[] data = udpClient.EndReceive(ar, ref ep);
            udpClient.BeginReceive(UdpReceive, null);
            LastUpdate = CreateDataStruct(data);
            PrintData();
        }

        private void PrintData ()
        {
            // First clear the console
            Console.Clear();

            // Print car ID and name (if it exists)
            Console.WriteLine($"ID: {LastUpdate.CarOrdinal}\t{VehicleDB.GetVehicle(LastUpdate.CarOrdinal).ToString()}");
        }

        private TelemetryData CreateDataStruct(byte[] data)
        {
            int pos = 0;
            TelemetryData td = new TelemetryData();

            td.IsRaceOn = data[pos];
            pos += 4;

            td.TimestampMS = data[pos];
            pos += 4;


            td.EngineMaxRPM = data[pos];
            pos += 4;

            td.EngineIdleRPM = data[pos];
            pos += 4;

            td.EngineCurrentRPM = data[pos];
            pos += 4;


            td.AccelerationX = data[pos];
            pos += 4;

            td.AccelerationY = data[pos];
            pos += 4;

            td.AccelerationZ = data[pos];
            pos += 4;


            td.VelocityX = data[pos];
            pos += 4;

            td.VelocityY = data[pos];
            pos += 4;

            td.VelocityZ = data[pos];
            pos += 4;


            td.AngularVelocityX = data[pos];
            pos += 4;

            td.AngularVelocityY = data[pos];
            pos += 4;

            td.AngularVelocityZ = data[pos];
            pos += 4;


            td.Yaw = data[pos];
            pos += 4;

            td.Pitch = data[pos];
            pos += 4;

            td.Roll = data[pos];
            pos += 4;


            td.NormalizedSuspensionTravelFrontLeft = data[pos];
            pos += 4;

            td.NormalizedSuspensionTravelFrontRight = data[pos];
            pos += 4;

            td.NormalizedSuspensionTravelRearLeft = data[pos];
            pos += 4;

            td.NormalizedSuspensionTravelRearRight = data[pos];
            pos += 4;


            td.TireSlipRatioFrontLeft = data[pos];
            pos += 4;

            td.TireSlipRatioFrontRight = data[pos];
            pos += 4;

            td.TireSlipRatioRearLeft = data[pos];
            pos += 4;

            td.TireSlipRatioRearRight = data[pos];
            pos += 4;


            td.WheelRotationSpeedFrontLeft = data[pos];
            pos += 4;

            td.WheelRotationSpeedFrontRight = data[pos];
            pos += 4;

            td.WheelRotationSpeedRearLeft = data[pos];
            pos += 4;

            td.WheelRotationSpeedRearRight = data[pos];
            pos += 4;


            td.WheelOnRumbleStripFrontLeft = data[pos];
            pos += 4;

            td.WheelOnRumbleStripFrontRight = data[pos];
            pos += 4;

            td.WheelOnRumbleStripRearLeft = data[pos];
            pos += 4;

            td.WheelOnRumbleStripRearRight = data[pos];
            pos += 4;


            td.WheelInPuddleDepthFrontLeft = data[pos];
            pos += 4;

            td.WheelInPuddleDepthFrontRight = data[pos];
            pos += 4;

            td.WheelInPuddleDepthRearLeft = data[pos];
            pos += 4;

            td.WheelInPuddleDepthRearRight = data[pos];
            pos += 4;


            td.SurfaceRumbleFrontLeft = data[pos];
            pos += 4;

            td.SurfaceRumbleFrontRight = data[pos];
            pos += 4;

            td.SurfaceRumbleRearLeft = data[pos];
            pos += 4;

            td.SurfaceRumbleRearRight = data[pos];
            pos += 4;


            td.TireSlipAngleFrontLeft = data[pos];
            pos += 4;

            td.TireSlipAngleFrontRight = data[pos];
            pos += 4;

            td.TireSlipAngleRearLeft = data[pos];
            pos += 4;

            td.TireSlipAngleRearRight = data[pos];
            pos += 4;


            td.TireCombinedSlipFrontLeft = data[pos];
            pos += 4;

            td.TireCombinedSlipFrontRight = data[pos];
            pos += 4;

            td.TireCombinedSlipRearLeft = data[pos];
            pos += 4;

            td.TireCombinedSlipRearRight = data[pos];
            pos += 4;


            td.SuspensionTravelFrontLeft = data[pos];
            pos += 4;

            td.SuspensionTravelFrontRight = data[pos];
            pos += 4;

            td.SuspensionTravelRearLeft = data[pos];
            pos += 4;

            td.SuspensionTravelRearRight = data[pos];
            pos += 4;


            td.CarOrdinal = data[pos];
            pos += 4;

            td.CarClass = data[pos];
            pos += 4;

            td.CarPI = data[pos];
            pos += 4;

            td.DrivetrainType = (EDrivetrainType)data[pos];
            pos += 4;

            td.NumCylinders = data[pos];
            pos += 4;


            td.PositionX = data[pos];
            pos += 4;

            td.PositionY = data[pos];
            pos += 4;

            td.PositionZ = data[pos];
            pos += 4;


            td.Speed = data[pos];
            pos += 4;

            td.Power = data[pos];
            pos += 4;

            td.Torque = data[pos];
            pos += 4;


            td.TireTempFrontLeft = data[pos];
            pos += 4;

            td.TireTempFrontRight = data[pos];
            pos += 4;

            td.TireTempRearLeft = data[pos];
            pos += 4;

            td.TireTempRearRight = data[pos];
            pos += 4;


            td.Boost = data[pos];
            pos += 4;

            td.Fuel = data[pos];
            pos += 4;

            td.DistanceTraveled = data[pos];
            pos += 4;

            td.BestLap = data[pos];
            pos += 4;

            td.LastLap = data[pos];
            pos += 4;

            td.CurrentLap = data[pos];
            pos += 4;

            td.CurrentRaceTime = data[pos];
            pos += 4;


            td.LapNumber = data[pos];
            pos += 2;

            td.RacePosition = data[pos];
            pos += 1;


            td.Accel = data[pos];
            pos += 1;

            td.Brake = data[pos];
            pos += 1;

            td.Clutch = data[pos];
            pos += 1;

            td.Handbrake = data[pos];
            pos += 1;

            td.Gear = data[pos];
            pos += 1;

            td.Steer = (sbyte)data[pos];
            pos += 1;


            td.NormalizedDrivingLine = (sbyte)data[pos];
            pos += 1;

            td.NormalizedAIBrakeDifference = (sbyte)data[pos];
            pos += 1;

            return td;
        }
    }
}
