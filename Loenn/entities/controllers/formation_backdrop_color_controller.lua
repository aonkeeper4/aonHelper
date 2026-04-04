local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local formationBackdropColorController = {}

formationBackdropColorController.name = "aonHelper/FormationBackdropColorController"
formationBackdropColorController.placements = {
    name = "formation_backdrop_color_controller",
    data = {
        color = "000000",
        alpha = 0.85
    }
}

formationBackdropColorController.fieldOrder = {
    "x", "y",
    "color", "alpha"
}
formationBackdropColorController.fieldInformation = {
    color = {
        fieldType = "color"
    },
    alpha = {
        minimumValue = 0.0,
        maximumValue = 1.0
    }
}

local bgSpritePath = "objects/aonHelper/formationBackdropColorController/base"
local overlaySpritePath = "objects/aonHelper/formationBackdropColorController/overlay"

function formationBackdropColorController.sprite(room, entity)
    local color = utils.getColor(entity.color or { 1.0, 1.0, 1.0, 1.0 })
    local alpha = entity.alpha or 0.85
    
    local baseSprite = drawableSprite.fromTexture(bgSpritePath, entity)
    baseSprite:setColor({ 1.0, 1.0, 1.0, 1.0 })
    local overlaySprite = drawableSprite.fromTexture(overlaySpritePath, entity)
    overlaySprite:setColor({ color[1], color[2], color[3], alpha })
    
    return { baseSprite, overlaySprite }
end

function formationBackdropColorController.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 12, y - 12, 24, 24)
end

return formationBackdropColorController