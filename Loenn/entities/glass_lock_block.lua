local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local glassLockBlock = {}

glassLockBlock.name = "aonHelper/GlassLockBlock"
glassLockBlock.depth = function(room, entity) return entity.behindFgTiles and -9995 or -10000 end
glassLockBlock.placements = {
    {
        name = "glassLockBlock",
        data = {
            spritePath = "",
            unlockSfx = "",
            stepMusicProgress = false,
            useVanillaKeys = true,
            dzhakeHelperKeySettings = "",
            behindFgTiles = false
        }
    }
}

glassLockBlock.fieldOrder = {
    "x", "y",
    "spritePath",
    "unlockSfx", "stepMusicProgress",
    "useVanillaKeys", "dzhakeHelperKeySettings",
    "behindFgTiles"
}
glassLockBlock.fieldInformation = {
    dzhakeHelperKeySettings = {
        fieldType = "string",
        validator = aonHelper.dzhakeHelperKeySettingsValidator
    }
}

local defaultLockTexture = "objects/aonHelper/lockBlocks/lock00"
local defaultBlockColor = { 13 / 255, 46 / 255, 137 / 255 }
local defaultBlockBorderColor = { 1.0, 1.0, 1.0, 1.0 }

function glassLockBlock.sprite(room, entity)
    local controllerBlockColor, controllerBlockBorderColor
    for _, e in ipairs(room.entities) do
        if e._name == "aonHelper/GlassLockBlockController"
                or e._name == "MoreLockBlocks/GlassLockBlockController" then
            controllerBlockColor = utils.getColor(e.bgColor)
            controllerBlockBorderColor = utils.getColor(e.lineColor)
            break
        end
    end

    local rectangle = drawableRectangle.fromRectangle(
        "bordered",
        entity.x, entity.y, 32, 32,
        controllerBlockColor or defaultBlockColor,
        controllerBlockBorderColor or defaultBlockBorderColor
    )

    local lockTexture = (entity.spritePath or "") ~= "" and (entity.spritePath .. "00") or defaultLockTexture
    local lockSprite = drawableSprite.fromTexture(lockTexture, entity)
    lockSprite:addPosition(16, 16)

    return { rectangle, lockSprite }
end

function glassLockBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 32, 32)
end

return glassLockBlock
