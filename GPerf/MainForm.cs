using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

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

    public partial class MainForm : Form
    {
        GPUTrace trace = null;
        DateTime startTime = DateTime.MinValue;
        DateTime endTime = DateTime.MaxValue;
        List<DmaPacketVisual> dmaVisuals = new List<DmaPacketVisual>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }

            trace = new GPUTrace();
            trace.Load(openDialog.FileName);

            startTime = trace.StartTime;
            endTime = trace.EndTime;
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            if (dmaVisuals.Count == 0)
            {
                BuildDmaVisuals();
            }

            startTime = dmaVisuals[1].StartTime;
            endTime = startTime + TimeSpan.FromSeconds(0.025f);

            float secondsToX = mainPanel.ClientSize.Width / (float)((endTime - startTime).TotalSeconds);
            e.Graphics.DrawLine(Pens.Black, new Point(0, 100), new Point(mainPanel.ClientSize.Width, 100));
            foreach (DmaPacketVisual visual in dmaVisuals)
            {
                if (visual.StartTime < endTime)
                {
                    if (visual.EndTime > startTime)
                    {
                        Rectangle rect = new Rectangle(
                            (int)((visual.StartTime - startTime).TotalSeconds * secondsToX),
                            90 - visual.Height * 10,
                            (int)((visual.EndTime - visual.StartTime).TotalSeconds * secondsToX),
                            10);
                        e.Graphics.FillRectangle(Brushes.Blue, rect);
                        e.Graphics.DrawRectangle(Pens.Black, rect);
                    }
                }
                else
                {
                    // Remainder of packets are off to the right offscreen
                    break;
                }
            }
        }

        private void BuildDmaVisuals()
        {
            List<DmaPacket> queue = new List<DmaPacket>();
            DateTime lastEndTime = DateTime.MinValue;
            foreach (DmaPacket packet in trace.GetDmaPackets())
            {
                // Three cases:
                // 1. We have a queue already, and the new packet's start time is less than
                //    the current queue's ending time. Add it to the queue for later processing
                // 2. We have a queue already, and the new packet's start time is after last
                //    ending time in queue. Process the full queue, then start a new one with this item
                // 3. The queue is empty, just start one with this

                if (queue.Count != 0)
                {
                    if (packet.Start < lastEndTime)
                    {
                        queue.Add(packet);
                        if (packet.End > lastEndTime)
                        {
                            lastEndTime = packet.End;
                        }
                    }
                    else
                    {
                        //
                        // Flush the queue
                        //

                        // first item gets a full visual in one piece
                        DmaPacketVisual visual = new DmaPacketVisual();
                        visual.Packet = queue[0];
                        visual.StartTime = visual.Packet.Start;
                        visual.EndTime = visual.Packet.End;
                        visual.Height = 0;
                        dmaVisuals.Add(visual);

                        // the remainder queue up & split up based on previous end times
                        for (int i = 1; i < queue.Count; ++i)
                        {
                            int height = i;
                            DateTime start = queue[i].Start;
                            for (int j = 0; j <= i; ++j)
                            {
                                visual = new DmaPacketVisual();
                                visual.Packet = queue[i];
                                visual.StartTime = start;
                                visual.EndTime = queue[j].End;
                                visual.Height = height;
                                Debug.Assert(height >= 0);
                                height--;
                                start = visual.EndTime;
                                dmaVisuals.Add(visual);
                            }
                        }
                        queue.Clear();

                        queue.Add(packet);
                        lastEndTime = packet.End;
                    }
                }
                else
                {
                    queue.Add(packet);
                    lastEndTime = packet.End;
                }
            }
        }
    }
}
