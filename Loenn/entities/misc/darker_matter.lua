local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local darkerMatter = {}

darkerMatter.name = "aonHelper/DarkerMatter"
darkerMatter.depth = -8000
darkerMatter.placements = {
    {
        name = "darker_matter",
        data = {
            width = 16,
            height = 16,
            warpHorizontal = false,
            warpVertical = false,
            refillDash = true,
            speedThreshold = 0,
            speedLimit = 200,
            colors = "5e0824,47134c",
            warpColors = "6a391c,775121",
            centerAlpha = 0.4,
            edgeAlpha = 1.0,
            particleAlpha = 1.0
        }
    }
}

darkerMatter.fieldOrder = {
    "x", "y", "width", "height",
    "warpHorizontal", "warpVertical", "refillDash",
    "speedThreshold", "speedLimit",
    "colors", "warpColors", "centerAlpha", "edgeAlpha", "particleAlpha"
}
darkerMatter.fieldInformation = {
    speedThreshold = {
        minimumValue = 0
    },
    colors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color"
        },
        minimumElements = 2
    },
    warpColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color"
        },
        minimumElements = 2
    },
    centerAlpha = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    edgeAlpha = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    particleAlpha = {
        minimumValue = 0.0,
        maximumValue = 1.0
    }
}

local function valueOrFallbackIfEmpty(value, fallback)
    return (value or "" ~= "") and value or fallback
end

function darkerMatter.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    
    local colors = (entity.colors or ""):split(",")
    local warpColors = (entity.warpColors or ""):split(",")
    
    local fillColor = utils.getColor(valueOrFallbackIfEmpty(colors[1], { 94 / 255, 8 / 255, 36 / 255 })); fillColor[4] = entity.centerAlpha or 0.4
    local borderColor = utils.getColor(valueOrFallbackIfEmpty(colors[2], { 71 / 255, 19 / 255, 76 / 255 })); borderColor[4] = entity.edgeAlpha or 1.0
    local warpLineColor = utils.getColor(valueOrFallbackIfEmpty(warpColors[1], { 119 / 255, 81 / 255, 33 / 255 })); warpLineColor[4] = borderColor[4]
    local warpLineThickness = 3
    
    local sprites = {}
    
    table.insert(sprites, drawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor))

    if entity.warpHorizontal or false then
        table.insert(sprites, drawableRectangle.fromRectangle("filled", x - warpLineThickness / 2, y, warpLineThickness, height, warpLineColor))
        table.insert(sprites, drawableRectangle.fromRectangle("filled", x + width - warpLineThickness / 2, y, warpLineThickness, height, warpLineColor))
    end
    if entity.warpVertical or false then
        table.insert(sprites, drawableRectangle.fromRectangle("filled", x, y - warpLineThickness / 2, width, warpLineThickness, warpLineColor))
        table.insert(sprites, drawableRectangle.fromRectangle("filled", x, y + height - warpLineThickness / 2, width, warpLineThickness, warpLineColor))
    end

    return sprites
end

function darkerMatter.rotate(room, entity, direction)
    local warpHorizontal = entity.warpHorizontal or false
    local warpVertical = entity.warpVertical or false
    
    local shouldRotate = aonHelper.mod(direction, 2) ~= 0
    if not shouldRotate then return end

    if warpHorizontal and not warpVertical then
        entity.warpHorizontal = false
        entity.warpVertical = true
    elseif warpVertical and not warpHorizontal then
        entity.warpHorizontal = true
        entity.warpVertical = false
    end
end

return darkerMatter