local enums = require("consts.celeste_enums")

local parallaxColorFade = {}

parallaxColorFade.name = "aonHelper/ParallaxColorFadeTrigger"
parallaxColorFade.fieldInformation = {
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
parallaxColorFade.placements = {
    name = "parallaxColorFade",
    data = {
        colorFrom = "000000",
        colorTo = "ffffff",
        positionMode = "LeftToRight",
        tagToAffect = ""
    }
}

return parallaxColorFade