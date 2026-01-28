local fakeTilesHelper = require("helpers.fake_tiles")

local lightOcclusionFixController = {}

lightOcclusionFixController.name = "aonHelper/LightOcclusionFixController"
lightOcclusionFixController.texture = "objects/aonHelper/lightOcclusionFixController"
lightOcclusionFixController.placements = {
    name = "light_occlusion_fix_controller",
    data = {
        noOcclusionTileTypes = ""
    }
}

lightOcclusionFixController.fieldOrder = {
    "x", "y",
    "noOcclusionTileTypes"
}
-- woo yay i love stealing from sorbet helper
lightOcclusionFixController.fieldInformation = {
    noOcclusionTileTypes = {
        fieldType = "aonHelper.unicode_char_list",
        minimumElements = 0,
        elementDefault = "3",
        elementOptions = {
            options = function()
                return fakeTilesHelper.getTilesOptions("tilesFg")
            end,
            editable = false
        }
    }
}

return lightOcclusionFixController