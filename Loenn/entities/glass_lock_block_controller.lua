local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local glassLockBlockController = {}

glassLockBlockController.name = "aonHelper/GlassLockBlockController"
glassLockBlockController.depth = 0
glassLockBlockController.placements = {
    {
        name = "controller",
        data = {
            bgColor = "302040",
            lineColor = "ffffff",
            rayColor = "ffffff",
            starColors = "ff7777,77ff77,7777ff,ff77ff,77ffff,ffff77",
            wavy = false,
            vanillaEdgeBehavior = false,
            persistent = false
        }
    },
    {
        name = "controller_vanilla",
        data = {
            bgColor = "0d2e89",
            lineColor = "ffffff",
            rayColor = "ffffff",
            starColors = "7f9fba,9bd1cd,bacae3",
            wavy = true,
            vanillaEdgeBehavior = true,
            persistent = false
        }
    },
}

glassLockBlockController.fieldOrder = {
    "x", "y",
    "bgColor", "lineColor", "rayColor", "starColors",
    "wavy", "vanillaEdgeBehavior", "persistent"
}
glassLockBlockController.fieldInformation = {
    bgColor = {
        fieldType = "color"
    },
    lineColor = {
        fieldType = "color"
    },
    rayColor = {
        fieldType = "color"
    },
    starColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    }
}

local texturePrefix = "objects/aonHelper/lockBlocks/glassLockBlockController"

local defaultBgColor = { 13 / 255, 46 / 255, 137 / 255, 1.0 }
local defaultLineColor = { 1.0, 1.0, 1.0, 1.0 }
local defaultRayColor = { 1.0, 1.0, 1.0, 1.0 }
local defaultStarColors = {
    { 127 / 255, 159 / 255, 186 / 255, 1.0 },
    { 155 / 255, 209 / 255, 205 / 255, 1.0 },
    { 186 / 255, 202 / 255, 227 / 255, 1.0 }
}

function glassLockBlockController.sprite(room, entity)
    local bgColor = utils.getColor(entity.bgColor or defaultBgColor)
    local lineColor = utils.getColor(entity.lineColor or defaultLineColor)
    local rayColor = utils.getColor(entity.rayColor or defaultRayColor)
    
    local starColors = {}
    if entity.starColors ~= nil then
        local starColorsStrings = entity.starColors:split(",")()
        for i = 0, 2 do
            local index = math.fmod(i, #starColorsStrings) + 1
            local starColor = utils.getColor(starColorsStrings[index]) or defaultStarColors[index]
            table.insert(starColors, i + 1, starColor)
        end
    else starColors = defaultStarColors end
    
    local sprites = {}
    
    local baseSprite = drawableSprite.fromTexture(texturePrefix .. "/base", entity)
    table.insert(sprites, baseSprite)
    
    local bgSprite = drawableSprite.fromTexture(texturePrefix .. "/bg", entity)
    bgSprite:setColor(bgColor)
    table.insert(sprites, bgSprite)
    
    for i = 0, 2 do
        local starTexture = texturePrefix .. "/stars" .. string.format("%02d", i)
        local starSprite = drawableSprite.fromTexture(starTexture, entity)
        starSprite:setColor(starColors[i + 1])
        table.insert(sprites, starSprite)
    end
    
    local rayTexture = texturePrefix .. (entity.wavy and "/rays01" or "/rays00")
    local raySprite = drawableSprite.fromTexture(rayTexture, entity)
    raySprite:setColor({ rayColor[1], rayColor[2], rayColor[3], 0.4 })
    table.insert(sprites, raySprite)
    
    local lockSprite = drawableSprite.fromTexture(texturePrefix .. "/lock", entity)
    table.insert(sprites, lockSprite)
    
    local vanillaEdgeBehavior = true
    if entity.vanillaEdgeBehavior ~= nil then vanillaEdgeBehavior = entity.vanillaEdgeBehavior end
    local outlineTexture = texturePrefix .. (vanillaEdgeBehavior and "/outline01" or "/outline00")
    local outlineSprite = drawableSprite.fromTexture(outlineTexture, entity)
    outlineSprite:setColor(lineColor)
    table.insert(sprites, outlineSprite)
    
    return sprites
end

function glassLockBlockController.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 12, y - 12, 24, 24)
end

return glassLockBlockController
