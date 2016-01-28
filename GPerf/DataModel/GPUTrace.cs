using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics;

namespace GPerf
{
    class GPUTrace
    {
        #region Properties

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public List<ProcessInfo> AllProcesses { get; private set; }
        public List<AdapterInfo> AllAdapters { get; private set; }
        public List<DeviceInfo> AllDevices { get; private set; }
        public List<ContextInfo> AllContexts { get; private set; }

        public List<DmaPacket> GetDmaPackets()
        {
            List<DmaPacket> allPackets = new List<DmaPacket>();
            foreach (ContextInfo context in AllContexts)
            {
                allPackets.AddRange(context.DmaPackets);
            }
            allPackets.Sort(new Comparison<DmaPacket>(
                (DmaPacket a, DmaPacket b) =>
                {
                    if (a.Start < b.Start)
                    {
                        return -1;
                    }
                    else if (b.Start < a.Start)
                    {
                        return 1;
                    }
                    return 0;
                }));
            return allPackets;
        }
        #endregion Properties

        #region Fields

        bool seenStartTime = false;

        // Provider Guids for providers we care about
        private Guid ntGuid = new Guid(); // all 0s is NT guid
        private Guid dxgkrnlGuid = new Guid("802ec45a-1e99-4b83-9920-87c98277ba9d");

        // Dxgkrnl event ids that we care about
        private enum DxgkrnlEvent
        {
            Adapter_DCStart = 26,
            Device_DCStart = 29,
            Context_DCStart = 32,
            DpiReportAdapter = 110,
            DmaPacket_Start = 175,
            DmaPacket_Stop = 176,
            DmaPacket_Info = 177,
            QueuePacket_Start = 178,
            QueuePacket_Start2 = 245,
            QueuePacket_Info = 179,
            QueuePacket_Stop = 180,
            VSyncInterrupt = 181,
            VSyncDPC = 17,
            VSyncDPCMultiPlane = 273,
            // NOTE: For list of other Dxgkrnl event ids, see bottom of this file
        }

        // Dictionary of event id -> handler
        private Dictionary<int, Action<TraceEvent>> dxgkrnlEventHandlers = new Dictionary<int, Action<TraceEvent>>();

        //
        // Parsed data
        //

        // Dictionary of processId -> ProcessInfo
        private Dictionary<int, ProcessInfo> processLookup = new Dictionary<int, ProcessInfo>();

        // Dictionary of pDxgiAdapter -> AdapterInfo
        private Dictionary<ulong, AdapterInfo> adapterLookup = new Dictionary<ulong, AdapterInfo>();

        // Dictionary of hDevice -> DeviceInfo
        private Dictionary<ulong, DeviceInfo> deviceLookup = new Dictionary<ulong, DeviceInfo>();

        // Dictionary of hContext -> ContextInfo
        private Dictionary<ulong, ContextInfo> contextLookup = new Dictionary<ulong, ContextInfo>();

        #endregion Fields

        #region Ctor & Load
        public GPUTrace()
        {
            AllProcesses = new List<ProcessInfo>();
            AllAdapters = new List<AdapterInfo>();
            AllDevices = new List<DeviceInfo>();
            AllContexts = new List<ContextInfo>();

            // set up the table
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.Adapter_DCStart, OnDxgkrnlAdapterStart);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.Device_DCStart, OnDxgkrnlDeviceStart);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.Context_DCStart, OnDxgkrnlContextStart);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.DpiReportAdapter, OnDxgkrnlDpiReportAdapter);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.DmaPacket_Start, OnDxgkrnlDmaPacketStart);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.DmaPacket_Info, OnDxgkrnlDmaPacketInfo);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.DmaPacket_Stop, OnDxgkrnlDmaPacketStop);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.QueuePacket_Start, OnDxgkrnlQueuePacketStart);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.QueuePacket_Start2, OnDxgkrnlQueuePacketStart2);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.QueuePacket_Info, OnDxgkrnlQueuePacketInfo);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.QueuePacket_Stop, OnDxgkrnlQueuePacketStop);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.VSyncInterrupt, OnDxgkrnlVSyncInterrupt);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.VSyncDPC, OnDxgkrnlVSyncDPC);
            dxgkrnlEventHandlers.Add((int)DxgkrnlEvent.VSyncDPCMultiPlane, OnDxgkrnlVSyncDPCMultiPlane);
        }

        public void Load(string fileName)
        {
            ETWTraceEventSource source = new ETWTraceEventSource(fileName);
            RegisteredTraceEventParser parser = new RegisteredTraceEventParser(source);

            parser.All += Parser_All;
            source.Process();
        }
        #endregion Ctor & Load

        #region Top Level Parsing
        private void Parser_All(TraceEvent obj)
        {
            if (!seenStartTime)
            {
                StartTime = obj.TimeStamp;
                seenStartTime = true;
            }

            EndTime = obj.TimeStamp;

            if (obj.ProviderGuid == ntGuid)
            {
                if (obj.EventName.Contains("Process"))
                {
                    Debug.WriteLine(obj.TaskName);
                }
            }
            else if (obj.ProviderGuid == dxgkrnlGuid)
            {
                int id = (int)obj.ID;
                if (dxgkrnlEventHandlers.ContainsKey(id))
                {
                    dxgkrnlEventHandlers[id](obj);
                }
            }
        }
        #endregion Top Level Parsing

        #region Dxgkrnl Events

        void OnDxgkrnlAdapterStart(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "Adapter/DC_Start" && obj.PayloadNames.Length == 27);

            Debug.Assert(obj.PayloadNames[1] == "pDxgAdapter");
            ulong pDxgAdapter = (ulong)obj.PayloadValue(1);

            if (adapterLookup.ContainsKey(pDxgAdapter))
            {
                Debug.Assert(false);
                return;
            }

            AdapterInfo adapter = new AdapterInfo();
            AllAdapters.Add(adapter);
            adapterLookup.Add(pDxgAdapter, adapter);

            adapter.pDxgiAdapter = pDxgAdapter;

            Debug.Assert(obj.PayloadNames[2] == "NbVidPnSources");
            adapter.NumVidPnSources = (int)obj.PayloadValue(2);

            Debug.Assert(obj.PayloadNames[22] == "PagingNode");
            adapter.PagingNode = (int)obj.PayloadValue(22);

            Debug.Assert(obj.PayloadNames[25] == "AdapterType");
            adapter.AdapterType = (AdapterType)(int)obj.PayloadValue(25);
        }

        void OnDxgkrnlDeviceStart(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "Device/DC_Start" && obj.PayloadNames.Length == 6);

            Debug.Assert(obj.PayloadNames[0] == "hProcessId");
            int processId = (int)(UInt64)obj.PayloadValue(0);

            Debug.Assert(obj.PayloadNames[1] == "pDxgAdapter");
            ulong pDxgAdapter = (ulong)obj.PayloadValue(1);

            Debug.Assert(obj.PayloadNames[3] == "hDevice");
            ulong hDevice = (ulong)obj.PayloadValue(3);

            // Make sure the device doesn't already exist (error if it does)
            if (deviceLookup.ContainsKey(hDevice))
            {
                Debug.Assert(false);
                return;
            }

            DeviceInfo device = new DeviceInfo();
            AllDevices.Add(device);
            deviceLookup.Add(hDevice, device);

            device.hDevice = hDevice;
            device.Adapter = FindOrCreateAdapter(pDxgAdapter);
            device.Process = FindOrCreateProcess(processId);
        }

        void OnDxgkrnlContextStart(TraceEvent obj)
        {
        }

        void OnDxgkrnlDpiReportAdapter(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "DpiReportAdapter" && obj.PayloadNames.Length == 12);

            Debug.Assert(obj.PayloadNames[0] == "pDxgAdapter");
            ulong pDxgiAdapter = (ulong)obj.PayloadValue(0);

            if (!adapterLookup.ContainsKey(pDxgiAdapter))
            {
                Debug.Assert(false);
                return;
            }

            AdapterInfo adapter = adapterLookup[pDxgiAdapter];

            Debug.Assert(obj.PayloadNames[6] == "VendorID");
            adapter.VendorID = (int)obj.PayloadValue(6);

            Debug.Assert(obj.PayloadNames[11] == "AdapterLuid");
            adapter.AdapterLuid = (long)obj.PayloadValue(11);
        }

        void OnDxgkrnlDmaPacketStart(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "DmaPacket/Start" && obj.PayloadNames.Length == 7);

            Debug.Assert(obj.PayloadNames[0] == "hContext");
            ulong hContext = (ulong)obj.PayloadValue(0);

            Debug.Assert(obj.PayloadNames[2] == "PacketType");
            int packetType = (int)obj.PayloadValue(2);

            Debug.Assert(obj.PayloadNames[3] == "uliSubmissionId");
            long submissionId = (long)obj.PayloadValue(3);

            Debug.Assert(obj.PayloadNames[4] == "ulQueueSubmitSequence");
            int queueSubmitSequence = (int)obj.PayloadValue(4);

            ContextInfo context = FindOrCreateContext(hContext);

            DmaPacket packet = null;
            if (context.DmaLookup.ContainsKey(submissionId))
            {
                packet = context.DmaLookup[submissionId];
            }
            else
            {
                packet = new DmaPacket();
                packet.SubmissionId = submissionId;
                packet.QueueSubmitSequence = queueSubmitSequence;
                packet.PacketType = packetType;
                context.DmaPackets.Add(packet);
                context.DmaLookup.Add(submissionId, packet);
            }

            packet.Start = obj.TimeStamp;
        }

        void OnDxgkrnlDmaPacketInfo(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "DmaPacket" && (int)obj.Opcode == 0);

            Debug.Assert(obj.PayloadNames[0] == "hContext");
            ulong hContext = (ulong)obj.PayloadValue(0);

            ContextInfo context = FindOrCreateContext(hContext);
        }

        void OnDxgkrnlDmaPacketStop(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "DmaPacket/Stop");

            Debug.Assert(obj.PayloadNames[0] == "hContext");
            ulong hContext = (ulong)obj.PayloadValue(0);

            Debug.Assert(obj.PayloadNames[1] == "PacketType");
            int packetType = (int)obj.PayloadValue(1);

            Debug.Assert(obj.PayloadNames[2] == "uliCompletionId");
            long submissionId = (long)obj.PayloadValue(2);

            Debug.Assert(obj.PayloadNames[3] == "ulQueueSubmitSequence");
            int queueSubmitSequence = (int)obj.PayloadValue(3);

            ContextInfo context = FindOrCreateContext(hContext);

            DmaPacket packet = null;
            if (!context.DmaLookup.ContainsKey(submissionId))
            {
                // End packet for something we don't about, drop it
                return;
            }

            packet = context.DmaLookup[submissionId];
            packet.End = obj.TimeStamp;
        }

        void OnDxgkrnlQueuePacketStart(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "QueuePacket/Start" && obj.PayloadNames.Length == 8);
        }

        void OnDxgkrnlQueuePacketStart2(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "QueuePacket/Start" && obj.PayloadNames.Length == 4);
        }

        void OnDxgkrnlQueuePacketInfo(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "QueuePacket" && (int)obj.Opcode == 0);
        }

        void OnDxgkrnlQueuePacketStop(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "QueuePacket/Stop");
        }

        void OnDxgkrnlVSyncInterrupt(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "VSyncInterrupt" && obj.PayloadNames.Length == 3);
            ulong pDxgiAdapter = (ulong)obj.PayloadValue(0);
            int vidPnTargetId = (int)obj.PayloadValue(1);
            long scannedPhysicalAddress = (long)obj.PayloadValue(2);
        }

        void OnDxgkrnlVSyncDPC(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "VSyncDPC" && obj.PayloadNames.Length == 9);
        }

        void OnDxgkrnlVSyncDPCMultiPlane(TraceEvent obj)
        {
            Debug.Assert(obj.EventName == "VSyncDPCMultiPlane" && obj.PayloadNames.Length == 5);
        }

        ProcessInfo FindOrCreateProcess(int processId)
        {
            // Already seen this process?
            if (processLookup.ContainsKey(processId))
            {
                return processLookup[processId];
            }
            else
            {
                ProcessInfo process = new ProcessInfo();
                process.ProcessId = processId;
                AllProcesses.Add(process);
                processLookup.Add(processId, process);
                return process;
            }
        }

        AdapterInfo FindOrCreateAdapter(ulong pDxgAdapter)
        {
            // Already seen this adapter?
            if (adapterLookup.ContainsKey(pDxgAdapter))
            {
                return adapterLookup[pDxgAdapter];
            }
            else
            {
                AdapterInfo adapter = new AdapterInfo();
                adapter.pDxgiAdapter = pDxgAdapter;
                AllAdapters.Add(adapter);
                adapterLookup.Add(pDxgAdapter, adapter);
                return adapter;
            }
        }

        DeviceInfo FindOrCreateDevice(ulong hDevice)
        {
            // Already seen this device?
            if (deviceLookup.ContainsKey(hDevice))
            {
                return deviceLookup[hDevice];
            }
            else
            {
                DeviceInfo device = new DeviceInfo();
                device.hDevice = hDevice;
                AllDevices.Add(device);
                deviceLookup.Add(hDevice, device);
                return device;
            }
        }

        ContextInfo FindOrCreateContext(ulong hContext)
        {
            // Already seen this context?
            if (contextLookup.ContainsKey(hContext))
            {
                return contextLookup[hContext];
            }
            else
            {
                ContextInfo context = new ContextInfo();
                context.hContext = hContext;
                AllContexts.Add(context);
                contextLookup.Add(hContext, context);
                return context;
            }
        }

        //
        // Unused Dxgkrnl Event IDs
        //
        //WorkerThread_Start = 18,
        //WorkerThread_Stop = 19,
        //UpdateContextStatus = 20,
        //ChangePriority = 21,
        //AttemptPreemption = 22,
        //SelectContext = 23,
        //AdapterAllocation_DCStart = 35,
        //DeviceAllocation_DC_Start = 38,
        //RefrenceAllocations = 43,
        //PatchLocationList = 45,
        //MarkAllocation = 72,
        //EvictAllocation = 74,
        //AddDmaBuffer_Start = 76,
        //ReportSegment = 78,
        //ReportCommittedAllocation = 80,
        //SynchronizationMutex_DCStart = 87,
        //Fence_DCStart = 95,
        //case 102:   // SetDisplayMode
        //case 103:   // BlockThread
        //case 105:   // Profiler/Start
        //case 106:   // Profiler/Stop
        //case 107:   // ExtendedProvider/Start
        //case 108:   // ExtendedProfiler/Stop
        //case 113:   // UpdateContextRunningTime
        //case 116:   // MMIOFlip
        //case 164:   // EtwVersion/Stop
        //case 168:   // Flip
        //case 169:   // Render
        //case 170:   // RenderKm
        //case 172:   // PresentHistory
        //case 173:   // PresentHistory/Stop
        //case 182:   // GetDeviceState
        //case 184:   // Present
        //case 189:   // ReportOfferAllocation
        //case 215:   // PresentHistoryDetailed/Start
        //case 225:   // ProcessAllocation/Start
        //case 227:   // ReportCommittedGlobalAllocation/DC_Start
        //case 230:   // SignalSynchronizationObject2
        //case 231:   // WaitForSynchronizationObject2
        //case 238:   // UnwaitQueuePacket
        //case 246:   // GpuWork
        //case 250:   // NodeMetadata
        //case 262:   // CalibrateGpuClockTask
        //case 275:   // Brightness
        //case 276:   // BacklightOptimizationLevel
        //case 281:   // PagingPreparation/Start
        //case 282:   // PagingPreparation/Stop
        //case 288:   // ProcessAllocationDetails/Start
        //case 293:   // MonitoredFence/DC_Start
        //case 294:   // SignalSynchronizationObjectFromGpu
        //case 300:   // UnwaitCpuWaiter
        //case 301:   // RecycleRangeTracking
        //case 305:   // ReferenceWrittenPrimaries
        //case 318:   // DWMVSyncCountWait
        //case 319:   // DWMVSyncSignal
        //case 320:   // MakeResident
        //case 321:   // VidMmEvict
        //case 322:   // PagingQueuePacket/Start
        //case 323:   // PagingQueuePacket/Start
        //case 324:   // PagingQueuePacket (Info)
        //case 325:   // PagingQueuePacket/Stop
        //case 344:   // ExtendedProfiler
        //case 354:   // YieldStartAdapter
        //case 355:   // YieldStartNode
        //case 356:   // YieldStopNode
        //case 359:   // YieldConditionEvaluation
        //case 360:   // FlushScheduler
        //case 1070:  // ReportSyncObject/Start
        #endregion Dxgkrnl Events
    }
}
