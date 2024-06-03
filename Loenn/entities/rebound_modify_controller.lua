local reboundModifyController = {}

local modes = { ["Multiplier"] = 0, ["Constant"] = 1 }

reboundModifyController.name = "aonHelper/ReboundModifyController"
reboundModifyController.texture = "objects/aonHelper/reboundModifyController"
reboundModifyController.depth = 0
reboundModifyController.placements = {
    {
        name = "reboundModifyController",
        data = {
            leftRightXMode = 0,
            leftRightYMode = 1,
            leftRightXModifier = -0.5,
            leftRightYModifier = -120,
            topXMode = 1,
            topYMode = 0,
            topXModifier = 0,
            topYModifier = 1,
            refillDash = false,
            flag = "",
        }
    }
}
reboundModifyController.fieldInformation = {
    leftRightXMode = {
        fieldType = "integer",
        options = modes,
        editable = false,
    },
    leftRightYMode = {
        fieldType = "integer",
        options = modes,
        editable = false,
    },
    topXMode = {
        fieldType = "integer",
        options = modes,
        editable = false,
    },
    topYMode = {
        fieldType = "integer",
        options = modes,
        editable = false,
    }
}

return reboundModifyController
