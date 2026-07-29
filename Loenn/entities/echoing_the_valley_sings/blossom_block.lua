local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")
local objectDepths = require("consts.object_depths")

local blossomBlock = {}

blossomBlock.name = "aonHelper/BlossomBlock"
blossomBlock.depth = function(room, entity) return entity.depth or -9000 end
blossomBlock.minimumSize = { 16, 16 }
blossomBlock.placements = {
    name = "blossom_block",
    data = {
        width = 16,
        height = 16,
        depth = -9000,
        doNotLoadFlag = "",
        flagOnBreak = ""
    }
}

blossomBlock.fieldOrder = {
    "x", "y", "width", "height",
    "depth", "doNotLoadFlag", "flagOnBreak"
}
blossomBlock.fieldInformation = {
    depth = {
        fieldType = "integer",
        options = objectDepths,
        editable = true
    }
}

local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name
                and (entity.depth or -9000) == (target.depth or -9000)
    end
end

local function getControllerPredicate(entity)
    return function(target)
        return target._name == "aonHelper/BlossomBlockController"
                and (target.affectedDepth or -9000) == (entity.depth or -9000)
    end
end

local function getTileSprite(entity, x, y, frame, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0
        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8
        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16
        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24
        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0
        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16
        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8
        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8
        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0
        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0
        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16
        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)
        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        
        return sprite
    end
end

function blossomBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)
    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 16, entity.height or 16
    local tileWidth, tileHeight = math.floor(width / 8), math.floor(height / 8)
    
    local controller = utils.filter(getControllerPredicate(entity), room.entities)[1] or { spritePath = "" }
    local spritePath = (controller.spritePath or "") ~= "" and controller.spritePath or "objects/aonHelper/blossomBlock/block"

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, spritePath, rectangles)
            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return blossomBlock