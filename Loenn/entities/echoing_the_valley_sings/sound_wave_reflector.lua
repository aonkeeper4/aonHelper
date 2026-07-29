local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local soundWaveReflector = {}

local orientations = {
    left = 0,
    right = 1,
    up = 2,
    down = 3
}
local orientationsOptions = {
    ["Left"] = orientations.left,
    ["Right"] = orientations.right,
    ["Up"] = orientations.up,
    ["Down"] = orientations.down
}

soundWaveReflector.name = "aonHelper/SoundWaveReflector"
soundWaveReflector.depth = -10001
soundWaveReflector.canResize = { true, true }
soundWaveReflector.placements = {}

for name, orientation in pairs(orientations) do
    table.insert(soundWaveReflector.placements, {
        name = "sound_wave_reflector_" .. name,
        data = {
            width = 8,
            height = 8,
            orientation = orientation,
            spriteDir = ""
        }
    })
end

soundWaveReflector.fieldOrder = {
    "x", "y", "width", "height",
    "orientation", "spriteDir"
}
soundWaveReflector.fieldInformation = {
    orientation = {
        fieldType = "integer",
        options = orientationsOptions,
        editable = false
    }
}

local orientationsToSpritePath = {
    [orientations.left] = "/left",
    [orientations.right] = "/right",
    [orientations.up] = "/up",
    [orientations.down] = "/down"
}

function soundWaveReflector.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 0, entity.height or 0
    local orientation = entity.orientation or orientations.left
    
    local spriteDir = (entity.spriteDir or "") ~= "" and entity.spriteDir or "objects/aonHelper/soundWaveReflector"
    local spritePath = spriteDir .. orientationsToSpritePath[orientation]
    
    local sprites = {}

    local w, h = math.floor(width / 8), math.floor(height / 8)
    if orientation == orientations.left or orientation == orientations.right then
        for i = 0, h - 1 do
            local tx = i == 0 and 0 or (i == h - 1 and 2 or 1)
            
            local sprite = drawableSprite.fromTexture(spritePath)
            sprite:addPosition(x, y + i * 8)
            sprite:useRelativeQuad(tx * 8, 0, 8, 8)
            table.insert(sprites, sprite)
        end
    elseif orientation == orientations.up or orientation == orientations.down then
        for i = 0, w - 1 do
            local ty = i == 0 and 0 or (i == w - 1 and 2 or 1)

            local sprite = drawableSprite.fromTexture(spritePath)
            sprite:addPosition(x + i * 8, y)
            sprite:useRelativeQuad(ty * 8, 0, 8, 8)
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function soundWaveReflector.rectangle(room, entity)
    local orientation = entity.orientation or orientations.left

    if orientation == orientations.left or orientation == orientations.right then
        entity.width = 8
    elseif orientation == orientations.up or orientation == orientations.down then
        entity.height = 8
    end

    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 8, entity.height or 8)
end

return soundWaveReflector