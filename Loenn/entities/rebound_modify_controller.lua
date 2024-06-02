local reboundModifyController = {}

reboundModifyController.name = "aonHelper/ReboundModifyController"
reboundModifyController.texture = "objects/aonHelper/reboundModifyController"
reboundModifyController.depth = 0
reboundModifyController.placements = {
    {
        name = "reboundModifyController",
        data = {
            reflectSpeed = true,
            reflectSpeedMultiplier = 0.5,
            refillDash = false,
            flag = "",
        }
    }
}

return reboundModifyController
