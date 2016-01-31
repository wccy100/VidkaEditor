# VidkaEditor

## what?

Non-linear video editor, better than sony vegas and final cut pro...
It relies on AviSynth to generate the final video, however the editor
provides an easy-to-use interface for video editing. It is called Vidka because I am Russian and I like vodka, but with "vid" part of the video it becomes vidka. NOW ENOUGH WITH THE QUESTIONS!

## requirements

You will need to have the following commands available in your PATH:

 - [ffmpeg/ffprobe](https://www.ffmpeg.org/download.html)
 - [mencoder/mplayer](http://mplayerwin.sourceforge.net/downloads.html)

And (of course! as mentioned above) you need to install the following: 

 - [avisynth 2.6](http://avs-plus.net/) or above

## Usage tutorial

Keyboard is your friend.

 - `F` toggle clip being read-only. Prevents accidental trimming (u know... the ooops...)
 - `S` split video clip. Green line on the split means the previous clip flows right into next clip, AKA the split it fresh. (e.g. [0..20][21..30]) If you trim at the "green" split it will no longer be green
 - `A` and `D` - respectively split and delete left and right half of clip. When working, the middle finger is on usually on `S`, while ring and index are on `A` and `D` respectively. (Exactly! Like arrow keys!)
 - `L` Splits and makes the clip read-only. Used for poetry videos along with linear shuffle.
 - `|` (vertical line) toggles the render splitting. Sometimes (all the time) the renderer will crash when projects are too big. So I split them into 2 or even 3 even pieces. Notice the render part in the "File" menu changes.
 - `~` (tilda) link audio and video. Select a video clip, press the tilda, then a red circle will appear and you will need to select an audio clip. This creates the bond.
	 - TODO: DELETE AUDIO-VIDEO LINK IS NOT YET IMPLEMENTED... :'(
 - `O` toggle console visibility
 - `P` toggle player style. There are 2 preview players: Windows Media Player and "nearest thumbnail player" (which is friggin 10000000 times faster, #$%^... I don't like WMP for being too slow, so I just use the "nearest thumbnail player". Don't worry though, when you hit play, the player is always WMP)
 - `F4` open clip properties. For a whole bunch of stuff that you can do to a clip!!!
 - 