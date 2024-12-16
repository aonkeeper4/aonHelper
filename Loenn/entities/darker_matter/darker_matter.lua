local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")

local darkerMatter = {}

darkerMatter.name = "aonHelper/DarkerMatter"
darkerMatter.depth = -9000
darkerMatter.placements = {
    {
        name = "darkerMatter",
        data = {
            width = 16,
            height = 16,
            wrapHorizontal = false,
            wrapVertical = false,
        }
    }
}
darkerMatter.canResize = { true, true }

local fillColor = { 94 / 255, 8 / 255, 36 / 255, 100 / 255 }
local borderColor = { 71 / 255, 19 / 255, 76 / 255 }
local warpLineColor = { 119 / 255, 81 / 255, 33 / 255 }
local warpLineThickness = 5
function darkerMatter.sprite(room, entity)
    local sprites = {}

    table.insert(sprites,
        drawableRectangle.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height, fillColor,
            borderColor))

    if entity.wrapHorizontal then
        table.insert(sprites,
            drawableLine.fromPoints({ entity.x, entity.y, entity.x, entity.y + entity.height }, warpLineColor,
                warpLineThickness))
        table.insert(sprites,
            drawableLine.fromPoints(
                { entity.x + entity.width, entity.y, entity.x + entity.width, entity.y + entity.height }, warpLineColor,
                warpLineThickness))
    end
    if entity.wrapVertical then
        table.insert(sprites,
            drawableLine.fromPoints({ entity.x, entity.y, entity.x + entity.width, entity.y }, warpLineColor,
                warpLineThickness))
        table.insert(sprites,
            drawableLine.fromPoints(
                { entity.x, entity.y + entity.height, entity.x + entity.width, entity.y + entity.height }, warpLineColor,
                warpLineThickness))
    end

    return sprites
end

-- return darkerMatter
