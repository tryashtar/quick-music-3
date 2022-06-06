## Quick Music 3
This is a small application I made to play music.

### Technologies
* [WPF](https://en.wikipedia.org/wiki/Windows_Presentation_Foundation): This was my first time using WPF to make a Windows desktop application instead of WinForms. I largely enjoyed it.
* [NAudio](https://github.com/naudio/NAudio): [My fork](https://github.com/tryashtar/NAudio) attempts to add seamless playback to MP3s by skipping the encoder padding. It also makes seeking with Media Foundation more accurate by throwing away samples until the desired time is reached. Also, it allows `AudioFileReader` to use a better strategy for selecting which implementation to use than just blindly guessing by the file extension.
* [taglib-sharp](https://github.com/mono/taglib-sharp): [My fork](https://github.com/tryashtar/taglib-sharp) has a few minor changes to make round-trip serialization more consistent in some cases.

### Features
* **Playback:** Auditory playback is seamless, as a virtual stream is constructed in the background, preparing the next file. If the tracks are numbered, the order will be sorted out quickly in the background.
* **Views:** Press <kbd>Tab</kbd> to switch between "Now Playing" view, a compact display with controls and album art, and "Queue" view, a navigable list of tracks. Each saves your preferred window size separately.
* **Themes:** Press <kbd>Ctrl</kbd>+<kbd>D</kbd> to switch between light and dark theme. Click `Browse` and open a `yaml` file to import a custom theme!
* **Lyrics:** Lyrics display instead of the album art if the track has them. If they're synchronized, the current line is highlighted, and any line can be clicked to seek to it.
* **Chapters:** If the track has timed chapters, the time bar is segmented and the current chapter is displayed.
* **Tray:** Can be minimized to an icon on the tray. When right-clicked, displays a fully interactive miniature version of the window.
* **Shortcuts:** Lots of shortcuts, all of which work no matter what you just clicked on.
  * <kbd>Space</kbd> / <kbd>K</kbd>: Play/pause
  * <kbd>Ctrl</kbd>+<kbd>Right</kbd> / <kbd>Ctrl</kbd>+<kbd>Left</kbd>: Next/previous track
  * <kbd>Up</kbd> / <kbd>Down</kbd>: Change volume
  * <kbd>R</kbd> / <kbd>S</kbd> / <kbd>M</kbd>: Toggle repeat/shuffle/mute
  * <kbd>,</kbd> / <kbd>Left</kbd> / <kbd>J</kbd>: Seek backward (hold <kbd>Shift</kbd> to seek further)
  * <kbd>.</kbd> / <kbd>Right</kbd> / <kbd>L</kbd>: Seek forward (hold <kbd>Shift</kbd> to seek further)
  * <kbd>Ctrl</kbd>+<kbd>O</kbd>: Browse for tracks
  * <kbd>Ctrl</kbd>+<kbd>Down</kbd>: Minimize to tray
