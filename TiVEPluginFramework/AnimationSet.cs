using System;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class AnimationSet
    {
        /// <summary>
        /// Time (in seconds) between animation frames
        /// </summary>
        private readonly float timeBetweenFrames;
        private readonly VoxelSprite[] spriteList;
        private readonly bool loopAnimation;
        private float currentTime;
        private short currentSpriteIndex;

        public AnimationSet(float timeBetweenFrames, bool loopAnimation, params VoxelSprite[] spriteList)
        {
            if (timeBetweenFrames <= 0.0f)
                throw new ArgumentException("Value must be greater than zero", nameof(timeBetweenFrames));
            if (spriteList.Length < 2)
                throw new ArgumentException("Sprite list must contain more than one sprite", nameof(spriteList));

            this.timeBetweenFrames = timeBetweenFrames;
            this.loopAnimation = loopAnimation;
            this.spriteList = spriteList;
        }

        public VoxelSprite CurrentSprite => 
            spriteList[currentSpriteIndex];

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the update changed the current sprite, false if nothing changed</returns>
        public bool Update(float timeSinceLastUpdate)
        {
            currentTime += timeSinceLastUpdate;
            if (currentTime >= timeBetweenFrames)
            {
                currentTime -= timeBetweenFrames;
                currentSpriteIndex++;
                if (currentSpriteIndex >= spriteList.Length)
                    currentSpriteIndex = (short)(loopAnimation ? 0 : spriteList.Length - 1);
                return true;
            }
            return false;
        }
    }
}
