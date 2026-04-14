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
        fieldType = "aon_helper.tiletype_list"
    }
}

return lightOcclusionFixController