# aon helper

my code mod for celeste/everest. updated whenever i feel like it

current features:

### Entities

- Resizable Heart
  - its a crystal heart you can resize the hitbox of
- Feather Dash Switch
  - dash switch except you bounce into it with a feather to activate it
- Feather Bounce Scam Controller
  - lets you change how low the feather timer needs to be in order for feather bouncing to kill your momentum
- Modify Rebound Controller
  - modifies the rebound you get from breaking dash blocks/hitting kevins to set/conserve speed (allows for independent control of the x and y directions as well as top/bottom vs left/right rebounds)
- Fling Bird No Skip Controller
  - makes vanilla flingbirds not run to the next position when you're too far past their current position
- Fg Styleground Bloom Controller
  - allows you to apply bloom to foreground stylegrounds (either all of them or only ones with a certain tag)
- Unforgiving Spikes
  - spikes that don't check player speed (or check player velocity instead) before killing you
- Clamp Light Color Controller
  - allows you to set the "maximum" light color, i.e. no light is allowed to have a color brighter than this. useful for when you want overlapping light sources to not blend into white or when you want to make white stuff a different color. allows both "clamp" and "tint" options
- Darker Matter
  - a (mostly faithful) port of the ahorn-only dark matter entity
- Lightning Wallbounce Controller
  - gives lightning solid hitboxes like fire and ice barriers while the player is dashing (or always)
- Seamless Lightning Controller
  - makes lightning visually connect to the edges of rooms
- Intro Facing Controller
  - makes the map intro animation face whatever way u want
- More Colorgrade Options Controller
  - lets you modify/fix some bugs with the vanilla colorgrading system, such as colorgrades interpolating between the closest colors and the colorgrading for the green channel being a little off what it should be
- Dream Dash Through Transition Controller
  - lets u use dream blocks across screen transitions properly. note: while this controller is active, dream dashing into the side of a screen where there is no available transition will kill you
- Dream Lock Block
  - an inactive dream block that can be unlocked to become active
- Glass Lock Block
  - a lock block version of glass blocks from vanilla
- Glass Lock Block Controller
  - a controller for the colors and visuals of glass lock blocks
- Fg Tile Light Occlusion Fix Controller
  - fixes lights with nonstandard radii clipping through foreground tiles and allows some fg tile types to not block light sources
- Disable Auto Camera Offset Controller
  - disables the automatic camera offset applied in some player states such as red bubbles and feathers
- Formation Backdrop Color Controller
  - lets you change the colour of the screen darkening effect that gets applied when you collect a crystal heart or cassette

for all the controllers listed here, just place it in the room where you want its effect. all of the gameplay-related ones have flag fields to only be active when a certain flag is set.

### Triggers

- Parallax Fade Trigger
  - allows you to fade alpha/color of a parallax. note that you need to reset it after every save and quit

if you want to report any bugs or suggest any features then ping me in celestecord (@aonkeeper4)

happy modding :glumbsup:
