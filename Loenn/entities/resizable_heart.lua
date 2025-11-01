local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local resizableHeart = {}

resizableHeart.name = "aonHelper/ResizableHeart"
resizableHeart.placements = {
    {
        name = "custom",
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            color = "00a81f",
            path = "",
            isFake = false,
            respawnTimer = 3.0,
            disableGhost = false
        }
    },
    {
        name = "blue",
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            color = "5caefa",
            path = "heartgem0",
            isFake = false,
            respawnTimer = 3.0,
            disableGhost = false
        }
    },
    {
        name = "red",
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            color = "ff2457",
            path = "heartgem1",
            isFake = false,
            respawnTimer = 3.0,
            disableGhost = false
        }
    },
    {
        name = "gold",
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            color = "fffc24",
            path = "heartgem2",
            isFake = false,
            respawnTimer = 3.0,
            disableGhost = false
        }
    },
    {
        name = "white",
        placementType = "rectangle",
        data = {
            width = 16,
            height = 16,
            color = "bebdb8",
            path = "heartgem3",
            isFake = false,
            respawnTimer = 3.0,
            disableGhost = false
        }
    }
}
resizableHeart.fieldInformation = {
    color = {
        fieldType = "color"
    },
    path = {
        options = {
            "",
            "heartgem0",
            "heartgem1",
            "heartgem2",
            "heartgem3"
        }
    },
}

local heartTextures = {
    ["heartgem0"] = "collectables/heartGem/0/00",
    ["heartgem1"] = "collectables/heartGem/1/00",
    ["heartgem2"] = "collectables/heartGem/2/00",
    ["heartgem3"] = "collectables/heartGem/3/00"
}
local heartColors = {
    [""] = "00a81f",
    ["heartgem0"] = "5caefa",
    ["heartgem1"] = "ff2457",
    ["heartgem2"] = "fffc24",
    ["heartgem3"] = "bebdb8"
}

local resizableHeartOutlineTexture = "collectables/aonHelper/resizableHeart_Outline/00"
local resizableHeartTexture = "collectables/aonHelper/resizableHeart/00"

function resizableHeart.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    
    local path = entity.path or ""
    local color = entity.color or heartColors[path] or heartColors["heartgem3"]

    local outlineColor = utils.getColor(color)
    local insideColor = { outlineColor[1], outlineColor[2], outlineColor[3], 0.5 }
    local rectangle = drawableRectangle.fromRectangle("bordered", x - width / 2, y - height / 2, width, height, insideColor, outlineColor)
    
    if path == "" then
        local heart = drawableSprite.fromTexture(resizableHeartTexture, entity)
        local outline = drawableSprite.fromTexture(resizableHeartOutlineTexture, entity)
        outline:setColor({ 1.0, 1.0, 1.0, 1.0 })
        
        return { rectangle, heart, outline }
    else
        local texture = heartTextures[path] or heartTextures["heartgem3"]
        local sprite = drawableSprite.fromTexture(texture, entity)
        sprite:setColor({ 1.0, 1.0, 1.0, 1.0 })

        return { rectangle, sprite }
    end
end

function resizableHeart.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    
    return utils.rectangle(x - width / 2, y - height / 2, width, height)
end

return resizableHeart
