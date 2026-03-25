local lightningCornerboostController = {}

lightningCornerboostController.name = "aonHelper/LightningCornerboostController"
lightningCornerboostController.texture = "objects/aonHelper/lightningCornerboostController"
lightningCornerboostController.depth = 0
lightningCornerboostController.placements = {
    {
        name = "lightning_cornerboost_controller",
        data = {
            always = false,
            flag = ""
        }
    }
}

lightningCornerboostController.fieldOrder = {
    "x", "y",
    "always", "flag"
}

return lightningCornerboostController