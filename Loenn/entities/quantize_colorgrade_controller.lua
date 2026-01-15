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
        fieldType = "list"
    }
}

return quantizeColorgradeController
