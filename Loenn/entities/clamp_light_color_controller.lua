local clampLightColorController = {}

clampLightColorController.name = "aonHelper/ClampLightColorController"
clampLightColorController.depth = 0
clampLightColorController.texture = "objects/aonHelper/clampLightColorController"
clampLightColorController.placements = {
    {
        name = "clampLightColorController",
        data = {
            color = "ffffff",
        }
    }
}
clampLightColorController.fieldInformation = {
    color = {
        fieldType = "color"
    }
}

return clampLightColorController
