local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local springSpeedThresholdController = {}

springSpeedThresholdController.name = "aonHelper/SpringSpeedThresholdController"
springSpeedThresholdController.texture = "objects/aonHelper/springSpeedThresholdController"
springSpeedThresholdController.placements = {
    {
        name = "spring_speed_threshold_controller",
        data = {
            thresholdX = 240.0,
            thresholdY = 0.0,
            flag = "",
            global = false
        }
    }
}

springSpeedThresholdController.fieldOrder = {
    "x", "y",
    "thresholdX", "thresholdY",
    "flag", "global"
}

return aonHelper.controllerify(springSpeedThresholdController, {
    global = {
        attributeName = "global",
        attributeDefault = false
    }
})