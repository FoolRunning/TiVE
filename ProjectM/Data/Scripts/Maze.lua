WorldXSize = 0;
WorldYSize = 0;
WorldZSize = 0;

function initialize(entity)
    camera = entity.GetComponent("CameraComponent")
    --gameWorld = LoadWorld("Maze")
    --WorldXSize = gameWorld.BlockSize.X
    --WorldYSize = gameWorld.BlockSize.Y
    --WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(0.02, 0.02, 0.02)

    camera.FieldOfView = PI / 3 --60 degrees
    camera.Location = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 345)
    camera.FarDistance = 1000
    camera.UpVector = vector(0, 0, 1)
end

function update(entity, timeSinceLastFrame)
    camera = entity.GetComponent("CameraComponent")
    camLoc = camera.Location

    speed = 2
    if (keyPressed(Keys.LShift)) then --Speed up
        speed = 10
    elseif (keyPressed(Keys.LControl)) then --Slow down
        speed = 0.2
    end

    if (keyPressed(Keys.A) and voxelAt((camLoc.X - speed), camLoc.Y, camLoc.Z) == 0) then --Move left
        camLoc.X = camLoc.X - speed
    end

    if (keyPressed(Keys.D) and voxelAt((camLoc.X + speed), camLoc.Y, camLoc.Z) == 0) then --Move right
        camLoc.X = camLoc.X + speed
    end

    if (keyPressed(Keys.W) and voxelAt(camLoc.X, camLoc.Y + speed, camLoc.Z) == 0) then --Move up
        camLoc.Y = camLoc.Y + speed
    end

    if (keyPressed(Keys.S) and voxelAt(camLoc.X, camLoc.Y - speed, camLoc.Z) == 0) then --Move down
        camLoc.Y = camLoc.Y - speed
    end

    if (keyPressed(Keys.KeypadPlus) and voxelAt(camLoc.X, camLoc.Y, camLoc.Z - 2) == 0) then --Zoom in
        camLoc.Z = max(camLoc.Z - 2, 2 * BlockSize)
    elseif (keyPressed(Keys.KeypadMinus) and voxelAt(camLoc.X, camLoc.Y, camLoc.Z + 2) == 0) then --Zoom out
        camLoc.Z = min(camLoc.Z + 2, 55 * BlockSize)
    end

    camera.Location = camLoc
    camera.LookAtLocation = vector(camLoc.X, camLoc.Y + 50, camLoc.Z - 150)
end

