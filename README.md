# NINA.Plugin.GuideWatcher

![Example of usage](https://github.com/rbarbera/NINA.Plugin.GuideWatcher/blob/75152dbaf9853c8bf43468e64d5c10e0b2c92390/doc/sample.png)

## Who watches the watcher?

We use PHD2, or any other guider solution, to watch for the quality of our mount. Using a guider, we can warrantee that the mount is able to track out targets with the desired precision.

But things happens. What about this windy nights on witch a wind gust can momentarily make your scope vibrate more that he guiding precision? You know the effect, a big spike on your guiding graph, well outside your usual ""good tracking band"". And what's worse, a ""moved"" image, that at the end you will remove from your stack. But this scenario could be worst if you are using long time exposures and the gust happens at the beginning of the exposure. You have ""wasted"" all the exposure time.

The ironic situation is that PHD2 is perfectly aware of this deviation, but nobody is watching the watcher.... So, here is Guider Watcher a set of sequencer instructions that try to help you on this task.

* *WaitForRMS:* This instruction will wait until the guiding RMS recovers after any glitch. It's based on a RMS computed using the history size number of frames, so you can control how close you follow the RMS variations.

* *LoopWhileRMS:* A loop condition, that used with the previous instruction allows you to build a ""good guiding"" loop. This instruction, also will abort any contained exposure once the RMS deviation is detected, so no more wasted night time.

* *LoopUntilDeviation:* You can abort the loop as soon as ONE frame deviated from the desired limit. With this one, you will be rejecting more frames, but you are ensuring that no big guiding deviation are allowed in your images.