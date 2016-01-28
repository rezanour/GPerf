using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPerf
{
    // a block displayed in the visualization of a dma packet.
    // May be just a portion of the work
    class DmaPacketVisual
    {
        public DmaPacket Packet;
        public DateTime StartTime;
        public DateTime EndTime;
        public int Height;  // how may "blocks" up is this (queue depth for this piece)
    }
}
