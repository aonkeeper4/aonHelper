local drawableSprite = require("structs.drawable_sprite")

local featherDashSwitchDirections = { ["Down"] = 0, ["Up"] = 1, ["Right"] = 2, ["Left"] = 3 }

local featherDashSwitch = {}

featherDashSwitch.name = "aonHelper/FeatherDashSwitch"
featherDashSwitch.fieldInformation = {
    side = {
        options = featherDashSwitchDirections,
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

for dir, i in pairs(featherDashSwitchDirections) do
    table.insert(featherDashSwitch.placements, {
        name = "featherDashSwitch" .. dir,
        data = {
            side = i,
            particleColor1 = "ff7b3d",
            particleColor2 = "ffb136",
        }
    })
end

function featherDashSwitch.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/aonHelper/featherDashSwitch/00", entity)
    local side = entity.side

    if side == 0 then
        sprite:addPosition(8, 0)
        sprite.rotation = -math.pi / 2
    elseif side == 1 then
        sprite:addPosition(8, 8)
        sprite.rotation = math.pi / 2
    elseif side == 2 then
        sprite:addPosition(0, 8)
        sprite.rotation = math.pi
    elseif side == 3 then
        sprite:addPosition(8, 8)
        sprite.rotation = 0
    end

    return sprite
end

return featherDashSwitch
