﻿WorldXSize = 200;
WorldYSize = 200;
WorldZSize = 20;

function Initialize(camera)
    gameWorld = CreateWorld(WorldXSize, WorldYSize, WorldZSize)

    camera.FoV = PI / 4 --45 degrees
    camera.Location = Vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2, 345)
end


function Update(camera, keyboard)
    camLoc = camera.Location

    speed = 2
    if (KeyPressed(Key.ShiftLeft)) then --Speed up
        speed = 10
    elseif (KeyPressed(Key.ControlLeft)) then --Slow down
        speed = 0.2
    end

    if (KeyPressed(Key.A)) then --Move left
        camLoc.X = camLoc.X - speed
    end

    if (KeyPressed(Key.D)) then --Move right
        camLoc.X = camLoc.X + speed
    end

    if (KeyPressed(Key.W)) then --Move up
        camLoc.Y = camLoc.Y + speed
    end

    if (KeyPressed(Key.S)) then --Move down
        camLoc.Y = camLoc.Y - speed
    end

    if (KeyPressed(Key.KeypadPlus)) then --Zoom in
        camLoc.Z = Max(camLoc.Z - 2.0, 2 * BlockSize)
    elseif (KeyPressed(Key.KeypadMinus)) then --Zoom out
        camLoc.Z = Min(camLoc.Z + 2.0, 60.0 * BlockSize)
    end

    camera.Location = camLoc
    camera.LookAtLocation = Vector(camLoc.X, camLoc.Y + 150, -20)
end

