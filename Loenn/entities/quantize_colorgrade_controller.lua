local celesteEnums = require("consts.celeste_enums")

local quantizeColorgradeController = {}

quantizeColorgradeController.name = "aonHelper/QuantizeColorgradeController"
quantizeColorgradeController.texture = "objects/aonHelper/quantizeColorgradeController"
quantizeColorgradeController.depth = 0
quantizeColorgradeController.placements = {
    {
        name = "quantizeColorgradeController",
        data = {
            affectedColorgrades = "*"
        }
    }
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
