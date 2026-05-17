local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local aonHelper = {}

-- validators

function aonHelper.dzhakeHelperKeySettings(settings)
    return settings == "" or settings == "*" or (tonumber(settings) ~= nil and not string.find(settings, ".", 1, true))
end

function aonHelper.numberAllowEmpty(min, max)
    return function(number)
        if number == "" then return true end

        local num = tonumber(number)
        if num == nil then return false end

        return num >= (min or -math.huge) and num <= (max or math.huge)
    end
end

-- utils

function aonHelper.mod(x, m)
    return math.fmod(math.fmod(x, m) + m, m)
end

-- controllers

local globalTextTexture = "objects/aonHelper/misc/globalText"

local function isDrawableSprite(table)
    return #table == 0 and utils.typeof(table) == "drawableSprite"
end

local function isEntityGlobal(entity, options)
    if options and options.global then
        if type(options.global) == "boolean" then return true end
        
        local attrName, attrDefault = options.global.attributeName, options.global.attributeDefault
        return attrDefault and (entity[attrName] ~= false) or (entity[attrName] or false)
    end
    
    return false
end

-- todo: more?
function aonHelper.controllerify(handler, options)
    handler.depth = -math.huge

    if not handler.selection then
        handler.selection = function(room, entity)
            local x, y = entity.x or 0, entity.y or 0
            return utils.rectangle(x - 12, y - 12, 24, 24)
        end
    end
    
    local origHandlerSprite = handler.sprite
    if not origHandlerSprite and handler.texture then
        origHandlerSprite = function(room, entity)
            local x, y = entity.x or 0, entity.y or 0
            local texture = utils.callIfFunction(handler.texture, room, entity)
            local scale = utils.callIfFunction(handler.scale, room, entity) or { 1.0, 1.0 }
            
            local sprite = drawableSprite.fromTexture(texture, { x = x, y = y })
            sprite:setScale(scale)
            return sprite
        end
    end
    if not origHandlerSprite then
        origHandlerSprite = function(room, entity) return {} end
    end

    handler.sprite = function(room, entity)
        local x, y = entity.x or 0, entity.y or 0
        
        local handlerSprites = origHandlerSprite(room, entity)
        local sprites = {}
        if handlerSprites then
            if isDrawableSprite(handlerSprites) then
                sprites = { handlerSprites }
            elseif #handlerSprites > 0 then
                local valid = true
                for _, sprite in ipairs(handlerSprites) do
                    if not isDrawableSprite(sprite) then
                        valid = false
                    end
                end
                
                sprites = valid and handlerSprites or {}
            end
        end
        
        if isEntityGlobal(entity, options) then
            local selectionRect = handler.selection(room, entity)
            local globalX, globalY = selectionRect.x + selectionRect.width / 2, selectionRect.y
            
            local globalSprite = drawableSprite.fromTexture(globalTextTexture, { x = globalX, y = globalY })
            globalSprite:setJustification({ 0.5, 1.0 })
            table.insert(sprites, globalSprite)
        end
        
        return sprites
    end
    
    return handler
end

return aonHelper
