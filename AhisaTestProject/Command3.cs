namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int deletedScopeBoxCount = 0;
            List<string> deletedScopeBoxNames = new List<string>();

            using (Transaction trans = new Transaction(doc, "Delete Unused Scope Boxes"))
            {
                trans.Start();

                // Get all scope boxes in the model
                List<Element> scopeBoxes = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_VolumeOfInterest) // Scope Boxes category
                    .WhereElementIsNotElementType()
                    .ToList();

                // Get all views and view templates
                List<View> views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate)
                    .ToList();

                List<View> viewTemplates = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => v.IsTemplate)
                    .ToList();

                // Find and delete unused scope boxes
                foreach (Element scopeBox in scopeBoxes)
                {
                    ElementId scopeBoxId = scopeBox.Id;
                    string scopeBoxName = scopeBox.Name;

                    // Check if the scope box is used in any view or template
                    bool isUsed = views.Any(v => v.GetParamValue(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) == scopeBoxId) ||
                                  viewTemplates.Any(vt => vt.GetParamValue(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) == scopeBoxId);

                    if (!isUsed)
                    {
                        doc.Delete(scopeBoxId);
                        deletedScopeBoxNames.Add(scopeBoxName);
                        deletedScopeBoxCount++;
                    }
                }

                trans.Commit();
            }

            // Prepare the TaskDialog message
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"{deletedScopeBoxCount} unused scope boxes deleted.");

            if (deletedScopeBoxNames.Count > 0)
            {
                messageBuilder.AppendLine("\nDeleted Scope Boxes:");
                foreach (string name in deletedScopeBoxNames)
                {
                    messageBuilder.AppendLine($"- {name}");
                }
            }
            else
            {
                messageBuilder.AppendLine("No unused scope boxes found.");
            }

            // Show TaskDialog with summary
            TaskDialog.Show("Scope Box Cleanup", messageBuilder.ToString());

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand3";
            string buttonTitle = "Delete Unused \nScope Boxes";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedSBoxes32,
                Properties.Resources.DeleteUnusedSBoxes16,
                "Deletes all unused \nscope boxes.");

            return myButtonData.Data;
        }
    }

    /// <summary>
    /// Extension method to safely retrieve parameter values from a View.
    /// </summary>
    public static class ViewExtensions
    {
        public static ElementId GetParamValue(this View view, BuiltInParameter parameter)
        {
            Parameter param = view.get_Parameter(parameter);
            return param != null ? param.AsElementId() : ElementId.InvalidElementId;
        }
    }
}