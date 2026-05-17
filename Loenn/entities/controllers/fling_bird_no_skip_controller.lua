local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local flingBirdNoSkipController = {}

flingBirdNoSkipController.name = "aonHelper/FlingBirdNoSkipController"
flingBirdNoSkipController.texture = "objects/aonHelper/flingBirdNoSkipController"
flingBirdNoSkipController.placements = {
    {
        name = "fling_bird_no_skip_controller",
        data = {
            flag = "",
            global = false
        }
    }
}

flingBirdNoSkipController.fieldOrder = {
    "x", "y",
    "flag", "global"
}

return aonHelper.controllerify(flingBirdNoSkipController, {
    global = {
        attributeName = "global",
        attributeDefault = false
    }
})