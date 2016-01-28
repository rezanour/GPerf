using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GPerf
{
    public partial class MainForm : Form
    {
        GPUTrace trace = null;
        DateTime startTime = DateTime.MinValue;
        DateTime endTime = DateTime.MaxValue;
        List<DmaPacketVisual> dmaVisuals = new List<DmaPacketVisual>();
        BufferedGraphicsContext bufferedContext = new BufferedGraphicsContext();
        Point currentMousePosition = new Point(-1, -1);
        bool selecting = false;
        bool selectionActive = false;
        DateTime selectionStart = DateTime.MinValue;
        DateTime selectionEnd = DateTime.MinValue;

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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectionActive && e.Control && e.KeyCode == Keys.Z)
            {
                // Zoom
                selectionActive = false;
                startTime = selectionStart;
                endTime = selectionEnd;
                Refresh();
            }
            else if (e.KeyCode == Keys.Z)
            {
                // Unzoom
                selectionActive = false;
                long deltaTicks = (endTime - startTime).Ticks / 4;
                startTime -= TimeSpan.FromTicks(deltaTicks);
                if (startTime < trace.StartTime)
                {
                    startTime = trace.StartTime;
                }
                endTime += TimeSpan.FromTicks(deltaTicks);
                if (endTime > trace.EndTime)
                {
                    endTime = trace.EndTime;
                }
                Refresh();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                selectionActive = false;
                selecting = false;
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (dmaVisuals.Count == 0)
            {
                BuildDmaVisuals();
            }

            float secondsToX = ClientSize.Width / (float)((endTime - startTime).TotalSeconds);
            if (float.IsNaN(secondsToX) || float.IsInfinity(secondsToX))
            {
                return;
            }

            e.Graphics.DrawLine(Pens.Black, new Point(0, 100), new Point(ClientSize.Width, 100));
            List<Rectangle> rectangles = new List<Rectangle>();
            foreach (DmaPacketVisual visual in dmaVisuals)
            {
                if (visual.StartTime < endTime)
                {
                    if (visual.EndTime > startTime)
                    {
                        Rectangle rect = new Rectangle(
                            (int)((visual.StartTime - startTime).TotalSeconds * secondsToX),
                            90 - visual.Height * 10,
                            Math.Max((int)((visual.EndTime - visual.StartTime).TotalSeconds * secondsToX), 1),
                            10);
                        rectangles.Add(rect);
                    }
                }
                else
                {
                    // Remainder of packets are off to the right offscreen
                    break;
                }
            }

            e.Graphics.FillRectangles(Brushes.Blue, rectangles.ToArray());
            e.Graphics.DrawRectangles(Pens.Black, rectangles.ToArray());

            // Selection/mouse stuff
            if (selecting)
            {
                // Draw start marker
                int selStartX = (int)((selectionStart - startTime).TotalSeconds * secondsToX);
                e.Graphics.DrawLine(Pens.Red, new Point(selStartX, 0), new Point(selStartX, ClientSize.Height));

                Color c = Color.FromArgb(64, 128, 0, 0);
                Brush b = new SolidBrush(c);

                e.Graphics.FillRectangle(b, new Rectangle(selStartX, 0, currentMousePosition.X - selStartX, ClientSize.Height));

                // Draw current
                e.Graphics.DrawLine(Pens.Red, new Point(currentMousePosition.X, 0), new Point(currentMousePosition.X, ClientSize.Height));
            }
            else if (selectionActive)
            {
                // Draw start marker
                int selStartX = (int)((selectionStart - startTime).TotalSeconds * secondsToX);
                e.Graphics.DrawLine(Pens.Red, new Point(selStartX, 0), new Point(selStartX, ClientSize.Height));

                Color c = Color.FromArgb(64, 128, 0, 0);
                Brush b = new SolidBrush(c);

                e.Graphics.FillRectangle(b, new Rectangle(selStartX, 0, currentMousePosition.X - selStartX, ClientSize.Height));

                // Draw end marker
                int selEndX = (int)((selectionEnd - startTime).TotalSeconds * secondsToX);
                e.Graphics.DrawLine(Pens.Red, new Point(selEndX, 0), new Point(selEndX, ClientSize.Height));
            }
            else
            {
                e.Graphics.DrawLine(Pens.Red, new Point(currentMousePosition.X, 0), new Point(currentMousePosition.X, ClientSize.Height));
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            float xToSeconds = (float)((endTime - startTime).TotalSeconds) / (float)ClientSize.Width;
            float seconds = e.Location.X * xToSeconds;
            DateTime time = startTime + TimeSpan.FromSeconds(seconds);

            selecting = true;
            selectionStart = time;
            Refresh();
        }

        private void MainForm_MouseLeave(object sender, EventArgs e)
        {
            if (selecting)
            {
                selecting = false;
            }

            currentMousePosition = new Point(-1, -1);
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePosition = e.Location;
            float xToSeconds = (float)((endTime - startTime).TotalSeconds) / (float)ClientSize.Width;
            float seconds = e.Location.X * xToSeconds;
            DateTime time = startTime + TimeSpan.FromSeconds(seconds);
            cursorPositionStatusLabel.Text = "Position: " + (time - trace.StartTime).TotalMilliseconds.ToString("##,###.####ms");
            Refresh();
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            float xToSeconds = (float)((endTime - startTime).TotalSeconds) / (float)ClientSize.Width;
            float seconds = e.Location.X * xToSeconds;
            DateTime time = startTime + TimeSpan.FromSeconds(seconds);

            selecting = false;
            selectionEnd = time;
            selectionActive = true;
            Refresh();
        }
    }
}
