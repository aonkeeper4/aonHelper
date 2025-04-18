local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local darkerMatter = {}

darkerMatter.name = "aonHelper/DarkerMatter"
darkerMatter.depth = -8000
darkerMatter.placements = {
    {
        name = "darkerMatter",
        data = {
            width = 16,
            height = 16,
            warpHorizontal = false,
            warpVertical = false,
            speedThreshold = 0,
            speedLimit = 200,
            colors = "5e0824,47134c",
            warpColors = "6a391c,775121"
        }
    }
}

darkerMatter.fieldOrder = {
    "x", "y", "width", "height",
    "warpHorizontal", "warpVertical",
    "speedThreshold", "speedLimit",
    "colors", "warpColors"
}
darkerMatter.fieldInformation = {
    speedThreshold = {
        minimumValue = 0
    },
    speedLimit = {
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
    }
}

local function valueOrFallbackIfEmpty(value, fallback)
    return ((value or "") ~= "") and value or fallback
end

function darkerMatter.sprite(room, entity)
    local colors = (entity.colors or ""):split(",")
    local warpColors = (entity.warpColors or ""):split(",")
    
    local fillColor = utils.getColor(valueOrFallbackIfEmpty(colors[1], { 94 / 255, 8 / 255, 36 / 255 })); fillColor[4] = 0.4
    local borderColor = utils.getColor(valueOrFallbackIfEmpty(colors[2], { 71 / 255, 19 / 255, 76 / 255 }))
    local warpLineColor = utils.getColor(valueOrFallbackIfEmpty(warpColors[1], { 119 / 255, 81 / 255, 33 / 255 }))
    local warpLineThickness = 3
    
    local sprites = {}
    
    table.insert(sprites, drawableRectangle.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height, fillColor, borderColor))

    if entity.warpHorizontal then
        table.insert(sprites, drawableLine.fromPoints({ entity.x, entity.y, entity.x, entity.y + entity.height }, warpLineColor, warpLineThickness))
        table.insert(sprites, drawableLine.fromPoints({ entity.x + entity.width, entity.y, entity.x + entity.width, entity.y + entity.height }, warpLineColor, warpLineThickness))
    end
    if entity.warpVertical then
        table.insert(sprites, drawableLine.fromPoints({ entity.x, entity.y, entity.x + entity.width, entity.y }, warpLineColor, warpLineThickness))
        table.insert(sprites, drawableLine.fromPoints({ entity.x, entity.y + entity.height, entity.x + entity.width, entity.y + entity.height }, warpLineColor, warpLineThickness))
    end

    return sprites
end

return darkerMatter