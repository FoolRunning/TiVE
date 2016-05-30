WorldXSize = 75
WorldYSize = 50
WorldZSize = 8

require("Common.lua")

function initialize(entity)
    local camera = entity.GetComponent(ComponentCamera)
    --WorldXSize = gameWorld.BlockSize.X
    --WorldYSize = gameWorld.BlockSize.Y
    --WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(0.2, 0.2, 0.2)

    camera.FieldOfView = math.rad(70)
    camera.Location = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2 - 50, 0)
    camera.LookAtLocation = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 0)
    camera.FarDistance = 3000
    camera.UpVector = vector(0, 0, 1)
end

cameraAngle = 150

function update(entity, timeSinceLastFrame)
    cameraAngle = cameraAngle + (20 * timeSinceLastFrame)

    local camera = entity.GetComponent(ComponentCamera)
    camera.Location = vector(WorldXSize * BlockSize / 2 + math.cos(math.rad(cameraAngle / 3)) * 100, 500 + math.sin(math.rad(cameraAngle)) * 60, BlockSize * 20)
end

