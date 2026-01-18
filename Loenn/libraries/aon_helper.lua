local aonHelper = {}

function aonHelper.dzhakeHelperKeySettingsValidator(settings)
    return settings == "" or settings == "*" or (tonumber(settings) ~= nil and not string.find(settings, ".", 1, true))
end

return aonHelper
