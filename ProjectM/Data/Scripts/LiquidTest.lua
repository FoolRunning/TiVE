--WorldXSize = 0;
--WorldYSize = 0;
--WorldZSize = 0;

function initialize(entity)
    camera = entity.GetComponent("CameraComponent")

    --gameWorld = LoadWorld("LiquidTest")
    --WorldXSize = gameWorld.BlockSize.X
    --WorldYSize = gameWorld.BlockSize.Y
    --WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(0.1, 0.1, 0.1)

    camera.FieldOfView = PI / 3 --60 degrees
    --camera.Location = Vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 345)
    camera.FarDistance = 1000
    camera.UpVector = Vector(0, 0, 1)
end

function update(entity, timeSinceLastFrame)
    camera = entity.GetComponent("CameraComponent")
    camLoc = camera.Location

    speed = 2
    if (KeyPressed(Keys.LShift)) then --Speed up
        speed = 10
    elseif (KeyPressed(Keys.LControl)) then --Slow down
        speed = 0.2
    end

    if (KeyPressed(Keys.A) and VoxelAt((camLoc.X - speed), camLoc.Y, camLoc.Z) == 0) then --Move left
        camLoc.X = camLoc.X - speed
    end

    if (KeyPressed(Keys.D) and VoxelAt((camLoc.X + speed), camLoc.Y, camLoc.Z) == 0) then --Move right
        camLoc.X = camLoc.X + speed
    end

    if (KeyPressed(Keys.W) and VoxelAt(camLoc.X, camLoc.Y + speed, camLoc.Z) == 0) then --Move up
        camLoc.Y = camLoc.Y + speed
    end

    if (KeyPressed(Keys.S) and VoxelAt(camLoc.X, camLoc.Y - speed, camLoc.Z) == 0) then --Move down
        camLoc.Y = camLoc.Y - speed
    end

    if (KeyPressed(Keys.KeypadPlus) and VoxelAt(camLoc.X, camLoc.Y, camLoc.Z - 2) == 0) then --Zoom in
        camLoc.Z = Max(camLoc.Z - 2, 2 * BlockSize)
    elseif (KeyPressed(Keys.KeypadMinus) and VoxelAt(camLoc.X, camLoc.Y, camLoc.Z + 2) == 0) then --Zoom out
        camLoc.Z = Min(camLoc.Z + 2, 55 * BlockSize)
    end

    camera.Location = camLoc
    camera.LookAtLocation = Vector(camLoc.X, camLoc.Y + 150, 50)
end

