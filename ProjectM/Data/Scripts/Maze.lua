WorldXSize = 500;
WorldYSize = 500;
WorldZSize = 0;

require("Common.lua")

function initialize(entity)
    local camera = entity.GetComponent(ComponentCamera)
    --gameWorld = LoadWorld("Maze")
    --WorldXSize = gameWorld.BlockSize.X
    --WorldYSize = gameWorld.BlockSize.Y
    --WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(0.02, 0.02, 0.02)

    camera.FieldOfView = math.rad(50)
    camera.Location = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 550)
    camera.FarDistance = 1000
    camera.UpVector = vector(0, 0, 1)
end

function update(entity, timeSinceLastFrame)
    local camera = entity.GetComponent(ComponentCamera)
    local camLoc = camera.Location

    if (keyPressed(Keys.Escape)) then -- Stop running demo
        stopRunning()
    end

    local speed = 2
    if (keyPressed(Keys.LShift)) then --Speed up
        speed = 6
    elseif (keyPressed(Keys.LControl)) then --Slow down
        speed = 0.2
    end

    if (keyPressed(Keys.A) and voxelAt(camLoc.X - speed, camLoc.Y, camLoc.Z) == EmptyVoxel) then --Move left
        camLoc.X = camLoc.X - speed
    end

    if (keyPressed(Keys.D) and voxelAt((camLoc.X + speed), camLoc.Y, camLoc.Z) == EmptyVoxel) then --Move right
        camLoc.X = camLoc.X + speed
    end

    if (keyPressed(Keys.W) and voxelAt(camLoc.X, camLoc.Y + speed, camLoc.Z) == EmptyVoxel) then --Move up
        camLoc.Y = camLoc.Y + speed
    end

    if (keyPressed(Keys.S) and voxelAt(camLoc.X, camLoc.Y - speed, camLoc.Z) == EmptyVoxel) then --Move down
        camLoc.Y = camLoc.Y - speed
    end

    if (keyPressed(Keys.KeypadPlus) and voxelAt(camLoc.X, camLoc.Y, camLoc.Z - 2) == EmptyVoxel) then --Zoom in
        camLoc.Z = math.max(camLoc.Z - 2, 2 * BlockSize)
    elseif (keyPressed(Keys.KeypadMinus) and voxelAt(camLoc.X, camLoc.Y, camLoc.Z + 2) == EmptyVoxel) then --Zoom out
        camLoc.Z = math.min(camLoc.Z + 2, 50 * BlockSize)
    end

    camera.Location = camLoc
    camera.LookAtLocation = vector(camLoc.X, camLoc.Y + 35, camLoc.Z - 100)
end

