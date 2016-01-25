using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics;

namespace GPerf
{
    class GPUTrace
    {
        Guid dxgkrnlGuid = new Guid("802ec45a-1e99-4b83-9920-87c98277ba9d");
        Guid ntGuid = new Guid(); // all 0s is NT guid

        public GPUTrace()
        {
        }

        public void Load(string fileName)
        {
            ETWTraceEventSource source = new ETWTraceEventSource(fileName);
            RegisteredTraceEventParser parser = new RegisteredTraceEventParser(source);

            parser.All += Parser_All;
            source.Process();
        }

        private void Parser_All(TraceEvent obj)
        {
            if (obj.ProviderGuid == ntGuid)
            {
                if (obj.EventName.Contains("Process"))
                {
                    Debug.WriteLine(obj.TaskName);
                }
            }
            else if (obj.ProviderGuid == dxgkrnlGuid)
            {
                switch ((int)obj.ID)
                {
                    case 26:    // Adapter/DC_Start
                        Debug.Assert(obj.EventName == "Adapter/DC_Start");
                        break;

                    case 110:   // DpiReportAdapter
                        {
                            Debug.Assert(obj.EventName == "DpiReportAdapter" && obj.PayloadNames.Length == 12);
                            ulong pDxgiAdapter = (ulong)obj.PayloadValue(0);
                            int vendorId = (int)obj.PayloadValue(6);
                            int deviceId = (int)obj.PayloadValue(7);
                            long adapterLuid = (long)obj.PayloadValue(11);
                        }
                        break;

                    case 175:   // DmaPacket/Start
                        Debug.Assert(obj.EventName == "DmaPacket/Start" && obj.PayloadNames.Length == 7);
                        break;

                    case 176:   // DmaPacket/Stop
                        Debug.Assert(obj.EventName == "DmaPacket/Stop");
                        break;

                    case 177:   // DmaPacket Info
                        Debug.Assert(obj.EventName == "DmaPacket" && (int)obj.Opcode == 0);
                        break;

                    case 178:   // QueuePacket/Start
                        Debug.Assert(obj.EventName == "QueuePacket/Start" && obj.PayloadNames.Length == 8);
                        break;

                    case 245:   // QueuePacket/Start
                        Debug.Assert(obj.EventName == "QueuePacket/Start" && obj.PayloadNames.Length == 4);
                        break;

                    case 179:   // QueuePacket Info
                        Debug.Assert(obj.EventName == "QueuePacket" && (int)obj.Opcode == 0);
                        break;

                    case 180:   // QueuePacket/Stop
                        Debug.Assert(obj.EventName == "QueuePacket/Stop");
                        break;

                    case 181:   // VSyncInterrupt
                        {
                            Debug.Assert(obj.EventName == "VSyncInterrupt" && obj.PayloadNames.Length == 3);
                            ulong pDxgiAdapter = (ulong)obj.PayloadValue(0);
                            int vidPnTargetId = (int)obj.PayloadValue(1);
                            long scannedPhysicalAddress = (long)obj.PayloadValue(2);
                        }
                        break;

                    case 17:    // VSyncDPC
                        Debug.Assert(obj.EventName == "VSyncDPC" && obj.PayloadNames.Length == 9);
                        break;

                    case 273:   // VSyncDPCMultiPlane
                        Debug.Assert(obj.EventName == "VSyncDPCMultiPlane" && obj.PayloadNames.Length == 5);
                        break;

                    #region Unused DxgKrnl events

                    case 18:    // WorkerThread/Start
                        break;

                    case 19:    // WorkerThread/Stop
                        break;

                    case 20:    // UpdateContextStatus
                        break;

                    case 22:    // AttemptPreemption
                        break;

                    case 23:    // SelectContext
                        break;

                    case 95:    // Fence/DC_Start
                        break;

                    case 105:   // Profiler/Start
                        break;

                    case 106:   // Profiler/Stop
                        break;

                    case 113:   // UpdateContextRunningTime
                        break;

                    case 116:   // MMIOFlip
                        break;

                    case 164:   // EtwVersion/Stop
                        break;

                    case 168:   // Flip
                        break;

                    case 169:   // Render
                        break;

                    case 172:   // PresentHistory
                        break;

                    case 173:   // PresentHistory/Stop
                        break;

                    case 182:   // GetDeviceState
                        break;

                    case 184:   // Present
                        break;

                    case 215:   // PresentHistoryDetailed/Start
                        break;

                    case 230:   // SignalSynchronizationObject2
                        break;

                    case 231:   // WaitForSynchronizationObject2
                        break;

                    case 238:   // UnwaitQueuePacket
                        break;

                    case 262:   // CalibrateGpuClockTask
                        break;

                    case 305:   // ReferenceWrittenPrimaries
                        break;

                    case 318:   // DWMVSyncCountWait
                        break;

                    case 319:   // DWMVSyncSignal
                        break;

                    case 354:   // YieldStartAdapter
                        break;

                    case 355:   // YieldStartNode
                        break;

                    case 356:   // YieldStopNode
                        break;

                    #endregion

                    default:
                        Debug.WriteLine(obj.TaskName);
                        break;
                }
            }
        }
    }
}
