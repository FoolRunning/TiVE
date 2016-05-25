WorldXSize = 500;
WorldYSize = 500;
WorldZSize = 16;

cameraAngleHoriz = 0;
cameraAngleVert = 0;

require("Common.lua")

function initialize(entity)
    local camera = entity.GetComponent(ComponentCamera)
    --gameWorld = LoadWorld("Maze")
    --WorldXSize = gameWorld.BlockSize.X
    --WorldYSize = gameWorld.BlockSize.Y
    --WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(0.02, 0.02, 0.02)

    camera.FieldOfView = math.rad(70)
    camera.Location = vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 450)
    camera.FarDistance = 1000
    camera.UpVector = vector(0, 0, 1)
end

function update(entity, timeSinceLastFrame)
    local camera = entity.GetComponent(ComponentCamera)
    local camLoc = camera.Location

    if (keyPressed(Keys.Escape)) then -- Stop running demo
        stopRunning()
    end

    local speed = BlockSize / 32
    if (keyPressed(Keys.LShift)) then --Speed up
        speed = speed * 3
    elseif (keyPressed(Keys.LControl)) then --Slow down
        speed = speed / 10
    end

    local mouseLoc = mouseLocation();
    local angleZ = math.rad((mouseLoc.X * -0.1) % 360); -- angle around the z-axis
    local dirVectorXY = rotateVectorZ(vector(0.0, 1.0, 0.0), angleZ); -- direction vector on the XY plane

    if (keyPressed(Keys.A)) then --Move left
        local moveDirVector = rotateVectorZ(dirVectorXY, math.rad(-90));
        camLoc.X = camLoc.X - speed * moveDirVector.X;
        camLoc.Y = camLoc.Y - speed * moveDirVector.Y;
    end

    if (keyPressed(Keys.D)) then --Move right
        local moveDirVector = rotateVectorZ(dirVectorXY, math.rad(90));
        camLoc.X = camLoc.X - speed * moveDirVector.X;
        camLoc.Y = camLoc.Y - speed * moveDirVector.Y;
    end

    if (keyPressed(Keys.W)) then --Move forwards
        camLoc.X = camLoc.X + speed * dirVectorXY.X;
        camLoc.Y = camLoc.Y + speed * dirVectorXY.Y;
    end

    if (keyPressed(Keys.S)) then --Move backwards
        camLoc.X = camLoc.X - speed * dirVectorXY.X;
        camLoc.Y = camLoc.Y - speed * dirVectorXY.Y;
    end

    if (keyPressed(Keys.KeypadPlus)) then --Zoom in
        camLoc.Z = math.max(camLoc.Z - 1, 2 * BlockSize)
    elseif (keyPressed(Keys.KeypadMinus)) then --Zoom out
        camLoc.Z = math.min(camLoc.Z + 1, 50 * BlockSize)
    end

    if (voxelAt(camLoc.X, camLoc.Y, camLoc.Z) == EmptyVoxel) then
        camera.Location = camLoc
    end

    camera.LookAtLocation = vector(camera.Location.X + dirVectorXY.X, camera.Location.Y + dirVectorXY.Y, camera.Location.Z + dirVectorXY.Z)
end

