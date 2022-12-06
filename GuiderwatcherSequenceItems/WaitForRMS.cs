﻿using RBC.NINA.Plugin.GuiderWatcher.Properties;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Equipment.MyGuider;
using NINA.Sequencer.SequenceItem;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Accord.IO;
using static NINA.Equipment.SDK.CameraSDKs.SBIGSDK.SbigSharp.SBIG;
using NINA.Core.Enum;
using NINA.Core.Interfaces;
using NINA.Equipment.Equipment;
using NINA.Core.Utility;

namespace RBC.NINA.Plugin.GuiderWatcher {
   
    [ExportMetadata("Name", "Wait for RMS")]
    [ExportMetadata("Description", "Wait until guiding RMS goes under your limit")]
    [ExportMetadata("Icon", "StopGuiderSVG")]
    [ExportMetadata("Category", "Guider Watcher")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitForRMS : SequenceItem {
    
        [ImportingConstructor]
        public WaitForRMS(IGuiderMediator guiderMediator) {
            this.guiderMediator = guiderMediator;
            this.RMS = 0.8;
            this.historySize = 10;
            this.history = new GuideStepsHistory(historySize, GuiderScaleEnum.ARCSECONDS, 1);
            this.guiderMediator.GuideEvent += OnGuideEvent;

        }

        public WaitForRMS(WaitForRMS copyMe) {
            this.guiderMediator = copyMe.guiderMediator;
            this.RMS = copyMe.RMS;
            this.historySize = copyMe.historySize;
            this.history = new GuideStepsHistory(historySize, GuiderScaleEnum.ARCSECONDS, 1);
            this.guiderMediator.GuideEvent += OnGuideEvent;

            CopyMetaData(copyMe);
        }
        private void OnGuideEvent(object sender, IGuideStep e) {
            history.AddGuideStep(e);
        }

        private IGuiderMediator guiderMediator;
        private GuideStepsHistory history;      

        [JsonProperty]
        public double RMS { get; set; }

        private int historySize;

        [JsonProperty]
        public int HistorySize {
            get {
                return historySize;
            }
            set {
                historySize = value;
                history.HistorySize = historySize;
            }
        }

        double RMSTotal {
            get { return history.RMS.Total * 3.0; }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            try {
                Logger.Info($"Execute RMSTotal:{RMSTotal:F2}, RMS:{RMS:F2}");
                while (RMSTotal > this.RMS) {     
                    progress?.Report(new ApplicationStatus() {
                        Status = $"Waiting for RMS {RMSTotal:F2}\" <= {this.RMS:F2}\""
                    });
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            } finally {
                progress?.Report(new ApplicationStatus() { Status = "" });
            }
        }

        public override object Clone() {
            return new WaitForRMS(this);
        }


        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(WaitForRMS)}, RMS: {RMS}\"";
        }
    }
}