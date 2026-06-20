local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local introFacingController = {}

local facings = {
    left = -1,
    right = 1
}
local facingsOptions = {
    ["Left"] = facings.left,
    ["Right"] = facings.right
}

introFacingController.name = "aonHelper/IntroFacingController"
introFacingController.texture = "objects/aonHelper/introFacingController"
introFacingController.scale = function(room, entity) return { entity.facing or facings.right, 1 } end
introFacingController.placements = {
    {
        name = "intro_facing_controller",
        data = {
            facing = facings.right,
            flag = "",
            global = false
        }
    }
}

introFacingController.fieldOrder = {
    "x", "y",
    "facing",
    "flag", "global"
}
introFacingController.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = facingsOptions,
        editable = false
    }
}

return aonHelper.controllerify(introFacingController)
