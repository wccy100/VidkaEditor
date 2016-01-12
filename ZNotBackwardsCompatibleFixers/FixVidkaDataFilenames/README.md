# FixVidkaDataFilenames

As of Dec 19, 2015 I have radically changed the standard for naming metadata files (meta, thumb, wave). This is because I discovered a small (or HUGE) bug: if a video and audio files in the same folder have the same filename (but different extensions), e.g. `IMG001.avi` and `IMG001.mp3` then the metadata files clash, because the naming standard for metadata files used to strip away the extension.

Don't look at me weird, I have a lot of custom audio that I record on my cellphone close to my face while I am on a camera far away. So I name the files same to keep track of them. I could have avoided this bug entirely by naming `IMG001.mp3` `IMG001_audio.mp3` instead, but **fuck it**!!! I refuse to bow down to stupid semantics.

The new naming standard will include the original file's extension instead of stripping it away. And it will work for all files, no matter how you name them.

This little program fixes the old metadata files (of which I have about 10,000). It will go through all given directories recursively and when it finds `.vidkadata` folder it renames the files in it, according to the new naming standard. It will find the original file, what its called and put its extension in the old metadata file names. When it discovers that there is more than 1 original file, it outputs a hilarious message for you to know.