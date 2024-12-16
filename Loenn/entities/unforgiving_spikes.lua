local spikeHelper = require("helpers.spikes")
local drawableSprite = require("structs.drawable_sprite")

local spikeOptions = {
    directionNames = {
        up = "aonHelper/UnforgivingSpikesUp",
        down = "aonHelper/UnforgivingSpikesDown",
        left = "aonHelper/UnforgivingSpikesLeft",
        right = "aonHelper/UnforgivingSpikesRight"
    }
}

--[[
    local handlers = spikeHelper.createEntityHandlers(spikeOptions)
    for _, handler in ipairs(handlers) do
        local oldSpriteFunc = handler.sprite

        handler.sprite = function(room, entity)
            local sprites = oldSpriteFunc(room, entity)

            for _, sprite in ipairs(sprites) do
                sprite:setColor({ 1, 0.5, 0.5, 1 }) -- make them evil
            end

            return sprites
        end
    end

    return handlers
]]

return spikeHelper.createEntityHandlers(spikeOptions)
