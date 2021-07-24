﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmniMIDIConfigurator
{
    public partial class OmniMapperCpl : Form
    {
        struct MIDIDevices
        {
            public String PName;
            public Boolean i386;
            public Boolean AMD64;
            public Boolean ARM64;
        }

        private const String CurDrvLab = "Current device: {0} (Platforms: {1})";
        private static RegistryKey ActiveMovieKey = null;
        private static RegistryKey MIDIMapperKey = null;
        private List<MIDIDevices> MDevs = new List<MIDIDevices>();

        private String[] ReturnMIDIDevices(String Mode)
        {
            IntPtr Dummy = new IntPtr();
            String Ex;
            String Output = "";

            if (Environment.Is64BitOperatingSystem)
                Functions.Wow64DisableWow64FsRedirection(ref Dummy);

            switch (Mode)
            {
                default:
                case "i386":
                    List<string> SDevs = new List<string>();
                    MIDIOUTCAPS Caps;
                    int Devs = WinMM.midiOutGetNumDevs();

                    for (int i = 0; i < Devs; i++)
                    {
                        if (WinMM.midiOutGetDevCaps((uint)i, out Caps, (uint)Marshal.SizeOf(typeof(MIDIOUTCAPS))) == 0) {
                            SDevs.Add(Caps.szPname);
                        }
                    }

                    if (Environment.Is64BitOperatingSystem)
                        Functions.Wow64RevertWow64FsRedirection(Dummy);

                    return SDevs.ToArray();
                case "AMD64":
                    if (!Environment.Is64BitOperatingSystem)
                        return new String[0];

                    Ex = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\OmniMIDI\\OmniMIDIDriverRegister.exe";
                    break;
                case "ARM64":
                    if (Functions.GetProcessorArchitecture() != "ARM64")
                        return new String[0];

                    Ex = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\OmniMIDI\\OmniMIDIDriverRegister.exe";
                    break;
            }

            using (Process process = new Process())
            {
                process.StartInfo.FileName = Ex;
                process.StartInfo.Arguments = "/enum";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                Output = process.StandardOutput.ReadToEnd();
            }

            if (Environment.Is64BitOperatingSystem)
                Functions.Wow64RevertWow64FsRedirection(Dummy);

            return Output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public OmniMapperCpl()
        {
            InitializeComponent();
        }

        private void OmniMapperCpl_Load(object sender, EventArgs e)
        {
            try
            {
                ActiveMovieKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\ActiveMovie\devenum\{4EFE2452-168A-11D1-BC76-00C04FB9453B}\Default MidiOut Device", true);
                MIDIMapperKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Multimedia\MIDIMap", true);

                Boolean IsARM64 = Functions.GetProcessorArchitecture() == "ARM64";
                String[] i386 = ReturnMIDIDevices("i386");
                String[] AMD64 = ReturnMIDIDevices("AMD64");
                String[] ARM64 = ReturnMIDIDevices("ARM64");

                PAMD64.Visible = Environment.Is64BitOperatingSystem;
                label3.Visible = Environment.Is64BitOperatingSystem;

                PAarch64.Visible = IsARM64;
                label4.Visible = IsARM64;

                foreach (String D in i386)
                {
                    var L = MDevs.FindIndex(x => x.PName == D);

                    if (L == -1)
                        MDevs.Add(new MIDIDevices() { PName = D, i386 = true, AMD64 = false, ARM64 = false });
                    else
                        MDevs[L] = new MIDIDevices() { PName = D, i386 = true, AMD64 = MDevs[L].AMD64, ARM64 = MDevs[L].ARM64 };
                }

                foreach (String D in AMD64)
                {
                    var L = MDevs.FindIndex(x => x.PName == D);

                    if (L == -1)
                        MDevs.Add(new MIDIDevices() { PName = D, i386 = false, AMD64 = true, ARM64 = false });
                    else
                        MDevs[L] = new MIDIDevices() { PName = D, i386 = MDevs[L].i386, AMD64 = true, ARM64 = MDevs[L].ARM64 };
                }

                foreach (String D in ARM64)
                {
                    var L = MDevs.FindIndex(x => x.PName == D);

                    if (L == -1)
                        MDevs.Add(new MIDIDevices() { PName = D, i386 = false, AMD64 = false, ARM64 = true });
                    else
                        MDevs[L] = new MIDIDevices() { PName = D, i386 = MDevs[L].i386, AMD64 = MDevs[L].AMD64, ARM64 = true };
                }

                foreach (MIDIDevices DV in MDevs)
                    MIDIOutList.Items.Add(DV.PName);

                if (Functions.CheckMIDIMapper())
                {
                    bool Found = false;
                    String SelDevice = Program.Mapper.GetValue("TrgtSynth", "Microsoft GS Wavetable Synth").ToString();
                    for (int i = 0; i < MIDIOutList.Items.Count; i++)
                    {
                        if (MIDIOutList.Items[i].ToString().Equals(SelDevice))
                        {
                            MIDIOutList.SelectedIndex = i;
                            Found = true;
                            break;
                        }
                    }

                    if (!Found) MIDIOutList.SelectedIndex = 0;
                }
                else
                {
                    Text = String.Format("Change {0} settings", Functions.IsWindows8OrLater() ? "Windows Media Player MIDI output" : "MIDI mapper");
                    if (ActiveMovieKey != null) MIDIOutList.SelectedIndex = Convert.ToInt32(ActiveMovieKey.GetValue("MidiOutId"));
                    else MIDIOutList.SelectedIndex = MIDIOutList.FindStringExact(MIDIMapperKey.GetValue("szPname", "Microsoft GS Wavetable Synth").ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetNormalMIDIMapperToo()
        {
            try
            {
                if (ActiveMovieKey != null)
                {
                    ActiveMovieKey.SetValue("MidiOutId", MIDIOutList.SelectedIndex, RegistryValueKind.DWord);
                    ActiveMovieKey.Close();
                }

                if (MIDIMapperKey != null)
                {
                    MIDIMapperKey.SetValue("szPname", MIDIOutList.SelectedItem.ToString(), RegistryValueKind.String);
                    MIDIMapperKey.Close();
                }
            }
            catch (Exception ex)
            {
                Program.ShowError(4, "Error", "An error has occured while setting the default MIDI out device output.", ex);
            }
        }

        private void MIDIOutList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var L = MDevs.FindIndex(x => x.PName == MIDIOutList.Items[MIDIOutList.SelectedIndex].ToString());

            if (L != -1)
            {
                Pi386.Image = MDevs[L].i386 ? Properties.Resources.ok : Properties.Resources.error;
                PAMD64.Image = MDevs[L].AMD64 ? Properties.Resources.ok : Properties.Resources.error;
                PAarch64.Image = MDevs[L].ARM64 ? Properties.Resources.ok : Properties.Resources.error;
            }
        }

        private void ApplyBtn_Click(object sender, EventArgs e)
        {
            var L = MDevs.FindIndex(x => x.PName == MIDIOutList.Items[MIDIOutList.SelectedIndex].ToString());

            if (Environment.Is64BitOperatingSystem && (!MDevs[L].i386 && !(MDevs[L].AMD64 || MDevs[L].ARM64)))
            {
                DialogResult Message = Program.ShowError(3, "Warning", "The selected driver isn't available for both 32-bit and 64-bit platforms.\n\nAre you sure you want to set it as the default MIDI Mapper device?", null);
                if (Message == DialogResult.No)
                    return;
            }

            Program.Mapper.SetValue("TrgtSynth", MIDIOutList.Items[MIDIOutList.SelectedIndex].ToString(), RegistryValueKind.String);
            SetNormalMIDIMapperToo();
            Close();
        }
    }
}
