using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Utility;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace GuiderWatcher.GuiderwatcherTestCategory {

    [ExportMetadata("Name", "Loop while RMS")]
    [ExportMetadata("Description", "Loop while RMS is under your limit")]
    [ExportMetadata("Icon", "GuiderSVG")]
    [ExportMetadata("Category", "Guider Watcher")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class LoopWhileRMS : SequenceCondition {
        [ImportingConstructor]
        public LoopWhileRMS(IGuiderMediator guiderMediator) {
            this.guiderMediator = guiderMediator;
            this.RMS = 0.8;
            this.lastRMS = 0;

            ConditionWatchdog = new ConditionWatchdog(InterruptWhenRMSIsOverLimits, TimeSpan.FromSeconds(5));
        }

        private IGuiderMediator guiderMediator;
        private double lastRMS;

        [JsonProperty]
        public double RMS { get; set; }

        [JsonProperty]
        public string LastCheckResult {
            get { return $"{lastRMS:F2}\" <= {RMS:F2}\""; }
        }

        private async Task InterruptWhenRMSIsOverLimits() {
            if (!Check(null, null)) {
                if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                    Logger.Info($"LastRMS is over limit RMS {lastRMS:F2} > {RMS:F2} - Interrupting current Instruction Set");
                    await this.Parent.Interrupt();
                }
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            this.lastRMS = guiderMediator.GetInfo().RMSError.Total.Arcseconds;
            Logger.Info($"LastRMS {lastRMS:F2}");
            RaisePropertyChanged("LastCheckResult");
            return lastRMS <= RMS;
        }

        public override object Clone() {
            var clon = new LoopWhileRMS(this.guiderMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };

            clon.RMS = this.RMS;

            return clon;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(LoopWhileRMS)}, lastRMS: {lastRMS:F2}, RMS:{RMS:F2}";
        }
    }
}