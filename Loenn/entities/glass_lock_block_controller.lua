local glassLockBlockController = {}

glassLockBlockController.name = "aonHelper/GlassLockBlockController"
glassLockBlockController.depth = 0
glassLockBlockController.texture = "objects/aonHelper/lockBlocks/glassLockBlockController"
glassLockBlockController.placements = {
    {
        name = "controller",
        data = {
            bgColor = "302040",
            lineColor = "ffffff",
            rayColor = "ffffff",
            starColors = "ff7777,77ff77,7777ff,ff77ff,77ffff,ffff77",
            wavy = false,
            vanillaEdgeBehavior = false,
            persistent = false
        }
    },
    {
        name = "controller_vanilla",
        data = {
            bgColor = "0d2e89",
            lineColor = "ffffff",
            rayColor = "ffffff",
            starColors = "7f9fba,9bd1cd,bacae3",
            wavy = true,
            vanillaEdgeBehavior = true,
            persistent = false
        }
    },
}

glassLockBlockController.fieldOrder = {
    "x", "y",
    "bgColor", "lineColor", "rayColor", "starColors",
    "wavy", "vanillaEdgeBehavior", "persistent"
}
glassLockBlockController.fieldInformation = {
    bgColor = {
        fieldType = "color"
    },
    lineColor = {
        fieldType = "color"
    },
    rayColor = {
        fieldType = "color"
    },
    starColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        }
    }
}

return glassLockBlockController
