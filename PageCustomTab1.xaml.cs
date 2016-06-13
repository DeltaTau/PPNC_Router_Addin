using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MCF;
using PowerPmacNC;

namespace PPNC_Router_Addin
{
    // PageCustom will be instantiated by the main application at start-up if one of these lines is added to "PowerPmacNC.ini"
    //     [External Assemblies]
    //     UserPage="..\..\..\PPNC_Router_Addin\bin\Debug\PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"
    //     CustomTab="..\..\..\PPNC_Router_Addin\bin\Debug\PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"
    //     LeftCustomFrame="..\..\..\PPNC_Router_Addin\bin\Debug\PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"
    //     CenterCustomFrame="..\..\..\PPNC_Router_Addin\bin\Debug\PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"
    //     RightCustomFrame="..\..\..\PPNC_Router_Addin\bin\Debug\PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"

    // Note:  When distributing the binaries to your machines, you must NOT move "PPNC_Router_Addin.dll" to the main directory with
    // "PowerPmacNC.exe" because the custom library has its own "DeviceMembers.xml" file.  Instead, create a subdirectory for
    // the library and its dependencies, and change the lines in "PowerPmacNC.ini" to
    //     [External Assemblies]
    //     Object="PPNC_Router_Addin\PPNC_Router_Addin.dll;PPNC_Router_Addin.CustomObject"
    //     CenterCustomFrame="PPNC_Router_Addin\PPNC_Router_Addin.dll;PPNC_Router_Addin.PageCustom"
    // You do not need to put copies of the MCF libraries in this subdirectory because they already exist in the main directory.

    public partial class PageCustomTab1 : Page
    {
        // Member references:
        private FMember AxesDisplay;
        private FDeviceInteger Tool;  // Tool index
        private FDeviceInteger Tool_Number_In_Spindle;  // Tool Number NOW in Spindle.

        // Local variables:
        private Brush backgroundBrush2, buttonBrush, highlightButtonBrush;  // for Skins support

        public PageCustomTab1()
        {
            InitializeComponent();


            ttbVersion.Text = "PPNC_Router_Addin v0.50";

            // Register for translation all TextBlocks whose names begin with "ttb" and all "btn" Buttons.
            g.theTranslator.RegisterAllUIElements<TextBlock>(this, "ttb");
            g.theTranslator.RegisterAllUIElements<Button>(this, "btn");

            // Controls and strings may also be registered individually.  (See MCF Dev Guide.)
            g.theTranslator.RegisterText("You clicked the ENABLE button!");
            g.theTranslator.RegisterText("You clicked the DISABLE button!");

            backgroundBrush2 = (Brush)FindResource("BackgroundBrush2");  // for Skins support
            buttonBrush = (Brush)FindResource("ButtonBrush");
            highlightButtonBrush = (Brush)FindResource("HighlightButtonBrush");

            // Obtain references to objects, devices and members, and attach Changed handlers.  (See MCF Dev Guide.)
            // Note: Members cannot be created here!  Members must be created in FObject constructors where they will be
            // added to the Machine View hierarchy.  All members must exist before the WPF Pages are created.

            AxesDisplay = FMember.GetRef("Axes.Display");
            AxesDisplay.ChangedAsString += AxesDisplay_ChangedAsString;
            //FMember.AddHandler("Axes.Display", AxesDisplay_ChangedAsString);  // equivalent if you don't need the member reference

            Tool = FDeviceInteger.GetRef("Tool.Tool", this);
            Tool.Changed += Tool_Changed;
            FDeviceInteger.AddHandler("Tool.Tool", Tool_Changed);

            Tool_Number_In_Spindle = FDeviceInteger.GetRef("CustomObject.Tool_Number_In_Spindle", this);
            Tool_Number_In_Spindle.Changed += Tool_Number_In_Spindle_Changed;
            FDeviceInteger.AddHandler("CustomObject.Tool_Number_In_Spindle", Tool_Number_In_Spindle_Changed);

            tbToolCurrent.Text = String.Format("Current Tool in Spindle:  {0}", Tool.Value);
            tbToolNext.Text = String.Format("Next Tool (Active Tcode): {0}", Tool_Number_In_Spindle.Value);
            tbEditTool.KeyDown += tbEditTool_KeyDown;
        }

        void tbEditTool_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                int newToolNumber = 0;
                try
                {
                    newToolNumber = Convert.ToInt32(tbEditTool.Text);
                    if (newToolNumber > -1 && newToolNumber < 13)
                    {
                        //here for valid tool number
                        Tool_Number_In_Spindle.Value = newToolNumber;
                    }
                    else tbEditTool.Text = Tool_Number_In_Spindle.Value.ToString();

                }
                catch (Exception)
                {
                    tbEditTool.Text = Tool_Number_In_Spindle.Value.ToString();
                }
            }
        
        }

        private void Tool_Number_In_Spindle_Changed(FMember member, MemberChangedEventArgs<int> e)
        {
            // Tool Number NOW in Spindle.
            //MyTextBlock.Text = e.NewValue.ToString();
            //MyTextBlock.Text = member.DisplayValue;
            tbToolCurrent.Text = String.Format("Current Tool in Spindle: {0}", e.NewValue.ToString());
            tbEditTool.Text = Tool_Number_In_Spindle.Value.ToString();
        }

        private void Tool_Changed(FMember member, MemberChangedEventArgs<int> e)
        {
            // Tool index
            //MyTextBlock.Text = e.NewValue.ToString();
            //MyTextBlock.Text = member.DisplayValue;
            tbToolNext.Text = String.Format("Next Tool (Active Tcode): {0}", e.NewValue.ToString());

        }

        private void AxesDisplay_ChangedAsString(FMember member, MemberChangedEventArgs<string> e)
        {
            //tbAxesDisplay.Text = e.NewValue;
        }

        public void SetSkin(ResourceDictionary dictionary)
        {
            if (dictionary != null)
            {
                Resources.BeginInit();
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(dictionary);
                Resources.EndInit();
            }

            backgroundBrush2 = (Brush)FindResource("BackgroundBrush2");
            buttonBrush = (Brush)FindResource("ButtonBrush");
            highlightButtonBrush = (Brush)FindResource("HighlightButtonBrush");

            // these Changed event handlers set colors:
            //Enabled_Changed(null, new MemberChangedEventArgs<bool>(Enabled.Value, Enabled.Value));
        }
    }
}
