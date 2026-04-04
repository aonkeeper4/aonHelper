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
            spritePath = "",
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
            spritePath = "collectables/heartGem/0/",
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
            spritePath = "collectables/heartGem/1/",
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
            spritePath = "collectables/heartGem/2/",
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
            spritePath = "collectables/heartGem/3/",
            isFake = false,
            respawnTimer = 3.0,
            disableGhost = false
        }
    }
}

resizableHeart.fieldOrder = {
    "x", "y", "width", "height",
    "color", "path", "spritePath",
    "isFake", "respawnTimer", "disableGhost"
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
        },
        editable = true
    },
    spritePath = {
        options = {
            "",
            "collectables/heartGem/0/",
            "collectables/heartGem/1/",
            "collectables/heartGem/2/",
            "collectables/heartGem/3/"
        },
        editable = true
    },
}

local heartPathTextures = {
    ["heartgem0"] = "collectables/heartGem/0/",
    ["heartgem1"] = "collectables/heartGem/1/",
    ["heartgem2"] = "collectables/heartGem/2/",
    ["heartgem3"] = "collectables/heartGem/3/"
}
local heartColors = {
    [""] = "00a81f",
    ["collectables/heartGem/0/"] = "5caefa",
    ["collectables/heartGem/1/"] = "ff2457",
    ["collectables/heartGem/2/"] = "fffc24",
    ["collectables/heartGem/3/"] = "bebdb8"
}

local resizableHeartOutlineTexture = "collectables/aonHelper/resizableHeart/outline00"
local resizableHeartTexture = "collectables/aonHelper/resizableHeart/heart00"

function resizableHeart.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    
    local spritePath = heartPathTextures[entity.path] or entity.spritePath or ""
    local color = entity.color or heartColors[spritePath] or heartColors["collectables/heartGem/3/"]

    local outlineColor = utils.getColor(color)
    local insideColor = { outlineColor[1], outlineColor[2], outlineColor[3], 0.5 }
    local rectangle = drawableRectangle.fromRectangle("bordered", x - width / 2, y - height / 2, width, height, insideColor, outlineColor)
    
    if spritePath == "" then
        local heart = drawableSprite.fromTexture(resizableHeartTexture, entity)
        local outline = drawableSprite.fromTexture(resizableHeartOutlineTexture, entity)
        outline:setColor({ 1.0, 1.0, 1.0, 1.0 })
        
        return { rectangle, heart, outline }
    else
        local sprite = drawableSprite.fromTexture(spritePath .. "00", entity)
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
