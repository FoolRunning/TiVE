WorldXSize = 71;
WorldYSize = 41;
WorldZSize = 20;


function Initialize(camera)
    gameWorld = CreateWorld(WorldXSize, WorldYSize, WorldZSize)

    camera.FoV = PI / 4 --45 degrees

    camera.Location = Vector(WorldXSize / 2 * BlockSize, -100, 450)
    camera.LookAtLocation = Vector(WorldXSize / 2 * BlockSize, WorldYSize / 2 * BlockSize, 0)
end


function Update(camera, keyboard)

end

