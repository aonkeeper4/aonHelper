local lightOcclusionFixController = {}

lightOcclusionFixController.name = "aonHelper/LightOcclusionFixController"
lightOcclusionFixController.texture = "objects/aonHelper/lightOcclusionFixController"
lightOcclusionFixController.placements = {
    name = "light_occlusion_fix_controller",
    data = {
        noOcclusionTiletypes = ""
    }
}

lightOcclusionFixController.fieldOrder = {
    "x", "y",
    "noOcclusionTiletypes"
}
-- woo yay i love stealing from sorbet helper
lightOcclusionFixController.fieldInformation = {
    noOcclusionTiletypes = {
        fieldType = "aon_helper.tiletype_list"
    }
}

return lightOcclusionFixController