namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Your code goes here


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand3";
            string buttonTitle = "Explode and \nDelete Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.ExplodeGroups32,
                Properties.Resources.ExplodeGroups16,
                "Ungroups all model and detail groups and deletes non placed groups afterwards");

            return myButtonData.Data;
        }
    }

}
