using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WavProducer
{
    public partial class frmMain : Form
    {
        clsWavStream mobjWavStream = new clsWavStream();
        Graphics mobjFormGraphics;
        int[] marrSamples;
        int mintX1;
        int mintY1;
        int mintX2;
        int mintY2;
        int mintLastX = 0;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnGenerateFile_Click(object sender, EventArgs e)
        {
            btnGenerateFile.Enabled = false;

            // add elements to array to fill from the last mouse entered point to the right side of the rectangle
            for (int intXCounter = mintLastX + 1; intXCounter <= 600; intXCounter++)
            {
                marrSamples[intXCounter - mintX1] = marrSamples[intXCounter - mintX1 - 1];
                mobjFormGraphics.DrawLine(Pens.Black, intXCounter - 1, ((mintY2 + mintY1) / 2 - marrSamples[intXCounter - mintX1] / 324) - 1, intXCounter, (mintY2 + mintY1) / 2 - marrSamples[intXCounter - mintX1] / 324);
            }
            for (int intSampleCount = 1; intSampleCount <= 100000; intSampleCount++)
            {
                int intIndex;
                intIndex = Convert.ToInt32(((double)intSampleCount * Convert.ToDouble(txtFrequency.Text) * 600) / 22050) % 600;

                mobjWavStream.AddData(marrSamples[intIndex], 1);
                mobjWavStream.AddData(marrSamples[intIndex], 2);
            }

            
            
            // this part is for filling the file with data generated using a function
            //for (int intSampleCount = 1; intSampleCount <= 22050; intSampleCount++)
            //{
            //    double dblStrength;
            //    dblStrength = Math.Sin(((double)intSampleCount) / 15) * 3;

            //    //dblStrength = Math.Sin(((double)intSampleCount) / 5) * 2;
            //    //dblStrength *= Math.Sin(((double)intSampleCount) / 5);
            //    //dblStrength *= Math.Sin(((double)intSampleCount) / 2);
            //    //dblStrength *= Math.Sin(((double)intSampleCount));
            //    //dblStrength += Math.Sin(((double)intSampleCount) / 20);
            //    //dblStrength += Math.Sin(((double)intSampleCount) / 30);
            //    //dblStrength *= Math.Sin(((double)intSampleCount) / 1000);

            //    mobjWavStream.AddData((Int16)(10000 * dblStrength), 1);
            //    mobjWavStream.AddData((Int16)(10000 * dblStrength), 2);
            //}


            //// this part is for filling the file with alarm clock sound, low to high sound
            //for (int intSampleCount = 1; intSampleCount <= 661500; intSampleCount++)
            //{
            //    double dblStrength;
            //    dblStrength = Math.Sin(((double)intSampleCount) / 15);  

            //    if (intSampleCount <= 22050)
            //    {
            //        dblStrength = dblStrength * 3000 * (1 - (Math.Pow(((double)intSampleCount),2) / Math.Pow(22050,2)));   // strenght diminishing in a second from 3000 to 0
            //    }
            //    else
            //    {
            //        dblStrength = dblStrength * 30000 * (Math.Pow(((double)intSampleCount - (double)22050),2) / Math.Pow(639450,2));   // strenght increasing in the rest of the 30 seconds from 0 till 30000
            //    }

            //    mobjWavStream.AddData((Int16)(dblStrength), 1);
            //    mobjWavStream.AddData((Int16)(dblStrength), 2);
            //}

            
            mobjWavStream.Save(Path.GetDirectoryName(Application.ExecutablePath) + "\\Product.wav");
            btnGenerateFile.Enabled = true;
        }

        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if ((e.X - mintX1 >= 0) & (e.X - mintX1 < 600) & (e.Y  >= 0) & (e.Y  <= 200))
                {
                    if ((mintLastX == 0))
                    {
                        mintLastX = mintX1 - 1;
                    }
                    for (int intXCounter = mintLastX + 1; intXCounter <= e.X; intXCounter++)
                    {
                        marrSamples[intXCounter - mintX1] = (((mintY2 + mintY1)/2)  - e.Y) * 324;
                        mobjFormGraphics.DrawLine(Pens.Black, intXCounter - 1, e.Y - 1, intXCounter, e.Y);
                        mintLastX = e.X;
                    }                    
                }                
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            marrSamples = new int[600];
            mobjFormGraphics = this.CreateGraphics();
            mintX1 = 1;
            mintY1 = 1;
            mintX2 = 600;
            mintY2 = 200;

        }

        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            mobjFormGraphics.DrawRectangle(Pens.Black, mintX1, mintY1, mintX2, mintY2);
            mobjFormGraphics.DrawLine(Pens.LightGray, mintX1, (mintY1 + mintY2) / 2, mintX2, (mintY1 + mintY2) / 2);
        }
    }

    public class clsWavStream
    {
        byte[] mbytFileContents;
        int mintArrayLength = 0;  // I have to save the actual length since i allocate 500,000 bytes from the start
        int mintChannel1Length = 0;
        int mintChannel2Length = 0;

        public clsWavStream()
        {
            mbytFileContents = new byte[3000000];
            AddString("RIFF");
            AddInt32(36); // chunksize for a file with 0 data
            AddString("WAVE");
            AddString("fmt ");
            AddInt32(16);  // subchunk1size
            AddInt16(1);   // 1 for PCM
            AddInt16(2);   // 2 channels (Stereo) 
            AddInt32(22050);  // sample rate
            AddInt32(88200);  // byte rate
            AddInt16(4);      // blockalign (number of bytes for one sample including all channels)
            AddInt16(16);     // bitspersample (for each channel)
            AddString("data");
            AddInt32(0);    // data size (subchunk2size)
        }

        private void AddString(string pstrText)
        {
            string strChar;
            for (int intCharIndex = 0; intCharIndex < pstrText.Length; intCharIndex++)
            {
                strChar = pstrText.Substring(intCharIndex, 1);
                mbytFileContents[mintArrayLength] = Encoding.Default.GetBytes(strChar)[0];
                mintArrayLength++;
            }
        }

        private void AddInt32(int pintNumber)
        {
            mbytFileContents [mintArrayLength] = (byte)pintNumber ;
            mintArrayLength++;
            mbytFileContents[mintArrayLength] = (byte)(pintNumber >> 8);
            mintArrayLength++;
            mbytFileContents[mintArrayLength] = (byte)(pintNumber >> 16);
            mintArrayLength++;
            mbytFileContents[mintArrayLength] = (byte)(pintNumber >> 24);
            mintArrayLength ++;
        }

        private void UpdateSizeFields()
        {
            int intChunkSize;
            int intChunk2Size;

            intChunk2Size = mintArrayLength - 44;
            intChunkSize = 36 + intChunk2Size;
            mbytFileContents[4] = (byte)intChunkSize;
            mbytFileContents[5] = (byte)(intChunkSize >> 8);
            mbytFileContents[6] = (byte)(intChunkSize >> 16);
            mbytFileContents[7] = (byte)(intChunkSize >> 24);

            mbytFileContents[40] = (byte)intChunk2Size;
            mbytFileContents[41] = (byte)(intChunk2Size >> 8);
            mbytFileContents[42] = (byte)(intChunk2Size >> 16);
            mbytFileContents[43] = (byte)(intChunk2Size >> 24);
        }

        private void AddInt16(Int16 pintNumber)
        {
            mbytFileContents[mintArrayLength] = (byte)pintNumber;
            mintArrayLength++;
            mbytFileContents[mintArrayLength] = (byte)(pintNumber >> 8);
            mintArrayLength++;
        }

        public void AddData(int pintData, int pintChannel)
        // adds one integer data to channel 1 or 2
        {
            int intIndex;

            if (pintChannel == 1)
            {
                intIndex = 44 + (mintChannel1Length * 4);
                mintChannel1Length++;
                if (44 + (mintChannel1Length * 4) > mintArrayLength)
                    mintArrayLength = 44 + (mintChannel1Length * 4); 
            }
            else
            {
                intIndex = 44 + (mintChannel2Length * 4) + 2;
                mintChannel2Length++;
                if (44 + (mintChannel2Length * 4) > mintArrayLength)
                    mintArrayLength = 44 + (mintChannel2Length * 4); 
            }

            mbytFileContents[intIndex] = (byte)pintData;
            intIndex++;
            mbytFileContents[intIndex] = (byte)(pintData >> 8);
            UpdateSizeFields();
        }

        public void Save(string pstrFileName)
        {
            FileStream objFileStream = new FileStream(pstrFileName, FileMode.Create, FileAccess.Write);
            objFileStream.Write(mbytFileContents , 0, mintArrayLength );
            objFileStream.Flush();
            objFileStream.Close();
       }
    }
}
