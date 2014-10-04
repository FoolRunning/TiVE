
function Update(particle, time, systemX, systemY, systemZ)
    if particle.X > systemX then
        particle.VelX = particle.VelX - FlameDeacceleration * time;
    end
    if particle.X < systemX then
        particle.VelX = particle.VelX + FlameDeacceleration * time;
    end
    if particle.Y > systemY then
        particle.VelY = particle.VelY - FlameDeacceleration * time;
    end
    if particle.Y < systemY then
        particle.VelY = particle.VelY + FlameDeacceleration * time;
    end
    --if particle.Z > systemZ then
    --    particle.VelZ -= FlameDeacceleration * timeSinceLastFrame;
    --end
    --if particle.Z < systemZ then
    --    particle.VelZ += FlameDeacceleration * timeSinceLastFrame;
    --end

    --float totalTime = (float)Math.Pow(particleAliveTime, 5);
    --part.size = 1.0f - (float)Math.pow(part.aliveTime, 5) / totalTime;

    particle.Time = particle.Time - time;

    -- set color
    if particle.Time > 0.0 then
        --int colorIndex = (int)(((AliveTime - particle.Time) / AliveTime) * (colorList.Length - 1));
        --particle.Color = colorList[Math.Min(colorIndex, colorList.Length - 1)];
    end
end
            
--private const float FlameDeacceleration = 27.0f;
--private const float AliveTime = 1.0f;
--private readonly Random random = new Random();
--private static readonly Color4b[] colorList = new Color4b[256];
--
--static FireUpdater()
--{
    --for (int i = 0; i < 256; i++)
    --{
        --if (i < 150)
            --colorList[i] = new Color4b(255, (byte)(255 - (i * 1.7f)), (byte)(50 - i / 3), 200);
        --if (i >= 150)
            --colorList[i] = new Color4b((byte)(255 - (i - 150) * 2.4f), 0, 0, 200);
    --}
--}
--
--#region Implementation of IParticleUpdater
--public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastFrame)
--{
    --return true;
--}
--
--public override void Update(Particle particle, float timeSinceLastFrame, float systemX, float systemY, float systemZ)
--{
    --ApplyVelocity(particle, timeSinceLastFrame);
--
    --if (particle.X > systemX)
        --particle.VelX -= FlameDeacceleration * timeSinceLastFrame;
    --if (particle.X < systemX)
        --particle.VelX += FlameDeacceleration * timeSinceLastFrame;
    --if (particle.Y > systemY)
        --particle.VelY -= FlameDeacceleration * timeSinceLastFrame;
    --if (particle.Y < systemY)
        --particle.VelY += FlameDeacceleration * timeSinceLastFrame;
    --//if (particle.Z > systemZ)
    --//    particle.VelZ -= FlameDeacceleration * timeSinceLastFrame;
    --//if (particle.Z < systemZ)
    --//    particle.VelZ += FlameDeacceleration * timeSinceLastFrame;
                --
    --//float totalTime = (float)Math.Pow(particleAliveTime, 5);
    --//part.size = 1.0f - (float)Math.pow(part.aliveTime, 5) / totalTime;
--
    --particle.Time -= timeSinceLastFrame;
--
    --// set color
    --if (particle.Time > 0.0f)
    --{
        --int colorIndex = (int)(((AliveTime - particle.Time) / AliveTime) * (colorList.Length - 1));
        --particle.Color = colorList[Math.Min(colorIndex, colorList.Length - 1)];
    --}
--}
--
--public override void InitializeNew(Particle particle, float startX, float startY, float startZ)
--{
    --float angle = (float)random.NextDouble() * 2.0f * 3.141592f;
    --float totalVel = (float)random.NextDouble() * 4.0f + 10.0f;
    --particle.VelX = (float)Math.Cos(angle) * totalVel;
    --particle.VelZ = (float)random.NextDouble() * 10.0f + 8.0f;
    --particle.VelY = (float)Math.Sin(angle) * totalVel;
--
    --particle.X = startX;
    --particle.Y = startY;
    --particle.Z = startZ;
                --
    --particle.Color = colorList[0];
    --particle.Time = AliveTime;
--}
--#endregion
--