local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local disableAutoCameraOffsetController = {}

disableAutoCameraOffsetController.name = "aonHelper/DisableAutoCameraOffsetController"
disableAutoCameraOffsetController.texture = "objects/aonHelper/disableAutoCameraOffsetController"
disableAutoCameraOffsetController.placements = {
    name = "disable_auto_camera_offset_controller",
    data = {
        disableAutoCameraOffset = true,
        disableCameraUpdate = false,
        flag = "",
        global = false
    }
}

disableAutoCameraOffsetController.fieldOrder = {
    "x", "y",
    "disableAutoCameraOffset", "disableCameraUpdate",
    "flag", "global"
}

return aonHelper.controllerify(disableAutoCameraOffsetController)