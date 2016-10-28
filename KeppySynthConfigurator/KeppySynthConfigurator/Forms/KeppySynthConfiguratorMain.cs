﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

namespace KeppySynthConfigurator
{
    public partial class KeppySynthConfiguratorMain : Form
    {
        // Delegate for BasicFunc
        public static KeppySynthConfiguratorMain Delegate;

        public static string LastBrowserPath { get; set; }
        public static string LastImportExportPath { get; set; }

        // Themes handler
        public static int CurrentTheme = 0;

        // Lists
        public static string List1PathOld { get; set; }
        public static string List2PathOld { get; set; }
        public static string List3PathOld { get; set; }
        public static string List4PathOld { get; set; }
        public static string List5PathOld { get; set; }
        public static string List6PathOld { get; set; }
        public static string List7PathOld { get; set; }
        public static string List8PathOld { get; set; }

        public static string soundfontnewlocation = System.Environment.GetEnvironmentVariable("USERPROFILE");

        public static string AbsolutePath = soundfontnewlocation + "\\Keppy's Synthesizer";
        public static string ListsPath = soundfontnewlocation + "\\Keppy's Synthesizer\\lists";
        public static string List1Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidi.sflist";
        public static string List2Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidib.sflist";
        public static string List3Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidic.sflist";
        public static string List4Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidid.sflist";
        public static string List5Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidie.sflist";
        public static string List6Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidif.sflist";
        public static string List7Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidig.sflist";
        public static string List8Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidih.sflist";
        public static string List9Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidii.sflist";
        public static string List10Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidij.sflist";
        public static string List11Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidik.sflist";
        public static string List12Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidil.sflist";
        public static string List13Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidim.sflist";
        public static string List14Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidin.sflist";
        public static string List15Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidio.sflist";
        public static string List16Path = soundfontnewlocation + "\\Keppy's Synthesizer\\lists\\keppymidip.sflist";
        // Lists

        // Work
        public static int openadvanced { get; set; }
        public static int whichone { get; set; }
        public static string CurrentList { get; set; }

        public static RegistryKey SynthSettings = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Keppy's Synthesizer\\Settings", true);
        public static RegistryKey Watchdog = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Keppy's Synthesizer\\Watchdog", true);
        public static RegistryKey SynthPaths = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Keppy's Synthesizer\\Paths", true);

        public KeppySynthConfiguratorMain(String[] args)
        {
            InitializeComponent();
            Delegate = this;
            VolTrackBar.BackColor = Color.Empty;
            this.FormClosing += new FormClosingEventHandler(CloseConfigurator);
            try
            {
                foreach (String s in args)
                {
                    switch (s.Substring(0, 4).ToUpper())
                    {
                        case "/ASP":
                            Functions.UserProfileMigration();
                            Environment.Exit(0);
                            return;
                        case "/AST":
                            openadvanced = 1;
                            break;
                        case "/NUL":
                            break;
                        case "/MIX":
                            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\keppysynth\\KeppySynthMixerWindow.exe");
                            return;
                        default:
                            // do other stuff...
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)Program.BringToFrontMessage)
            {
                WinAPI.ShowWindow(Handle, WinAPI.SW_RESTORE);
                WinAPI.SetForegroundWindow(Handle);
            }
            base.WndProc(ref m);
        }

        private void KeppySynthConfiguratorMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SynthSettings.Close();
            Watchdog.Close();
        }

        // Just stuff to reduce code's length
        private void SFZCompliant()
        {
            MessageBox.Show("This driver is \"SFZ format 2.0\" compliant.", "SFZ format support", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddSoundfontDragNDrop(String SelectedList, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int i;
            for (i = 0; i < s.Length; i++)
            {
                if (Path.GetExtension(s[i]) == ".sf2" | Path.GetExtension(s[i]) == ".SF2" | Path.GetExtension(s[i]) == ".sf3" | Path.GetExtension(s[i]) == ".SF3" | Path.GetExtension(s[i]) == ".sfpack" | Path.GetExtension(s[i]) == ".SFPACK")
                {
                    if (BankPresetOverride.Checked == true)
                    {
                        using (var form = new BankNPresetSel(Path.GetFileName(s[i]), 0, 1))
                        {
                            var result = form.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                string sbank = form.BankValueReturn;
                                string spreset = form.PresetValueReturn;
                                string dbank = form.DesBankValueReturn;
                                string dpreset = form.DesPresetValueReturn;
                                Lis.Items.Add("p" + sbank + "," + spreset + "=" + dbank + "," + dpreset + "|" + s[i]);
                            }
                        }
                    }
                    else
                    {
                        Lis.Items.Add(s[i]);
                    }
                    Functions.SaveList(CurrentList);
                    Functions.TriggerReload();
                }
                else if (Path.GetExtension(s[i]) == ".sfz" | Path.GetExtension(s[i]) == ".SFZ")
                {
                    using (var form = new BankNPresetSel(Path.GetFileName(s[i]), 1, 0))
                    {
                        var result = form.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            string sbank = form.BankValueReturn;
                            string spreset = form.PresetValueReturn;
                            string dbank = form.DesBankValueReturn;
                            string dpreset = form.DesPresetValueReturn;
                            Lis.Items.Add("p" + sbank + "," + spreset + "=" + dbank + "," + dpreset + "|" + s[i]);
                        }
                    }
                    Functions.SaveList(CurrentList);
                    Functions.TriggerReload();
                }
                else if (Path.GetExtension(s[i]) == ".dls" | Path.GetExtension(s[i]) == ".DLS")
                {
                    MessageBox.Show("BASSMIDI does NOT support the downloadable sounds (DLS) format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (Path.GetExtension(s[i]) == ".exe" | Path.GetExtension(s[i]) == ".EXE" | Path.GetExtension(s[i]) == ".dll" | Path.GetExtension(s[i]) == ".DLL")
                {
                    MessageBox.Show("Are you really trying to add executables to the soundfonts list?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Invalid soundfont!\n\nPlease select a valid soundfont and try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void AddSoundfontDragNDropTriv(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        // Here we go!
        private void KeppySynthConfiguratorMain_Load(object sender, EventArgs e)
        {
            // SAS THEME HANDLER        
            this.ThemeCheck.RunWorkerAsync();
            this.Size = new Size(665, 481);
            // MIDI out selector disabler
            if (Functions.IsWindows8OrNewer() == true)
            {
                changeDefaultMIDIOutDeviceToolStripMenuItem1.Text = "Change default MIDI out device for Windows Media Player";
                changeDefaultMIDIOutDeviceToolStripMenuItem.Text = "Change default MIDI out device for Windows Media Player 32-bit";
                changeDefault64bitMIDIOutDeviceToolStripMenuItem.Text = "Change default MIDI out device for Windows Media Player 64-bit";
                getTheMIDIMapperForWindows8xToolStripMenuItem.Visible = true;
                getTheMIDIMapperForWindows10ToolStripMenuItem.Visible = true;
                SetSynthDefault.Visible = true;
            }
            if (Functions.IsWindowsXP() == true)
            {
                MessageBox.Show("This release of Keppy's Synthesizer will be the last one to support Windows XP.\n\nPlease upgrade to a newer OS, like Vista or 7, in order to get further updates to the driver.", "Keppy's Synthesizer - Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                menuItem17.Visible = false;
                manageFolderFavouritesToolStripMenuItem.Visible = false;
            }
            if (Environment.Is64BitOperatingSystem == false)
            {
                changeDefaultMIDIOutDeviceToolStripMenuItem1.Visible = true;
                changeDefaultMIDIOutDeviceToolStripMenuItem.Visible = false;
                changeDefault64bitMIDIOutDeviceToolStripMenuItem.Visible = false;
            }
            else
            {
                changeDefaultMIDIOutDeviceToolStripMenuItem1.Visible = false;
                changeDefaultMIDIOutDeviceToolStripMenuItem.Visible = true;
                changeDefault64bitMIDIOutDeviceToolStripMenuItem.Visible = true;
            }

            Lis.ContextMenu = RightClickMenu;
            Functions.InitializeLastPath();
            SelectedListBox.Text = "List 1";
            KeppySynthConfiguratorMain.whichone = 1;

            Functions.LoadSettings();

            // Jakuberino
            if (openadvanced == 1)
            {
                TabsForTheControls.SelectedIndex = 1;
            }
        }

        private void VolTrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                int VolumeValue = 0;
                double x = VolTrackBar.Value / 100;
                VolumeValue = Convert.ToInt32(x);
                VolSimView.Text = VolumeValue.ToString("000\\%");
                VolIntView.Text = "Value: " + VolTrackBar.Value.ToString("00000");
                SynthSettings.SetValue("volume", VolTrackBar.Value.ToString(), RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Crap, an error!\n\nError:\n" + ex.ToString(), "Oh no! Keppy's Synthesizer encountered an error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportSettings_Click(object sender, EventArgs e)
        {
            if (ExportSettingsDialog.ShowDialog() == DialogResult.OK)
            {
                Functions.ExportSettings(ExportSettingsDialog.FileName);
                MessageBox.Show("The settings have been exported to the selected registry file!", "Keppy's Synthesizer - Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ImportSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (ImportSettingsDialog.ShowDialog() == DialogResult.OK)
                {
                    string line = File.ReadLines(ImportSettingsDialog.FileName).Skip(2).Take(1).First();

                    if (line == "; Keppy's Synthesizer Settings File")
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = "reg.exe";
                        startInfo.Arguments = String.Format("import \"{0}\"", ImportSettingsDialog.FileName);
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;

                        Process processTemp = new Process();
                        processTemp.StartInfo = startInfo;
                        processTemp.EnableRaisingEvents = true;
                        processTemp.Start();
                        processTemp.WaitForExit();

                        Functions.LoadSettings();

                        MessageBox.Show("The settings have been imported from the selected registry file!", "Keppy's Synthesizer - Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Invalid registry file!\n\nThis file doesn't contain valid Keppy's Synthesizer settings!!!", "Keppy's Synthesizer - Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
            }
            catch
            {
                // Something bad happened hehe
                MessageBox.Show("Fatal error during the execution of this program!\n\nPress OK to quit.", "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Application.Exit();
            }
        }

        private void CLi_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear the list?", "Keppy's Synthesizer Configurator ~ Clear list " + whichone.ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    Lis.Items.Clear();
                    File.Delete(CurrentList);
                    var TempFile = File.Create(CurrentList);
                    TempFile.Close();
                    if (Convert.ToInt32(Watchdog.GetValue("currentsflist")) == whichone)
                    {
                        Watchdog.SetValue("rel" + whichone.ToString(), "1", RegistryValueKind.DWord);
                    }
                }
                catch (Exception ex)
                {
                    Functions.ReinitializeList(ex, CurrentList);
                }
            }
        }

        private void AddSF_Click(object sender, EventArgs e)
        {
            try
            {
                SoundfontImport.InitialDirectory = LastBrowserPath;
                SoundfontImport.FileName = "";
                Functions.OpenFileDialogAddCustomPaths(SoundfontImport);
                if (SoundfontImport.ShowDialog() == DialogResult.OK)
                {
                    foreach (string str in SoundfontImport.FileNames)
                    {
                        if (Path.GetExtension(str).ToLower() == ".sf2" | Path.GetExtension(str).ToLower() == ".sfpack")
                        {
                            if (BankPresetOverride.Checked == true)
                            {
                                using (var form = new BankNPresetSel(Path.GetFileName(str), 0, 1))
                                {
                                    var result = form.ShowDialog();
                                    if (result == DialogResult.OK)
                                    {
                                        string sbank = form.BankValueReturn;
                                        string spreset = form.PresetValueReturn;
                                        string dbank = form.DesBankValueReturn;
                                        string dpreset = form.DesPresetValueReturn;
                                        Lis.Items.Add("p" + sbank + "," + spreset + "=" + dbank + "," + dpreset + "|" + str);
                                    }
                                }
                            }
                            else
                            {
                                Lis.Items.Add(str);
                            }
                        }
                        else if (Path.GetExtension(str).ToLower() == ".sfz")
                        {
                            using (var form = new BankNPresetSel(Path.GetFileName(str), 0, 0))
                            {
                                var result = form.ShowDialog();
                                if (result == DialogResult.OK)
                                {
                                    string sbank = form.BankValueReturn;
                                    string spreset = form.PresetValueReturn;
                                    string dbank = form.DesBankValueReturn;
                                    string dpreset = form.DesPresetValueReturn;
                                    Lis.Items.Add("p" + sbank + "," + spreset + "=" + dbank + "," + dpreset + "|" + str);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(Path.GetFileName(str) + " is not a valid soundfont file!", "Error while adding soundfont", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        LastBrowserPath = Path.GetDirectoryName(str);
                        Functions.SetLastPath(LastBrowserPath);
                    }
                    Functions.SaveList(CurrentList);
                    Functions.TriggerReload();
                }
            }
            catch (Exception ex)
            {
                Functions.ReinitializeList(ex, CurrentList);
            }
        }

        private void RmvSF_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = Lis.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    Lis.Items.RemoveAt(Lis.SelectedIndices[i]);
                }
                Functions.SaveList(CurrentList);
                Functions.TriggerReload();
            }
            catch (Exception ex)
            {
                Functions.ReinitializeList(ex, CurrentList);
            }
        }

        private void MvU_Click(object sender, EventArgs e)
        {
            try
            {
                int howmany = Lis.SelectedItems.Count;
                if (howmany > 0)
                {
                    object selected = Lis.SelectedItem;
                    int indx = Lis.Items.IndexOf(selected);
                    int totl = Lis.Items.Count;
                    if (indx == 0)
                    {
                        Lis.Items.Remove(selected);
                        Lis.Items.Insert(totl - 1, selected);
                        Lis.SetSelected(totl - 1, true);
                    }
                    else
                    {
                        Lis.Items.Remove(selected);
                        Lis.Items.Insert(indx - 1, selected);
                        Lis.SetSelected(indx - 1, true);
                    }
                }
                Functions.SaveList(CurrentList);
                Functions.TriggerReload();
            }
            catch (Exception ex)
            {
                Functions.ReinitializeList(ex, CurrentList);
            }
        }

        private void MvD_Click(object sender, EventArgs e)
        {
            try
            {
                int howmany = Lis.SelectedItems.Count;
                if (howmany > 0)
                {
                    object selected = Lis.SelectedItem;
                    int indx = Lis.Items.IndexOf(selected);
                    int totl = Lis.Items.Count;
                    if (indx == totl - 1)
                    {
                        Lis.Items.Remove(selected);
                        Lis.Items.Insert(0, selected);
                        Lis.SetSelected(0, true);
                    }
                    else
                    {
                        Lis.Items.Remove(selected);
                        Lis.Items.Insert(indx + 1, selected);
                        Lis.SetSelected(indx + 1, true);
                    }

                }
                Functions.SaveList(CurrentList);
                Functions.TriggerReload();
            }
            catch (Exception ex)
            {
                Functions.ReinitializeList(ex, CurrentList);
            }
        }

        private void LoadToApp_Click(object sender, EventArgs e)
        {
            Watchdog.SetValue("currentsflist", whichone, RegistryValueKind.DWord);
            Watchdog.SetValue("rel" + whichone.ToString(), "1", RegistryValueKind.DWord);
        }

        private void EnableSF_Click(object sender, EventArgs e)
        {
            try
            {
                if (Lis.SelectedIndex == -1)
                {
                    MessageBox.Show("Select a soundfont first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (Lis.SelectedIndices.Count > 1)
                    {
                        MessageBox.Show("You can only enable one soundfont at the time!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        string result = Lis.SelectedItem.ToString().Substring(0, 1);
                        if (result == "@")
                        {
                            string newvalue = Lis.SelectedItem.ToString().Remove(0, 1);
                            int index = Lis.Items.IndexOf(Lis.SelectedItem);
                            Lis.Items.RemoveAt(index);
                            Lis.Items.Insert(index, newvalue);
                            Functions.SaveList(CurrentList);
                            Functions.TriggerReload();
                        }
                        else
                        {
                            MessageBox.Show("The soundfont is already enabled!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.ReinitializeList(ex, CurrentList);
            }
        }

        private void DisableSF_Click(object sender, EventArgs e)
        {
            try
            {
                if (Lis.SelectedIndex == -1)
                {
                    MessageBox.Show("Select a soundfont first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (Lis.SelectedIndices.Count > 1)
                    {
                        MessageBox.Show("You can only disable one soundfont at the time!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        string result = Lis.SelectedItem.ToString().Substring(0, 1);
                        if (result != "@")
                        {
                            string newvalue = "@" + Lis.SelectedItem.ToString();
                            int index = Lis.Items.IndexOf(Lis.SelectedItem);
                            Lis.Items.RemoveAt(index);
                            Lis.Items.Insert(index, newvalue);
                            Functions.SaveList(CurrentList);
                            Functions.TriggerReload();
                        }
                        else
                        {
                            MessageBox.Show("The soundfont is already disabled!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.ReinitializeList(ex, CurrentList);
            }
        }

        private void IEL_Click(object sender, EventArgs e)
        {
            try
            {
                ExternalListImport.FileName = "";
                ExternalListImport.InitialDirectory = LastImportExportPath;
                if (ExternalListImport.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in ExternalListImport.FileNames)
                    {
                        LastImportExportPath = Path.GetDirectoryName(file);
                        Functions.SetLastPath(LastBrowserPath);
                        using (StreamReader r = new StreamReader(file))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                Lis.Items.Add(line); // Read the external list and add the items to the selected list
                            }
                        }
                        Functions.SaveList(CurrentList);
                        Functions.TriggerReload();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error during the import process of the list!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void EL_Click(object sender, EventArgs e)
        {
            ExternalListExport.FileName = "";
            ExternalListExport.InitialDirectory = LastImportExportPath;
            if (ExternalListExport.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(ExternalListExport.FileName);
                Functions.SetLastPath(LastBrowserPath);
                foreach (var item in Lis.Items)
                {
                    SaveFile.WriteLine(item.ToString());
                }
                SaveFile.Close();
                MessageBox.Show("Soundfont list exported succesfully to \"" + Path.GetDirectoryName(ExternalListExport.FileName) + "\\\"", "Soundfont list exported!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void IEL_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = IEL.ClientRectangle;
            rect.Width--;
            rect.Height--;
            e.Graphics.DrawRectangle(Pens.Green, rect);
        }

        private void EL_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = EL.ClientRectangle;
            rect.Width--;
            rect.Height--;
            e.Graphics.DrawRectangle(Pens.Red, rect);
        }

        private string ListRelativePathSystem(string originalpath)
        {
            return "a";
        }

        private void SelectedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Functions.ChangeList(SelectedListBox.SelectedIndex + 1);
        }

        private void Lis_DragDrop(object sender, DragEventArgs e)
        {
            AddSoundfontDragNDrop(CurrentList, e);
        }

        private void Lis_DragEnter(object sender, DragEventArgs e)
        {
            AddSoundfontDragNDropTriv(e);
        }

        // End of the soundfont lists functions
        // ------------------------------------

        private void resetToDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set some values...
            VolTrackBar.Value = 10000;
            PolyphonyLimit.Value = 512;
            MaxCPU.Value = 65;
            Frequency.Text = "48000";
            bufsize.Value = 30;
            TracksLimit.Value = 16;
            Preload.Checked = true;
            NoteOffCheck.Checked = false;
            SincInter.Checked = false;
            EnableSFX.Checked = false;
            SysResetIgnore.Checked = false;
            OutputWAV.Checked = false;
            XAudioDisable.Checked = false;
            ManualAddBuffer.Checked = false;

            // And then...
            Functions.SaveSettings();

            // Messagebox here
            MessageBox.Show("Settings restored to the default values!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void applySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Just save the Settings
            Functions.SaveSettings();

            // Messagebox here
            MessageBox.Show("Settings saved to the registry!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void blackMIDIsPresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set some values...
            VolTrackBar.Value = 10000;
            PolyphonyLimit.Value = 1000;
            MaxCPU.Value = 75;
            Frequency.Text = "44100";
            bufsize.Value = 15;
            TracksLimit.Value = 16;
            Preload.Checked = true;
            NoteOffCheck.Checked = false;
            SincInter.Checked = false;
            EnableSFX.Checked = false;
            SysResetIgnore.Checked = true;
            OutputWAV.Checked = false;
            XAudioDisable.Checked = false;
            ManualAddBuffer.Checked = false;

            // Additional settings
            SynthSettings.SetValue("rco", "1", RegistryValueKind.DWord);
            ReduceCPUOver.Checked = true;

            // And then...
            Functions.SaveSettings();

            // Messagebox here
            MessageBox.Show("The Black MIDIs preset has been applied!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lowLatencyPresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set some values...
            VolTrackBar.Value = 10000;
            PolyphonyLimit.Value = 500;
            MaxCPU.Value = 80;
            Frequency.Text = "44100";
            bufsize.Value = 10;
            TracksLimit.Value = 16;
            Preload.Checked = true;
            NoteOffCheck.Checked = false;
            SincInter.Checked = true;
            EnableSFX.Checked = true;
            SysResetIgnore.Checked = true;
            OutputWAV.Checked = false;
            XAudioDisable.Checked = false;
            ManualAddBuffer.Checked = false;

            // Additional settings
            SynthSettings.SetValue("rco", "1", RegistryValueKind.DWord);
            ReduceCPUOver.Checked = true;

            // And then...
            Functions.SaveSettings();

            // Messagebox here
            MessageBox.Show("The low latency preset has been applied!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void chiptunesRetrogamingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set some values...
            VolTrackBar.Value = 10000;
            PolyphonyLimit.Value = 64;
            MaxCPU.Value = 0;
            Frequency.Text = "22050";
            XAudioDisable.Checked = true;
            XAudioDisable_CheckedChanged(null, null);
            bufsize.Value = 0;
            TracksLimit.Value = 16;
            Preload.Checked = true;
            NoteOffCheck.Checked = false;
            SincInter.Checked = false;
            EnableSFX.Checked = true;
            SysResetIgnore.Checked = false;
            OutputWAV.Checked = false;
            ManualAddBuffer.Checked = false;

            // Additional settings
            SynthSettings.SetValue("rco", "1", RegistryValueKind.DWord);
            ReduceCPUOver.Checked = true;

            // And then...
            Functions.SaveSettings();

            // Messagebox here
            MessageBox.Show("The chiptunes/retrogaming preset has been applied!\n\n\"The NES soundfont\" is recommended for chiptunes.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void keppysSteinwayPianoRealismToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set some values...
            VolTrackBar.Value = 10000;
            PolyphonyLimit.Value = 850;
            MaxCPU.Value = 85;
            Frequency.Text = "66150";
            bufsize.Value = 20;
            TracksLimit.Value = 16;
            Preload.Checked = true;
            NoteOffCheck.Checked = true;
            SincInter.Checked = true;
            EnableSFX.Checked = true;
            SysResetIgnore.Checked = true;
            OutputWAV.Checked = false;
            XAudioDisable.Checked = false;
            ManualAddBuffer.Checked = false;

            // Additional settings
            SynthSettings.SetValue("rco", "0", RegistryValueKind.DWord);
            ReduceCPUOver.Checked = true;

            // And then...
            Functions.SaveSettings();

            // Messagebox here
            MessageBox.Show("\"Keppy's Steinway Piano (Realism)\" has been applied!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Now, menustrip functions here

        private void openDebugWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\keppysynth\\KeppySynthDebugWindow.exe");
        }

        private void openTheMixerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\keppysynth\\KeppySynthMixerWindow.exe");
        }

        private void openTheBlacklistManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthBlacklistSystem frm = new KeppySynthBlacklistSystem();
            frm.ShowDialog();
        }

        private void informationAboutTheDriverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthInformation frm = new KeppySynthInformation();
            frm.ShowDialog();
        }

        private void changeDefaultMIDIOutDeviceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Functions.IsWindowsXP() == false)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\keppysynth\\midioutsetter32.exe");
            }
            else
            {
                Process MIDIPanelXP = new Process();
                MIDIPanelXP.StartInfo.FileName = "rundll32.exe";
                MIDIPanelXP.StartInfo.Arguments = "MMSYS.CPL,ShowAudioPropertySheet";
                MIDIPanelXP.Start();
                this.Enabled = false;
                MIDIPanelXP.WaitForExit();
                this.Enabled = true;
            }
        }

        private void changeDefault64bitMIDIOutDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Functions.IsWindowsXP() == false)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\keppysynth\\midioutsetter64.exe");
            }
            else
            {
                Process MIDIPanelXP = new Process();
                MIDIPanelXP.StartInfo.FileName = "rundll32.exe";
                MIDIPanelXP.StartInfo.Arguments = "MMSYS.CPL,ShowAudioPropertySheet";
                MIDIPanelXP.Start();
                this.Enabled = false;
                MIDIPanelXP.WaitForExit();
                this.Enabled = true;
            }
        }

        private void openUpdaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Functions.CheckForUpdates();
        }

        private void reportABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Do you want to report a bug about Keppy's Synthesizer?\n\nWARNING: Only use this function to report serious bugs, like memory leaks and security flaws.", "Report a bug...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Process.Start("https://github.com/KaleidonKep99/Keppy-s-MIDI-Driver/issues");
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }

        private void downloadTheSourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/KaleidonKep99/Keppy-s-MIDI-Driver");
        }

        private void visitKeppyStudiosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://keppystudios.com");
        }

        private void getTheMIDIMapperForWindows8xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://drive.google.com/file/d/0B05Sp4zxPFR6UW9CQ0RRak85eDA/view?usp=sharing");
        }

        private void getTheMIDIMapperForWindows10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://plus.google.com/+RichardForhenson/posts/bkrqUfbV3xz");
        }

        private void donateToSupportUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = "";

            string business = "prapapappo1999@gmail.com";
            string description = "Donation";
            string country = "US";
            string currency = "USD";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            Process.Start(url);
        }

        private void changeTheSizeOfTheEVBufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthEVBuffer frm = new KeppySynthEVBuffer();
            frm.ShowDialog();
        }

        private void changeDirectoryOfTheOutputToWAVModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthOutputWAVDir frm = new KeppySynthOutputWAVDir();
            frm.ShowDialog();
        }

        private void changeDefaultSoundfontListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthDefaultSFList frm = new KeppySynthDefaultSFList();
            frm.ShowDialog();
        }

        private void changeDefaultSoundfontListToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            KeppySynthDefaultSFList frm = new KeppySynthDefaultSFList();
            frm.ShowDialog();
        }

        private void changeTheMaximumSamplesPerFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthSamplePerFrameSetting frm = new KeppySynthSamplePerFrameSetting();
            frm.ShowDialog();
        }

        private void SPFSecondaryBut_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            KeppySynthSamplePerFrameSetting frm = new KeppySynthSamplePerFrameSetting();
            frm.ShowDialog();
        }

        private void assignASoundfontListToASpecificAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthSFListAssign frm = new KeppySynthSFListAssign();
            frm.ShowDialog();
        }

        private void manageFolderFavouritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeppySynthFavouritesManager frm = new KeppySynthFavouritesManager();
            frm.ShowDialog();
        }

        private void DefaultOut810enabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SynthSettings.SetValue("defaultmidiout", "1", RegistryValueKind.DWord);
            DefaultOut810enabledToolStripMenuItem.Checked = true;
            DefaultOut810disabledToolStripMenuItem.Checked = false;
            DefaultOut810enabledToolStripMenuItem.Enabled = false;
            DefaultOut810disabledToolStripMenuItem.Enabled = true;
        }

        private void DefaultOut810disabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SynthSettings.SetValue("defaultmidiout", "0", RegistryValueKind.DWord);
            DefaultOut810enabledToolStripMenuItem.Checked = false;
            DefaultOut810disabledToolStripMenuItem.Checked = true;
            DefaultOut810enabledToolStripMenuItem.Enabled = true;
            DefaultOut810disabledToolStripMenuItem.Enabled = false;
        }

        private void RegDriver_Click(object sender, EventArgs e)
        {
            Functions.DriverRegistry(0);
        }

        private void UnregDriver_Click(object sender, EventArgs e)
        {
            Functions.DriverRegistry(1);
        }

        private void CloseConfigurator(object sender, CancelEventArgs e)
        {
            Application.ExitThread();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        // Guide part
        private void isThereAnyShortcutForToOpenTheConfiguratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To open the configurator while playing a MIDI, press ALT+9.\nYou could also press ALT+0 to directly open the \"Settings\" tab.",
                "What are the hotkeys to open the configurator?", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void whatAreTheHotkeysToChangeTheVolumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To change the volume, simply press \"Add\" or \"Subtract\" buttons of the numeric keypad.\n\nYou can disable the hotkeys through \"Advanced settings > Volume hotkeys\".",
                "What are the hotkeys to change the volume?", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void howCanIChangeTheSoundfontListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To change the current soundfont list, press and hold ALT, then click a number from 1 to 8.\n\n" +
                "ALT+1: Load soundfont list 1\nALT+2: Load soundfont list 2\nALT+3: Load soundfont list 3\nALT+4: Load soundfont list 4\nALT+5: Load soundfont list 5\nALT+6: Load soundfont list 6\nALT+7: Load soundfont list 7\nALT+8: Load soundfont list 8\nCTRL+ALT+1: Load soundfont list 9\nCTRL+ALT+2: Load soundfont list 10\nCTRL+ALT+3: Load soundfont list 11\nCTRL+ALT+4: Load soundfont list 12\nCTRL+ALT+5: Load soundfont list 13\nCTRL+ALT+6: Load soundfont list 14\nCTRL+ALT+7: Load soundfont list 15\nCTRL+ALT+8: Load soundfont list 16\n\n" +
                "You can also reload lists that are already loaded in memory.", "How can I change the soundfont list?", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void howCanIResetTheDriverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To reset the driver, press INS.\nThis will stop all the samples that are currently playing, and it'll also send a \"System Reset\" to all the MIDI channels.", "How can I reset the driver?", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void whatsTheBestSettingsForTheBufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("For SoundBlaster-based audio cards, it's 10.\nFor Realtek audio cards, it's 15-20.\nFor VIA audio cards, it's 20.\nFor Conexant audio cards, it's 30.\nFor USB DACs, it's 25-35.\nFor all the AC'97 audio cards, it's 35.\n\nIt's possible to set it to 10 with really fast computers.", "What's the best settings for the buffer?", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void WhatsAutoPanic_Click(object sender, EventArgs e)
        {
            MessageBox.Show("\"Automatic MIDI panic\" will tell the driver to kill all the active notes, when the CPU usage is equal or higher than 100%.", "What's the automatic MIDI panic?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void whyCanNotDisableSoftwareRendering_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Since Windows Vista, DirectSound is emulated through the \"Windows Audio Session API\".\n\nIt's not hardware rendered anymore, so the function is set to \"Enabled\" by default.\n\nWindows XP is the last O.S. to support native hardware rendering.", "Why can't I disable software rendering?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Brand new output mode
        private void WhatIsOutput_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If you check this option, the driver will create a WAV file on your desktop, called \"(programname).exe - Keppy's Synthesizer Output File.wav\".\n\n" +
                "You can change the output directory by clicking \"Settings > Change directory of the \"Output to WAV\" mode\".\n\n" +
                "(The audio output to the speakers/headphones will be disabled, to avoid corruptions in the audio export.)", "\"Output to WAV mode\"? What is it?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Brand new XAudio disabler
        private void WhatIsXAudio_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Check this, if you don't want static noises or the XAudio interface doesn't work properly and/or it's buggy.\n\n(Notice: Disabling XAudio also increases the latency by a bit, and disables the \"Output to WAV\" mode.)", "\"Disable the XAudio engine\"? What is it?", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void XAudioDisable_CheckedChanged(object sender, EventArgs e)
        {
            if (XAudioDisable.Checked == true)
            {
                OutputWAV.Enabled = false;
                OutputWAV.Checked = false;
                ManualAddBuffer.Visible = true;
                SPFSecondaryBut.Visible = false;
                changeTheMaximumSamplesPerFrameToolStripMenuItem.Enabled = false;
                changeDirectoryOfTheOutputToWAVModeToolStripMenuItem.Enabled = false;
                BufferText.Text = "Set a additional buffer length for the driver, from 0 to 1000:";
                SincInter.Checked = false;
                SincInter.Enabled = true;
                SincInter.Text = "Enable sinc interpolation. (Improves sample rate conversion and overall audio quality, but increases rendering time.)";
                bufsize.Minimum = 0;
                bufsize.Maximum = 1000;
                bufsize.Enabled = false;
                if (ManualAddBuffer.Checked == true)
                {
                    bufsize.Enabled = true;
                }
                else
                {
                    bufsize.Enabled = false;
                    bufsize.Value = 0;
                }
            }
            else if (XAudioDisable.Checked == false)
            {
                OutputWAV.Enabled = true;
                ManualAddBuffer.Visible = false;
                SPFSecondaryBut.Visible = true;
                changeTheMaximumSamplesPerFrameToolStripMenuItem.Enabled = true;
                changeDirectoryOfTheOutputToWAVModeToolStripMenuItem.Enabled = true;
                BufferText.Text = "Set a buffer length for the driver, from 1 to 100 (             ):";
                SincInter.Checked = true;
                SincInter.Enabled = false;
                SincInter.Text = "Enable sinc interpolation. (Already applied by XAudio itself, it doesn't cause CPU overhead.)";
                bufsize.Minimum = 1;
                bufsize.Maximum = 100;
                bufsize.Enabled = true;
                bufsize.Value = 15;
            }
        }

        private void OutputWAV_CheckedChanged(object sender, EventArgs e)
        {
            if (OutputWAV.Checked == true)
            {
                XAudioDisable.Enabled = false;
                XAudioDisable.Checked = false;
                Label5.Enabled = false;
                bufsize.Enabled = false;
                MaxCPU.Enabled = false;
                BufferText.Enabled = false;
                bufsize.Enabled = false;
                bufsize.Minimum = 0;
                bufsize.Value = 0;
                MaxCPU.Value = 0;
            }
            else if (OutputWAV.Checked == false)
            {
                XAudioDisable.Enabled = true;
                Label5.Enabled = true;
                bufsize.Enabled = true;
                MaxCPU.Enabled = true;
                BufferText.Enabled = true;
                bufsize.Enabled = true;
                bufsize.Minimum = 1;
                bufsize.Value = 15;
                MaxCPU.Value = 75;
            }
        }

        private void VMSEmu_CheckedChanged(object sender, EventArgs e)
        {
            if (ManualAddBuffer.Checked == true)
            {
                bufsize.Enabled = true;
            }
            else
            {
                bufsize.Enabled = false;
            }
        }

        private void MIDINameNoSpace_Click(object sender, EventArgs e)
        {
            if (MIDINameNoSpace.Checked == false)
            {
                SynthSettings.SetValue("shortname", "1", RegistryValueKind.DWord);
                MIDINameNoSpace.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("shortname", "0", RegistryValueKind.DWord);
                MIDINameNoSpace.Checked = false;
            }
        }

        private void useoldbuffersystem_Click(object sender, EventArgs e)
        {
            if (useoldbuffersystem.Checked == false)
            {
                SynthSettings.SetValue("oldbuffersystem", "1", RegistryValueKind.DWord);
                useoldbuffersystem.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("oldbuffersystem", "0", RegistryValueKind.DWord);
                useoldbuffersystem.Checked = false;
            }
        }

        private void slowdownnoskip_Click(object sender, EventArgs e)
        {
            if (slowdownnoskip.Checked == false)
            {
                SynthSettings.SetValue("vms2emu", "1", RegistryValueKind.DWord);
                slowdownnoskip.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("vms2emu", "0", RegistryValueKind.DWord);
                slowdownnoskip.Checked = false;
            }
        }

        private void autopanicmode_Click(object sender, EventArgs e)
        {
            if (autopanicmode.Checked == false)
            {
                SynthSettings.SetValue("autopanic", "1", RegistryValueKind.DWord);
                autopanicmode.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("autopanic", "0", RegistryValueKind.DWord);
                autopanicmode.Checked = false;
            }
        }

        private void useSWrendering_Click(object sender, EventArgs e)
        {
            if (useSWrendering.Checked == false)
            {
                SynthSettings.SetValue("softwarerendering", "1", RegistryValueKind.DWord);
                useSWrendering.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("softwarerendering", "0", RegistryValueKind.DWord);
                useSWrendering.Checked = false;
            }
        }

        private void hotkeys_Click(object sender, EventArgs e)
        {
            if (hotkeys.Checked == false)
            {
                SynthSettings.SetValue("allhotkeys", "1", RegistryValueKind.DWord);
                hotkeys.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("allhotkeys", "0", RegistryValueKind.DWord);
                hotkeys.Checked = false;
            }
        }

        private void autoupdate_Click(object sender, EventArgs e)
        {
            if (autoupdate.Checked == false)
            {
                SynthSettings.SetValue("autoupdatecheck", "1", RegistryValueKind.DWord);
                autoupdate.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("autoupdatecheck", "0", RegistryValueKind.DWord);
                autoupdate.Checked = false;
            }
        }

        private void ReduceCPUOver_Click(object sender, EventArgs e)
        {
            if (ReduceCPUOver.Checked == false)
            {
                SynthSettings.SetValue("rco", "1", RegistryValueKind.DWord);
                ReduceCPUOver.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("rco", "0", RegistryValueKind.DWord);
                ReduceCPUOver.Checked = false;
            }
        }

        private void enableextra8sf_Click(object sender, EventArgs e)
        {
            if (enableextra8sf.Checked == false)
            {
                SynthSettings.SetValue("extra8lists", "1", RegistryValueKind.DWord);
                enableextra8sf.Checked = true;
                SelectedListBox.Items.Add("List 9");
                SelectedListBox.Items.Add("List 10");
                SelectedListBox.Items.Add("List 11");
                SelectedListBox.Items.Add("List 12");
                SelectedListBox.Items.Add("List 13");
                SelectedListBox.Items.Add("List 14");
                SelectedListBox.Items.Add("List 15");
                SelectedListBox.Items.Add("List 16");
            }
            else
            {
                SynthSettings.SetValue("extra8lists", "0", RegistryValueKind.DWord);
                enableextra8sf.Checked = false;
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
                SelectedListBox.Items.RemoveAt(8);
            }
        }

        private void floatingpointaudio_Click(object sender, EventArgs e)
        {
            if (floatingpointaudio.Checked == false)
            {
                SynthSettings.SetValue("32bit", "1", RegistryValueKind.DWord);
                floatingpointaudio.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("32bit", "0", RegistryValueKind.DWord);
                floatingpointaudio.Checked = false;
            }
        }

        private void SysExIgnore_Click(object sender, EventArgs e)
        {
            if (SysExIgnore.Checked == false)
            {
                SynthSettings.SetValue("sysexignore", "1", RegistryValueKind.DWord);
                SysExIgnore.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("sysexignore", "0", RegistryValueKind.DWord);
                SysExIgnore.Checked = false;
            }
        }

        private void AllNotesIgnore_Click(object sender, EventArgs e)
        {
            if (AllNotesIgnore.Checked == false)
            {
                SynthSettings.SetValue("allnotesignore", "1", RegistryValueKind.DWord);
                AllNotesIgnore.Checked = true;
                SysExIgnore.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("allnotesignore", "0", RegistryValueKind.DWord);
                AllNotesIgnore.Checked = false;
            }
        }

        private void DebugModePls_Click(object sender, EventArgs e)
        {
            if (DebugModePls.Checked == false)
            {
                SynthSettings.SetValue("debugmode", "1", RegistryValueKind.DWord);
                DebugModePls.Checked = true;
            }
            else
            {
                SynthSettings.SetValue("debugmode", "0", RegistryValueKind.DWord);
                DebugModePls.Checked = false;
            }
        }

        // Snap feature

        private const int SnapDist = 25;

        private bool DoSnap(int pos, int edge)
        {
            int delta = pos - edge;
            return delta > 0 && delta <= SnapDist;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Screen scn = Screen.FromPoint(this.Location);
            if (DoSnap(this.Left, scn.WorkingArea.Left)) this.Left = scn.WorkingArea.Left;
            if (DoSnap(this.Top, scn.WorkingArea.Top)) this.Top = scn.WorkingArea.Top;
            if (DoSnap(scn.WorkingArea.Right, this.Right)) this.Left = scn.WorkingArea.Right - this.Width;
            if (DoSnap(scn.WorkingArea.Bottom, this.Bottom)) this.Top = scn.WorkingArea.Bottom - this.Height;
        }

        private void ThemeCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (true)
                {
                    if (VisualStyleInformation.IsEnabledByUser == true)
                    {
                        if (CurrentTheme == 0)
                        {
                            CurrentTheme = 1;
                            this.Invoke(new MethodInvoker(delegate { List.BackColor = Color.White; }));
                            this.Invoke(new MethodInvoker(delegate { Settings.BackColor = Color.White; }));
                            this.Invoke(new MethodInvoker(delegate { this.Refresh(); }));
                        }
                    }
                    else
                    {
                        if (CurrentTheme == 1)
                        {
                            CurrentTheme = 0;
                            this.Invoke(new MethodInvoker(delegate { List.BackColor = SystemColors.Control; }));
                            this.Invoke(new MethodInvoker(delegate { Settings.BackColor = SystemColors.Control; }));
                            this.Invoke(new MethodInvoker(delegate { this.Refresh(); }));
                        }
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }
            catch
            {

            }
        }

        private void LoudMaxInstallMenu_Click(object sender, EventArgs e)
        {
            Functions.LoudMaxInstall();
        }

        private void LoudMaxUninstallMenu_Click(object sender, EventArgs e)
        {
            Functions.LoudMaxUninstall();
        }
    }
}