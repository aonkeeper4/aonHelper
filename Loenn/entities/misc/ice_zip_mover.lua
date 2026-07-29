local utils = require("utils")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")

local iceZipMover = {}

iceZipMover.name = "aonHelper/IceZipMover"
iceZipMover.depth = -9000
iceZipMover.nodeVisibility = "never"
iceZipMover.nodeLimits = {1, 1}
iceZipMover.warnBelowSize = {16, 16}
iceZipMover.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            breakEarly = false,
            spriteDir = "",
            ropeColor = "663931",
            ropeLightColor = "9b6157",
            sparkParticleColor = "fff538",
            breakParticleColor = "33ffe7",
            breakParticleFadeColor = "0151d0",
            surfaceIndex = 8,
            moveSfx = "event:/game/01_forsaken_city/zip_mover",
            breakSfx = "event:/game/09_core/iceblock_touch",
            respawnSfx = "event:/game/09_core/iceblock_reappear"
        }
    },
    {
        name = "break_early",
        data = {
            width = 16,
            height = 16,
            breakEarly = true,
            spriteDir = "",
            ropeColor = "663931",
            ropeLightColor = "9b6157",
            sparkParticleColor = "fff538",
            breakParticleColor = "33ffe7",
            breakParticleFadeColor = "0151d0",
            surfaceIndex = 8,
            moveSfx = "event:/game/01_forsaken_city/zip_mover",
            breakSfx = "event:/game/09_core/iceball_break",
            respawnSfx = "event:/game/09_core/iceblock_reappear"
        }
    },
}

iceZipMover.fieldOrder = {
    "x", "y", "width", "height",
    "breakEarly",
    "spriteDir", "ropeColor", "ropeLightColor",
    "sparkParticleColor", "breakParticleColor", "breakParticleFadeColor",
    "surfaceIndex", "moveSfx", "breakSfx", "respawnSfx"
}
iceZipMover.fieldInformation = {
    ropeColor = {
        fieldType = "color"
    },
    ropeLightColor = {
        fieldType = "color"
    },
    sparkParticleColor = {
        fieldType = "color"
    },
    breakParticleColor = {
        fieldType = "color"
    },
    breakParticleFadeColor = {
        fieldType = "color"
    },
    surfaceIndex = {
        fieldType = "integer"
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local defaultSpriteDir = "objects/aonHelper/iceZipMover"

function iceZipMover.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    
    local centerX, centerY = x + halfWidth, y + halfHeight
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local spriteDir = (entity.spriteDir or "") ~= "" and entity.spriteDir or defaultSpriteDir
    local blockTexture = spriteDir .. "/block"
    local crystalTexture = spriteDir .. "/center00"
    local cogTexture = spriteDir .. "/cog"
    local ropeColor = utils.getColor(entity.ropeColor or {102 / 255, 57 / 255, 49 / 255})
    
    local sprites = {}

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = drawableLine.fromPoints(points, ropeColor, 1)
    local rightLine = drawableLine.fromPoints(points, ropeColor, 1)
    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)
    leftLine.depth = 5000
    rightLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    local nodeCogSprite = drawableSprite.fromTexture(cogTexture, entity)
    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)
    table.insert(sprites, nodeCogSprite)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    for _, sprite in ipairs(ninePatch:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)

    return sprites
end

function iceZipMover.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local spriteDir = (entity.spriteDir or "") ~= "" and entity.spriteDir or defaultSpriteDir
    local cogTexture = spriteDir .. "/cog"
    
    local cogSprite = drawableSprite.fromTexture(cogTexture, entity)
    local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height

    local mainRectangle = utils.rectangle(x, y, width, height)
    local nodeRectangle = utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)

    return mainRectangle, {nodeRectangle}
end

return iceZipMover