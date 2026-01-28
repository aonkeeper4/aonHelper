local celesteEnums = require("consts.celeste_enums")

local quantizeColorgradeController = {}

local modes = {
    quantize = 0,
    normalize = 1,
    both = 2
}
local modesOptions = {
    ["Quantize"] = modes.quantize,
    ["Normalize"] = modes.normalize,
    ["Both"] = modes.both
}

quantizeColorgradeController.name = "aonHelper/QuantizeColorgradeController"
quantizeColorgradeController.texture = "objects/aonHelper/quantizeColorgradeController"
quantizeColorgradeController.depth = 0
quantizeColorgradeController.placements = {
    {
        name = "quantize",
        data = {
            affectedColorgrades = "*",
            mode = modes.quantize
        }
    },
    {
        name = "normalize",
        data = {
            affectedColorgrades = "*",
            mode = modes.normalize
        }
    },
    {
        name = "both",
        data = {
            affectedColorgrades = "*",
            mode = modes.both
        }
    },
}

quantizeColorgradeController.fieldOrder = {
    "x", "y",
    "affectedColorgrades", "mode"
}
quantizeColorgradeController.fieldInformation = {
    affectedColorgrades = {
        fieldType = "list",
        elementOptions = {
            options = celesteEnums.color_grades,
            editable = true
        }
    },
    mode = {
        options = modesOptions,
        editable = false
    }
}

return quantizeColorgradeController
