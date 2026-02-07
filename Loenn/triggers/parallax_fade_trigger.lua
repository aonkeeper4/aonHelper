local enums = require("consts.celeste_enums")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local parallaxFade = {}

parallaxFade.name = "aonHelper/ParallaxFadeTrigger"
parallaxFade.fieldInformation = {
    colorFrom = {
        fieldType = "color",
        allowEmpty = true
    },
    colorTo = {
        fieldType = "color",
        allowEmpty = true
    },
    alphaFrom = {
        validator = aonHelper.numberAllowEmpty(0.0, 1.0)
    },
    alphaTo = {
        validator = aonHelper.numberAllowEmpty(0.0, 1.0)
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
        alphaFrom = "0.0",
        alphaTo = "1.0",
        positionMode = "LeftToRight",
        tagToAffect = ""
    }
}

return parallaxFade