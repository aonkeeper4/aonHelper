local introFacingController = {}

local facings = {
    left = -1,
    right = 1
}
local facingsOptions = {
    ["Left"] = facings.left,
    ["Right"] = facings.right
}

introFacingController.name = "aonHelper/IntroFacingController"
introFacingController.texture = "objects/aonHelper/introFacingController"
introFacingController.scale = function(room, entity) return { entity.facing or facings.right, 1 } end
introFacingController.depth = 0
introFacingController.placements = {
    {
        name = "introFacingController",
        data = {
            facing = facings.right
        }
    }
}
introFacingController.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = facingsOptions,
        editable = false
    }
}

return introFacingController
