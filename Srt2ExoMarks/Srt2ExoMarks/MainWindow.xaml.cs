using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Srt2ExoMarks
{
    
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = false;
            }
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            StartConvert(((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString());
        }

        Thread convertThread;
        private void StartConvert(string videoPath)
        {
            bool includeStart = (bool)IncludeStartBox.IsChecked || (bool)IncludeBothBox.IsChecked;
            bool includeEnd = (bool)IncludeEndBox.IsChecked || (bool)IncludeBothBox.IsChecked;

            InfoBox.Text = "转换中...";
            this.AllowDrop = false;
            if (convertThread != null)
                convertThread.Abort();
            convertThread = new Thread(delegate ()
            {
                Exo exo = CreateExo(videoPath, videoPath.Substring(0, videoPath.LastIndexOf('.')) + ".srt", includeStart, includeEnd);
                SaveExo(videoPath.Substring(0, videoPath.LastIndexOf('.')) + ".exo", exo);
                Dispatcher.Invoke(new Action(() =>
                {
                    InfoBox.Text = "转换完成!";
                    this.AllowDrop = true;
                }));
            });
            convertThread.Start();
        }

        private void SaveExo(string path, Exo exo)
        {
            StreamWriter streamWriter = new StreamWriter(path, false, Encoding.Default);
            streamWriter.Write(exo);
            streamWriter.Close();
        }

        private Exo CreateExo(string videoPath, string subtitlePath, bool includeStart, bool includeEnd)
        {
            // Open files
            Info info = new Info(videoPath);
            FileStream subtitleStream = new FileStream(subtitlePath, FileMode.Open);
            Srt srt = new Srt(subtitleStream);
            subtitleStream.Close();

            // Get marks
            List<ulong> marks = new List<ulong>();
            foreach (Srt.SrtItem i in srt.Items)
            {
                if (includeStart)
                    marks.Add((ulong)Math.Round(i.StartTime.ToSeconds() * info.GetFrameRate()));
                if (includeEnd)
                    marks.Add((ulong)Math.Round(i.EndTime.ToSeconds() * info.GetFrameRate()));
            }

            // Create Exo
            Exo exo;
            if (info.GetHasAudio())
                exo = new Exo(info.GetWidth(), info.GetHeight(), info.GetFrameRate(), 1, info.GetFrameCount(), info.GetSamplingRate(), info.GetChannels());
            else
                exo = new Exo(info.GetWidth(), info.GetHeight(), info.GetFrameRate(), 1, info.GetFrameCount(), 0, 0);

            if(info.GetFrameCount() > 1)
            {
                if (marks.LongCount() > 0)
                {
                    // First Video Item
                    Exo.Item firstVideoItem = new Exo.Item(0, 1, marks[0], 1, 1, 1, 1, false, false);
                    Exo.Item.SubItem firstVideoSubItem0 = new Exo.Item.SubItem(0, 0, "Video file");
                    firstVideoSubItem0.AddProperty("Playback position", "1");
                    firstVideoSubItem0.AddProperty("vPlay", "100.0");
                    firstVideoSubItem0.AddProperty("Loop playback", "0");
                    firstVideoSubItem0.AddProperty("Import alpha channel", "0");
                    firstVideoSubItem0.AddProperty("file", videoPath);
                    firstVideoItem.AddSubItem(firstVideoSubItem0);
                    Exo.Item.SubItem firstVideoSubItem1 = new Exo.Item.SubItem(0, 1, "Standard drawing");
                    firstVideoSubItem1.AddProperty("X", "0.0");
                    firstVideoSubItem1.AddProperty("Y", "0.0");
                    firstVideoSubItem1.AddProperty("Z", "0.0");
                    firstVideoSubItem1.AddProperty("Zoom%", "100.00");
                    firstVideoSubItem1.AddProperty("Clearness", "0.0");
                    firstVideoSubItem1.AddProperty("Rotation", "0.00");
                    firstVideoSubItem1.AddProperty("blend", "0");
                    firstVideoItem.AddSubItem(firstVideoSubItem1);
                    exo.AddItem(firstVideoItem);

                    if (marks.LongCount() > 1)
                    {
                        // Video Items chain
                        for (ulong i = 1; i < (ulong)marks.LongCount(); i++)
                        {
                            if (marks[(int)i - 1] + 1 >= info.GetFrameCount())
                                break;
                            Exo.Item videoItem = new Exo.Item(i, marks[(int)i - 1] + 1, marks[(int)i], 1, 1, 1, 1, false, true);
                            Exo.Item.SubItem videoSubItem0 = new Exo.Item.SubItem(i, 0, "Video file");
                            videoSubItem0.AddProperty("Playback position", "1");
                            videoSubItem0.AddProperty("vPlay", "100.0");
                            videoSubItem0.AddProperty("Loop playback", "0");
                            videoSubItem0.AddProperty("Import alpha channel", "0");
                            videoItem.AddSubItem(videoSubItem0);
                            Exo.Item.SubItem videoSubItem1 = new Exo.Item.SubItem(i, 1, "Standard drawing");
                            videoSubItem1.AddProperty("X", "0.0");
                            videoSubItem1.AddProperty("Y", "0.0");
                            videoSubItem1.AddProperty("Z", "0.0");
                            videoSubItem1.AddProperty("Zoom%", "100.00");
                            videoSubItem1.AddProperty("Clearness", "0.0");
                            videoSubItem1.AddProperty("Rotation", "0.00");
                            videoItem.AddSubItem(videoSubItem1);
                            exo.AddItem(videoItem);
                        }
                    }
                    // Last Video Item
                    Exo.Item lastVideoItem = new Exo.Item((ulong)exo.Items.LongCount(), exo.Items.Last().end + 1, info.GetFrameCount(), 1, 1, 1, 1, false, true);
                    Exo.Item.SubItem lastVideoSubItem0 = new Exo.Item.SubItem((ulong)exo.Items.LongCount(), 0, "Video file");
                    lastVideoSubItem0.AddProperty("Playback position", "1");
                    lastVideoSubItem0.AddProperty("vPlay", "100.0");
                    lastVideoSubItem0.AddProperty("Loop playback", "0");
                    lastVideoSubItem0.AddProperty("Import alpha channel", "0");
                    lastVideoItem.AddSubItem(lastVideoSubItem0);
                    Exo.Item.SubItem lastVideoSubItem1 = new Exo.Item.SubItem((ulong)exo.Items.LongCount(), 1, "Standard drawing");
                    lastVideoSubItem1.AddProperty("X", "0.0");
                    lastVideoSubItem1.AddProperty("Y", "0.0");
                    lastVideoSubItem1.AddProperty("Z", "0.0");
                    lastVideoSubItem1.AddProperty("Zoom%", "100.00");
                    lastVideoSubItem1.AddProperty("Clearness", "0.0");
                    lastVideoSubItem1.AddProperty("Rotation", "0.00");
                    lastVideoItem.AddSubItem(lastVideoSubItem1);
                    exo.AddItem(lastVideoItem);
                }
                else
                {
                    // Video Item without mark
                    Exo.Item firstVideoItem = new Exo.Item(0, 1, info.GetFrameCount(), 1, 1, 1, 1, false, false);
                    Exo.Item.SubItem firstVideoSubItem0 = new Exo.Item.SubItem(0, 0, "Video file");
                    firstVideoSubItem0.AddProperty("Playback position", "1");
                    firstVideoSubItem0.AddProperty("vPlay", "100.0");
                    firstVideoSubItem0.AddProperty("Loop playback", "0");
                    firstVideoSubItem0.AddProperty("Import alpha channel", "0");
                    firstVideoSubItem0.AddProperty("file", videoPath);
                    firstVideoItem.AddSubItem(firstVideoSubItem0);
                    Exo.Item.SubItem firstVideoSubItem1 = new Exo.Item.SubItem(0, 1, "Standard drawing");
                    firstVideoSubItem1.AddProperty("X", "0.0");
                    firstVideoSubItem1.AddProperty("Y", "0.0");
                    firstVideoSubItem1.AddProperty("Z", "0.0");
                    firstVideoSubItem1.AddProperty("Zoom%", "100.00");
                    firstVideoSubItem1.AddProperty("Clearness", "0.0");
                    firstVideoSubItem1.AddProperty("Rotation", "0.00");
                    firstVideoSubItem1.AddProperty("blend", "0");
                    firstVideoItem.AddSubItem(firstVideoSubItem1);
                    exo.AddItem(firstVideoItem);
                }
                // Add Audio Item
                if (info.GetHasAudio())
                {
                    Exo.Item audioItem = new Exo.Item((ulong)exo.Items.LongCount(), 1, info.GetFrameCount(), 2, 1, 1, 0, true, false);
                    Exo.Item.SubItem audioSubItem0 = new Exo.Item.SubItem(audioItem.index, 0, "Audio file");
                    audioSubItem0.AddProperty("Playback position", "0.00");
                    audioSubItem0.AddProperty("vPlay", "100.0");
                    audioSubItem0.AddProperty("Loop playback", "0");
                    audioSubItem0.AddProperty("Sync with video files", "1");
                    audioSubItem0.AddProperty("file", videoPath);
                    audioItem.AddSubItem(audioSubItem0);
                    Exo.Item.SubItem audioSubItem1 = new Exo.Item.SubItem(audioItem.index, 1, "Standard playback");
                    audioSubItem1.AddProperty("Volume", "100.0");
                    audioSubItem1.AddProperty("Left-Right", "0.0");
                    audioItem.AddSubItem(audioSubItem1);
                    exo.AddItem(audioItem);
                }
            }
            info.Close();
            return exo;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (convertThread != null)
                convertThread.Abort();
        }
    }

    
}
