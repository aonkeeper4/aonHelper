local clampLightColorController = {}

local clampModes = {
    "Clamp",
    "Tint"
}

clampLightColorController.name = "aonHelper/ClampLightColorController"
clampLightColorController.depth = 0
clampLightColorController.placements = {
    {
        name = "clampLightColorController",
        data = {
            color = "ffffff",
            clampMode = "Clamp"
        }
    }
}
clampLightColorController.fieldInformation = {
    color = {
        fieldType = "color"
    },
    clampMode = {
        options = clampModes,
        editable = false
    }
}

clampLightColorController.texture = function(room, entity)
    return "objects/aonHelper/clampLightColorController/" .. string.lower(entity.clampMode or "clamp")
end

return clampLightColorController
