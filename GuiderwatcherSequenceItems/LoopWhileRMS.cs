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
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RBC.NINA.Plugin.GuiderWatcher {

    [ExportMetadata("Name", "Loop while RMS")]
    [ExportMetadata("Description", "Loop while RMS is under your limit")]
    [ExportMetadata("Icon", "GuiderSVG")]
    [ExportMetadata("Category", "Guider Watcher")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class LoopWhileRMS : SequenceCondition, IValidatable {
        private IList<string> issues = new List<string>();
        public IList<string> Issues { get => issues; set { issues = value; RaisePropertyChanged(); } }

        [ImportingConstructor]
        public LoopWhileRMS(IGuiderMediator guiderMediator) {
            this.guiderMediator = guiderMediator;
            this.RMS = 0.8;
            this.historySize = 10;
            this.history = new GuideStepsHistory(historySize, GuiderScaleEnum.ARCSECONDS, 1);
            guiderMediator.GuideEvent += OnGuideEvent;
        }

        private async void OnGuideEvent(object sender, IGuideStep e) {
            history.AddGuideStep(e);
            RaisePropertyChanged(nameof(LastCheckResult));
            if (isLooping && RMSTotal > RMS) {
                Logger.Info("Interrupt loop");
                await InterruptLoop();
            }
        }

        private IGuiderMediator guiderMediator;
        private GuideStepsHistory history;
        private bool isLooping = false;

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
                RaisePropertyChanged(nameof(LastCheckResult));
            }
        }

        
        [JsonProperty]
        public string LastCheckResult {
            get {
                if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                    return $"{RMSTotal:F2}\" <= {RMS:F2}\"";
                } else {
                    return "";
                }
            }
        }
        [JsonProperty]
        public string Inherited {
            get {
                return $"History size:{HistorySize}| RMS:{RMS:F2}";
            }
        }



        double RMSTotal {
            get { return history.RMS.Total * 3.0; }
        }

        private async Task InterruptLoop() {
            if (this.Parent != null && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                Logger.Info($"RMS is over limit {RMSTotal:F2} > {RMS:F2} - Interrupting current Instruction Set");
                isLooping = false;
                await this.Parent.Interrupt();
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            isLooping = RMSTotal <= RMS;
            Logger.Info($"Check N:{history.GuideSteps.Count}, RMDTotal:{RMSTotal:F2}, RMS:{RMS:F2}, isLooping:{isLooping}");
            return RMSTotal <= RMS;
        }

        public override object Clone() {
            var clon = new LoopWhileRMS(this.guiderMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };

            clon.RMS = this.RMS;
            clon.HistorySize = this.HistorySize;

            return clon;
        }

        public bool Validate() {
            var i = new List<String>();
            if (this.Parent.Parent == null) {
                i.Add("This item should be used on an inner Loop");
            } else {
                var items = (IList<ISequenceItem>)this.Parent.Parent.GetItemsSnapshot();
                var found = false;
                foreach (var item in items) {
                    if (item is WaitForRMS waitFor) {
                        this.RMS = waitFor.RMS;
                        this.HistorySize = waitFor.HistorySize;
                        found = true;
                    }
                }

                if (!found) { i.Add("No WaitForRMS found on parent"); }
            }
            Issues = i;
            return i.Count == 0;
        }


        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(LoopWhileRMS)}, RMS:{RMS:F2}, HistorySize:{HistorySize}";
        }

    }
}