using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AhisaTestProject
{
    public class ProgressBarHelper
    {
        private ProgressBar _progressBar;

        // Show your existing progress window
        public void ShowProgress(int totalOperations)
        {
            // Create your WPF window
            _progressBar = new ProgressBar(totalOperations);

            // Make the window a child of Revit's main window
            var mainWindowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            var helper = new System.Windows.Interop.WindowInteropHelper(_progressBar);
            helper.Owner = mainWindowHandle;

            // Show the window
            _progressBar.Show();
        }

        // Update the progress
        public void UpdateProgress(int currentOperation, string message = null)
        {
            if (_progressBar == null)
                return;

            // Update progress bar value
            _progressBar.pbProgress.Value = currentOperation;

            // Update status text if present
            if (message != null && _progressBar.lblText != null)
                _progressBar.lblText.Text = message;
            else
                _progressBar.lblText.Text = $"Updating {currentOperation} of {_progressBar.Total} elements";

            // Process any pending UI operations to ensure window is fully rendered
            DoEvents();
        }

        // Close the progress window
        public void CloseProgress()
        {
            if (_progressBar != null)
            {
                _progressBar.Close();
                _progressBar = null;
            }
        }

        public bool IsCancelled()
        {
            return _progressBar.CancelFlag;
        }

        // Helper method to process UI events
        private void DoEvents()
        {
            // Create a new temporary message processing loop
            DispatcherFrame frame = new DispatcherFrame();

            // Schedule a callback that will terminate the loop
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            // Process UI events
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
}
