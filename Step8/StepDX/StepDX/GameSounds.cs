using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace StepDX
{
    class GameSounds
    {
        private Device SoundDevice = null;

        private SecondaryBuffer [] clank = new SecondaryBuffer[10];
        int clankToUse = 0;

        private SecondaryBuffer coin = null;
        private SecondaryBuffer jump = null;

        public GameSounds(Form form)
        {
            SoundDevice = new Device();
            SoundDevice.SetCooperativeLevel(form, CooperativeLevel.Priority);

            Load(ref coin, "../../../CollectCoin.wav");
            Load(ref jump, "../../../Jump.wav");
         
        }

        private void Load(ref SecondaryBuffer buffer, string filename)
        {
            try
            {
                buffer = new SecondaryBuffer(filename, SoundDevice);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to load " + filename, 
                                "Danger, Will Robinson", MessageBoxButtons.OK);
                buffer = null;
            }
        }

        public void Coin()
        {
            if (coin == null)
            {
                return;
            }

            if (!coin.Status.Playing)
            {
                coin.Play(0, BufferPlayFlags.Default);
            }
        }

        public void Jump()
        {
            if (jump == null)
            {
                return;
            }

            if (!jump.Status.Playing)
            {
                jump.Play(0, BufferPlayFlags.Default);
            }
        }

        public void Clank()
        {
            clankToUse = (clankToUse + 1) % clank.Length;

            if (clank[clankToUse] == null)
                return;

            if (!clank[clankToUse].Status.Playing)
                clank[clankToUse].Play(0, BufferPlayFlags.Default);
        }

    }
}
