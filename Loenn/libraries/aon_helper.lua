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

return aonHelper
