using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Utility;
using System;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RBC.NINA.Plugin.GuiderWatcher {

    [ExportMetadata("Name", "Loop until deviation")]
    [ExportMetadata("Description", "Loop until deviatiom is over your limit")]
    [ExportMetadata("Icon", "GuiderSVG")]
    [ExportMetadata("Category", "Guider Watcher")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class LoopUntilDeviation : SequenceCondition {
        [ImportingConstructor]
        public LoopUntilDeviation(IGuiderMediator guiderMediator) {
            this.guiderMediator = guiderMediator;
            this.RMS = 0.8;
        }

        private IGuiderMediator guiderMediator;
        private double deviation;

        private async void GuiderMediator_GuideEvent(object sender, global::NINA.Core.Interfaces.IGuideStep e) {
            deviation = Math.Sqrt(e.DECDistanceRaw * e.DECDistanceRaw + e.RADistanceRaw * e.RADistanceRaw) * 3.0;
            if (deviation > this.RMS) {
                if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                    Logger.Info($"Deviatiom is over limit RMS {deviation:F2} > {RMS:F2} - Interrupting current Instruction Set");
                    await this.Parent.Interrupt();
                }
            }
            RaisePropertyChanged("LastCheckResult");
        }

        [JsonProperty]
        public double RMS { get; set; }

        [JsonProperty]
        public string LastCheckResult {
            get {
                /*
                if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                    return $"{deviation:F2}\" <= {RMS:F2}\"";
                } else {
                    return "";
                }
                */
                if (deviation > 0) {
                    return $"{deviation:F2}\" <= {RMS:F2}\"";
                } else {
                    return "";
                }
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            guiderMediator.GuideEvent -= GuiderMediator_GuideEvent;
            guiderMediator.GuideEvent += GuiderMediator_GuideEvent;
            return true;
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
            return $"Category: {Category}, Item: {nameof(LoopUntilDeviation)}, RMS:{RMS:F2}";
        }
    }
}