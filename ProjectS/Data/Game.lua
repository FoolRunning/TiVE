WorldXSize = 0;
WorldYSize = 0;
WorldZSize = 0;

ambientLightUpdateTime = 0
currentAmbientLight = 0.4

function Initialize(camera)
    gameWorld = LoadWorld("Bla")
    WorldXSize = gameWorld.BlockSize.X
    WorldYSize = gameWorld.BlockSize.Y
    WorldZSize = gameWorld.BlockSize.Z
    --Renderer().LightProvider.AmbientLight = Color(currentAmbientLight, currentAmbientLight, currentAmbientLight)

    camera.FarDistance = 2000
    camera.FoV = PI / 4 --45 degrees

    camera.LookAtLocation = Vector(WorldXSize * BlockSize / 2, WorldYSize * BlockSize / 2 - 50, 0)
end

cameraAngle = 25

function Update(camera, keyboard)
    cameraAngle = cameraAngle + 1

    camera.Location = Vector(WorldXSize * BlockSize / 2 + Cos(ToRad(cameraAngle / 3)) * 70, -100 + Sin(ToRad(cameraAngle)) * 40, 450)

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

