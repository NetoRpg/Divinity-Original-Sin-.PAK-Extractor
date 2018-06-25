using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LZ4;
using zlib;
using LSLib.Native;
using LSLib.Granny;
using System.Drawing;

namespace PAK_Unpacker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Package file|*.pak";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPackagePath.Text = ofd.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtExtractionPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtPackagePath.Text))
            {
                MessageBox.Show("Invalid file");
                return;
            }

            if (!Directory.Exists(txtExtractionPath.Text))
            {
                MessageBox.Show("Invalid extraction path");
                return;
            }

            richTextBox1.Clear();
            using (FileStream fs = new FileStream(txtPackagePath.Text, FileMode.Open))
            {
                fs.Position = fs.Length - 8;
                fs.Position = fs.Length - fs.ExtractInt32LE();
                fs.ExtractInt32LE(); //-- Versão
                int FileListOffset = fs.ExtractInt32LE();
                int FileListSize = fs.ExtractInt32LE();
                fs.Position = FileListOffset;
                int NumberOfFiles = fs.ExtractInt32LE();
                int FileUncompressedSize = NumberOfFiles * 280;

                byte[] files = new byte[FileUncompressedSize];
                LZ4Codec.Decode(fs.ExtractPiece(FileListSize - 4), 0, FileListSize - 4, files, 0, FileUncompressedSize, false);

                for (int i = 0; i < NumberOfFiles; i++)
                {
                    string filename = string.Empty;
                    for (int j = 0; j < 0x100; j++)
                    {
                        if (files[(i * 280) + j] != 0x00)
                            filename += (char)files[(i * 280) + j];
                        else
                            break;
                    }
                    filename = Path.Combine(txtExtractionPath.Text, filename);
                    string Dir = Path.GetDirectoryName(filename);
                    if (!Directory.Exists(Dir))
                    {
                        Directory.CreateDirectory(Dir);
                    }

                    fs.Position = files.ExtractUInt32LE((i * 280) + 0x100);
                    int CompSize = files.ExtractInt32LE((i * 280) + 0x104);
                    int UncpSize = files.ExtractInt32LE((i * 280) + 0x108);
                    uint Flags = files.ExtractUInt32LE((i * 280) + 0x110);
                    uint Crc = files.ExtractUInt32LE((i * 280) + 0x114);
                    byte[] UncompressedFile = new byte[UncpSize];
                    byte[] CompressedFile = fs.ExtractPiece(CompSize);
                    if (Crc == 0 || Crc == Crc32.Compute(CompressedFile))
                    {
                        UncompressedFile = this.Decompress(CompressedFile, UncpSize, (byte)Flags, false);
                        richTextBox1.SelectionColor = Color.Green;
                        richTextBox1.AppendText("    OK - ");
                    }
                    else
                    {
                        richTextBox1.SelectionColor = Color.Red;
                        richTextBox1.AppendText("    CORRUPTED - ");
                    }
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText(filename + "\r\n");
                    using (FileStream uFile = new FileStream(filename, FileMode.OpenOrCreate))
                    {
                        uFile.Write(UncompressedFile, 0, UncompressedFile.Length);
                    }
                }
            }
        }

        public byte[] Decompress(byte[] compressed, int decompressedSize, byte compressionFlags, bool chunked = false)
        {
            switch (compressionFlags & 15)
            {
                case 0:
                    return compressed;
                case 1:
                    using (MemoryStream memoryStream = new MemoryStream(compressed))
                    {
                        using (MemoryStream memoryStream2 = new MemoryStream())
                        {
                            using (ZInputStream zInputStream = new ZInputStream(memoryStream))
                            {
                                byte[] array = new byte[65536];
                                int count;
                                while ((count = zInputStream.read(array, 0, array.Length)) > 0)
                                {
                                    memoryStream2.Write(array, 0, count);
                                }
                                return memoryStream2.ToArray();
                            }
                        }
                    }
                    break;
                case 2:
                    break;
                default:
                    throw new InvalidDataException(string.Format("No decompressor found for this format: {0}", compressionFlags));
            }
            if (chunked)
            {
                return LZ4FrameCompressor.Decompress(compressed);
            }
            byte[] array2 = new byte[decompressedSize];
            LZ4Codec.Decode(compressed, 0, compressed.Length, array2, 0, decompressedSize, false);
            return array2;
        }


    }
}
