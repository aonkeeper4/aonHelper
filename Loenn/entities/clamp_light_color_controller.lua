local clampLightColorController = {}

local clampMethods = {
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
            clampMethod = "Clamp"
        }
    }
}
clampLightColorController.fieldInformation = {
    color = {
        fieldType = "color"
    },
    clampMethod = {
        options = clampMethods,
        editable = false
    }
}

clampLightColorController.texture = function(room, entity)
    return "objects/aonHelper/clampLightColorController/" .. string.lower(entity.clampMethod or "clamp")
end

return clampLightColorController
