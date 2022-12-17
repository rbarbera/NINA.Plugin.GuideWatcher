using RBC.NINA.Plugin.GuiderWatcher.Properties;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Image.ImageData;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Settings = RBC.NINA.Plugin.GuiderWatcher.Properties.Settings;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Equipment;
using NINA.Core.Enum;
using NINA.Equipment.Interfaces;
using NINA.WPF.Base.Mediator;

namespace RBC.NINA.Plugin.GuiderWatcher {

    public sealed class GuiderWatcherShared {
        private readonly static GuiderWatcherShared _instance = new GuiderWatcherShared();

        public GuideStepsHistory history;
        private GuiderWatcherShared() {
            history = new GuideStepsHistory(10, GuiderScaleEnum.PIXELS, 1);
        }

        public static GuiderWatcherShared Instance {
            get {
                return _instance;
            }
        }

        public double LastDeviation {
            get {
                if (history == null || history.GuideSteps.Count == 0) {
                    return 0;
                } else {
                    var last = history.GuideSteps.Last();
                    return Math.Sqrt(last.DECDistanceRaw * last.DECDistanceRaw + last.RADistanceRaw * last.RADistanceRaw);
                }
            }
        }
    }
    /// <summary>
    /// This class exports the IPluginManifest interface and will be used for the general plugin information and options
    /// The base class "PluginBase" will populate all the necessary Manifest Meta Data out of the AssemblyInfo attributes. Please fill these accoringly
    /// 
    /// An instance of this class will be created and set as datacontext on the plugin options tab in N.I.N.A. to be able to configure global plugin settings
    /// The user interface for the settings will be defined by a DataTemplate with the key having the naming convention "Guiderwatcher_Options" where Guiderwatcher corresponds to the AssemblyTitle - In this template example it is found in the Options.xaml
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class GuiderWatcher : PluginBase, INotifyPropertyChanged {
        private readonly IPluginOptionsAccessor pluginSettings;
        private readonly IProfileService profileService;

        [ImportingConstructor]
        public GuiderWatcher(IProfileService profileService, IGuiderMediator guiderMediator) {

            if (Properties.Settings.Default.UpdateSettings) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Properties.Settings.Default);
            }

            // This helper class can be used to store plugin settings that are dependent on the current profile
            this.pluginSettings = new PluginOptionsAccessor(profileService, Guid.Parse(this.Identifier));
            this.profileService = profileService;
            if (GuiderWatcher.guiderMediator == null) {
                GuiderWatcher.guiderMediator = guiderMediator;
                guiderMediator.GuideEvent += GuiderMediator_GuideEvent;
                GuiderWatcherShared.Instance.history.HistorySize = this.HistorySize;

                //history.PixelScale = (guiderMediator.GetDevice() as IGuider).PixelScale;
            }
        }

        private void GuiderMediator_GuideEvent(object sender, global::NINA.Core.Interfaces.IGuideStep e) {
            GuiderWatcherShared.Instance.history.AddGuideStep(e);
            var arcsec = guiderMediator.GetInfo().RMSError.RA.Arcseconds;
            var pix = guiderMediator.GetInfo().RMSError.RA.Pixel;
            GuiderWatcherShared.Instance.history.PixelScale = arcsec / pix;
        }

        public override Task Teardown() {
            GuiderWatcher.guiderMediator.GuideEvent -= GuiderMediator_GuideEvent;
            return base.Teardown();
        }

        private static IGuiderMediator guiderMediator;

        public int HistorySize {
            get => Properties.Settings.Default.HistorySize;
            set {
                Properties.Settings.Default.HistorySize = value;
                GuiderWatcherShared.Instance.history.HistorySize = value;
                CoreUtil.SaveSettings(Properties.Settings.Default);
            }
        }
     
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
