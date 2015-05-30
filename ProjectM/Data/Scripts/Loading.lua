WorldXSize = 75;
WorldYSize = 50;
WorldZSize = 8;

function initialize(entity)
    camera = entity.GetComponent("CameraComponent")
    --WorldXSize = gameWorld.BlockSize.X
    --WorldYSize = gameWorld.BlockSize.Y
    --WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(0.2, 0.2, 0.2)

    camera.FieldOfView = PI / 4 --45 degrees
    camera.Location = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2 - 50, 0)
    camera.LookAtLocation = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 0)
    camera.FarDistance = 2000
    camera.UpVector = vector(0, 0, 1)
end

cameraAngle = 150

function update(entity, timeSinceLastFrame)
    cameraAngle = cameraAngle + (20 * timeSinceLastFrame)

    camera = entity.GetComponent("CameraComponent")
    camera.Location = vector(WorldXSize * BlockSize / 2 + cos(toRadians(cameraAngle / 3)) * 70, 150 + sin(toRadians(cameraAngle)) * 40, 550)
end

