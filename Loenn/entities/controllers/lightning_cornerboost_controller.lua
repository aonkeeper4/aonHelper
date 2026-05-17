local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local lightningCornerboostController = {}

lightningCornerboostController.name = "aonHelper/LightningCornerboostController"
lightningCornerboostController.texture = "objects/aonHelper/lightningCornerboostController"
lightningCornerboostController.placements = {
    {
        name = "lightning_cornerboost_controller",
        data = {
            always = false,
            flag = "",
            global = false
        }
    }
}

lightningCornerboostController.fieldOrder = {
    "x", "y",
    "always",
    "flag", "global"
}

return aonHelper.controllerify(lightningCornerboostController, {
    global = {
        attributeName = "global",
        attributeDefault = false
    }
})