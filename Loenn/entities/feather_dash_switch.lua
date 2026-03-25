local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local directions = {
    up = 0,
    down = 1,
    left = 2,
    right = 3
}
local directionsOptions = {
    ["Down"] = directions.up,
    ["Up"] = directions.down,
    ["Right"] = directions.left,
    ["Left"] = directions.right
}
local directionsInOrder = { directions.up, directions.right, directions.down, directions.left }

local refillBehavior = {
    none = 0,
    refill = 1,
    twoDashRefill = 2
}
local refillBehaviorOptions = {
    ["None"] = refillBehavior.none,
    ["Refill"] = refillBehavior.refill,
    ["Two Dash Refill"] = refillBehavior.twoDashRefill
}

local spriteOptions = {
    [directions.up] = { position = { x = 8, y = 0 }, rotation = -math.pi / 2 },
    [directions.down] = { position = { x = 8, y = 8 }, rotation = math.pi / 2 },
    [directions.left] = { position = { x = 0, y = 8 }, rotation = math.pi },
    [directions.right] = { position = { x = 8, y = 8 }, rotation = 0 },
}
local selections = function(x, y) 
    return {
        [directions.up] = utils.rectangle(x - 1, y - 2, 18, 12),
        [directions.down] = utils.rectangle(x - 1, y - 2, 18, 12),
        [directions.left] = utils.rectangle(x - 2, y - 1, 12, 18),
        [directions.right] = utils.rectangle(x - 2, y - 1, 12, 18)
    } 
end

local featherDashSwitch = {}

featherDashSwitch.name = "aonHelper/FeatherDashSwitchV2"
featherDashSwitch.placements = {}

for dir, side in pairs(directionsOptions) do
    table.insert(featherDashSwitch.placements, {
        name = "feather_dash_switch_" .. dir:lower(),
        data = {
            side = side,
            dashActivated = false,
            holdableActivated = false,
            featherActivated = true,
            refillBehavior = refillBehavior.none,
            flagOnPress = "",
            persistent = false,
            allGates = false,
            spriteDir = "",
            particleColor1 = "ff8000",
            particleColor2 = "ffd65c",
        }
    })
end

featherDashSwitch.fieldOrder = {
    "x", "y", "side",
    "dashActivated", "holdableActivated", "featherActivated",
    "refillBehavior", "flagOnPress",
    "persistent", "allGates",
    "spriteDir", "particleColor1", "particleColor2"
}
featherDashSwitch.fieldInformation = {
    side = {
        options = directionsOptions,
        editable = false
    },
    refillBehavior = {
        options = refillBehaviorOptions,
        editable = false
    },
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    }
}

function featherDashSwitch.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/aonHelper/featherDashSwitch/00", entity)
    local side = entity.side or directions.up

    local options = spriteOptions[side]
    local x, y = options.position.x or 0, options.position.y or 0
    local rotation = options.rotation or 0
    sprite:addPosition(x, y)
    sprite.rotation = rotation

    return sprite
end

function featherDashSwitch.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local side = entity.side or directions.up
    
    return selections(x, y)[side]
end

function featherDashSwitch.rotate(room, entity, direction)
    local side = entity.side or directions.up
    local directionIndex = table.flip(directionsInOrder)[side] or 1
    local newDirectionIndex = aonHelper.mod(directionIndex - 1 + direction, #directionsInOrder) + 1
    entity.side = directionsInOrder[newDirectionIndex]
end

function featherDashSwitch.flip(room, entity, horizontal, vertical)
    local side = entity.side or directions.up

    if horizontal then
        if side == directions.left then
            side = directions.right
        elseif side == directions.right then
            side = directions.left
        end
    end
    if vertical then
        if side == directions.up then
            side = directions.down
        elseif side == directions.down then
            side = directions.up
        end
    end
    
    entity.side = side
end

return featherDashSwitch
