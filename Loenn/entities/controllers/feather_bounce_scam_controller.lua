local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local featherBounceScamController = {}

featherBounceScamController.name = "aonHelper/FeatherBounceScamController"
featherBounceScamController.texture = "objects/aonHelper/featherBounceScamController"
featherBounceScamController.placements = {
    {
        name = "feather_bounce_scam_controller",
        data = {
            featherBounceScamThreshold = 0.2,
            flag = "",
            global = false
        }
    }
}

featherBounceScamController.fieldOrder = {
    "x", "y",
    "featherBounceScamThreshold",
    "flag", "global"
}

return aonHelper.controllerify(featherBounceScamController)