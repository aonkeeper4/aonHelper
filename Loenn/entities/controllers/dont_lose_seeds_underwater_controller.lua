local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local dontLoseSeedsUnderwaterController = {}

dontLoseSeedsUnderwaterController.name = "aonHelper/DontLoseSeedsUnderwaterController"
dontLoseSeedsUnderwaterController.texture = "objects/aonHelper/dontLoseSeedsUnderwaterController"
dontLoseSeedsUnderwaterController.placements = {
    {
        name = "dont_lose_seeds_underwater_controller",
        data = {
            flag = "",
            global = false
        }
    }
}

dontLoseSeedsUnderwaterController.fieldOrder = {
    "x", "y",
    "flag", "global"
}

return aonHelper.controllerify(springSpeedThresholdController)
