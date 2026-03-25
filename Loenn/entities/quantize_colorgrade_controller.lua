local celesteEnums = require("consts.celeste_enums")

local quantizeColorgradeController = {}

quantizeColorgradeController.name = "aonHelper/QuantizeColorgradeController"
quantizeColorgradeController.texture = "objects/aonHelper/quantizeColorgradeController"
quantizeColorgradeController.depth = 0
quantizeColorgradeController.placements = {
    {
        name = "quantize_colorgrade_controller",
        data = {
            affectedColorgrades = "*",
            quantize = true,
            normalize = true
        }
    }
}

quantizeColorgradeController.fieldOrder = {
    "x", "y",
    "affectedColorgrades",
    "quantize", "normalize"
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

return quantizeColorgradeController
