local enums = require("consts.celeste_enums")

local parallaxAlphaFade = {}

parallaxAlphaFade.name = "aonHelper/ParallaxAlphaFadeTrigger"
parallaxAlphaFade.fieldInformation = {
    positionMode = {
        options = enums.trigger_position_modes,
        editable = false
    }
}
parallaxAlphaFade.placements = {
    name = "parallaxAlphaFade",
    data = {
        alphaFrom = 0.0,
        alphaTo = 1.0,
        positionMode = "LeftToRight",
        tagToAffect = ""
    }
}

return parallaxAlphaFade