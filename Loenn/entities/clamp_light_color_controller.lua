local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local clampLightColorController = {}

local clampMethods = {
    clamp = 0,
    tint = 1
}
local clampMethodsOptions = {
    ["Clamp"] = clampMethods.clamp,
    ["Tint"] = clampMethods.tint
}

clampLightColorController.name = "aonHelper/ClampLightColorController"
clampLightColorController.depth = 0
clampLightColorController.placements = {
    {
        name = "clampLightColorController",
        data = {
            color = "ffffff",
            clampMethod = clampMethods.clamp
        }
    }
}

clampLightColorController.fieldOrder = {
    "x", "y",
    "color", "clampMethod"
}
clampLightColorController.fieldInformation = {
    color = {
        fieldType = "color"
    },
    clampMethod = {
        options = clampMethodsOptions,
        editable = false
    }
}

local controllerSprites = {
    [clampMethods.clamp] = "objects/aonHelper/clampLightColorController/clamp",
    ["Clamp"] = "objects/aonHelper/clampLightColorController/clamp",
    [clampMethods.tint] = "objects/aonHelper/clampLightColorController/tint",
    ["Tint"] = "objects/aonHelper/clampLightColorController/tint",
}

function clampLightColorController.sprite(room, entity)
    local clampMethod = entity.clampMethod or clampMethods.clamp
    local spritePath = controllerSprites[clampMethod]
    local clampColor = utils.getColor(entity.color or { 1.0, 1.0, 1.0, 1.0 })
    
    local baseSprite = drawableSprite.fromTexture(spritePath .. "00", entity)
    baseSprite:setColor({ 1.0, 1.0, 1.0, 1.0 })
    local colorSpriteA = drawableSprite.fromTexture(spritePath .. "01", entity)
    colorSpriteA:setColor(clampColor)
    local colorSpriteB = drawableSprite.fromTexture(spritePath .. "02", entity)
    colorSpriteB:setColor({
        math.min(clampColor[1] * 2, 1.0),
        math.min(clampColor[2] * 2, 1.0),
        math.min(clampColor[3] * 2, 1.0),
        1.0
    })
    
    return { baseSprite, colorSpriteA, colorSpriteB }
end

function clampLightColorController.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 12, y - 12, 24, 24)
end

return clampLightColorController
