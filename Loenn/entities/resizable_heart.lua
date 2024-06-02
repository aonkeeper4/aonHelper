local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local resizableHeart = {}

resizableHeart.name = "aonHelper/ResizableHeart"

resizableHeart.fieldInformation = {
    color = {
        fieldType = "color"
    },
    path = {
        options = { "", "heartgem0", "heartgem1", "heartgem2", "heartgem3" }
    },
}

resizableHeart.placements = {
    {
        name = "custom",
        data = {
            width = 16,
            height = 16,
            color = "00a81f",
            path = "",
            isFake = false,
            respawnTimer = 3.0,
        }
    },
    {
        name = "blue",
        data = {
            width = 16,
            height = 16,
            color = "00a81f",
            path = "heartgem0",
            isFake = false,
            respawnTimer = 3.0,
        }
    },
    {
        name = "red",
        data = {
            width = 16,
            height = 16,
            color = "00a81f",
            path = "heartgem1",
            isFake = false,
            respawnTimer = 3.0,
        }
    },
    {
        name = "gold",
        data = {
            width = 16,
            height = 16,
            color = "00a81f",
            path = "heartgem2",
            isFake = false,
            respawnTimer = 3.0,
        }
    },
    {
        name = "white",
        data = {
            width = 16,
            height = 16,
            color = "00a81f",
            path = "heartgem3",
            isFake = false,
            respawnTimer = 3.0,
        }
    }
}

local heartTextures = {
    ["heartgem0"] = "collectables/heartGem/0/00",
    ["heartgem1"] = "collectables/heartGem/1/00",
    ["heartgem2"] = "collectables/heartGem/2/00",
    ["heartgem3"] = "collectables/heartGem/3/00"
}

local resizableHeartOutlineTexture = "collectables/aonHelper/resizableHeart_Outline/00"
local resizableHeartTexture = "collectables/aonHelper/resizableHeart/00"

function resizableHeart.sprite(room, entity)
    local path = entity.path

    if path == "" then
        local outline = drawableSprite.fromTexture(resizableHeartOutlineTexture, entity)
        local heart = drawableSprite.fromTexture(resizableHeartTexture, entity)
        return { heart, outline }
    end

    local texture = heartTextures[path] or heartTextures["heartgem3"]
    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite.color = { 1.0, 1.0, 1.0, 1.0 }
    return sprite
end

function resizableHeart.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local w, h = entity.width or 16, entity.height or 16
    return utils.rectangle(x - w / 2, y - h / 2, w, h)
end

return resizableHeart
