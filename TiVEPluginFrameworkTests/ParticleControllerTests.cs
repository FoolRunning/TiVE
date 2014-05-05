using NUnit.Framework;
using ProdigalSoftware.TiVEPluginFramework.Particles;

namespace TiVEPluginFrameworkTests
{
    /// <summary>
    /// Tests for the ParticleController class
    /// </summary>
    [TestFixture]
    public class ParticleControllerTests
    {
        #region ApplyVelocity tests
        /// <summary>
        /// Tests the ApplyVelocity method when there is no velocity
        /// </summary>
        [Test]
        public void ApplyVelocity_NoVelocity()
        {
            Particle part = new Particle();
            part.SetLocation(120, 100, 52);

            OnlyVelocityController controller = new OnlyVelocityController();
            controller.Update(part, 0.5f, 0, 0, 0);

            part.VerifyLocation(120, 100, 52);
        }

        /// <summary>
        /// Tests the ApplyVelocity method when there is velocity, but no time has passed
        /// </summary>
        [Test]
        public void ApplyVelocity_NoTimePassed()
        {
            Particle part = new Particle();
            part.SetLocation(120, 100, 52);
            part.SetVelocity(0.5f, -1.0f, 0.1f);

            OnlyVelocityController controller = new OnlyVelocityController();
            controller.Update(part, 0.0f, 0, 0, 0);

            part.VerifyLocation(120, 100, 52);
        }

        /// <summary>
        /// Tests the ApplyVelocity method when there is velocity and time has passed
        /// </summary>
        [Test]
        public void ApplyVelocity_VelocityAndTime()
        {
            Particle part = new Particle();
            part.SetLocation(120, 100, 52);
            part.SetVelocity(0.5f, -1.0f, 0.1f);

            OnlyVelocityController controller = new OnlyVelocityController();
            controller.Update(part, 0.5f, 0, 0, 0);

            part.VerifyLocation(120.25f, 99.5f, 52.05f);
        }
        #endregion

        #region OnlyVelocityController class
        /// <summary>
        /// Simple ParticleController that applies the particle's current velocity to itself
        /// </summary>
        private class OnlyVelocityController : ParticleController
        {
            public override bool BeginUpdate(IParticleSystem particleSystem, float timeSinceLastUpdate)
            {
                throw new System.NotImplementedException();
            }

            public override void Update(Particle particle, float timeSinceLastUpdate, float systemX, float systemY, float systemZ)
            {
                ApplyVelocity(particle, timeSinceLastUpdate);
            }

            public override void InitializeNew(Particle particle, float systemX, float systemY, float systemZ)
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion
    }

    #region ParticleTestExtensions class
    /// <summary>
    /// Extension methods for the Particle class to aid in testing
    /// </summary>
    public static class ParticleTestExtensions
    {
        /// <summary>
        /// Sets the location of this Particle to the specified location
        /// </summary>
        public static void SetLocation(this Particle part, float x, float y, float z)
        {
            part.X = x;
            part.Y = y;
            part.Z = z;
        }

        /// <summary>
        /// Sets the velocity of this Particle to the specified velocity
        /// </summary>
        public static void SetVelocity(this Particle part, float velX, float velY, float velZ)
        {
            part.VelX = velX;
            part.VelY = velY;
            part.VelZ = velZ;
        }

        /// <summary>
        /// Verifies the location of this Particle is the specified location
        /// </summary>
        public static void VerifyLocation(this Particle part, float expectedX, float expectedY, float expectedZ)
        {
            Assert.That(part.X, Is.EqualTo(expectedX), "Particle location on the x-axis was wrong");
            Assert.That(part.Y, Is.EqualTo(expectedY), "Particle location on the y-axis was wrong");
            Assert.That(part.Z, Is.EqualTo(expectedZ), "Particle location on the z-axis was wrong");
        }

        /// <summary>
        /// Verifies the velocity of this Particle is the specified velocity
        /// </summary>
        public static void VerifyVelocity(this Particle part, float expectedXVel, float expectedYVel, float expectedZVel)
        {
            Assert.That(part.VelX, Is.EqualTo(expectedXVel), "Particle velocity on the x-axis was wrong");
            Assert.That(part.VelY, Is.EqualTo(expectedYVel), "Particle velocity on the y-axis was wrong");
            Assert.That(part.VelZ, Is.EqualTo(expectedZVel), "Particle velocity on the z-axis was wrong");
        }
    }
    #endregion
}
