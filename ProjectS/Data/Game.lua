﻿WorldXSize = 41;
WorldYSize = 31;
WorldZSize = 5;
HalfWorld = WorldXSize * BlockSize / 2

ambientLightUpdateTime = 0
currentAmbientLight = 0.4

function Initialize(camera)
    LoadWorld("Bla")
    --Renderer().LightProvider.AmbientLight = Color(currentAmbientLight, currentAmbientLight, currentAmbientLight)

    camera.FarDistance = 2000
    camera.FoV = PI / 4 --45 degrees

    camera.LookAtLocation = Vector(WorldXSize / 2 * BlockSize, WorldYSize / 2 * BlockSize - 50, 0)
end

cameraAngle = 25

function Update(camera, keyboard)
    cameraAngle = cameraAngle + 1

    camera.Location = Vector(HalfWorld + Cos(ToRad(cameraAngle / 3)) * 70, -100 + Sin(ToRad(cameraAngle)) * 40, 450)

    ambientLightUpdateTime = ambientLightUpdateTime + 1;

    if (ambientLightUpdateTime > 120) then
        ambientLightUpdateTime = 0

        if (currentAmbientLight > 0) then
            currentAmbientLight = currentAmbientLight - 0.01
            if (currentAmbientLight < 0) then
                currentAmbientLight = 0;
            end

            --Renderer().LightProvider.AmbientLight = Color(currentAmbientLight, currentAmbientLight, currentAmbientLight)
            --ReloadLevel()
        end
    end
end

