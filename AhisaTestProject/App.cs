using Autodesk.Revit.UI;

namespace AhisaTestProject
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            // 1. Set up ribbon tab and panel names
            string tabName = "Ahisa's Tab";
            string panelName = "Project Cleaner";

            // 1. Create ribbon tab
            try { app.CreateRibbonTab(tabName); }
            catch { } // Ignore if the tab already exists

            // 2. Create ribbon panel 
            RibbonPanel panel = Common.Utils.CreateRibbonPanel(app, tabName, panelName);



            // 3. Create button data instances
            PushButtonData btnData1 = Command1.GetButtonData();
            PushButtonData btnData2 = Command2.GetButtonData();
            PushButtonData btnData2B = Command2B.GetButtonData();
            PushButtonData btnData3 = Command3.GetButtonData();
            PushButtonData btnData4 = Command4.GetButtonData();
            PushButtonData btnData5 = Command5.GetButtonData();

            // Create the pull-down button
            PulldownButtonData pullDownData = new PulldownButtonData("Group Actions", "Groups");
            pullDownData.Image = btnData1.Image; // Set initial image (optional)
            pullDownData.LargeImage = btnData1.LargeImage;

            PulldownButton pullDownButton = panel.AddItem(pullDownData) as PulldownButton;

            // Add the buttons with separators
            pullDownButton.AddPushButton(btnData2);
            pullDownButton.AddPushButton(btnData1);
            pullDownButton.AddPushButton(btnData2B);


            //pullDownButton.AddSeparator(); // <-- Adds a separator line
            //pullDownButton.AddPushButton(btnData1);
            //
            PushButton myButton3 = panel.AddItem(btnData3) as PushButton;
            PushButton myButton5 = panel.AddItem(btnData5) as PushButton;
            PushButton myButton4 = panel.AddItem(btnData4) as PushButton;
            

            //panel.AddStackedItems(btnData1, btnData2);





            //PushButtonData btnData3 = cmdChallenge03.GetButtonData();
            //PushButtonData btnData4 = cmdChallenge04.GetButtonData();

            ////3b. Create button data
            //PushButtonData btnData1 = new PushButtonData(
            //    "Exlode and Delete Groups",
            //    "Explode and then delete all groups in the project.",
            //    Assembly.GetExecutingAssembly().Location,
            //    "RevitAddinBootcamp.Command1");

            //// 4. Create buttons
            //PushButton myButton1 = panel.AddItem(btnData1) as PushButton;
            //PushButton myButton2 = panel.AddItem(btnData2) as PushButton;

            //PushButton myButton4 = panel.AddItem(btnData4) as PushButton;

            //// 4. Add Tooltips
            //btnData1.ToolTip = "Explode and delete all groups in the project";
            ////buttonData2.ToolTip = "This is Command 2";

            return Result.Succeeded;
        }

        private RibbonPanel CreateGetPanel(UIControlledApplication app, string tabName, string panelName1)
        {
            //look for panel in tab
            foreach (RibbonPanel panel in app.GetRibbonPanels(tabName))
            {
                if (panel.Name == panelName1)
                {
                    return panel;
                }

            }
            // panel not found, create it
            RibbonPanel returnPanel = app.CreateRibbonPanel(tabName, panelName1);
            //return returnPanel;

            return app.CreateRibbonPanel(tabName, panelName1);

        }
        public BitmapImage ConvertToImageSource(byte[] imageData)
        {
            using (MemoryStream mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = mem;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                return bmi;
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

}
