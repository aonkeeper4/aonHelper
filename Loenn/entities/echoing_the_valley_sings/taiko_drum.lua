local drawableSprite = require("structs.drawable_sprite")
local celesteEnums = require("consts.celeste_enums")

local taikoDrum = {}

local axes = {
    horizontal = 0,
    vertical = 1,
    both = 2
}
local axesOptions = {
    ["Horizontal"] = axes.horizontal,
    ["Vertical"] = axes.vertical,
    ["Both"] = axes.both
}

taikoDrum.name = "aonHelper/TaikoDrum"
taikoDrum.depth = -9000
taikoDrum.minimumSize = { 24, 24 }
taikoDrum.placements = {}

for name, axis in pairs(axes) do
    table.insert(taikoDrum.placements, {
        name = "taiko_drum_" .. name,
        data = {
            width = 24,
            height = 24,
            axes = axis,
            fragile = false,
            doNotLoadFlag = "",
            flagOnBreak = "",
            spriteDir = "",
            surfaceIndex = 13,
            activateParticleColor = "f3dbc5"
        }
    })
end

taikoDrum.fieldOrder = {
    "x", "y", "width", "height",
    "axes", "fragile",
    "doNotLoadFlag", "flagOnBreak",
    "spriteDir", "surfaceIndex", "activateParticleColor"
}
taikoDrum.fieldInformation = {
    axes = {
        fieldType = "integer",
        options = axesOptions,
        editable = false
    },
    surfaceIndex = {
        fieldType = "integer",
        options = celesteEnums.tileset_sound_ids,
        editable = true
    },
    activateParticleColor = {
        fieldType = "color"
    }
}

local axesToSpritePath = {
    [axes.horizontal] = "/horizontal",
    [axes.vertical] = "/vertical",
    [axes.both] = "/both"
}

function taikoDrum.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
    
    local w, h = math.floor(width / 8), math.floor(height / 8)
    local entityAxes = entity.axes or axes.horizontal
    local fragile = entity.fragile or false
    
    local spriteDir = (entity.spriteDir or "") ~= "" and entity.spriteDir or "objects/aonHelper/taikoDrum"
    local spritePath = spriteDir .. axesToSpritePath[entityAxes] .. (fragile and "_fragile" or "")
    local sprites = {}
    
    math.randomseed(x, y)

    if entityAxes == axes.horizontal then
        for j = 0, h - 1 do
            -- ensure horizontal consistency
            local tyOffset = math.random(0, 1)

            for i = 0, w - 1 do
                local tx, ty
                local centerY = math.floor(h / 2 - 0.5)

                if i == 0 then tx = 0
                elseif i == w - 1 then tx = 3
                else tx = math.random(1, 2) end

                if j == 0 then ty = 0
                elseif j == h - 1 then ty = 6
                elseif j < centerY then ty = 1 + tyOffset
                elseif j > centerY then ty = 4 + tyOffset
                else ty = 3 end

                local sprite = drawableSprite.fromTexture(spritePath)
                sprite:addPosition(x + i * 8, y + j * 8)
                sprite:useRelativeQuad(tx * 8, ty * 8, 8, 8)
                table.insert(sprites, sprite)
            end
        end
    elseif entityAxes == axes.vertical or entityAxes == axes.both then
        for i = 0, w - 1 do
            -- ensure vertical consistency
            local txOffset = math.random(0, 1)
            
            for j = 0, h - 1 do
                local tx, ty
                local centerY = math.floor(h / 2 - 0.5)

                if i == 0 then tx = 0
                elseif i == w - 1 then tx = 3
                else tx = 1 + txOffset end

                if j == 0 then ty = 0
                elseif j == h - 1 then ty = 6
                elseif j < centerY then ty = math.random(1, 2)
                elseif j > centerY then ty = math.random(4, 5)
                else ty = 3 end

                local sprite = drawableSprite.fromTexture(spritePath)
                sprite:addPosition(x + i * 8, y + j * 8)
                sprite:useRelativeQuad(tx * 8, ty * 8, 8, 8)
                table.insert(sprites, sprite)
            end
        end
    end
    
    return sprites
end

return taikoDrum
