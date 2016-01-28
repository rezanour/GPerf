using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPerf
{
    [Flags]
    enum AdapterType
    {
        Unknown = 0,
        RenderSupported = 0x0001,
        PostDevice = 0x0002,
        SoftwareDevice = 0x0004,
        DisplaySupported = 0x0008,
    }

    class AdapterInfo
    {
        public ulong pDxgiAdapter = 0;
        public int NumVidPnSources = 0;
        public int NumNodes = 0;
        public int PagingNode = 0;
        public AdapterType AdapterType = AdapterType.Unknown;

        public long AdapterLuid = 0;
        public int VendorID = 0;
        public string Name = "";
        public ulong VideoMemoryBytes = 0;
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

    class ContextInfo
    {
        public ulong hContext = 0;
        public AdapterNode Node = null;
        public List<DmaPacket> DmaPackets = new List<DmaPacket>();
        public Dictionary<long, DmaPacket> DmaLookup = new Dictionary<long, DmaPacket>();
    }
}
