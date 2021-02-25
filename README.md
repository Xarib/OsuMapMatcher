# OsuMapMatcher
A API/Programm that finds similarly mapped maps in a easy manner.

![](https://raw.githubusercontent.com/Xarib/OsuMapMatcher/main/OMM.Desktop/Images/Gui.PNG)

## First time user?
You can find the installation guide here: https://github.com/Xarib/OsuMapMatcher/wiki/Installation-guide

## Known limitations
- It only works with osu!std.
- When searching, you cany only select maps that are also on the server. There is no local calculation.
- It currently struggles with techmaps or any other slider heavy maps.
- Some maps are just to exotic for the API.
- Old maps don't have a preview because the underling .osu file is missing necessary data.
- osu!lazer is not supported

## Roadmap / Planned features
- [ ] Improve parser in the slider department to get better results when searching for techmaps.
- [ ] Cleanup the .osu file parser and make it public.
- [ ] Make all osu!std maps selectable
- [ ] Ask peppy for unranked maps
- [ ] Add search history
- [ ] Redesign the programm

## API usage
Find similar maps: `https://omm.xarib.ch/api/knn/search?id={beatmapId}&count={count}`

Parameter | Description
--------- | -----------
beatmapId | The id of the beatmap. Do not confuse it with the beatmapsetId
count     | (optional) The number of maps you want to get. You can get 1-50 maps. The default is 10.

Example: https://omm.xarib.ch/api/knn/search?id=129891&count=5

List of available maps: `https://omm.xarib.ch/api/knn/maps`

**Overusage will be met with a temporary ban.** It warns you when you are going to fast.

## Special thanks to
- [Peppy](https://github.com/peppy) for showing me where to mass download .osu **text** files
- [xMOINx](https://osu.ppy.sh/users/12957744) for feedback

## Licenses
The following projects where used in the project.
- [ProcessMemoryDataFinder](https://github.com/Piotrekol/ProcessMemoryDataFinder), [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html)
- [Font Awesome](https://fontawesome.com/), [Creative Commons By 4.0](https://creativecommons.org/licenses/by/4.0/)
- [Fontdasu Mochiypop](https://github.com/fontdasu/Mochiypop), [SIL Open Font License 1.1](https://opensource.org/licenses/OFL-1.1)
