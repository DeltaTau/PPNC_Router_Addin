using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

using MCF;
//using MCF.CncSupport;
using MCF.CNC;

namespace PPNC_Router_Addin
{
    // CodeGroups will be instantiated by the main application at start-up if this line is added to "PowerPmacNC.ini"
    //      *** here we put the Dll in same dir as the hmi.exe
    //     [External Assemblies]
    //     CodeGroups="PPNC_Router_Addin.dll;PPNC_Router_Addin.CustomCodeGroups"

    public class CustomCodeGroups
    {
        // Member references:
        private FDevice controller;
        private FFile MainProgram;
        private String LogStartTime;
        private String LogEndTime;
        public IniFile theIniFile = new IniFile();
        private String OutfilePath = "C:\\Outfile\\";
        private String OutfileName = "Outfile.txt";
        private Boolean OutfileOk = true;
        private String MachineName = "Watoga";
        private String MachineModeM32 = "Unknown";
        private String IdleTimeSinceM32 = "0";
        private String OutfileDelimiter = ",";
        private String AvgFeedrateEnable = "0";
        private String AvgOverideEnable = "0";



        /*
        /// <summary>Custom G-code Group enumeration.</summary>
        public enum ECustomGGroup
        {
            /// <summary>No G-codes set</summary>
            None,
            /// <summary>Do first custom calculation</summary>
            [Description("G101 - Do first custom calculation")]
            G100,
            /// <summary>Do second custom calculation</summary>
            [Description("G102 - Do second custom calculation")]
            G101,
        };

        /// <summary>Custom M-code Group enumeration.</summary>
        public enum ECustomMGroup
        {
            /// <summary>Turn output OFF</summary>
            [Description("M200 - Turn output OFF")]
            M200,
            /// <summary>Turn output ON</summary>
            [Description("M201 - Turn output ON")]
            M201,
        };
         */

        /// <summary>Outfile Logging M-code Group enumeration.</summary>
        public enum EOutfileLoggingGroup
        {
            [Description("M31 - Outfile Logging Start")]
            M31,
            [Description("M32 - Outfile Logging Stop")]
            M32,
        };
        private FCodeGroup<EOutfileLoggingGroup> OutfileLoggingGroup;

        /// <summary>Hold Downs M-code Group enumeration.</summary>
        public enum EHoldDownsGroup
        {
            [Description("M41 - Roller Override ACTIVE")]
            M41,
            [Description("M42 - Roller Override CANCEL")]
            M42,
        };
        private FCodeGroup<EHoldDownsGroup> HoldDownsGroup;

        /// <summary>Locator Pins M-code Group enumeration.</summary>
        public enum ELocatorPinsGroup
        {
            [Description("M51 - Locator Pins Override ACTIVE")]
            M51,
            [Description("M52 - Loactor Pins Override CANCEL")]
            M52,
        };
        private FCodeGroup<ELocatorPinsGroup> LocatorPinsGroup;


        public CustomCodeGroups()
        {
            // Obtain references to the main application's objects, devices and members. (See MCF Dev Guide.)
            FCodeGroupFolder gcodes = (FCodeGroupFolder)FObject.GetRef("GCodes");
            FCodeGroupFolder mcodes = (FCodeGroupFolder)FObject.GetRef("MCodes");
            controller = FDevice.GetRef("Controller");

            MainProgram = FFile.GetRef("NcFile.MainProgram");
            MainProgram.Changed += MainProgram_Changed;  // Hint: tab twice after you type the "+=" for some VS magic!

            theIniFile.TheFile = g.ApplicationDirectory + @"\PowerPmacNC.ini";

            // Add G-code groups:
            // - the order of items below will determine the order they display in the Machine View list of Gcode Groups
            //new FCodeGroup<ECustomGGroup>(gcodes, controller, "CustomGGroup", "Custom G-code group", ECustomGGroup.None);

            // Add M-code groups:
            // - the order of items below will determine the order they display in the Machine View list of Mcode Groups
            //new FCodeGroup<ECustomMGroup>(mcodes, controller, "CustomMGroup", "Custom M-code group", ECustomMGroup.M200);
            OutfileLoggingGroup = new FCodeGroup<EOutfileLoggingGroup>(mcodes, controller, "OutfileLoggingGroup", "Locator Pins M-code group", EOutfileLoggingGroup.M31);
            HoldDownsGroup = new FCodeGroup<EHoldDownsGroup>(mcodes, controller, "HoldDownsGroup", "Hold Downs M-code group", EHoldDownsGroup.M41);
            LocatorPinsGroup = new FCodeGroup<ELocatorPinsGroup>(mcodes, controller, "LocatorPinsGroup", "Locator Pins M-code group", ELocatorPinsGroup.M51);

            // Handlers (if needed) to react to a G-Mcode group changing
            //HoldDownsGroup.Changed += HoldDownsGroup_Changed;  // add a Changed handler
            OutfileLoggingGroup.Changed += OutfileLoggingGroup_Changed;  // add a Changed handler

            //go to INI file to get user set parameters for Outfile processing
            // - these are in the "PowerPmacNC.ini" file on the main NC14 application
            // - these are in the section [Router_Addin] of Ini file
            OutfilePath = theIniFile.GetValue("Router_Addin", "OutfilePath", "C:\\Outfile\\");
            OutfileName = theIniFile.GetValue("Router_Addin", "OutfileName", "Outfile.txt");
            OutfileDelimiter = theIniFile.GetValue("Router_Addin", "OutfileDelimiter", ",");
            MachineName = theIniFile.GetValue("Router_Addin", "MachineName", "Watoga");
            AvgFeedrateEnable = theIniFile.GetValue("Router_Addin", "AvgFeedrateEnable", "1");
            AvgOverideEnable = theIniFile.GetValue("Router_Addin", "AvgOverideEnable", "1");


            //Key Down Handler defined
            g.theMachine.KeyDown += OnKeyDown;

        }

        void MainProgram_Changed(FMember member, MemberChangedEventArgs<string> e)
        {
            
        }

        private void OutfileLoggingGroup_Changed(FMember member, MemberChangedEventArgs<EOutfileLoggingGroup> e)
        {

            if (e.NewValue == EOutfileLoggingGroup.M31 && controller.DeviceStatus.Value == EDeviceStatus.Open)
            {
                LogStartTime = DateTime.Now.ToLongTimeString();
                IdleTimeSinceM32 = controller.SendCommand2("P858");
            }

            if (e.NewValue == EOutfileLoggingGroup.M32 && controller.DeviceStatus.Value == EDeviceStatus.Open && OutfileOk && LogStartTime != null)
            {
                // Append the current values of some P-vars to a CSV file each time that M42 becomes active
                bool fileNeedHeader = true;

                try
                {
                    if (!Directory.Exists(OutfilePath)) Directory.CreateDirectory(OutfilePath);
                    if (File.Exists(OutfilePath + OutfileName)) fileNeedHeader = false;

                    //using (StreamWriter sw = new StreamWriter("C:\\Outfile\\Outfile.txt", true))
                    using (StreamWriter sw = new StreamWriter(OutfilePath + OutfileName, true))
                    {
                        //here to add a log line to the outfile for the previous machine run
                        //
                        //we log:
                        //  Machine Name:  "Thermie"
                        //  Program Name:  currently loaded program name
                        //  Date:
                        //  Start Time:  time when M31 executed
                        //  End Time:    time when M32 executed
                        //  P850 Prog Cut Time:   G01,G02,G03 Accumulated
                        //  P851 Prog Cycle Time: time for entire program run - all moves,dwells,etc. - NOT In Feedhold
                        //  P852 Prog Total Time: cycle time + M0,M1, Stop(feedhold), Estop not included since would abort rewind
                        //  P853 Cycle Start Cntr: # times cycle start pressed
                        //  P854 Cycle Stop Cntr:  # times cycle Stop pressed
                        //  P855 Tool Change Cntr: # times tool change executed
                        //  P856 Prog Feedrate at M32:  current "F" value at M32 execution
                        //	P857 Prog Override at M32:  current feedrate oride at M32 execution
                        //  P858 Idle Time:  time since last M32
                        //  Auto or Manual: Auto if M32 from program, Manual if from Diag (?)

                        string path = g.ConvertToAbsolutePath(MainProgram.Value);
                        if (path.Length == 0) path = "Unknown";

                        string date = DateTime.Now.ToShortDateString();
                        LogEndTime = DateTime.Now.ToLongTimeString();

                        MachineModeM32 = controller.SendCommand2("P859");
                        switch (MachineModeM32)
                        {
                            case "0":
                                MachineModeM32 = "Auto";
                                break;
                            case "1":
                                MachineModeM32 = "Manual";
                                path = "Manual";
                                break;
                            case "2":
                                MachineModeM32 = "MDI";
                                path = "MDI";
                                break;
                            default:
                                MachineModeM32 = "Unknown";
                                break;
                        }

                        //here we either report the feed/oride at M32 OR the Average during program running
                        // - this is setup in the INI to enable avg or NOT
                        string reportFeedrate = controller.SendCommand2("P856");
                        if (AvgFeedrateEnable == "1") reportFeedrate = controller.SendCommand2("P860");
                        string reportOverride = controller.SendCommand2("P857");
                        if (AvgOverideEnable == "1") reportOverride = controller.SendCommand2("P861");


                        //sw.WriteLine(String.Format("{0},{1},{2}",
                        //controller.SendCommand2("P850"), controller.SendCommand2("P851"), controller.SendCommand2("P852")));
                        if (fileNeedHeader)
                        {
                            string feedLabel = "ProgFeed";
                            if (AvgFeedrateEnable == "1") feedLabel = "ProgFeedAvg";
                            string orideLabel = "Override";
                            if (AvgFeedrateEnable == "1") orideLabel = "OverrideAvg";

                            sw.WriteLine(String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}",
                            OutfileDelimiter,"MachName", "ProgName", "Date", "M31StartTm", "M32EndTm", "CutTm", "CycleTm", "RunTm", "NumStrts",
                            "NumStop", "NumToolChng", feedLabel, orideLabel, "IdleTm", "Auto/Manual/MDI"));
                        }

                        sw.WriteLine(String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}",
                            OutfileDelimiter, MachineName, path, date, LogStartTime, LogEndTime, controller.SendCommand2("P850"), controller.SendCommand2("P851"),
                            controller.SendCommand2("P852"), controller.SendCommand2("P853"), controller.SendCommand2("P854"),
                            controller.SendCommand2("P855"), reportFeedrate, reportOverride,
                            IdleTimeSinceM32, MachineModeM32));

                        controller.SendCommand2("P858=0");  //clear timer IdleTimeSinceM32
                    }
                }
                catch
                {
                    //here failed for some reason to create the Outfile, too bad, too sad.
                    LargeMessageBox.Show(String.Format("Failed to Write Outfile!\n Path: {0}{1}", OutfilePath, OutfileName),LargeMessageBoxImage.Error);
                    OutfileOk = false;
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.F1 && e.Key <= Key.F12)  // Fkeys, Ctrl+Fkeys, Shift+Fkeys
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))  // Ctrl+Fkeys
                {
                    g.Report(this, EReportType.Message, "User pressed Ctrl+" + e.Key.ToString());
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))  // Shift+Fkeys
                {
                    g.Report(this, EReportType.Message, "User pressed Shift+" + e.Key.ToString());
                }
                else  // Fkeys
                {
                    g.Report(this, EReportType.Message, "User pressed " + e.Key.ToString());
                    if (e.Key == Key.F3)
                        HoldDownsGroup.Value = EHoldDownsGroup.M42;
                }
            }

            if (e.Key == Key.System && e.SystemKey != Key.LeftAlt && e.SystemKey != Key.RightAlt)  // Alt+(any key)
            {
                g.Report(this, EReportType.Message, "User pressed Alt+" + e.SystemKey.ToString());
                if (e.SystemKey == Key.F3)
                    HoldDownsGroup.Value = EHoldDownsGroup.M41;
            }
        }


    }
}
