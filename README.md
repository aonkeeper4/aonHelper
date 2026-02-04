# aon helper

yea this mod does stuff alright

current features:

### Entities

- Resizable Heart
  - its a crystal heart. you can resize it too (pretty funky)
- Feather Dash Switch
  - dash switch except you bounce into it with a feather to activate it
- Feather Bounce Scam Controller
  - if your feather timer is low enough trying to bounce will uh instead Not Do That which is annoying
  - this fixes that (you can set the feather timer scam threshold to whatever u want. including 0 for it to not happen at all)
- Modify Rebound Controller
  - modifies the player's Rebound method to set/conserve speed (allows for independent control of the x and y directions as well as top vs side rebounds)
- Fling Bird No Skip Controller
  - makes vanilla flingbirds not run away when you're too far past the node
- Fg Styleground Bloom Controller
  - allows you to apply bloom to foreground stylegrounds (either all of them or only ones with a certain tag)
- Unforgiving Spikes
  - spikes that kill you no matter what. basically pretty killboxes
- Clamp Light Color Controller
  - allows you to set the "maximum" light color, i.e. no light is allowed to have a color brighter than this. useful for when you want overlapping light sources to not blend into white or when you want to make white stuff a different color. allows both "clamp" and "tint" options
- Darker Matter
  - basically a port of the Ahorn-only entity Dark Matter, but with 2000% less crashes
- Lightning Cornerboost Controller
  - should really be called "Lightning Wallbounce Controller" but . gives lightning solid hitboxes like fire and ice barriers while the player is dashing (or always)
- Seamless Lightning Controller
  - ever gotten really pissed when trying to connect lightning between rooms? me too! makes lightning visually connect to the edges of rooms so u dont have to fiddle with eeveehelper global modifiers not working
- Intro Facing Controller
  - some intro types don't let you set the player facing with spawn facing triggers which sucks so i made this. makes the map intro animation face whatever way u want
- Quantize Colorgrade Controller
  - when applying a colorgrade, if the color to be mapped does not exist, celeste will interpolate between the 4 adjacent colors on the colorgrade to calculate the resulting color. with this controller, this will not happen and instead the closest color will be used
- Dream Dash Through Transition Controller
  - lets u use dream blocks across screen transitions properly. note: while this controller is active, dream dashing into the side of a screen where there is no available transition will kill you
- Dream Lock Block
  - an inactive dream block that can be unlocked to become active
- Glass Lock Block
  - a lock block version of Glass Blocks from vanilla
- Glass Lock Block Controller
  - a controller for the colors and visuals of Glass Lock Blocks
- Fg Tile Light Occlusion Fix Controller
  - fixes lights with non-multiple of 8 radii clipping through foreground tiles and allows some fg tile types to not block light sources
- Disable Auto Camera Offset Controller
  - disables the automatic camera offset applied in some player states such as red bubbles and feathers
- Formation Backdrop Color Controller
  - lets you change the colour of the screen darkening effect that gets applied when you collect a crystal heart for example

for all the controllers listed here, just place it in the room where you want its effect. all of the gameplay-related ones have flag fields to only be active when a certain flag is set.

### Triggers

- Parallax Fade Trigger
  - allows you to fade alpha/color of a parallax. note that you do need to reset it after every save and quit because i don't understand what black magic frosthelper is doing to make styleground blend modes persist

if you want to report any bugs or suggest any features then ping me in celestecord (@aonkeeper4)
happy modding :glumbsup:
