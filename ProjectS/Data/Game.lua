﻿WorldXSize = 71;
WorldYSize = 41;
WorldZSize = 5;
HalfWorld = WorldXSize * BlockSize / 2

ambientLightUpdateTime = 0
currentAmbientLight = 0.4

function Initialize(camera)
    gameWorld = CreateWorld(WorldXSize, WorldYSize, WorldZSize)
    gameWorld.AmbientLight = Color(currentAmbientLight, currentAmbientLight, currentAmbientLight)

    camera.FoV = PI / 4 --45 degrees

    camera.LookAtLocation = Vector(WorldXSize / 2 * BlockSize, WorldYSize / 2 * BlockSize, 0)
end

cameraAngle = 25

function Update(camera, keyboard)
    cameraAngle = cameraAngle + 1

    camera.Location = Vector(HalfWorld + Cos(ToRad(cameraAngle / 3)) * HalfWorld, -100 + Sin(ToRad(cameraAngle)) * 100, 400)


    ambientLightUpdateTime = ambientLightUpdateTime + 1;

    if (ambientLightUpdateTime > 120) then
        ambientLightUpdateTime = 0

        currentAmbientLight = currentAmbientLight - 0.01
        if (currentAmbientLight < 0) then
            currentAmbientLight = 0;
        end

        gameWorld = GameWorld()
        gameWorld.AmbientLight = Color(currentAmbientLight, currentAmbientLight, currentAmbientLight)
        ReloadLevel()
    end
end
