namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int ungroupedDetailCount = 0;
            int ungroupedModelCount = 0;

            using (Transaction trans = new Transaction(doc, "Ungroup All Groups"))
            {
                trans.Start();

                // Collect and ungroup all placed detail groups
                List<Group> detailGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                    .WhereElementIsNotElementType()
                    .Cast<Group>()
                    .ToList();

                foreach (Group group in detailGroups)
                {
                    group.UngroupMembers();
                    ungroupedDetailCount++;
                }

                // Collect and ungroup all placed model groups
                List<Group> modelGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                    .WhereElementIsNotElementType()
                    .Cast<Group>()
                    .ToList();

                foreach (Group group in modelGroups)
                {
                    group.UngroupMembers();
                    ungroupedModelCount++;
                }

                trans.Commit();
            }

            // Show TaskDialog with summary of actions
            TaskDialog.Show("Group Cleanup",
                $"{ungroupedDetailCount} placed detail groups ungrouped.\n" +
                $"{ungroupedModelCount} placed model groups ungrouped.");

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Ungroup All Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UngroupAndDeleteUnusedGroups32,
                Properties.Resources.UngroupAndDeleteUnusedGroups16,
                "Ungroup all placed model and detail groups in the project.");

            return myButtonData.Data;
        }
    }
}
