local celesteEnums = require("consts.celeste_enums")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local quantizeColorgradeController = {}

quantizeColorgradeController.name = "aonHelper/QuantizeColorgradeController"
quantizeColorgradeController.texture = "objects/aonHelper/quantizeColorgradeController"
quantizeColorgradeController.placements = {
    {
        name = "quantize_colorgrade_controller",
        data = {
            affectedColorgrades = "*",
            quantize = true,
            normalize = true,
            global = true
        }
    }
}

quantizeColorgradeController.fieldOrder = {
    "x", "y",
    "affectedColorgrades",
    "quantize", "normalize",
    "global"
}
quantizeColorgradeController.fieldInformation = {
    affectedColorgrades = {
        fieldType = "list",
        elementOptions = {
            options = celesteEnums.color_grades,
            editable = true
        }
    }
}

return aonHelper.controllerify(quantizeColorgradeController, {
    global = {
        attributeName = "global",
        attributeDefault = true
    }
})
