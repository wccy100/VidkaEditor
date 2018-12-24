global proj_frameRate = {proj-fps}
global proj_width = {proj-width}
global proj_height = {proj-height}
global proj_audiorate = 44100
global proj_audiochannels = 2
audiorate = proj_audiorate

#################### all video files ############################
{video-files}

#################### render ###########################

{video-clips}

{audio-clips}

#All these things we can chain outside on the NeutralClip function...
# - FadeIn2 adds 2 frames, FadeIn adds 1 frame, FadeIn0 adds no frames (so use that one I guess).
#   The frames added are solid black, otherwise fade is not complete. See "Fade" documentation.
# - Reverse does not work
# .FadeIn0(10).FlipHorizontal().FlipVertical().Reverse()


#debug log file sample:
#~ nctest1 = NeutralClip("vid2.mp4", 500, 530)#.FadeIn2(10)
#~ Version().WriteFileStart("test.avs.log", String(nctest1.FrameCount))


#################### functions! ###########################

function NeutralClip(Clip vclip, int fstart, int fend,\
		float "slowmo")
{
	# if need be convert to RGB, see NOTE: 25d79748 in template version 2015
	vclip = (!vclip.IsRGB32()) ? vclip.ConvertToRGB32() : vclip
		
	# convert clip so that they are all same video size, fps, audio format, etc...
	vclip = (vclip.framerate != proj_frameRate) ? vclip.ChangeFPS(proj_frameRate) : vclip
	vclip = (vclip.width != proj_width || vclip.height != proj_height) ? vclip.BilinearResize(proj_width, proj_height) : vclip
	audioSilence = Tone(length=(vclip.FrameCount/vclip.FrameRate), samplerate=proj_audiorate, channels=proj_audiochannels, type="silence", level=0.0)
	vclip = !vclip.HasAudio ? AudioDub(vclip, audioSilence) : vclip
	vclip = (vclip.HasAudio && vclip.AudioChannels == 1 && proj_audiochannels == 2) \
		? AudioDub(vclip, vclip.GetChannel(1,1)) \
		: vclip
	vclip = (vclip.AudioRate != proj_audiorate) ? vclip.ResampleAudio(proj_audiorate) : vclip
	# trim, of course!
	vclip = vclip.Trim(fstart, fend, true)
	# pad audio. see http://avisynth.nl/index.php/Trim (...when last_frame=0 => till end)
	vclip = vclip.Trim(0, 0, pad=true)
	
	# misc manipulations
	#vclip = Defined(slowmo) ? ConvertFPS(slowmo * proj_frameRate).AssumeFPS(proj_frameRate, sync_audio=true).ResampleAudio(44100) : vclip
	
	# other samples...
	# vclip = fadein ? vclip.Reverse.FadeOut(fend-fstart).Reverse : vclip #fade in (doesnt work cause reverse doesnt work)
	
	#debug
	#pixType = vclip.PixelType()
	#~ vclip = vclip.subtitle("this is how to debug:" + pixType)
	#vclip = vclip.Subtitle(String(vclip.framerate), align=2) #DEBUG
	return vclip
}

function NeutralClipImage(Clip vclip, int frames)
{
	vclip = (vclip.width != proj_width || vclip.height != proj_height) ? vclip.BilinearResize(proj_width, proj_height) : vclip
	audioSilence = Tone(length=(vclip.FrameCount/vclip.FrameRate), samplerate=proj_audiorate, channels=proj_audiochannels, type="silence", level=0.0)
	vclip = !vclip.HasAudio ? AudioDub(vclip, audioSilence) : vclip
	vclip = (!vclip.IsRGB32()) ? vclip.ConvertToRGB32() : vclip
	return vclip
}

# fstart and fend are optional trimmings (in frames) used on vclip, to synch audio with it
function AddCustomAudio(Clip vclip, Clip audio, float audioOffset, int "fstart", int "fend")
{
	fstart = Default(fstart, 0)
	fend = Default(fend, -1)
	audio = (audio.AudioRate != proj_audiorate) ? audio.ResampleAudio(proj_audiorate) : audio
	audioTrimStart = audioOffset + fstart / float(proj_frameRate)
	audioTrimEnd = (fend == -1) ? audio.audioduration-1 : audioOffset + fend / float(proj_frameRate)
	#audio = audio.AudioTrim(audioTrimStart, audioTrimEnd)
	audio = (audioTrimStart < 0) ? audio.BlankClip(length=int(-audioTrimStart * proj_frameRate), fps=proj_frameRate) + audio.AudioTrim(0, audioTrimEnd) : audio.AudioTrim(audioTrimStart, audioTrimEnd)
	vclip = AudioDub(vclip, audio)
	vclip = vclip.Trim(0, 0, pad=true) # pad audio. see http://avisynth.nl/index.php/Trim (...when last_frame=0 => till end)
	return vclip
}

function MuteThisClip(Clip vclip)
{
	audioSilence = Tone(length=(vclip.FrameCount/vclip.FrameRate), samplerate=proj_audiorate, channels=proj_audiochannels, type="silence", level=0.0)
	vclip = AudioDub(vclip, audioSilence)
	return vclip
}

function MixAudioFromClip(Clip vclip, Clip vclipAudio, int fstart, int fend,\
		int "fOffset")
{
	fOffset = Default(fOffset, 0)
	aclip = vclipAudio.Trim(fstart, fend)
	aclip = (fOffset == 0) ? aclip : BlankClip(aclip, fOffset) ++ aclip
	vclip = vclip.MixAudio(aclip, clip1_factor=1, clip2_factor=1)
	return vclip
}

function FadeOutAudioOnly(Clip vclip, int frames)
{
	return AudioDub(vclip, vclip.FadeOut(frames))
}

function FadeInAudioOnly(Clip vclip, int frames)
{
	return AudioDub(vclip, vclip.FadeIn(frames))
}
