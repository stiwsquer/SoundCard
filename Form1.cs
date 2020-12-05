using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;



namespace SoundCard
{
    public partial class Form1 : Form
    {
        string fileName = null;
        SoundPlayer soundPlayer = null;
        WMPLib.WindowsMediaPlayer wplayer;

        struct WavHeader
        {
            public byte[] riffID;
            public uint size;
            public byte[] wavID;
            public byte[] fmtID;
            public uint fmtSize;
            public ushort format;
            public ushort channels;
            public uint sampleRate;
            public uint bytePerSec;
            public ushort blockSize;
            public ushort bit;
            public byte[] dataID;
            public uint dataSize;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void chooseFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fileName = ofd.FileName;
                soundPlayer = null;
                wplayer = null;
            }
            else
            {
                MessageBox.Show("Nie udało się otworzyć pliku!");
            }
        }

        private void playSoundButton_Click(object sender, EventArgs e)
        {
            if (fileName != null && fileName.EndsWith(".wav"))
            {
                soundPlayer = new SoundPlayer(fileName);
                soundPlayer.Play();
            }
            else
            {
                MessageBox.Show("Nie wybrano pliku lub wybrany plik jest nieprawidłowy");
            }
        }

        private void stopSoundButton_Click(object sender, EventArgs e)
        {
            if (soundPlayer != null) soundPlayer.Stop();
            else if (wplayer != null) wplayer.controls.stop();
        }

        private void wavHeaderButton_Click(object sender, EventArgs e)
        {


            if (fileName != null && fileName.EndsWith(".wav"))
            {
                WavHeader Header = new WavHeader();
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);

                Header.riffID = br.ReadBytes(4);
                Header.size = br.ReadUInt32();
                Header.wavID = br.ReadBytes(4);
                Header.fmtID = br.ReadBytes(4);
                Header.fmtSize = br.ReadUInt32();
                Header.format = br.ReadUInt16();
                Header.channels = br.ReadUInt16();
                Header.sampleRate = br.ReadUInt32();
                Header.bytePerSec = br.ReadUInt32();
                Header.blockSize = br.ReadUInt16();
                Header.bit = br.ReadUInt16();
                Header.dataID = br.ReadBytes(4);
                Header.dataSize = br.ReadUInt32();

                messageBox.Text = " ";
                string text;
                int index = fileName.LastIndexOf("\\");
                messageBox.Text += "_____" + fileName.Substring(index + 1) + "_____ \n";
                text = System.Text.Encoding.UTF8.GetString(Header.riffID);
                messageBox.Text += "RiffID: " + text + "\n";
                messageBox.Text += "Size: " + Header.size + "\n";
                text = System.Text.Encoding.UTF8.GetString(Header.wavID);
                messageBox.Text += "WavID: " + text + "\n";
                text = System.Text.Encoding.UTF8.GetString(Header.fmtID);
                messageBox.Text += "FtmID: " + text + "\n";
                messageBox.Text += "FtmSize: " + Header.fmtSize + "\n";
                messageBox.Text += "Format: " + Header.format + "\n";
                messageBox.Text += "Channels: " + Header.channels + "\n";
                messageBox.Text += "SampleRate: " + Header.sampleRate + "\n";
                messageBox.Text += "BytePerSec: " + Header.bytePerSec + "\n";
                messageBox.Text += "BlockSize: " + Header.blockSize + "\n";
                messageBox.Text += "Bit: " + Header.bit + "\n";
                text = System.Text.Encoding.UTF8.GetString(Header.dataID);
                messageBox.Text += "DataID: " + text + "\n";
                messageBox.Text += "DataSize: " + Header.dataSize + "\n";
            }
            else
            {
                MessageBox.Show("Nie wybrano pliku lub wybrany plik jest nieprawidłowy");
            }
        }

        NAudio.Wave.WaveIn sourceStream = null;
        
        NAudio.Wave.WaveFileWriter waveWriter = null;


        private void playMp3Button_Click(object sender, EventArgs e)
        {
            if (fileName != null && fileName.EndsWith(".mp3"))
            {
                wplayer = new WMPLib.WindowsMediaPlayer();
                wplayer.URL = fileName;
                wplayer.controls.play();

                messageBox.Text = " ";
                string text;
                int index = fileName.LastIndexOf("\\");
                messageBox.Text += "_____" + fileName.Substring(index + 1) + "_____ \n";

            }
            else
            {
                MessageBox.Show("Nie wybrano pliku lub wybrany plik jest nieprawidłowy");
            }
        }

        private void recordBatton_Click(object sender, EventArgs e)
        {
            if (deviceListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Nie wybrano żadnego urządzenia");
            }
            else
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Wave File (*.wav)|*.wav;";
                if (save.ShowDialog() != DialogResult.OK) return;

                messageBox.Text = " ";
                messageBox.Text += "Recording...\n";
                int deviceNumber = deviceListBox.SelectedIndex;

                sourceStream = new NAudio.Wave.WaveIn();
                sourceStream.DeviceNumber = deviceNumber;
                sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);

                sourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);
                waveWriter = new NAudio.Wave.WaveFileWriter(save.FileName, sourceStream.WaveFormat);

                sourceStream.StartRecording();
            }


        }
        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (waveWriter == null) return;
            waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        private void stopRecordingButton_Click(object sender, EventArgs e)
        {

            if (waveWriter != null)
            {
                
                messageBox.Text += "Stop recording";
                waveWriter.Dispose();
                waveWriter = null;
            }
        }

        private void showRecordDevicesButton_Click(object sender, EventArgs e)
        {
            List<NAudio.Wave.WaveInCapabilities> sources = new List<NAudio.Wave.WaveInCapabilities>();

            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                sources.Add(NAudio.Wave.WaveIn.GetCapabilities(i));
            }

            deviceListBox.Items.Clear();

            foreach (var source in sources)
            {
                ListViewItem item = new ListViewItem(source.ProductName);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, source.Channels.ToString()));
                deviceListBox.Items.Add(item);
            }
        }

        private void showWaveButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Wave File (*.wav)|*.wav;";
            if (open.ShowDialog() != DialogResult.OK) return;

            waveViewer.SamplesPerPixel = 500;
            waveViewer.WaveStream = new NAudio.Wave.WaveFileReader("utwor3.wav");
        }

      
    }
}
