local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local dreamLockBlock = {}

dreamLockBlock.name = "aonHelper/DreamLockBlock"
dreamLockBlock.depth = function(room, entity) return entity.below and 5000 or -11000 end
dreamLockBlock.placements = {
    {
        name = "dreamLockBlock",
        data = {
            spritePath = "",
            unlockSfx = "",
            stepMusicProgress = false,
            useVanillaKeys = true,
            dzhakeHelperKeySettings = "",
            below = false,
            ignoreInventory = true,
        }
    }
}

dreamLockBlock.fieldOrder = {
    "x", "y",
    "spritePath",
    "unlockSfx", "stepMusicProgress",
    "useVanillaKeys", "dzhakeHelperKeySettings",
    "below", "ignoreInventory"
}
dreamLockBlock.fieldInformation = {
    dzhakeHelperKeySettings = {
        fieldType = "string",
        validator = aonHelper.dzhakeHelperKeySettings
    }
}

local defaultLockTexture = "objects/aonHelper/lockBlocks/lock00"
local blockColor = { 0.0, 0.0, 0.0, 1.0 }
local blockBorderColor = { 1.0, 1.0, 1.0, 1.0 }

function dreamLockBlock.sprite(room, entity)
    local rectangle = drawableRectangle.fromRectangle(
        "bordered",
        entity.x, entity.y, 32, 32,
        blockColor,
        blockBorderColor
    )

    local lockTexture = (entity.spritePath or "") ~= "" and (entity.spritePath .. "00") or defaultLockTexture
    local lockSprite = drawableSprite.fromTexture(lockTexture, entity)
    lockSprite:addPosition(16, 16)

    return { rectangle, lockSprite }
end

function dreamLockBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 32, 32)
end

return dreamLockBlock
