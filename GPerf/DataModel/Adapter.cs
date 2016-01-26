using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPerf
{
    class AdapterInfo
    {
        public ulong pDxgiAdapter = 0;
        public long AdapterLuid = 0;
        public int VendorID = 0;
        public string Name = "";
        public ulong VideoMemoryBytes = 0;
        public ulong NodeCount = 0;
    }

    class AdapterNode
    {
        public AdapterInfo Adapter = null;
        public Dictionary<int, DmaPacket> DmaPackets = new Dictionary<int, DmaPacket>();
    }

    class DmaPacket
    {
        public AdapterNode Node = null;
        public int PacketType = 0;
        public long SubmissionId = 0;    // Global (to the node) submission id
        public long QueueSubmitSequence = 0; // The sequence number within the queue that submitted it (to match with queue packet)
        public DateTime Start = DateTime.MinValue;
        public DateTime End = DateTime.MaxValue;
    }

    class SchedulingContext
    {
        public ulong hContext = 0;
        public AdapterNode Node = null;
        public Dictionary<long, DmaPacket> DmaPackets = new Dictionary<long, DmaPacket>();
    }
}
