namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Collect all imported (not linked) DWG files
            List<ImportInstance> importedDWGs = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .Where(i => IsImportedDWG(i, doc))
                .ToList();

            if (importedDWGs.Count == 0)
            {
                TaskDialog.Show("Imported DWGs", "No imported (not linked) DWG files found in the project.");
                return Result.Succeeded;
            }

            // Prepare the list of DWG names
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Found {importedDWGs.Count} imported (not linked) DWG files:\n");

            foreach (ImportInstance dwg in importedDWGs)
            {
                messageBuilder.AppendLine($"- {dwg.Name}");
            }

            messageBuilder.AppendLine("\nDo you want to delete these imported DWG files?");

            // Show confirmation dialog
            TaskDialog dialog = new TaskDialog("Delete Imported DWGs");
            dialog.MainInstruction = "Imported DWG Files Detected";
            dialog.MainContent = messageBuilder.ToString();
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;

            TaskDialogResult result = dialog.Show();

            if (result == TaskDialogResult.Yes)
            {
                int deletedCount = 0;

                using (Transaction trans = new Transaction(doc, "Delete Imported DWGs"))
                {
                    trans.Start();

                    foreach (ImportInstance dwg in importedDWGs)
                    {
                        doc.Delete(dwg.Id);
                        deletedCount++;
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Imported DWGs", $"{deletedCount} imported DWG files were deleted.");
            }
            else
            {
                TaskDialog.Show("Imported DWGs", "No imported DWG files were deleted.");
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Checks if an ImportInstance is an imported (not linked) DWG file.
        /// </summary>
        private bool IsImportedDWG(ImportInstance instance, Document doc)
        {
            ElementId typeId = instance.GetTypeId();
            Element elementType = doc.GetElement(typeId);

            // Check if the imported instance is a DWG (specifically ACAD files)
            if (elementType != null && elementType is ElementType importType)
            {
                string importCategory = importType.Category?.Name ?? string.Empty;
                return importCategory.Contains("Imports in Families") || importCategory.Contains("Imports in Project");
            }
            return false;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand4";
            string buttonTitle = "Delete Imported DWGs";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                null, null,
                //Properties.Resources.DeleteDWGs32,
                //Properties.Resources.DeleteDWGs16,
                "Finds all imported (not linked) DWG files and optionally deletes them.");

            return myButtonData.Data;
        }
    }
}