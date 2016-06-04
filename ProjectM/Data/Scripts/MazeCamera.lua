require("Common.lua")

function initialize(entity)
    local camera = entity.GetComponent(ComponentCamera)
    --scene().AmbientLight = color(0.05, 0.05, 0.05)
    local playerBlock = block("player")
    local gameWorld = scene().GameWorld
    local sizeX = gameWorld.BlockSize.X - 1
    local sizeY = gameWorld.BlockSize.Y - 1
    local sizeZ = gameWorld.BlockSize.Z - 1

    local foundPlayer = false;
    for z = 0, sizeZ do
        for x = 0, sizeX do
            for y = 0, sizeY do -- y-major for speed
                if (gameWorld[x, y, z] == playerBlock) then
                    camera.Location = vector(x * BlockSize + HalfBlockSize, y * BlockSize + HalfBlockSize, z * BlockSize + HalfBlockSize)
                    foundPlayer = true
                    break
                end
            end
            if (foundPlayer) then
                break
            end
        end 
        if (foundPlayer) then
            break
        end
    end

    camera.FieldOfView = math.rad(70)
    camera.FarDistance = 1500
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

    local mouseLoc = mouseLocation()
    local angleZ = math.rad((mouseLoc.X * -0.1) % 360) -- angle around the z-axis
    local angleX = math.rad((mouseLoc.Y * -0.1) % 360) -- angle around the x-axis
    local moveVectorXY = rotateVectorZ(vector(0.0, 1.0, 0.0), angleZ) -- direction vector on the XY plane

    if (keyPressed(Keys.A)) then --Move left
        local moveDirVector = rotateVectorZ(moveVectorXY, math.rad(-90))
        camLoc.X = camLoc.X - speed * moveDirVector.X
        camLoc.Y = camLoc.Y - speed * moveDirVector.Y
    end

    if (keyPressed(Keys.D)) then --Move right
        local moveDirVector = rotateVectorZ(moveVectorXY, math.rad(90))
        camLoc.X = camLoc.X - speed * moveDirVector.X
        camLoc.Y = camLoc.Y - speed * moveDirVector.Y
    end

    if (keyPressed(Keys.W)) then --Move forwards
        camLoc.X = camLoc.X + speed * moveVectorXY.X
        camLoc.Y = camLoc.Y + speed * moveVectorXY.Y
    end

    if (keyPressed(Keys.S)) then --Move backwards
        camLoc.X = camLoc.X - speed * moveVectorXY.X
        camLoc.Y = camLoc.Y - speed * moveVectorXY.Y
    end

    if (keyPressed(Keys.KeypadPlus)) then --Zoom in
        camLoc.Z = math.max(camLoc.Z - 1, 2 * BlockSize)
    elseif (keyPressed(Keys.KeypadMinus)) then --Zoom out
        camLoc.Z = math.min(camLoc.Z + 1, 50 * BlockSize)
    end

    if (voxelAt(camLoc.X, camLoc.Y, camLoc.Z) == EmptyVoxel) then
        camera.Location = camLoc
    end

    -- Calculate the look vector
    -- With only a unit-length vector for direction, the look-at location creates some random rounding errors that cause the camera to shake back and forth
    -- so we create a 50-length vector to remove those rounding errors.
    local lookVector = rotateVectorX(vector(0.0, 50.0, 0.0), angleX)
    lookVector = rotateVectorZ(lookVector, angleZ)

    camera.LookAtLocation = vector(camera.Location.X + lookVector.X, camera.Location.Y + lookVector.Y, camera.Location.Z + lookVector.Z)
end

