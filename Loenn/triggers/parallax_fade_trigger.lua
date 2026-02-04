local enums = require("consts.celeste_enums")

local parallaxFade = {}

parallaxFade.name = "aonHelper/ParallaxFadeTrigger"
parallaxFade.fieldInformation = {
    colorFrom = {
        fieldType = "color"
    },
    colorTo = {
        fieldType = "color"
    },
    positionMode = {
        options = enums.trigger_position_modes,
        editable = false
    }
}
parallaxFade.placements = {
    name = "parallax_fade",
    data = {
        colorFrom = "000000",
        colorTo = "ffffff",
        alphaFrom = 0.0,
        alphaTo = 1.0,
        positionMode = "LeftToRight",
        tagToAffect = ""
    }
}

return parallaxFade