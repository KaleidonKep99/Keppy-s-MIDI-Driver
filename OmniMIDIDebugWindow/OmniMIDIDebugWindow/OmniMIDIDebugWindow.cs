﻿/*
 * 
 * OmniMIDI Debug Window
 * by KaleidonKep99
 * 
 * Full of potatoes
 *
 */

using System;
using System.Management;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.IO.Pipes;
using System.IO;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

namespace OmniMIDIDebugWindow
{
    public partial class OmniMIDIDebugWindow : Form
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA
           lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr hFindFile);

        // Topmost
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr Handle, IntPtr HandleInsertAfter, int PosX, int PosY, int SizeX, int SizeY, uint Flags);

        static readonly IntPtr TOPMOST = new IntPtr(-1);
        static readonly IntPtr NOTOPMOST = new IntPtr(-2);
        const UInt32 KEEPPOS = 2 | 1;

        // Voices
        UInt64[] CHs = new UInt64[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // Debug information
        private CultureInfo CultureTo = CultureInfo.CreateSpecificCulture("it-IT");
        string currentappreturn;
        string bitappreturn;

        // Required for KS
        Random RND = new Random();
        FileVersionInfo Driver { get; set; }
        RegistryKey Settings = Registry.CurrentUser.OpenSubKey("SOFTWARE\\OmniMIDI\\Configuration", false);
        RegistryKey WinVer = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", false);

        // Windows information
        ManagementObjectSearcher mosGPU = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_VideoController");
        ManagementObjectSearcher mosEnc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_Chassis");
        ComputerInfo CI = new ComputerInfo();
        string FullVersion;
        string bit;

        // CPU/GPU information
        string cpubit = "32";
        int cpuclock = 0;
        string cpumanufacturer = "Unknown";
        string cpuname = "Unknown";
        string gpuchip = "Unknown";
        string gpuname = "Unknown";
        string gpuver = "N/A";
        UInt32 gpuvram = 0;
        string enclosure = "Unknown";
        int coreCount = 0;

        public OmniMIDIDebugWindow()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true); // AAAAA I hate flickering
        }

        private string ParseEgg()
        {
            int ThisOne = RND.Next(0, Properties.Settings.Default.LeMessages.Count - 1);
            return Properties.Settings.Default.LeMessages[ThisOne];
        }

        private void KeppySynthDebugWindow_Load(object sender, EventArgs e)
        {
            try
            {
                Driver = FileVersionInfo.GetVersionInfo(Environment.SystemDirectory + "\\OmniMIDI.dll"); // Gets OmniMIDI version
                Text += String.Format(" (v{0})", Driver.FileVersion);
                VersionLabel.Text = String.Format("{0}", ParseEgg());
                GetWindowsInfoData();                   // Get info about your Windows installation
                SynthDbg.ContextMenu = MainCont;        // Assign ContextMenu (Not the strip one) to the tab
                ChannelVoices.ContextMenu = MainCont;   // Assign ContextMenu (Not the strip one) to the tab
                PCSpecs.ContextMenu = MainCont;         // Assign ContextMenu (Not the strip one) to the tab

                CheckMem.RunWorkerAsync();

                DebugInfo.Enabled = true;

                Program.ConnectToFirstAvailablePipe();
                DebugInfoCheck.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

        private string GetCurrentRAMUsage(UInt64 length)
        {
            string size;
            try
            {
                if (length >= 1099511627776)
                {
                    if (length < 10995116277760)
                        size = ((((length / 1024f) / 1024f) / 1024f) / 1024f).ToString("0.00 TB");
                    else
                        size = ((((length / 1024f) / 1024f) / 1024f) / 1024f).ToString("0.0 TB");
                }
                else if (length >= 1073741824)
                {
                    if (length < 10737418240)
                        size = (((length / 1024f) / 1024f) / 1024f).ToString("0.00 GB");
                    else
                        size = (((length / 1024f) / 1024f) / 1024f).ToString("0.0 GB");
                }
                else if (length >= 1048576)
                {
                    if (length < 10485760)
                        size = ((length / 1024f) / 1024f).ToString("0.00 MB");
                    else
                        size = ((length / 1024f) / 1024f).ToString("0.0 MB");
                }
                else if (length >= 1024)
                {
                    if (length < 10240)
                        size = (length / 1024f).ToString("0.00 KB");
                    else
                        size = (length / 1024f).ToString("0.0 KB");
                }
                else
                {
                        size = (length).ToString("0 B");
                }
            }
            catch { size = "-"; }

            if (length > 0) return size;
            else return "No usage";
        }

        private System.Drawing.Bitmap CPUImage()
        {
            if (cpumanufacturer == "GenuineIntel")
            {
                CPULogoTT.SetToolTip(CPULogo, "You're using an Intel CPU.");
                return Properties.Resources.intel;
            }
            else if (cpumanufacturer == "AuthenticAMD")
            {
                CPULogoTT.SetToolTip(CPULogo, "You're using an AMD CPU.");
                return Properties.Resources.amd;
            }
            else if (cpumanufacturer == "CentaurHauls" || cpumanufacturer == "VIA VIA VIA ")
            {
                CPULogoTT.SetToolTip(CPULogo, "You're using a VIA CPU.");
                return Properties.Resources.via;
            }
            else if (cpumanufacturer == "VMwareVMware")
            {
                CPULogoTT.SetToolTip(CPULogo, "You're running the app inside a VMware virtual machine.");
                return Properties.Resources.vmware;
            }
            else if (cpumanufacturer == " lrpepyh vr")
            {
                CPULogoTT.SetToolTip(CPULogo, "You're running the app inside a Parallels virtual machine.");
                return Properties.Resources.parallels;
            }
            else if (cpumanufacturer == "KVMKVMKVM" || cpumanufacturer.Contains("KVMKVMKVM"))
            {
                CPULogoTT.SetToolTip(CPULogo, "You're running the app inside a KVM.");
                return Properties.Resources.kvm;
            }
            else if (cpumanufacturer == "Microsoft Hv")
            {
                CPULogoTT.SetToolTip(CPULogo, "You're running the app inside a Hyper-V virtual machine.");
                return Properties.Resources.w8;
            }
            else
            {
                CPULogoTT.SetToolTip(CPULogo, "You're using an unknown CPU.");
                return Properties.Resources.unknown;
            }
        }

        private Bitmap WinImage()
        {
            OSInfo.OSVERSIONINFOEX osVersionInfo = new OSInfo.OSVERSIONINFOEX
            { dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSInfo.OSVERSIONINFOEX)) };

            if (!OSInfo.GetVersionEx(ref osVersionInfo))
            {
                WinLogoTT.SetToolTip(WinLogo, "You're using an unknown OS.");
                return Properties.Resources.unknown;
            }
            else
            {
                int p = (int)Environment.OSVersion.Platform;
                if ((p == 4) || (p == 6) || (p == 128))
                {
                    WinLogoTT.SetToolTip(WinLogo, "You're using an unknown OS.");
                    return Properties.Resources.other;
                }
                else
                {
                    if (Environment.OSVersion.Version.Major == 5 && (Environment.OSVersion.Version.Minor == 1 || Environment.OSVersion.Version.Minor == 2))
                    {
                        if (osVersionInfo.wProductType == OSInfo.VER_NT_SERVER)
                            WinLogoTT.SetToolTip(WinLogo, "You're using Windows Server 2003.");
                        else
                            WinLogoTT.SetToolTip(WinLogo, "You're using Windows XP.");

                        if (VisualStyleInformation.IsEnabledByUser)
                            return Properties.Resources.wxp;
                        else
                            return Properties.Resources.w9x;
                    }
                    if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0)
                    {
                        if (osVersionInfo.wProductType == OSInfo.VER_NT_SERVER)
                            WinLogoTT.SetToolTip(WinLogo, "You're using Windows Server 2008.");
                        else
                            WinLogoTT.SetToolTip(WinLogo, "You're using Windows Vista.");

                        if (VisualStyleInformation.IsEnabledByUser == true)
                            return Properties.Resources.wvista;
                        else
                            return Properties.Resources.w9x;
                    }
                    else if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                    {
                        if (osVersionInfo.wProductType == OSInfo.VER_NT_SERVER)
                            WinLogoTT.SetToolTip(WinLogo, "You're using Windows Server 2008 R2.");
                        else
                            WinLogoTT.SetToolTip(WinLogo, "You're using Windows 7.");

                        if (VisualStyleInformation.IsEnabledByUser == true)
                            return Properties.Resources.w7;
                        else
                            return Properties.Resources.w9x;
                    }
                    else if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2)
                    {
                        if (osVersionInfo.wProductType == OSInfo.VER_NT_SERVER) WinLogoTT.SetToolTip(WinLogo, "You're using Windows Server 2012.");
                        else WinLogoTT.SetToolTip(WinLogo, "You're using Windows 8.");

                        return Properties.Resources.w8;
                    }
                    else if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 3)
                    {
                        if (osVersionInfo.wProductType == OSInfo.VER_NT_SERVER) WinLogoTT.SetToolTip(WinLogo, "You're using Windows Server 2012 R2.");
                        else WinLogoTT.SetToolTip(WinLogo, "You're using Windows 8.1.");

                        return Properties.Resources.w8;
                    }
                    else if (Environment.OSVersion.Version.Major == 10)
                    {
                        if (osVersionInfo.wProductType == OSInfo.VER_NT_SERVER) WinLogoTT.SetToolTip(WinLogo, "You're using Windows Server.");
                        else WinLogoTT.SetToolTip(WinLogo, "You're using Windows 10.");

                        return Properties.Resources.w8;
                    }
                    else
                    {
                        WinLogoTT.SetToolTip(WinLogo, "You're using an unknown/unsupported OS.");
                        return Properties.Resources.other;
                    }
                }
            }
        }

        private string CPUArch(int Value)
        {
            switch (Value)
            {
                case 0:
                    return "x86";
                case 5:
                    return "ARM";
                case 6:
                    return "IA64";
                case 9:
                    return "AMD64";
                default:
                    return "UNKNOWN";
            }
        }

        private void GetWindowsInfoData()
        {
            try
            {
                String Frequency = "";

                try
                {
                    // Get CPU info
                    using (var managementObject = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
                    {
                        cpuclock = int.Parse(managementObject["MaxClockSpeed"].ToString());
                        cpubit = CPUArch(int.Parse(managementObject["Architecture"].ToString()));
                        cpuname = managementObject["Name"].ToString();
                        cpumanufacturer = managementObject["Manufacturer"].ToString();
                        coreCount += int.Parse(managementObject["NumberOfCores"].ToString());
                    }
                }
                catch
                {
                    cpuclock = 0;
                    cpubit = "Unknown";
                    cpuname = "Unknown CPU";
                    cpumanufacturer = "Unknown manufacturer";
                    coreCount += Environment.ProcessorCount;
                }

                try
                {
                    // Get GPU info
                    foreach (ManagementObject moGPU in mosGPU.Get())
                    {
                        gpuchip = moGPU["VideoProcessor"].ToString();
                        gpuname = moGPU["Name"].ToString();
                        gpuvram = Convert.ToUInt32(moGPU["AdapterRAM"]);
                        gpuver = moGPU["DriverVersion"].ToString();
                    }
                }
                catch
                {
                    gpuchip = "Error while parsing info for GPU";
                    gpuname = "N/A";
                    gpuvram = 0;
                    gpuver = "N/A";
                }

                // Get enclosure info
                foreach (ManagementObject moEnc in mosEnc.Get())
                {
                    foreach (int i in (UInt16[])(moEnc["ChassisTypes"]))
                        enclosure = OSInfo.Chassis[i];
                }

                FullVersion = String.Format("{0}.{1}.{2}{3}",
                         Environment.OSVersion.Version.Major.ToString(), Environment.OSVersion.Version.Minor.ToString(),
                         Environment.OSVersion.Version.Build.ToString(),
                         // If using Windows 10, get UBR too
                         (Environment.OSVersion.Version.Major == 10) ? String.Format(".{0}", WinVer.GetValue("UBR", 0).ToString()) : null);

                WinLogo.Image = WinImage();
                CPULogo.Image = CPUImage();

                switch (Program.GetProcessorArchitecture())
                {
                    case Program.PROCESSOR_ARCHITECTURE_AMD64:
                        bit = "AMD64";
                        break;
                    case Program.PROCESSOR_ARCHITECTURE_ARM64:
                        bit = "ARM64";
                        break;
                    case Program.PROCESSOR_ARCHITECTURE_IA64:
                        bit = "IA64";
                        break;
                    case Program.PROCESSOR_ARCHITECTURE_INTEL:
                        bit = "i386";
                        break;
                    default:
                        bit = "UNK";
                        break;
                }

                if (cpuclock < 1000)
                    Frequency = String.Format("{0}MHz", cpuclock);
                else
                    Frequency = String.Format("{0}GHz", ((float)cpuclock / 1000).ToString("0.00"));

                COS.Text = String.Format("{0} ({1}, {2})", OSInfo.Name.Replace("Microsoft ", ""), FullVersion, bit);
                CPU.Text = String.Format("{0} ({1} architecture)", cpuname, cpubit);
                CPUInfo.Text = String.Format("{0}, {1} cores, {2} threads, {3} ({4}MHz)", cpumanufacturer, coreCount, Environment.ProcessorCount, Frequency, cpuclock);
                GPU.Text = gpuname;
                GPUInternalChip.Text = gpuchip;
                GPUInfo.Text = String.Format("{0}MB VRAM, driver version {1}", (gpuvram / 1048576), gpuver);
                MT.Text = enclosure;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OpenAppLocat_Click(object sender, EventArgs e) // Opens the directory of the current app that's using OmniMIDI
        {
            try { Process.Start(Path.GetDirectoryName(CurrentApp)); }
            catch { }
        }

        private void CopyToClipBoardCmd() // Copies content of window to clipboard
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(String.Format("OmniMIDI version {0}", Driver.FileVersion));
                sb.AppendLine("========= Debug information =========");
                sb.AppendLine(String.Format("Driver version: {0}", Driver.FileVersion));
                sb.AppendLine(String.Format("{0} {1}", CMALabel.Text, CMA.Text));
                sb.AppendLine(String.Format("{0} {1}", AVLabel.Text, AV.Text));
                sb.AppendLine(String.Format("{0} {1}", HeadsPosLabel.Text, HeadsPos.Text));
                sb.AppendLine(String.Format("{0} {1}", RTLabel.Text, RT.Text));
                sb.AppendLine(String.Format("{0} {1}", Latency.Text, LatencyLabel.Text));
                sb.AppendLine(String.Format("{0} {1}", RAMUsageVLabel.Text, RAMUsageV.Text));
                sb.AppendLine(String.Format("{0} {1}", HCountVLabel.Text, HCountV.Text));
                sb.AppendLine(String.Format("{0} {1}", KDMAPILabel.Text, KDMAPI.Text));
                sb.AppendLine("======= Channels  information =======");
                sb.AppendLine(String.Format("{0} {1}", CHV1L.Text, CHV1.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV2L.Text, CHV2.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV3L.Text, CHV3.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV4L.Text, CHV4.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV5L.Text, CHV5.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV6L.Text, CHV6.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV7L.Text, CHV7.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV8L.Text, CHV8.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV9L.Text, CHV9.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV10L.Text, CHV10.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV11L.Text, CHV11.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV12L.Text, CHV12.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV13L.Text, CHV13.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV14L.Text, CHV14.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV15L.Text, CHV15.Text));
                sb.AppendLine(String.Format("{0} {1}", CHV16L.Text, CHV16.Text));
                sb.AppendLine("======== System  information ========");
                sb.AppendLine(String.Format("Driver version: {0}", Driver.FileVersion));
                sb.AppendLine(String.Format("{0} {1}", COSLabel.Text, COS.Text));
                sb.AppendLine(String.Format("{0} {1}", CPULabel.Text, CPU.Text));
                sb.AppendLine(String.Format("{0} {1}", CPUInfoLabel.Text, CPUInfo.Text));
                sb.AppendLine(String.Format("{0} {1}", GPULabel.Text, GPU.Text));
                sb.AppendLine(String.Format("{0} {1}", GPUInternalChipLabel.Text, GPUInternalChip.Text));
                sb.AppendLine(String.Format("{0} {1}", GPUInfoLabel.Text, GPUInfo.Text));
                sb.AppendLine(String.Format("{0} {1}", TMLabel.Text, TM.Text));
                sb.AppendLine(String.Format("{0} {1}", AMLabel.Text, AM.Text));
                sb.AppendLine(String.Format("{0} {1}", MTLabel.Text, MT.Text));

                Thread thread = new Thread(() => Clipboard.SetText(sb.ToString())); // Creates another thread, otherwise the form locks up while copying the richtextbox
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            finally
            {
                MessageBox.Show("Info copied to clipboard.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Done, now get out
            }
        }

        private void CopyToClip_Click(object sender, EventArgs e) // Allows you to copy the content of the RichTextBox to clipboard
        {
            CopyToClipBoardCmd();
        }

        private void Exit_Click(object sender, EventArgs e) // Exit? lel
        {
            Application.ExitThread(); // R.I.P. debug
        }

        private void UpdateActiveVoicesPerChannel(String Value, Boolean ClosingPipe)
        {
            int x;

            if (ClosingPipe) for (x = 0; x <= 15; ++x) CHs[x] = 0;
            else for(x = 0; x <= 15; ++x) if (!ReadPipeUInt64(Value, String.Format("CV{0}", x), ref CHs[x]));
        }

        private string GetActiveVoices()
        {
            try
            {
                return String.Format("{0}", (CHs[0] + CHs[1] + CHs[2] + CHs[3] + CHs[4] + CHs[5] + CHs[6] + CHs[7] + CHs[8] + CHs[9] + CHs[10] + CHs[11] + CHs[12] + CHs[13] + CHs[14] + CHs[15]));
            }
            catch { return "0"; }
        }

        private string DebugName(string value)
        {
            int A = value.IndexOf(" = ");
            if (A == -1) return "";
            return value.Substring(0, A);
        }

        private string DebugValue(string value)
        {
            int A = value.LastIndexOf(" = ");
            if (A == -1) return "";
            int A2 = A + (" = ").Length;
            if (A2 >= value.Length) return "";
            return value.Substring(A2);
        }

        private bool ReadPipeBoolean(String Value, String RequestedValue, ref Int32 ValueToChange)
        {
            try
            {
                if (DebugName(Value).Equals(RequestedValue)) ValueToChange = Convert.ToInt32(DebugValue(Value));
                return true;
            }
            catch { return false; }
        }

        private bool ReadPipeString(String Value, String RequestedValue, ref String ValueToChange)
        {
            try
            {
                if (DebugName(Value).Equals(RequestedValue)) ValueToChange = DebugValue(Value); 
                return true;
            }
            catch { return false; }
        }

        private bool ReadPipeSingle(String Value, String RequestedValue, ref Single ValueToChange)
        {
            try
            {
                if (DebugName(Value).Equals(RequestedValue)) ValueToChange = (Single.Parse(DebugValue(Value), NumberStyles.Any, CultureTo) / 1000000.0f);
                return true;
            }
            catch { return false; }
        }

        private bool ReadPipeDouble(String Value, String RequestedValue, ref Double ValueToChange)
        {
            try
            {
                if (DebugName(Value).Equals(RequestedValue)) ValueToChange = (Double.Parse(DebugValue(Value), NumberStyles.Any, CultureTo) / 1000000.0);
                return true;
            }
            catch { return false; }
        }

        private bool ReadPipeUInt64(String Value, String RequestedValue, ref UInt64 ValueToChange)
        {
            try
            {
                if (DebugName(Value).Equals(RequestedValue)) ValueToChange = Convert.ToUInt64(DebugValue(Value));
                return true;
            }
            catch { return false; }
        }

        String CurrentApp = "None";
        String BitApp = "N/A";
        Single CurCPU = 0.0f;
        UInt64 AudioBufSize = 0;
        UInt64 Handles = 0;
        UInt64 RAMUsage = 0;
        UInt64 SFsList = 0;
        UInt64 BufferSize = 0;
        UInt64 ReadHead = 0;
        UInt64 WriteHead = 0;
        Int32 KDMAPIStatus = 0;
        Int32 KDMAPIViaWinMM = 0;
        Double AudioLatency = 0.0f;
        private void ParseInfoFromPipe(StreamReader StreamDebugReader, Boolean ClosingPipe)
        {
            try
            {
                if (!ClosingPipe)
                {
                    String LN = StreamDebugReader.ReadLine();

                    if (String.IsNullOrEmpty(LN))
                        return;

                    String[] STRs = LN.Split(new char[] { '|' });
                    foreach (String STR in STRs)
                    {
                        if (!ReadPipeString(STR, "CurrentApp", ref CurrentApp));
                        if (!ReadPipeString(STR, "BitApp", ref BitApp));
                        if (!ReadPipeSingle(STR, "CurCPU", ref CurCPU));
                        if (!ReadPipeUInt64(STR, "AudioBufSize", ref AudioBufSize));
                        if (!ReadPipeUInt64(STR, "Handles", ref Handles));
                        if (!ReadPipeUInt64(STR, "RAMUsage", ref RAMUsage));
                        if (!ReadPipeUInt64(STR, "EVBufferSize", ref BufferSize));
                        if (!ReadPipeUInt64(STR, "EVReadHead", ref ReadHead));
                        if (!ReadPipeUInt64(STR, "EVWriteHead", ref WriteHead));
                        if (!ReadPipeBoolean(STR, "OMDirect", ref KDMAPIStatus));
                        if (!ReadPipeBoolean(STR, "WinMMKDMAPI", ref KDMAPIViaWinMM));
                        if (!ReadPipeDouble(STR, "AudioLatency", ref AudioLatency));
                        if (!ReadPipeUInt64(STR, "SFsList", ref SFsList));
                        UpdateActiveVoicesPerChannel(STR, ClosingPipe);
                    }
                }
                else
                {
                    CurrentApp = "None";
                    BitApp = "N/A";
                    CurCPU = 0.0f;
                    Handles = 0;
                    RAMUsage = 0;
                    KDMAPIStatus = 0;
                    AudioLatency = 0.0f;
                    UpdateActiveVoicesPerChannel(null, ClosingPipe);
                }
            }
            catch (Exception ex)
            {
                // If something goes wrong, here's an error handler
                MessageBox.Show(ex.ToString() + "\n\nPress OK to stop the debug mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.ExitThread();
            }
        }

        private void GetInfo()
        {
            this.DebugInfo.Interval = 1;
            if (Tabs.SelectedIndex == 0)
            {
                // Time to write all the stuff to the string builder
                if (Path.GetFileName(CurrentApp) == "0")
                {
                    OpenAppLocat.Enabled = false;
                    currentappreturn = "Nothing";
                }
                else currentappreturn = System.IO.Path.GetFileName(CurrentApp);

                if (BitApp == "0") bitappreturn = "...";
                else bitappreturn = BitApp;

                HCountV.Text = String.Format("{0} handles", Handles);
                RAMUsageV.Text = GetCurrentRAMUsage(RAMUsage);
                CMA.Text = String.Format("{0} ({1})", currentappreturn, bitappreturn); // Removes garbage characters

                Int32 AVColor = (int)Math.Round((double)(100 * Convert.ToInt32(GetActiveVoices())) / Convert.ToInt32(Settings.GetValue("MaxVoices", "512")));

                if (Convert.ToInt32(GetActiveVoices()) > Convert.ToInt32(Settings.GetValue("MaxVoices", "512")))
                    AV.Font = new Font(AV.Font, FontStyle.Bold);
                else
                    AV.Font = new Font(AV.Font, FontStyle.Regular);

                AV.ForeColor = ValueBlend.GetBlendedColor(AVColor.LimitIntToRange(0, 100));
                AV.Text = GetActiveVoices();

                // Magic
                String MagicSauce = String.Format("D{0}", BufferSize.ToString().Length);
                HeadsPos.Text = String.Format("{0}/{1} ({2})", ReadHead.ToString(MagicSauce), WriteHead.ToString(MagicSauce), BufferSize);

                if (Convert.ToInt32(Settings.GetValue("CurrentEngine", "3")) == 0)
                {
                    RT.Font = new System.Drawing.Font(RT.Font, System.Drawing.FontStyle.Italic);
                    RT.Text = "Unavailable"; // If BASS is in encoding mode, BASS usage will stay at constant 100%.
                }
                else
                {
                    Int32 RTColor = (int)Math.Round((double)(100 * CurCPU) / Convert.ToInt32(Settings.GetValue("MaxRenderingTime", "75")));

                    if ((CurCPU > Convert.ToInt32(Settings.GetValue("MaxRenderingTime", "75"))) && (Convert.ToInt32(Settings.GetValue("MaxRenderingTime", "75")) != 0))
                    {
                        RT.Font = new System.Drawing.Font(RT.Font, System.Drawing.FontStyle.Bold);
                        RT.Text = String.Format("{0}% (Beyond limit!)", CurCPU.ToString("0.0"), Settings.GetValue("MaxRenderingTime", "75").ToString());
                    }
                    else
                    {
                        RT.Font = new System.Drawing.Font(RT.Font, System.Drawing.FontStyle.Regular);
                        RT.Text = String.Format("{0}%", CurCPU.ToString("0.0")); // Else, it'll give you the info about how many cycles it needs to work.
                    }

                    RT.ForeColor = ValueBlend.GetBlendedColor(RTColor.LimitIntToRange(0, 100));
                }

                Boolean IsDX = (Convert.ToInt32(Settings.GetValue("CurrentEngine", "3")) == 1);
                Latency.Text = (Handles > 0) ? String.Format("{0}ms{1}", AudioLatency.ToString("0.00"), IsDX ? null : String.Format(" ({0} frames)", AudioBufSize)) : "Unavailable";

                if (KDMAPIStatus == 0)
                    KDMAPI.Text = (Handles > 0) ? "Disabled, using WinMM" : "Unavailable";
                else
                {
                    KDMAPI.Text = String.Format("Enabled, using {0}", Convert.ToBoolean(KDMAPIViaWinMM) ? "WinMMWRP" : "KDMAPI");
                }

                CurSFsList.Text = (SFsList != 0) ? String.Format("List {0}", SFsList) : "Unavailable";

                /*
                WIP
                if (BufferOverload == 0)
                {
                    BufStatus.ForeColor = (Handles > 0) ? Color.FromArgb(32, 150, 0) : Color.Black;
                    BufStatus.Font = new Font(BufStatus.Font, FontStyle.Regular);
                    BufStatus.Text = (Handles > 0) ? "Healthy." : "Unavailable.";
                }
                else
                {
                    BufStatus.ForeColor = Color.FromArgb(209, 0, 31);
                    BufStatus.Font = new Font(BufStatus.Font, FontStyle.Bold);
                    if (Convert.ToInt32(Settings.GetValue("vms2emu", "0")) == 0)
                        BufStatus.Text = "Full, skipping notes.";
                    else
                        BufStatus.Text = "Full, slowing down playback.";
                }
                WIP 
                */
            }
            else if (Tabs.SelectedIndex == 1)
            {
                String FormatForVoices = "{0} voices";
                CHV1.Text = String.Format(FormatForVoices, CHs[0]);
                CHV2.Text = String.Format(FormatForVoices, CHs[1]);
                CHV3.Text = String.Format(FormatForVoices, CHs[2]);
                CHV4.Text = String.Format(FormatForVoices, CHs[3]);
                CHV5.Text = String.Format(FormatForVoices, CHs[4]);
                CHV6.Text = String.Format(FormatForVoices, CHs[5]);
                CHV7.Text = String.Format(FormatForVoices, CHs[6]);
                CHV8.Text = String.Format(FormatForVoices, CHs[7]);
                CHV9.Text = String.Format(FormatForVoices, CHs[8]);
                CHV10.Text = String.Format(FormatForVoices, CHs[9]);
                CHV11.Text = String.Format(FormatForVoices, CHs[10]);
                CHV12.Text = String.Format(FormatForVoices, CHs[11]);
                CHV13.Text = String.Format(FormatForVoices, CHs[12]);
                CHV14.Text = String.Format(FormatForVoices, CHs[13]);
                CHV15.Text = String.Format(FormatForVoices, CHs[14]);
                CHV16.Text = String.Format(FormatForVoices, CHs[15]);
            }
            else if (Tabs.SelectedIndex == 2)
            {
                this.DebugInfo.Interval = 500;
                AM.Text = String.Format("{0} ({1:0.#}%, {2} bytes)", (avmemint + "MB").ToString(), Math.Round(percentage, 1).ToString(), avmem.ToString("N0", CultureInfo.GetCultureInfo("de")));
            }
        }

        private void DebugInfo_Tick(object sender, EventArgs e)
        {
            try
            {
                GetInfo();
            }
            catch (Exception ex)
            {
                // If something goes wrong, here's an error handler
                MessageBox.Show(ex.ToString() + "\n\nPress OK to stop the debug mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.ExitThread();
            }
        }

        private void SelectDebugPipe_Click(object sender, EventArgs e)
        {
            using (var SP = new SelectPipe())
            {
                var result = SP.ShowDialog();
                if (result == DialogResult.OK)
                    SwitchPipe(SP.SelectedPipe);
            }
        }

        private void SwitchPipe(int pipe)
        {
            try
            {
                if (Program.DoesPipeStillExist(pipe))
                {
                    Program.SelectedDebugVal = pipe;
                    if (!DebugInfoCheck.IsBusy) DebugInfoCheck.RunWorkerAsync();
                    else DebugInfoCheck.CancelAsync();
                }
                else MessageBox.Show("This debug pipe is not available anymore.", "OmniMIDI - Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }

        NamedPipeClientStream PipeClient = null;
        private void DebugInfoCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Program.DoesPipeStillExist(Program.SelectedDebugVal))
            {
                using (PipeClient = new NamedPipeClientStream(".", String.Format("OmniMIDIDbg{0}", Program.SelectedDebugVal), PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    PipeClient.Connect();

                    if (PipeClient.IsConnected)
                    {
                        using (StreamReader StreamDebugReader = new StreamReader(PipeClient, Encoding.Unicode))
                        {
                            try
                            {
                                while (PipeClient.IsConnected)
                                {
                                    if (DebugInfoCheck.CancellationPending) break;
                                    ParseInfoFromPipe(StreamDebugReader, false);
                                    Thread.Sleep(1);
                                }
                                DebugInfoCheck.CancelAsync();
                            }
                            catch (Exception ex)
                            {
                                // If something goes wrong, here's an error handler
                                MessageBox.Show(ex.ToString() + "\n\nPress OK to stop the debug mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Application.ExitThread();
                            }
                        }
                    }
                    else DebugInfoCheck.CancelAsync();
                }
            }
            Thread.Sleep(100);
        }

        private void DebugInfoCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ParseInfoFromPipe(null, true);
            DebugInfoCheck.RunWorkerAsync();
        }

        private void OpenConfigurator_Click(object sender, EventArgs e)
        {
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\OmniMIDI\\OmniMIDIConfigurator.exe");
        }

        int paintReps = 0;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Thread.Sleep(1);

            if (paintReps++ % 500 == 0)
                Application.DoEvents();
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

        private void DebugWinTop_Click(object sender, EventArgs e)
        {
            if (DebugWinTop.Checked)
            {
                DebugWinTop.Checked = false;
                SetWindowPos(this.Handle, NOTOPMOST, 0, 0, 0, 0, KEEPPOS);
            }
            else
            {
                DebugWinTop.Checked = true;
                SetWindowPos(this.Handle, TOPMOST, 0, 0, 0, 0, KEEPPOS);
            }
        }

        static ulong avmem = 0;
        static ulong tlmem = 0;
        static ulong avmemint = 0;
        static ulong tlmemint = 0;
        static double percentage = 0.0;
        private void CheckMem_DoWork(object sender, DoWorkEventArgs e)
        {
            CI = new ComputerInfo();
            tlmem = CI.TotalPhysicalMemory;
            tlmemint = tlmem / (1024 * 1024);
            TM.Text = String.Format("{0} ({1} bytes)", (tlmemint + "MB").ToString(), tlmem.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de")));
            while (CI != null)
            {
                avmem = CI.AvailablePhysicalMemory;
                avmemint = avmem / (1024 * 1024);
                percentage = avmem * 100.0 / tlmem;
                Thread.Sleep(DebugInfo.Interval);
            }
        }

        private void KSLogo_Click(object sender, EventArgs e)
        {
            VersionLabel.Text = String.Format("{0}", ParseEgg());
        }
    }
}

public static class ValueBlend
{
    public static Color GetBlendedColor(int percentage)
    {
        if (percentage < 50)
            return Interpolate(Color.FromArgb(32, 150, 0), Color.FromArgb(175, 111, 0), percentage / 50.0);
        return Interpolate(Color.FromArgb(175, 111, 0), Color.FromArgb(209, 0, 31), (percentage - 50) / 50.0);
    }

    private static Color Interpolate(Color color1, Color color2, double fraction)
    {
        double r = Interpolate(color1.R, color2.R, fraction);
        double g = Interpolate(color1.G, color2.G, fraction);
        double b = Interpolate(color1.B, color2.B, fraction);
        return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
    }

    private static double Interpolate(double d1, double d2, double fraction)
    {
        return d1 + (d2 - d1) * fraction;
    }
}

public static class InputExtensions
{
    public static int LimitIntToRange(
        this int value, int inclusiveMinimum, int inclusiveMaximum)
    {
        if (value < inclusiveMinimum) { return inclusiveMinimum; }
        if (value > inclusiveMaximum) { return inclusiveMaximum; }
        return value;
    }

    public static double LimitDoubleToRange(
    this double value, double inclusiveMinimum, double inclusiveMaximum)
    {
        if (value < inclusiveMinimum) { return inclusiveMinimum; }
        if (value > inclusiveMaximum) { return inclusiveMaximum; }
        return value;
    }
}