
using System;

namespace MagneticDoorSensorShared
{
    public class SensorData
    {
        public SensorState State { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
