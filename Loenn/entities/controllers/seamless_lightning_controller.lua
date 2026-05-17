local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local seamlessLightningController = {}

seamlessLightningController.name = "aonHelper/SeamlessLightningController"
seamlessLightningController.texture = "objects/aonHelper/seamlessLightningController"
seamlessLightningController.placements = {
    {
        name = "seamless_lightning_controller",
        data = { }
    }
}

return aonHelper.controllerify(seamlessLightningController, {
    global = true
})