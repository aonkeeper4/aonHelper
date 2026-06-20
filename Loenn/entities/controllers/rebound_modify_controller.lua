local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local reboundModifyController = {}

local modes = {
    multiplier = 0,
    constant = 1
}
local modesOptions = {
    ["Multiplier"] = modes.multiplier,
    ["Constant"] = modes.constant
}

reboundModifyController.name = "aonHelper/ReboundModifyController"
reboundModifyController.texture = "objects/aonHelper/reboundModifyController"
reboundModifyController.placements = {
    {
        name = "rebound_modify_controller",
        data = {
            leftRightXMode = modes.multiplier,
            leftRightYMode = modes.constant,
            leftRightXModifier = -0.5,
            leftRightYModifier = -120,
            topXMode = modes.constant,
            topYMode = modes.multiplier,
            topXModifier = 0,
            topYModifier = 1,
            refillDash = false,
            flag = "",
            global = false
        }
    }
}

reboundModifyController.fieldOrder = {
    "x", "y",
    "leftRightXMode", "leftRightYMode", "leftRightXModifier", "leftRightYModifier",
    "topXMode", "topYMode", "topXModifier", "topYModifier",
    "refillDash",
    "flag", "global"
}
reboundModifyController.fieldInformation = {
    leftRightXMode = {
        fieldType = "integer",
        options = modesOptions,
        editable = false,
    },
    leftRightYMode = {
        fieldType = "integer",
        options = modesOptions,
        editable = false,
    },
    topXMode = {
        fieldType = "integer",
        options = modesOptions,
        editable = false,
    },
    topYMode = {
        fieldType = "integer",
        options = modesOptions,
        editable = false,
    }
}

return aonHelper.controllerify(reboundModifyController)
