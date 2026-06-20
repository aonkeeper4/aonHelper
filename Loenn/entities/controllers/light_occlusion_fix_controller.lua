local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local lightOcclusionFixController = {}

lightOcclusionFixController.name = "aonHelper/LightOcclusionFixController"
lightOcclusionFixController.texture = "objects/aonHelper/lightOcclusionFixController"
lightOcclusionFixController.placements = {
    name = "light_occlusion_fix_controller",
    data = {
        noOcclusionTiletypes = "",
        global = true
    }
}

lightOcclusionFixController.fieldOrder = {
    "x", "y",
    "noOcclusionTiletypes",
    "global"
}
-- woo yay i love stealing from sorbet helper
lightOcclusionFixController.fieldInformation = {
    noOcclusionTiletypes = {
        fieldType = "aon_helper.tiletype_list"
    }
}

return aonHelper.controllerify(lightOcclusionFixController, aonHelper.globalByDefault)