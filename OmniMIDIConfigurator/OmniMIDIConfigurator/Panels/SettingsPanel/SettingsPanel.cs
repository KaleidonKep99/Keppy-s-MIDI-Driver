﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace OmniMIDIConfigurator
{
    public partial class SettingsPanel : UserControl
    {    
        public static SettingsPanel Delegate;

        public SettingsPanel()
        {
            InitializeComponent();

            Delegate = this;

            KSDAPIBoxWhat.Image = Properties.Resources.what;
            OverrideNoteLengthWA1.Image = Properties.Resources.wi;
            OverrideNoteLengthWA2.Image = Properties.Resources.wi;

            VolTrackBar.ContextMenu = VolTrackBarMenu;

            if (!(Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build >= 15063))
            {
                SpatialSound.Enabled = false;
                SpatialSound.Text += " (Not available on your version of Windows)";
            }

            this.MouseWheel += new MouseEventHandler(SettingsPanel_MouseWheel);

            LoadSettings();
        }

        private const int WM_VSCROLL = 0x0115;
        private const int WM_HSCROLL = 0x0114;

        private void SettingsPanel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Double buffering is useless, refresh it by yourself lol

        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= Properties.Settings.Default.DrawControlsFaster ? 0 : 0x02000000;
                cp.Style &= ~0x02000000;
                return cp;
            }
        }

        private void CheckSincEnabled()
        {
            SincConvLab.Enabled = SincInter.Checked;
            SincConv.Enabled = SincInter.Checked;
        }

        private int PreviousValue = 2;
        private void AudioEngBoxTrigger(bool S)
        {
            int AE = AudioEngBox.SelectedIndex;
            bool NOAtWASIOXAE = (AE != AudioEngine.AUDTOWAV && AE != AudioEngine.ASIO_ENGINE && AE != AudioEngine.XA_ENGINE);
            bool NoASIOXAE = (AE != AudioEngine.ASIO_ENGINE && AE != AudioEngine.XA_ENGINE);
            bool NoASIOWAXAE = (AE != AudioEngine.ASIO_ENGINE && AE != AudioEngine.WASAPI_ENGINE && AE != AudioEngine.XA_ENGINE);
            bool NoAtW = (AE != AudioEngine.AUDTOWAV);
            bool NoASIOE = (AE != AudioEngine.ASIO_ENGINE);
            bool NoXAE = (AE != AudioEngine.XA_ENGINE);

            switch (AudioEngBox.SelectedIndex)
            {
                case AudioEngine.XA_ENGINE:
                    ChangeDefaultOutput.Text = "Advanced...";
                    BufferText.Text = "The output buffer can be controlled through the engine's advanced settings";
                    break;
                case AudioEngine.AUDTOWAV:
                    ChangeDefaultOutput.Text = "Directory...";
                    BufferText.Text = "The output buffer isn't needed when outputting to a .WAV file";
                    break;
                case AudioEngine.ASIO_ENGINE:
                    ChangeDefaultOutput.Text = "Devices...";
                    if (DefaultASIOAudioOutput.GetASIODevicesCount() < 1 && !S)
                    {
                        DialogResult RES = 
                            Program.ShowError(
                                3,
                                "Error",
                                "You selected ASIO, but no ASIO devices are installed on your computer.\n" +
                                "Running any MIDI app with this configuration might lead to an error on startup.\n\n" +
                                "Are you sure you want to continue?\nPress Yes to keep this configuration, or No to switch back to the previous engine.",
                                null);

                        if (RES == DialogResult.No)
                        {
                            AudioEngBox.SelectedIndex = PreviousValue;
                            AudioEngBoxTrigger(true);
                        }

                        return;
                    }

                    BufferText.Text = "The output buffer is controlled by the ASIO device itself";
                    break;
                case AudioEngine.WASAPI_ENGINE:
                case AudioEngine.DSOUND_ENGINE:
                default:
                    ChangeDefaultOutput.Text = "Output...";
                    BufferText.Text = "Output buffer (in ms, from 1 to 1000. If the buffer is too small, it'll be set automatically to the lowest value possible)";
                    break;
            }

            AudioBitDepthLabel.Enabled = NoASIOWAXAE ? true : false;
            AudioBitDepth.Enabled = NoASIOWAXAE ? true : false;
            BufferText.Enabled = NOAtWASIOXAE ? true : false;
            DrvHzLabel.Enabled = true;
            Frequency.Enabled = true;
            MaxCPU.Enabled = NoAtW ? true : false;
            RenderingTimeLabel.Enabled = NoAtW ? true : false;
            VolLabel.Enabled = NoAtW ? true : false;
            VolSimView.Enabled = NoAtW ? true : false;
            VolTrackBar.Enabled = NoAtW ? true : false;
            bufsize.Enabled = NoASIOXAE ? true : false;
            OldBuff.Enabled = NoAtW ? true : false;

            PreviousValue = AudioEngBox.SelectedIndex;
            if (S) Program.SynthSettings.SetValue("CurrentEngine", AudioEngBox.SelectedIndex, RegistryValueKind.DWord);
        }

        private void LoadSettings()
        {
            try
            {
                AudioBitDepth.SelectedIndex = Convert.ToInt32(Program.SynthSettings.GetValue("AudioBitDepth", 0));
                Frequency.Text = Program.SynthSettings.GetValue("AudioFrequency", 44100).ToString();
                PolyphonyLimit.Value = Convert.ToInt32(Program.SynthSettings.GetValue("MaxVoices", 512));
                MaxCPU.Value = Convert.ToInt32(Program.SynthSettings.GetValue("MaxRenderingTime", 75));
                FastHotKeys.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("FastHotkeys", 0));
                DebugMode.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("DebugMode", 0));

                try { PrioBox.SelectedIndex = Convert.ToInt32(Program.SynthSettings.GetValue("DriverPriority", 0)); }
                catch { PrioBox.SelectedIndex = 0; }

                LiveChangesTrigger.Checked = Properties.Settings.Default.LiveChanges;

                VolumeBoost.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("VolumeBoost", 0));
                VolTrackBar.Maximum = VolumeBoost.Checked ? 50000 : 10000;
                VolTrackBar.Value = Convert.ToInt32(Program.SynthSettings.GetValue("OutputVolume", 10000));

                AutoLoad.Checked = Properties.Settings.Default.AutoLoadList;

                Preload.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("PreloadSoundfonts", 1));
                EnableSFX.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("EnableSFX", 1));
                NoteOffCheck.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("NoteOff1", 0));
                SysResetIgnore.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("IgnoreSysReset", 0));
                bufsize.Value = Convert.ToInt32(Program.SynthSettings.GetValue("BufferLength", 30));

                SincInter.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("SincInter", 0));
                CheckSincEnabled();
                try { SincConv.SelectedIndex = Convert.ToInt32(Program.SynthSettings.GetValue("SincConv", 0)); }
                catch { SincConv.SelectedIndex = 2; }

                FadeoutDisable.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("DisableNotesFadeOut", 0));
                MonophonicFunc.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("MonoRendering", 0));
                SlowDownPlayback.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("DontMissNotes", 0));
                KSDAPIBox.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("KDMAPIEnabled", 1));
                HMode.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("HyperPlayback", 0));
                OldBuff.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("NotesCatcherWithAudio", 0));

                ReverbV.Value = Functions.Between0And127(Convert.ToInt32(Program.SynthSettings.GetValue("Reverb", 64)));
                ChorusV.Value = Functions.Between0And127(Convert.ToInt32(Program.SynthSettings.GetValue("Chorus", 64)));
                EnableRCOverride.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("RCOverride", 0));
                EnableRCOverride_CheckedChanged(null, null);

                DisableCookedPlayer.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("DisableCookedPlayer", 0));
                AllNotesIgnore.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("IgnoreAllNotes", 0));
                IgnoreNotes.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("IgnoreNotesBetweenVel", 0));
                AudioRampIn.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("AudioRampIn", 1));
                LinAttMod.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("LinAttMod", 0));
                LinDecVol.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("LinDecVol", 0));

                Int32 LV = Convert.ToInt32(Program.SynthSettings.GetValue("MinVelIgnore", 1));
                Int32 HV = Convert.ToInt32(Program.SynthSettings.GetValue("MaxVelIgnore", 2));
                if (LV > HV) LV = HV;
                if (LV < IgnoreNotesLV.Minimum | LV > IgnoreNotesLV.Maximum) LV = (int)IgnoreNotesLV.Minimum;
                if (HV < IgnoreNotesHV.Minimum | HV > IgnoreNotesHV.Maximum) HV = (int)IgnoreNotesHV.Minimum;
                IgnoreNotesLV.Value = LV;
                IgnoreNotesHV.Value = HV;

                CapFram.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("CapFramerate", 1));
                Limit88.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("LimitTo88Keys", 0));
                FullVelocityMode.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("FullVelocityMode", 0));
                OverrideNoteLength.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("OverrideNoteLength", 0));
                NoteLengthValue.Value = Convert.ToDecimal((double)Convert.ToInt32(Program.SynthSettings.GetValue("NoteLengthValue", 5)) / 1000.0);
                DelayNoteOff.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("DelayNoteOff", 0));
                NoteOffDelayValue.Value = Convert.ToDecimal((double)Convert.ToInt32(Program.SynthSettings.GetValue("DelayNoteOffValue", 5)) / 1000.0);
                HMode_CheckedChanged(null, null);

                AudioEngBox.SelectedIndexChanged -= AudioEngBox_SelectedIndexChanged;
                switch (Convert.ToInt32(Program.SynthSettings.GetValue("CurrentEngine", AudioEngine.WASAPI_ENGINE)))
                {
                    case AudioEngine.AUDTOWAV:
                        AudioEngBox.SelectedIndex = AudioEngine.AUDTOWAV;
                        break;
                    case AudioEngine.DSOUND_ENGINE:
                        AudioEngBox.SelectedIndex = AudioEngine.DSOUND_ENGINE;
                        break;
                    case AudioEngine.ASIO_ENGINE:
                        AudioEngBox.SelectedIndex = AudioEngine.ASIO_ENGINE;
                        break;
                    case AudioEngine.XA_ENGINE:
                        AudioEngBox.SelectedIndex = AudioEngine.XA_ENGINE;
                        break;
                    case AudioEngine.WASAPI_ENGINE:
                    default:
                        AudioEngBox.SelectedIndex = AudioEngine.WASAPI_ENGINE;
                        break;
                }
                PreviousValue = AudioEngBox.SelectedIndex;
                AudioEngBoxTrigger(true);
                AudioEngBox.SelectedIndexChanged += AudioEngBox_SelectedIndexChanged;

                UseTGT.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("StockWinMM", 0));
                IgnoreCloseCalls.Checked = Convert.ToBoolean(Program.SynthSettings.GetValue("KeepAlive", 0));
                ShowChangelogUpdate.Checked = Properties.Settings.Default.ShowChangelogStartUp;

                Functions.LiveChanges.PreviousEngine = (int)Program.SynthSettings.GetValue("CurrentEngine", AudioEngine.WASAPI_ENGINE);
                Functions.LiveChanges.PreviousFrequency = (int)Program.SynthSettings.GetValue("AudioFrequency", 44100);
                Functions.LiveChanges.PreviousBuffer = (int)Program.SynthSettings.GetValue("BufferLength", 50);
                Functions.LiveChanges.MonophonicRender = (int)Program.SynthSettings.GetValue("MonoRendering", 0);
                Functions.LiveChanges.AudioBitDepth = (int)Program.SynthSettings.GetValue("AudioBitDepth", 0);
                Functions.LiveChanges.NotesCatcherWithAudio = (int)Program.SynthSettings.GetValue("NotesCatcherWithAudio", 0);
            }
            catch (Exception ex)
            {
                Program.ShowError(4, "FATAL ERROR", "The configurator is unable to load its settings.\n\nPress OK to quit.", ex);
                Application.ExitThread();
            }
        }

        private void SaveSettings(bool OV)
        {
            // Normal settings
            Program.SynthSettings.SetValue("AudioBitDepth", AudioBitDepth.SelectedIndex, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("AudioFrequency", Frequency.Text, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("MaxVoices", PolyphonyLimit.Value, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("MaxRenderingTime", MaxCPU.Value, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("FastHotkeys", Convert.ToInt32(FastHotKeys.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("DebugMode", Convert.ToInt32(DebugMode.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("NotesCatcherWithAudio", Convert.ToInt32(OldBuff.Checked), RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("DriverPriority", PrioBox.SelectedIndex, RegistryValueKind.DWord);
            Program.SynthSettings.GetValue("AudioBitDepth", AudioBitDepth.SelectedIndex);

            Properties.Settings.Default.LiveChanges = LiveChangesTrigger.Checked;

            Program.SynthSettings.SetValue("VolumeBoost", Convert.ToInt32(VolumeBoost.Checked), RegistryValueKind.DWord);
            Properties.Settings.Default.AutoLoadList = AutoLoad.Checked;

            Program.SynthSettings.SetValue("PreloadSoundfonts", Convert.ToInt32(Preload.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("EnableSFX", Convert.ToInt32(EnableSFX.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("NoteOff1", Convert.ToInt32(NoteOffCheck.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("IgnoreSysReset", Convert.ToInt32(SysResetIgnore.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("BufferLength", bufsize.Value, RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("SincInter", Convert.ToInt32(SincInter.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("SincConv", SincConv.SelectedIndex, RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("DisableNotesFadeOut", Convert.ToInt32(FadeoutDisable.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("MonoRendering", Convert.ToInt32(MonophonicFunc.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("DontMissNotes", Convert.ToInt32(SlowDownPlayback.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("KDMAPIEnabled", Convert.ToInt32(KSDAPIBox.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("HyperPlayback", Convert.ToInt32(HMode.Checked), RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("Reverb", ReverbV.Value, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("Chorus", ChorusV.Value, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("RCOverride", Convert.ToInt32(EnableRCOverride.Checked), RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("DisableCookedPlayer", Convert.ToInt32(DisableCookedPlayer.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("IgnoreAllNotes", Convert.ToInt32(AllNotesIgnore.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("IgnoreNotesBetweenVel", Convert.ToInt32(IgnoreNotes.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("AudioRampIn", Convert.ToInt32(AudioRampIn.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("LinAttMod", Convert.ToInt32(LinAttMod.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("LinDecVol", Convert.ToInt32(LinDecVol.Checked), RegistryValueKind.DWord);

            if (IgnoreNotesLV.Value > IgnoreNotesHV.Value) IgnoreNotesLV.Value = IgnoreNotesHV.Value;
            Program.SynthSettings.SetValue("MinVelIgnore", IgnoreNotesLV.Value, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("MaxVelIgnore", IgnoreNotesHV.Value, RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("CapFramerate", Convert.ToInt32(CapFram.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("LimitTo88Keys", Convert.ToInt32(Limit88.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("FullVelocityMode", Convert.ToInt32(FullVelocityMode.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("OverrideNoteLength", Convert.ToInt32(OverrideNoteLength.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("NoteLengthValue", Convert.ToInt32(NoteLengthValue.Value * 1000), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("DelayNoteOff", Convert.ToInt32(DelayNoteOff.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("DelayNoteOffValue", Convert.ToInt32(NoteOffDelayValue.Value * 1000), RegistryValueKind.DWord);

            Program.SynthSettings.SetValue("CurrentEngine", AudioEngBox.SelectedIndex, RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("StockWinMM", Convert.ToInt32(UseTGT.Checked), RegistryValueKind.DWord);
            Program.SynthSettings.SetValue("KeepAlive", Convert.ToInt32(IgnoreCloseCalls.Checked), RegistryValueKind.DWord);

            Properties.Settings.Default.ShowChangelogStartUp = ShowChangelogUpdate.Checked;

            if (OV)
            {
                Program.SynthSettings.SetValue("LiveChanges", "1", RegistryValueKind.DWord);
                Functions.LiveChanges.PreviousEngine = (int)Program.SynthSettings.GetValue("CurrentEngine", AudioEngine.WASAPI_ENGINE);
                Functions.LiveChanges.PreviousFrequency = (int)Program.SynthSettings.GetValue("AudioFrequency", 44100);
                Functions.LiveChanges.PreviousBuffer = (int)Program.SynthSettings.GetValue("BufferLength", 50);
                Functions.LiveChanges.MonophonicRender = (int)Program.SynthSettings.GetValue("MonoRendering", 0);
                Functions.LiveChanges.AudioBitDepth = (int)Program.SynthSettings.GetValue("AudioBitDepth", 0);
                Functions.LiveChanges.NotesCatcherWithAudio = (int)Program.SynthSettings.GetValue("NotesCatcherWithAudio", 0);
            }
            else
            {
                if (Properties.Settings.Default.LiveChanges)
                {
                    if (Functions.LiveChanges.PreviousEngine != (int)Program.SynthSettings.GetValue("CurrentEngine", AudioEngine.WASAPI_ENGINE) ||
                        Functions.LiveChanges.PreviousFrequency != (int)Program.SynthSettings.GetValue("AudioFrequency", 44100) ||
                        Functions.LiveChanges.PreviousBuffer != (int)Program.SynthSettings.GetValue("BufferLength", 50) ||
                        Functions.LiveChanges.MonophonicRender != (int)Program.SynthSettings.GetValue("MonoRendering", 0) ||
                        Functions.LiveChanges.AudioBitDepth != (int)Program.SynthSettings.GetValue("AudioBitDepth", 0) ||
                        Functions.LiveChanges.NotesCatcherWithAudio != (int)Program.SynthSettings.GetValue("NotesCatcherWithAudio", 0))
                    {
                        Program.SynthSettings.SetValue("LiveChanges", 1, RegistryValueKind.DWord);
                        Functions.LiveChanges.PreviousEngine = (int)Program.SynthSettings.GetValue("CurrentEngine", AudioEngine.WASAPI_ENGINE);
                        Functions.LiveChanges.PreviousFrequency = (int)Program.SynthSettings.GetValue("AudioFrequency", 44100);
                        Functions.LiveChanges.PreviousBuffer = (int)Program.SynthSettings.GetValue("BufferLength", 50);
                        Functions.LiveChanges.MonophonicRender = (int)Program.SynthSettings.GetValue("MonoRendering", 0);
                        Functions.LiveChanges.AudioBitDepth = (int)Program.SynthSettings.GetValue("AudioBitDepth", 0);
                        Functions.LiveChanges.NotesCatcherWithAudio = (int)Program.SynthSettings.GetValue("NotesCatcherWithAudio", 0);
                    }
                }
            }

            System.Media.SystemSounds.Question.Play();
            Properties.Settings.Default.Save();
        }

        private void SettingsPanel_Load(object sender, EventArgs e)
        {
            // Nothing lul
        }

        private void VolumeBoost_Click(object sender, EventArgs e)
        {
            if (VolumeBoost.Checked)
            {
                if (VolTrackBar.Value > 10000)
                {
                    VolTrackBar.Value = 10000;
                    Program.SynthSettings.SetValue("OutputVolume", 10000, RegistryValueKind.DWord);
                }
                Program.SynthSettings.SetValue("VolumeBoost", 0, RegistryValueKind.DWord);
                VolTrackBar.Maximum = 10000;
                VolumeBoost.Checked = false;
                VolTrackBar.Refresh();
            }
            else
            {
                Program.SynthSettings.SetValue("VolumeBoost", 1, RegistryValueKind.DWord);
                VolTrackBar.Maximum = 50000;
                VolumeBoost.Checked = true;
                VolTrackBar.Refresh();
            }
        }

        private void SpatialSound_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Functions.OpenAdvancedAudioSettings("spatial", "This function requires Windows 10 Creators Update or newer.");
        }

        private void ChangeEVBuf_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new EVBufferManager().ShowDialog();
        }

        private void PitchShifting_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new PitchAndTranspose().ShowDialog();
        }

        private void WinMMSpeedDiag_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new WinMMSpeed().ShowDialog();
        }

        private void VolTrackBar_Scroll(object sender)
        {
            if (VolTrackBar.Value <= 49) VolSimView.ForeColor = Color.Red;
            else VolSimView.ForeColor = Color.FromArgb(255, 53, 0, 119);

            decimal VolVal = (decimal)VolTrackBar.Value / 100;
            VolSimView.Text = String.Format("{0}", Math.Round(VolVal, MidpointRounding.AwayFromZero).ToString());

            Program.SynthSettings.SetValue("OutputVolume", VolTrackBar.Value.ToString(), RegistryValueKind.DWord);
        }

        private void KSDAPIBoxWhat_Click(object sender, EventArgs e)
        {
            Program.ShowError(
                0,
                "Info",
                "If you uncheck this option, some apps might be forced to fallback to the stock Windows Multimedia API, which increases latency." +
                "\nApps that only make use of the Keppy's Direct MIDI API, with no Windows Multimedia API fallback, will probably ignore this setting." +
                "\n\n(This value will not affect the Windows Multimedia Wrapper.)",
                null);
        }

        private void HModeWhat_Click(object sender, EventArgs e)
        {
            Program.ShowError(
                0,
                "Info",
                "Clicking this checkbox will remove all the checks done to the events, for example transposing and other settings in the configurator.\n" +
                "The events will be sent straight to the buffer, and played immediately.\n\n" +
                "The \"Slow down playback instead of skipping notes\" checkbox will not work, while this mode is enabled, along with \"Running Status\" support and other event processing-related functions.\n\n" +
                "WARNING: Playing too much with the live changes while this setting is enabled might crash the threads, rendering the synth unusable until a full restart of the application!",
                null);
        }

        private void OverrideNoteLengthWA1_Click(object sender, EventArgs e)
        {
            Program.ShowError(
                0,
                "Info",
                "This option doesn't guarantee that all the notes will be turned off immediately after the specified amount of time on the left numericbox." +
                "\nPedal hold and other special events might delay the noteoff event even more.",
                null);
        }

        private void AudioEngBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioEngBoxTrigger(false);
        }

        private void ChangeDefaultOutput_Click(object sender, EventArgs e)
        {

            switch (AudioEngBox.SelectedIndex)
            {
                case AudioEngine.AUDTOWAV:
                    new OutputWAVDir().ShowDialog(this);
                    break;
                case AudioEngine.DSOUND_ENGINE:
                    new DefaultAudioOutput(false).ShowDialog(this);
                    break;
                case AudioEngine.WASAPI_ENGINE:
                ReturnHere:
                    Boolean OWM = Convert.ToBoolean(Convert.ToInt32(Program.SynthSettings.GetValue("OldWASAPIMode", "0")));

                    if (OWM)
                    {
                        DefaultAudioOutput Dlg = new DefaultAudioOutput(true);
                        if (Dlg.ShowDialog() == DialogResult.Yes)
                        {
                            if (Properties.Settings.Default.LiveChanges)
                                Program.SynthSettings.SetValue("LiveChanges", 1, RegistryValueKind.DWord);
                            goto ReturnHere;
                        }
                    }
                    else
                    {
                        DefaultWASAPIAudioOutput Dlg = new DefaultWASAPIAudioOutput();
                        if (Dlg.ShowDialog() == DialogResult.Yes)
                        {
                            if (Properties.Settings.Default.LiveChanges)
                                Program.SynthSettings.SetValue("LiveChanges", 1, RegistryValueKind.DWord);
                            goto ReturnHere;
                        }
                    }

                    break;
                case AudioEngine.ASIO_ENGINE:
                    new DefaultASIOAudioOutput(Control.ModifierKeys == Keys.Shift).ShowDialog();
                    break;
                case AudioEngine.XA_ENGINE:
                    new XAOutputSettings().ShowDialog();
                    break;
                default:
                    break;
            }
        }

        private void DebugModeFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            String DirectoryDebug = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\OmniMIDI\\debug\\";
            try
            {
                Process.Start(DirectoryDebug);
            }
            catch
            {
                Directory.CreateDirectory(DirectoryDebug);
                Process.Start(DirectoryDebug);
            }
        }

        private void FineTuneKnobIt_Click(object sender, EventArgs e)
        {
            PreciseControlVol PCV = new PreciseControlVol(VolTrackBar.Value, VolTrackBar.Maximum);

            if (PCV.ShowDialog() == DialogResult.OK)
                VolTrackBar.Value = PCV.NewVolume;

            PCV.Dispose();
        }

        private void LiveChangesTrigger_CheckedChanged(object sender, EventArgs e)
        {
            String Desc1 = LiveChangesTrigger.Checked ? "Requires a restart of the audio stream to work." : null;
            String Desc2 = LiveChangesTrigger.Checked ? "Requires a restart of the application to work." : null;

            // Stream restart
            Requirements.SetToolTip(AudioEngBox, Desc1);
            Requirements.SetToolTip(AudioBitDepth, Desc1);
            Requirements.SetToolTip(Frequency, Desc1);
            Requirements.SetToolTip(bufsize, Desc1);
            Requirements.SetToolTip(MonophonicFunc, Desc1);
            Requirements.SetToolTip(PrioBox, Desc1);

            // App restart
            Requirements.SetToolTip(KSDAPIBox, Desc2);
            Requirements.SetToolTip(DisableCookedPlayer, Desc2);
            Requirements.SetToolTip(DebugMode, Desc2);
            Requirements.SetToolTip(UseTGT, Desc2);
        }

        private void SincInter_CheckedChanged(object sender, EventArgs e)
        {
            SincConvLab.Enabled = SincInter.Checked;
            SincConv.Enabled = SincInter.Checked;
        }

        private void EnableRCOverride_CheckedChanged(object sender, EventArgs e)
        {
            ReverbL.Enabled = EnableRCOverride.Checked;
            ReverbV.Enabled = EnableRCOverride.Checked;
            ChorusL.Enabled = EnableRCOverride.Checked;
            ChorusV.Enabled = EnableRCOverride.Checked;
        }

        private void IgnoreNotes_CheckedChanged(object sender, EventArgs e)
        {
            IgnoreNotesLL.Enabled = IgnoreNotes.Checked;
            IgnoreNotesLV.Enabled = IgnoreNotes.Checked;
            IgnoreNotesHL.Enabled = IgnoreNotes.Checked;
            IgnoreNotesHV.Enabled = IgnoreNotes.Checked;
        }

        private void OverrideNoteLength_CheckedChanged(object sender, EventArgs e)
        {
            NoteLengthValue.Enabled = OverrideNoteLength.Checked;
        }

        private void DelayNoteOff_CheckedChanged(object sender, EventArgs e)
        {
            NoteOffDelayValue.Enabled = DelayNoteOff.Checked;
        }

        private void DebugMode_CheckedChanged(object sender, EventArgs e)
        {
            DebugModeFolder.Visible = DebugMode.Checked;
        }

        private void HMode_CheckedChanged(object sender, EventArgs e)
        {
            OverrideNoteLength.Enabled = !HMode.Checked;
            NoteLengthValue.Enabled = (HMode.Checked) ? false : OverrideNoteLength.Checked;

            DelayNoteOff.Enabled = !HMode.Checked;
            NoteOffDelayValue.Enabled = (HMode.Checked) ? false : DelayNoteOff.Checked;

            PitchShifting.Enabled = !HMode.Checked;

            IgnoreNotes.Enabled = !HMode.Checked;
            IgnoreNotesLL.Enabled = (HMode.Checked) ? false : IgnoreNotes.Checked;
            IgnoreNotesLV.Enabled = (HMode.Checked) ? false : IgnoreNotes.Checked;
            IgnoreNotesHL.Enabled = (HMode.Checked) ? false : IgnoreNotes.Checked;
            IgnoreNotesHV.Enabled = (HMode.Checked) ? false : IgnoreNotes.Checked;

            SlowDownPlayback.Enabled = !HMode.Checked;
            CapFram.Enabled = !HMode.Checked;
            SysResetIgnore.Enabled = !HMode.Checked;
            FullVelocityMode.Enabled = !HMode.Checked;
            Limit88.Enabled = !HMode.Checked;
            AllNotesIgnore.Enabled = !HMode.Checked;
        }

        public void ButtonToSaveSettings(object sender, EventArgs e)
        {
            SaveSettings((ModifierKeys & Keys.Control) == Keys.Control);
        }

        public void ButtonToResetSettings(object sender, EventArgs e)
        {
            DialogResult RES = Program.ShowError(3, "Reset settings", "Are you sure you want to reset your settings?\n\nAll your custom values will be lost.", null);

            if (RES == DialogResult.Yes)
            {
                Functions.ResetDriverSettings();
                LoadSettings();
            }                   
        }
    }
}
