global proj_frameRate = 30
global proj_width = 1280
global proj_height = 720
global proj_audiorate = 44100
global proj_audiochannels = 2
audiorate = proj_audiorate

# NOTE: we collect YUV (for which no conversion exists) and RGB, but the actual pixel type is more specific
# a video that is YV12 or YUV2 is gonna also be YUV, like wise RGB32 will also be RGB
global stat_nRGB = 0
global stat_nRGB24 = 0
global stat_nRGB32 = 0
global stat_nY8 = 0
global stat_nYUV = 0
global stat_nYUY2 = 0
global stat_nYV12 = 0
global stat_nYV16 = 0
global stat_nYV24 = 0
global stat_nYV411 = 0
global mostFrequentPixelType = "???"

#################### all video files ############################
_costaetruschimap_avi = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\_costa-etruschi-map.avi", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4188_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4188.AVI", audio=True, fps=proj_frameRate, convertfps=true)
_plane_travel_avi = DirectShowSource("C:\Users\Mikhail\Videos\travel\_plane_travel.avi", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4184_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4184.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4183_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4183.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4187_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4187.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4191_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4191.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4200_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4200.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4195_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4195.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4189_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4189.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4190_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4190.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4203_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4203.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4193_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4193.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4201_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4201.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4202_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4202.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4192_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4192.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4196_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4196.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4197_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4197.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4199_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4199.AVI", audio=True, fps=proj_frameRate, convertfps=true)
_20150321_172617_mp4 = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\20150321_172617.mp4", audio=True, fps=proj_frameRate, convertfps=true)
_20150321_165336_mp4 = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\20150321_165336.mp4", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4207_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4207.AVI", audio=True, fps=proj_frameRate, convertfps=true)
IMGP4208_AVI = DirectShowSource("C:\Users\Mikhail\Pictures\2015_02_italia\2015_03_21_etruschi\IMGP4208.AVI", audio=True, fps=proj_frameRate, convertfps=true)


#################### determine the most frequently used pixel type ############################
collectpixeltypestat(IMGP4188_AVI, 0)


global mostFrequentPixelType = "RGB32"
curStatMax = stat_nRGB32
global mostFrequentPixelType = (stat_nRGB24 > curStatMax) ? "RGB24" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nRGB24)
global mostFrequentPixelType = (stat_nYV12 > curStatMax) ? "YV12" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nRGB32)
global mostFrequentPixelType = (stat_nYV16 > curStatMax) ? "YV16" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nYV16)
global mostFrequentPixelType = (stat_nY8 > curStatMax) ? "Y8" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nY8)
#~ global mostFrequentPixelType = (stat_nYUV > curStatMax) ? "YUV" : mostFrequentPixelType
#~ curStatMax = Max(curStatMax, stat_nYUV)
global mostFrequentPixelType = (stat_nYUY2 > curStatMax) ? "YUY2" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nYUY2)
global mostFrequentPixelType = (stat_nYV24 > curStatMax) ? "YV24" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nYV24)
global mostFrequentPixelType = (stat_nYV411 > curStatMax) ? "YV411" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nYV411)
global mostFrequentPixelType = (stat_nRGB > curStatMax) ? "RGB" : mostFrequentPixelType
curStatMax = Max(curStatMax, stat_nRGB)

#################### render ###########################

_costaetruschimap_avi_0 = NeutralClip(_costaetruschimap_avi, 0, 105)
IMGP4188_AVI_1 = NeutralClip(IMGP4188_AVI, 0, 266)
_plane_travel_avi_2 = NeutralClip(_plane_travel_avi, 126, 159)
IMGP4184_AVI_3 = NeutralClip(IMGP4184_AVI, 0, 64).MuteThisClip()
IMGP4183_AVI_4 = NeutralClip(IMGP4183_AVI, 0, 61).MuteThisClip()
IMGP4187_AVI_5 = NeutralClip(IMGP4187_AVI, 0, 70).MuteThisClip()
IMGP4191_AVI_6 = NeutralClip(IMGP4191_AVI, 0, 600)
IMGP4200_AVI_7 = NeutralClip(IMGP4200_AVI, 0, 125).MuteThisClip()
IMGP4195_AVI_8 = NeutralClip(IMGP4195_AVI, 0, 67).MuteThisClip()
IMGP4189_AVI_9 = NeutralClip(IMGP4189_AVI, 0, 46).MuteThisClip()
IMGP4190_AVI_10 = NeutralClip(IMGP4190_AVI, 84, 134).MuteThisClip()
IMGP4203_AVI_11 = NeutralClip(IMGP4203_AVI, 43, 143).MuteThisClip()
IMGP4191_AVI_12 = NeutralClip(IMGP4191_AVI, 601, 999)
IMGP4193_AVI_13 = NeutralClip(IMGP4193_AVI, 69, 291)
_plane_travel_avi_14 = NeutralClip(_plane_travel_avi, 159, 188)
IMGP4201_AVI_15 = NeutralClip(IMGP4201_AVI, 0, 185)
IMGP4202_AVI_16 = NeutralClip(IMGP4202_AVI, 42, 607)
IMGP4192_AVI_17 = NeutralClip(IMGP4192_AVI, 10, 78)
IMGP4196_AVI_18 = NeutralClip(IMGP4196_AVI, 119, 187)
IMGP4197_AVI_19 = NeutralClip(IMGP4197_AVI, 49, 130)
IMGP4199_AVI_20 = NeutralClip(IMGP4199_AVI, 196, 331)
IMGP4196_AVI_21 = NeutralClip(IMGP4196_AVI, 444, 517)
IMGP4197_AVI_22 = NeutralClip(IMGP4197_AVI, 366, 420)
IMGP4199_AVI_23 = NeutralClip(IMGP4199_AVI, 450, 549)
_20150321_172617_mp4_24 = NeutralClip(_20150321_172617_mp4, 0, 194)
_20150321_165336_mp4_25 = NeutralClip(_20150321_165336_mp4, 0, 165)
_20150321_165336_mp4_26 = NeutralClip(_20150321_165336_mp4, 839, 974)
IMGP4207_AVI_27 = NeutralClip(IMGP4207_AVI, 1143, 1844)
IMGP4208_AVI_28 = NeutralClip(IMGP4208_AVI, 15, 177)
IMGP4208_AVI_29 = NeutralClip(IMGP4208_AVI, 355, 608)
_plane_travel_avi_30 = NeutralClip(_plane_travel_avi, 126, 159)


UnalignedSplice( \
	_costaetruschimap_avi_0, \
	IMGP4188_AVI_1, \
	_plane_travel_avi_2, \
	IMGP4184_AVI_3.MixAudioFromClip(IMGP4191_AVI_6, 0, 65, 0), \
	IMGP4183_AVI_4.MixAudioFromClip(IMGP4191_AVI_6, 65, 127, 0), \
	IMGP4187_AVI_5.MixAudioFromClip(IMGP4191_AVI_6, 127, 198, 0), \
	IMGP4191_AVI_6.Trim(198, 475), \
	IMGP4200_AVI_7.MixAudioFromClip(IMGP4191_AVI_6, 475, 601, 0), \
	IMGP4195_AVI_8.MixAudioFromClip(IMGP4191_AVI_12, 0, 68, 0), \
	IMGP4189_AVI_9.MixAudioFromClip(IMGP4191_AVI_12, 68, 115, 0), \
	IMGP4190_AVI_10.MixAudioFromClip(IMGP4191_AVI_12, 115, 166, 0), \
	IMGP4203_AVI_11.MixAudioFromClip(IMGP4191_AVI_12, 166, 267, 0), \
	IMGP4191_AVI_12.Trim(267, 399), \
	IMGP4193_AVI_13, \
	_plane_travel_avi_14, \
	IMGP4201_AVI_15, \
	IMGP4202_AVI_16.Trim(0, 210), \
	IMGP4192_AVI_17.MixAudioFromClip(IMGP4202_AVI_16, 210, 279, 0), \
	IMGP4196_AVI_18.MixAudioFromClip(IMGP4202_AVI_16, 279, 348, 0), \
	IMGP4197_AVI_19.MixAudioFromClip(IMGP4202_AVI_16, 348, 430, 0), \
	IMGP4199_AVI_20.MixAudioFromClip(IMGP4202_AVI_16, 430, 566, 0), \
	IMGP4196_AVI_21, \
	IMGP4197_AVI_22, \
	IMGP4199_AVI_23, \
	_20150321_172617_mp4_24, \
	_20150321_165336_mp4_25, \
	_20150321_165336_mp4_26, \
	IMGP4207_AVI_27, \
	IMGP4208_AVI_28, \
	IMGP4208_AVI_29, \
	_plane_travel_avi_30 \
)


voiceover=BlankClip(last,0) ++BlankClip(last, 334).AudioDub(DirectShowSource("C:\Users\Mikhail\Videos\travel\buratino-banjo.mp3", fps=proj_frameRate, convertfps=true).ResampleAudio(44100)).Trim(192, 334)
MixAudio(last,voiceover, clip1_factor=1, clip2_factor=1)

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
	# if need be convert to color space of the most frequent one
	vclip = vclip.toProperPixelType()
		
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
	#~ vclip = vclip.subtitle("pixType:" + mostFrequentPixelType)
	#~ vclip = vclip.subtitle("stats:" + String(stat_nRGB) + "," + String(stat_nRGB24) + "," + String(stat_nRGB32) + "," + String(stat_nY8) + "," + String(stat_nYUV) + "," + String(stat_nYUY2) + "," + String(stat_nYV12) + "," + String(stat_nYV16) + "," + String(stat_nYV24) + "," + String(stat_nYV411))
	#vclip = vclip.Subtitle(String(vclip.framerate), align=2) #DEBUG
	return vclip
}

function NeutralClipImage(Clip vclip, int frames)
{
	vclip = (vclip.width != proj_width || vclip.height != proj_height) ? vclip.BilinearResize(proj_width, proj_height) : vclip
	audioSilence = Tone(length=(vclip.FrameCount/vclip.FrameRate), samplerate=proj_audiorate, channels=proj_audiochannels, type="silence", level=0.0)
	vclip = !vclip.HasAudio ? AudioDub(vclip, audioSilence) : vclip
	vclip = toProperPixelType(vclip)
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

function toProperPixelType(vclip)
{
	vclip = (mostFrequentPixelType == "RGB" && !vclip.IsRGB()) ? vclip.ConvertToRGB() : vclip
	vclip = (mostFrequentPixelType == "RGB24" && !vclip.IsRGB24()) ? vclip.ConvertToRGB24() : vclip
	vclip = (mostFrequentPixelType == "RGB32" && !vclip.IsRGB32()) ? vclip.ConvertToRGB32() : vclip
	vclip = (mostFrequentPixelType == "Y8" && !vclip.IsY8()) ? vclip.ConvertToY8() : vclip
	###### vclip = (mostFrequentPixelType == "YUV" && !vclip.IsYUV()) ? vclip.ConvertToYV12(matrix="Rec601", interlaced=false) : vclip
	vclip = (mostFrequentPixelType == "YUY2" && !vclip.IsYUY2()) ? vclip.ConvertToYUY2(matrix="Rec601", interlaced=false) : vclip
	vclip = (mostFrequentPixelType == "YV12" && !vclip.IsYV12()) ? vclip.ConvertToYV12(matrix="Rec601", interlaced=false) : vclip
	vclip = (mostFrequentPixelType == "YV16" && !vclip.IsYV16()) ? vclip.ConvertToYV16(interlaced=false) : vclip
	vclip = (mostFrequentPixelType == "YV24" && !vclip.IsYV24()) ? vclip.ConvertToYV24(interlaced=false) : vclip
	vclip = (mostFrequentPixelType == "YV411" && !vclip.IsYV411()) ? vclip.ConvertToYV411(interlaced=false) : vclip
	#vclip = vclip.ConvertToYV12(matrix="Rec601", interlaced=false)
	return vclip
}

function CollectPixelTypeStat(Clip vclip, weight)
{
	global stat_nRGB = stat_nRGB + (vclip.IsRGB() ? weight : 0)
	global stat_nRGB24 = stat_nRGB24 + (vclip.IsRGB24() ? weight : 0)
	global stat_nRGB32 = stat_nRGB32 + (vclip.IsRGB32() ? weight : 0)
	global stat_nY8 = stat_nY8 + (vclip.IsY8() ? weight : 0)
	global stat_nYUV = stat_nYUV + (vclip.IsYUV() ? weight : 0)
	global stat_nYUY2 = stat_nYUY2 + (vclip.IsYUY2() ? weight : 0)
	global stat_nYV12 = stat_nYV12 + (vclip.IsYV12() ? weight : 0)
	global stat_nYV16 = stat_nYV16 + (vclip.IsYV16() ? weight : 0)
	global stat_nYV24 = stat_nYV24 + (vclip.IsYV24() ? weight : 0)
	global stat_nYV411 = stat_nYV411 + (vclip.IsYV411() ? weight : 0)
}

