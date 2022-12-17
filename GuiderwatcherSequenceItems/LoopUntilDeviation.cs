using Accord;
using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Interfaces;
using NINA.Core.Utility;
using NINA.Equipment.Equipment;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Utility;
using System;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RBC.NINA.Plugin.GuiderWatcher {

    [ExportMetadata("Name", "Loop until Deviation")]
    [ExportMetadata("Description", "Loop untila the deviation on one frame is over your limit")]
    [ExportMetadata("Icon", "GuiderSVG")]
    [ExportMetadata("Category", "Guider Watcher")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class LoopUntilDeviation : SequenceCondition {
        [ImportingConstructor]
        public LoopUntilDeviation(IGuiderMediator guiderMediator) {
            this.guiderMediator = guiderMediator;
            this.RMS = 0.8;
            guiderMediator.GuideEvent += OnGuideEvent;
        }

        private async void OnGuideEvent(object sender, IGuideStep e) {
            RaisePropertyChanged(nameof(LastCheckResult));
            if (isLooping && LastDeviation > RMS) {
                Logger.Info("Interrupt loop");
                await InterruptLoop();
            }
        }

        private IGuiderMediator guiderMediator;
        private bool isLooping = false;

        [JsonProperty]
        public double RMS { get; set; }

        
        [JsonProperty]
        public string LastCheckResult {
            get {
                if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                    return $"{LastDeviation:F2}\" <= {RMS:F2}\"";
                } else {
                    return "";
                }
            }
        }

        double LastDeviation {
            get {
               return GuiderWatcherShared.Instance.LastDeviation * GuiderWatcherShared.Instance.history.PixelScale; 
            }
        }

        private async Task InterruptLoop() {
            if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                Logger.Info($"RMS is over limit {LastDeviation:F2} > {RMS:F2} - Interrupting current Instruction Set");
                isLooping = false;
                GuiderWatcherShared.Instance.history.Clear();
                await this.Parent.Interrupt();
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            Logger.Info("Check");
            isLooping = LastDeviation <= RMS;
            return LastDeviation <= RMS;
        }

        public override object Clone() {
            var clon = new LoopUntilDeviation(this.guiderMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };

            clon.RMS = this.RMS;

            return clon;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(LoopUntilDeviation)}, RMS:{RMS:F2}, HistorySize:{GuiderWatcherShared.Instance.history.HistorySize}";
        }
    }
}