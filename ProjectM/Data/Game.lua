﻿WorldXSize = 0;
WorldYSize = 0;
WorldZSize = 0;

function Initialize(camera)
    gameWorld = LoadWorld("bla")
    WorldXSize = gameWorld.BlockSize.X
    WorldYSize = gameWorld.BlockSize.Y
    WorldZSize = gameWorld.BlockSize.Z
    Renderer().LightProvider.AmbientLight = Color(0.1, 0.1, 0.1)

    camera.FarDistance = 1000
    camera.FoV = PI / 4 --45 degrees
    camera.Location = Vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 345)
    camera.FarDistance = 1000
end

function Update(camera, keyboard)
    camLoc = camera.Location

    speed = 2
    if (KeyPressed(Keys.LShift)) then --Speed up
        speed = 10
    elseif (KeyPressed(Keys.LControl)) then --Slow down
        speed = 0.2
    end

    if (KeyPressed(Keys.A)) then --Move left
        camLoc.X = camLoc.X - speed
    end

    if (KeyPressed(Keys.D)) then --Move right
        camLoc.X = camLoc.X + speed
    end

    if (KeyPressed(Keys.W)) then --Move up
        camLoc.Y = camLoc.Y + speed
    end

    if (KeyPressed(Keys.S)) then --Move down
        camLoc.Y = camLoc.Y - speed
    end

    if (KeyPressed(Keys.KeypadPlus)) then --Zoom in
        camLoc.Z = Max(camLoc.Z - 2.0, 2 * BlockSize)
    elseif (KeyPressed(Keys.KeypadMinus)) then --Zoom out
        camLoc.Z = Min(camLoc.Z + 2.0, 55.0 * BlockSize)
    end

    camera.Location = camLoc
    camera.LookAtLocation = Vector(camLoc.X, camLoc.Y + 150, 20)
end

