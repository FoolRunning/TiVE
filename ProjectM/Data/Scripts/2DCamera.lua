require("Common.lua")

function initialize(entity)
    local camera = entity.GetComponent(ComponentCamera)
    --scene().AmbientLight = color(0.05, 0.05, 0.05)
    local gameWorld = scene().GameWorld
    local playerBlock = block("player")
    local playerLoc = gameWorld.FindBlock(playerBlock);
    camera.Location = vector(playerLoc.X * BlockSize + HalfBlockSize, playerLoc.Y * BlockSize + HalfBlockSize - 600, playerLoc.Z * BlockSize + HalfBlockSize)

    --local sizeX = gameWorld.BlockSize.X - 1
    --local sizeY = gameWorld.BlockSize.Y - 1
    --local sizeZ = gameWorld.BlockSize.Z - 1
    --local foundPlayer = false;
    --for z = 0, sizeZ do
    --    for x = 0, sizeX do
    --        for y = 0, sizeY do -- y-major for speed
    --            if (gameWorld[x, y, z] == playerBlock) then
    --                camera.Location = vector(x * BlockSize + HalfBlockSize, y * BlockSize + HalfBlockSize, z * BlockSize + HalfBlockSize)
    --                --foundPlayer = true
    --                break
    --            end
    --        end
    --        --if (foundPlayer) then
    --        --    break
    --        --end
    --    end 
    --    --if (foundPlayer) then
    --    --    break
    --    --end
    --end

    camera.FieldOfView = math.rad(40)
    camera.FarDistance = BlockSize * 50
    camera.UpVector = vector(0, 0, 1)
end

function update(entity, timeSinceLastFrame)
    local camera = entity.GetComponent(ComponentCamera)
    local camLoc = camera.Location

    if (keyPressed(Keys.Escape)) then -- Stop running demo
        stopRunning()
    end

    local speed = BlockSize * timeSinceLastFrame * 2.5
    if (keyPressed(Keys.LShift)) then --Speed up
        speed = speed * 3
    elseif (keyPressed(Keys.LControl)) then --Slow down
        speed = speed / 10
    end

    if (keyPressed(Keys.A)) then --Move left
        camLoc.X = camLoc.X - speed
    end

    if (keyPressed(Keys.D)) then --Move right
        camLoc.X = camLoc.X + speed
    end

    if (keyPressed(Keys.W)) then --Move forwards
        camLoc.Z = camLoc.Z + speed
    end

    if (keyPressed(Keys.S)) then --Move backwards
        camLoc.Z = camLoc.Z - speed
    end

    if (keyPressed(Keys.KeypadPlus)) then --Zoom in
        camLoc.Y = camLoc.Y + speed
    elseif (keyPressed(Keys.KeypadMinus)) then --Zoom out
        camLoc.Y = camLoc.Y - speed
    end

    if (voxelAt(camLoc.X, camLoc.Y, camLoc.Z) == EmptyVoxel) then
        camera.Location = camLoc
    end

    camera.LookAtLocation = vector(camera.Location.X, camera.Location.Y + 10, camera.Location.Z)
end

