using System;

namespace MagneticDoorSensorShared
{
    public class SensorData
    {
        public string PartitionKey { get; set; } = "rpi2b";

        public string RowKey { get;  set; } = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);

        public SensorState State { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
