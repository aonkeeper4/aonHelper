local spikeHelper = require("helpers.spikes")

local spikeOptions = {
    directionNames = {
        up = "aonHelper/UnforgivingSpikesUp",
        down = "aonHelper/UnforgivingSpikesDown",
        left = "aonHelper/UnforgivingSpikesLeft",
        right = "aonHelper/UnforgivingSpikesRight"
    },
    placementData = {
        attachToSolid = true,
        checkVelocity = false
    }
}

return spikeHelper.createEntityHandlers(spikeOptions)
