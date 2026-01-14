local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local featherDashSwitchDirections = {
    up = 0,
    down = 1,
    left = 2,
    right = 3
}
local featherDashSwitchDirectionOptions = {
    ["Down"] = featherDashSwitchDirections.up,
    ["Up"] = featherDashSwitchDirections.down,
    ["Right"] = featherDashSwitchDirections.left,
    ["Left"] = featherDashSwitchDirections.right
}

local featherDashSwitchSpriteOptions = {
    [featherDashSwitchDirections.up] = { position = { x = 8, y = 0 }, rotation = -math.pi / 2 },
    [featherDashSwitchDirections.down] = { position = { x = 8, y = 8 }, rotation = math.pi / 2 },
    [featherDashSwitchDirections.left] = { position = { x = 0, y = 8 }, rotation = math.pi },
    [featherDashSwitchDirections.right] = { position = { x = 8, y = 8 }, rotation = 0 },
}
local featherDashSwitchSelections = function(x, y) 
    return {
        [featherDashSwitchDirections.up] = utils.rectangle(x - 1, y - 2, 18, 12),
        [featherDashSwitchDirections.down] = utils.rectangle(x - 1, y - 2, 18, 12),
        [featherDashSwitchDirections.left] = utils.rectangle(x - 2, y - 1, 12, 18),
        [featherDashSwitchDirections.right] = utils.rectangle(x - 2, y - 1, 12, 18)
    } 
end

local featherDashSwitch = {}

featherDashSwitch.name = "aonHelper/FeatherDashSwitchV2"
featherDashSwitch.fieldInformation = {
    side = {
        options = featherDashSwitchDirectionOptions,
        editable = false
    },
    particleColor1 = {
        fieldType = "color",
    },
    particleColor2 = {
        fieldType = "color",
    }
}
featherDashSwitch.placements = {}

for dir, side in pairs(featherDashSwitchDirectionOptions) do
    table.insert(featherDashSwitch.placements, {
        name = "featherDashSwitch" .. dir,
        data = {
            side = side,
            persistent = false,
            allGates = false,
            spriteDir = "",
            particleColor1 = "ff7b3d",
            particleColor2 = "ffb136",
        }
    })
end

function featherDashSwitch.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/aonHelper/featherDashSwitch/00", entity)
    local side = entity.side or 0

    local spriteOptions = featherDashSwitchSpriteOptions[side]
    local x, y = spriteOptions.position.x or 0, spriteOptions.position.y or 0
    local rotation = spriteOptions.rotation or 0
    sprite:addPosition(x, y)
    sprite.rotation = rotation

    return sprite
end

function featherDashSwitch.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local side = entity.side or 0
    
    return featherDashSwitchSelections(x, y)[side]
end

return featherDashSwitch
