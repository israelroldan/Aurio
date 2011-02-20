﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;

namespace AudioAlign.Audio.TaskMonitor {
    public class ProgressMonitor {

        private static ProgressMonitor singletonInstance = null;

        private List<ProgressReporter> reporters;
        private Dictionary<ProgressReporter, int> reporterProgress;

        public event EventHandler ProcessingStarted;
        public event EventHandler<ValueEventArgs<float>> ProcessingProgressChanged;
        public event EventHandler ProcessingFinished;

        private ProgressMonitor() {
            reporters = new List<ProgressReporter>();
            reporterProgress = new Dictionary<ProgressReporter, int>();
        }

        public static ProgressMonitor Instance {
            get {
                if (singletonInstance == null) {
                    singletonInstance = new ProgressMonitor();
                }
                return singletonInstance;
            }
        }

        public ProgressReporter BeginTask(string taskName) {
            return BeginTask(new ProgressReporter(taskName));
        }

        public ProgressReporter BeginTask(string taskName, bool reportProgress) {
            return BeginTask(new ProgressReporter(taskName, reportProgress));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private ProgressReporter BeginTask(ProgressReporter reporter) {
            if (reporters.Count == 0) {
                OnProcessingStarted();
            }
            reporters.Add(reporter);
            reporter.PropertyChanged += progressReporter_PropertyChanged;
            reporterProgress.Add(reporter, 0);
            return reporter;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EndTask(ProgressReporter reporter) {
            reporter.PropertyChanged -= progressReporter_PropertyChanged;
            reporters.Remove(reporter);
            reporterProgress.Remove(reporter);
            if (reporters.Count == 0) {
                OnProcessingFinished();
            }
        }

        private void progressReporter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            ProgressReporter senderTaskStatus = (ProgressReporter)sender;
            if (reporterProgress[senderTaskStatus] != (int)senderTaskStatus.Progress) {
                reporterProgress[senderTaskStatus] = (int)senderTaskStatus.Progress;
                Debug.WriteLine(senderTaskStatus.Name + ": " + reporterProgress[senderTaskStatus] + "%");
            }
            OnProcessingProgressChanged();
        }

        private void OnProcessingStarted() {
            if(ProcessingStarted != null) {
                ProcessingStarted(this, EventArgs.Empty);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnProcessingProgressChanged() {
            if (ProcessingProgressChanged != null) {
                float progress = 0;
                foreach (ProgressReporter reporter in reporters) {
                    progress += (float)reporter.Progress;
                }
                progress /= reporters.Count;
                ProcessingProgressChanged(this, new ValueEventArgs<float>(progress));
            }
        }

        private void OnProcessingFinished() {
            if (ProcessingFinished != null) {
                ProcessingFinished(this, EventArgs.Empty);
            }
        }
    }
}
