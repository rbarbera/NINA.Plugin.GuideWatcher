using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin. Generate a fresh one for your plugin!
[assembly: Guid("2862b789-7eed-4647-9365-d5481f36df24")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("GuiderWatcher")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("A watcher to watches your guider")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Rafa Barberá <rbarberac@gmail.com>")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("GuiderWatcher")]
[assembly: AssemblyCopyright("Copyright © 2022 ")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.0.9001")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/rbarbera/NINA.Plugin.GuideWatcher")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage URL - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "https://github.com/rbarbera/NINA.Plugin.GuideWatcher")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
//[assembly: AssemblyMetadata("ChangelogURL", "https://mypluginsourcerepo.com/project/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"Who watches the watcher?

We use PHD2, or any other guider solution, to watch for the quality of our mount. Using a guider, we can warrantee that the mount is able to track out targets with the desired precision.

But things happens. What about this windy nights on witch a wind gust can momentarily make your scope vibrate more that he guiding precision? You know the effect, a big spike on your guiding graph, well outside your usual ""good tracking band"". And what's worse, a ""moved"" image, that at the end you will remove from your stack. But this scenario could be worst if you are using long time exposures and the gust happens at the beginning of the exposure. You have ""wasted"" all the exposure time.

The ironic situation is that PHD2 is perfectly aware of this deviation, but nobody is watching the watcher.... So, here is Guider Watcher a set of sequencer instructions that try to help you on this task.

* *WaitForRMS:* This instruction will wait until the guiding RMS recovers after any glitch. It's based on a RMS computed using the history size number of frames, so you can control how close you follow the RMS variations.

* *LoopWhileRMS:* A loop condition, that used with the previous instruction allows you to build a ""good guiding"" loop. This instruction, also will abort any contained exposure once the RMS deviation is detected, so no more wasted night time.

* *LoopUntilDeviation:* You can abort the loop as soon as ONE frame deviated from the desired limit. With this one, you will be rejecting more frames, but you are ensuring that no big guiding deviation are allowed in your images.
")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]