local springSpeedThresholdController = {}

springSpeedThresholdController.name = "aonHelper/SpringSpeedThresholdController"
springSpeedThresholdController.texture = "objects/aonHelper/springSpeedThresholdController"
springSpeedThresholdController.depth = 0
springSpeedThresholdController.placements = {
    {
        name = "spring_speed_threshold_controller",
        data = {
            thresholdX = 240.0,
            thresholdY = 0.0,
            flag = ""
        }
    }
}

springSpeedThresholdController.fieldOrder = {
    "x", "y",
    "thresholdX", "thresholdY",
    "flag"
}

return springSpeedThresholdController