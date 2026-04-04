local jumpThrusApplyLiftSpeedController = {}

jumpThrusApplyLiftSpeedController.name = "aonHelper/JumpThrusApplyLiftSpeedController"
jumpThrusApplyLiftSpeedController.texture = "objects/aonHelper/jumpThrusApplyLiftSpeedController"
jumpThrusApplyLiftSpeedController.depth = 0
jumpThrusApplyLiftSpeedController.placements = {
    {
        name = "jumpthrus_apply_liftspeed_controller",
        data = {
            flag = ""
        }
    }
}

jumpThrusApplyLiftSpeedController.fieldOrder = {
    "x", "y",
    "flag"
}

return jumpThrusApplyLiftSpeedController