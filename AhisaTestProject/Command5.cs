namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command5 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Collect all legends
            List<View> allLegends = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.ViewType == ViewType.Legend)
                .ToList();

            // Find unplaced legends (not placed on any sheet)
            List<View> unplacedLegends = allLegends.Where(legend => legend.GetAllPlacedInstances().Count() == 0).ToList();

            if (!unplacedLegends.Any())
            {
                TaskDialog.Show("Legend Cleanup", "No unplaced legends found in the project.");
                return Result.Succeeded;
            }

            // Prepare list of unplaced legends
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Found {unplacedLegends.Count} unplaced legends:\n");
            foreach (View legend in unplacedLegends)
            {
                messageBuilder.AppendLine($"- {legend.Name}");
            }
            messageBuilder.AppendLine("\nDo you want to delete these unplaced legends?");

            // Show confirmation dialog
            TaskDialog dialog = new TaskDialog("Delete Unplaced \nLegends");
            dialog.MainInstruction = "Unplaced Legends Detected";
            dialog.MainContent = messageBuilder.ToString();
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;

            TaskDialogResult result = dialog.Show();

            if (result == TaskDialogResult.Yes)
            {
                int deletedCount = 0;

                using (Transaction trans = new Transaction(doc, "Delete Unplaced Legends"))
                {
                    trans.Start();

                    foreach (View legend in unplacedLegends)
                    {
                        doc.Delete(legend.Id);
                        deletedCount++;
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Legend Cleanup", $"{deletedCount} unplaced legends were deleted.");
            }
            else
            {
                TaskDialog.Show("Legend Cleanup", "No legends were deleted.");
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand5";
            string buttonTitle = "Delete Unplaced \nLegends";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedSBoxes32,
                Properties.Resources.DeleteDWG16,
                "Finds and optionally deletes unplaced legends in the project.");

            return myButtonData.Data;
        }
    }

    public static class ViewCollector
    {
        /// <summary>
        /// Retrieves all sheets where the given view is placed.
        /// </summary>
        public static IEnumerable<Element> GetAllPlacedInstances(this View view)
        {
            Document doc = view.Document;
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(sheet => sheet.GetAllPlacedViews().Contains(view.Id));
        }

        /// <summary>
        /// Retrieves all view IDs placed on a given sheet.
        /// </summary>
        public static IEnumerable<ElementId> GetAllPlacedViews(this ViewSheet sheet)
        {
            return sheet.GetAllViewports().Select(vpId =>
            {
                Viewport vp = sheet.Document.GetElement(vpId) as Viewport;
                return vp?.ViewId ?? ElementId.InvalidElementId;
            }).Where(viewId => viewId != ElementId.InvalidElementId);
        }
    }
}