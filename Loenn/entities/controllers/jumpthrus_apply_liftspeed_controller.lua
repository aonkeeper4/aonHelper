local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local jumpThrusApplyLiftSpeedController = {}

jumpThrusApplyLiftSpeedController.name = "aonHelper/JumpThrusApplyLiftSpeedController"
jumpThrusApplyLiftSpeedController.texture = "objects/aonHelper/jumpThrusApplyLiftSpeedController"
jumpThrusApplyLiftSpeedController.placements = {
    {
        name = "jumpthrus_apply_liftspeed_controller",
        data = {
            flag = "",
            global = false
        }
    }
}

jumpThrusApplyLiftSpeedController.fieldOrder = {
    "x", "y",
    "flag", "global"
}

return aonHelper.controllerify(jumpThrusApplyLiftSpeedController)