local darkerMatterController = {}

darkerMatterController.name = "aonHelper/DarkerMatterController"
darkerMatterController.texture = "objects/aonHelper/darkerMatter/darkerMatterController"
darkerMatterController.depth = 0
darkerMatterController.placements = {
    {
        name = "darkerMatterController",
        data = {
            speedThreshold = 0,
            speedLimit = 200,
            darkerMatterColors = "5e0824,47134c",
            darkerMatterWarpColors = "6a391c,775121",
            stopGraceTimer = 0.05,
        }
    }
}

-- return darkerMatterController
