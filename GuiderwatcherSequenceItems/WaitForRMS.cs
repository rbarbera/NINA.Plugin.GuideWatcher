using RBC.NINA.Plugin.GuiderWatcher.Properties;
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

namespace RBC.NINA.Plugin.GuiderWatcher {
   
    [ExportMetadata("Name", "Wait for RMS")]
    [ExportMetadata("Description", "Wait until guiding RMS goes under your limit")]
    [ExportMetadata("Icon", "StopGuiderSVG")]
    [ExportMetadata("Category", "Guider Watcher")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitForRMS : SequenceItem {
    
        [ImportingConstructor]
        public WaitForRMS() {
            this.RMS = 0.8;
        }

        public WaitForRMS(WaitForRMS copyMe) {
            this.RMS = copyMe.RMS;

            CopyMetaData(copyMe);
        }
        
        [JsonProperty]
        public double RMS { get; set; }


        double RMSTotal {
            get {
                if (GuiderWatcherShared.Instance.history.RMS.Total == 0) {
                    return this.RMS * GuiderWatcherShared.Instance.history.PixelScale;
                } else {
                    return GuiderWatcherShared.Instance.history.RMS.Total * GuiderWatcherShared.Instance.history.PixelScale;
                }
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            try {
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