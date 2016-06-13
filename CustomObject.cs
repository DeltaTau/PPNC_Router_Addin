using System;
using System.Windows.Media.Imaging;
using MCF;

namespace PPNC_Router_Addin
{
    // CustomObject will be instantiated by the main application at start-up if this line is added to "PowerPmacNC.ini"
    //     [External Assemblies]
    //     Object="PPNC_Router_Addin.dll;PPNC_Router_Addin.CustomObject"

    // Note:  When distributing the binaries to your machines, you must NOT move "PPNC_Router_Addin.dll" to the main directory with
    // "PowerPmacNC.exe" because the custom library has its own "DeviceMembers.xml" file.  Instead, create a subdirectory for
    // the library and its dependencies, and change the lines in "PowerPmacNC.ini" to
    //     [External Assemblies]
    //     Object="PPNC_Router_Addin.dll;PPNC_Router_Addin.CustomObject"
    //     CenterCustomFrame="PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"
    // You do not need to put copies of the MCF libraries in this subdirectory because they already exist in the main directory.

    public class CustomObject : FObject
    {
        // Member references:
        private FDevice theController;
        private FMember AxesDisplay;

        public CustomObject(FObject Owner)
            : base(Owner, "CustomObject")
        {
            //DisplayName = "Custom Object";
            DisplayName = "CNC Router";

            Description = "Use this Custom Object as a starting point for your development.";
            Image = new BitmapImage(new Uri("/PPNC_Router_Addin;component/Images/stingray.png", UriKind.Relative));  // 24x24 PNG

            // Obtain references to the main application's objects, devices and members, and attach Changed handlers.
            // (See MCF Dev Guide.)
            theController = FDevice.GetRef("Controller");
            AxesDisplay = FMember.GetRef("Axes.Display");
            AxesDisplay.ChangedAsString += AxesDisplay_ChangedAsString;

            

            // Device members created here will appear in Machine View in the main application.  (See MCF Dev Guide.)
            // FDeviceBoolean, FDeviceInteger, FDeviceFloat, FDeviceDouble, FDeviceString, FDeviceList<T>, FDeviceFile.
            // The PMAC variables and update rates are specified in this project's "DeviceMembers.xml" file.

            var ToolNumberInSpindle = new FDeviceInteger(this, theController, "Tool Number In Spindle", "P821", 500, EMemberCategory.Setting)
            {
                Description = "Tool Number NOW in Spindle."
            };

            var PowerPendantEnabled = new FDeviceBoolean(this, theController, "PowerPendantEnabled", "P820", 500, EMemberCategory.Setting)
            {
                Description = "This boolean highlights one of the two buttons on \"Custom Page\"."
            };

            var XaxisParkPosition = new FDeviceDouble(this, theController, "XAxisParkPositionInch", "P822", 500, EMemberCategory.Setting)
            {
                Description = "X Axis Park Position in Absolute Machine Inches."
            };
            var YaxisParkPosition = new FDeviceDouble(this, theController, "YAxisParkPositionInch", "P823", 500, EMemberCategory.Setting)
            {
                Description = "Y Axis Park Position in Absolute Machine Inches."
            };
            var ZaxisParkPosition = new FDeviceDouble(this, theController, "ZAxisParkPositionInch", "P824", 500, EMemberCategory.Setting)
            {
                Description = "Z Axis Park Position in Absolute Machine Inches."
            };
            var XaxisParkSpeed = new FDeviceDouble(this, theController, "XAxisParkSpeedIpm", "P825", 500, EMemberCategory.Setting)
            {
                Description = "X Axis Park Speed (In/Min)"
            };
            var YaxisParkSpeed = new FDeviceDouble(this, theController, "YAxisParkSpeedIpm", "P826", 500, EMemberCategory.Setting)
            {
                Description = "Y Axis Park Speed (In/Min)"
            };
            var ZaxisParkSpeed = new FDeviceDouble(this, theController, "ZAxisParkSpeedIpm", "P827", 500, EMemberCategory.Setting)
            {
                Description = "Z Axis Park Speed (In/Min)"
            };
            var XAwayPos = new FDeviceDouble(this, theController, "XAwayPosInch", "P828", 500, EMemberCategory.Setting)
            {
                Description = "X Axis Away from Park Inches (Pins Go Down)"
            };
            var SheetZeroOffset = new FDeviceDouble(this, theController, "SheetZeroOffsetInch", "P829", 500, EMemberCategory.Setting)
            {
                Description = "X Distance to Start of Sheet (Inches)"
            };
            var SheetLength = new FDeviceDouble(this, theController, "SheetLengthInch", "P830", 500, EMemberCategory.Setting)
            {
                Description = "Sheet Length Default (Inches)"
            };

            var Roller1MinPos = new FDeviceDouble(this, theController, "Roller1MinPosInch", "P831", 500, EMemberCategory.Setting)
            {
                Description = "Roller 1 Min Position Inches"
            };
            var Roller1MaxPos = new FDeviceDouble(this, theController, "Roller1MaxPos", "P832", 500, EMemberCategory.Setting)
            {
                Description = "Roller 1 Max Position Inches"
            };
            var Roller2MinPos = new FDeviceDouble(this, theController, "Roller2MinPos", "P833", 500, EMemberCategory.Setting)
            {
                Description = "Roller 2 Min Position Inches"
            };
            var Roller2MaxPos = new FDeviceDouble(this, theController, "Roller2MaxPos", "P834", 500, EMemberCategory.Setting)
            {
                Description = "Roller 2 Max Position Inches"
            };            
            var Roller3MinPos = new FDeviceDouble(this, theController, "Roller3MinPos", "P835", 500, EMemberCategory.Setting)
            {
                Description = "Roller 3 Min Position Inches"
            };
            var Roller3MaxPos = new FDeviceDouble(this, theController, "Roller3MaxPos", "P836", 500, EMemberCategory.Setting)
            {
                Description = "Roller 3 Max Position Inches"
            };
            var Roller4MinPos = new FDeviceDouble(this, theController, "Roller4MinPos", "P837", 500, EMemberCategory.Setting)
            {
                Description = "Roller 4 Min Position Inches"
            };
            var Roller4MaxPos = new FDeviceDouble(this, theController, "Roller4MaxPos", "P838", 500, EMemberCategory.Setting)
            {
                Description = "Roller 4 Max Position Inches"
            };


        }

        private void AxesDisplay_ChangedAsString(FMember member, MemberChangedEventArgs<string> e)
        {
            //if (e.NewValue != e.OldValue)
                //g.Report(this, EReportType.InformationAlarm, "Axes Display has been changed from {0} to {1}.", e.OldValue, e.NewValue);
        }
    }
}
