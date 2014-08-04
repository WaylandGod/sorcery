using UnityEngine;
using System.Collections;

namespace Sorcery
{
    public class AnimControl
    {
        private Animation m_animation;

        public string Init(GameObject obj)
        {
            m_animation = obj.GetComponent<Animation>();
            if (m_animation == null)
            {
                return "Don't have Animation!";
            }

            return null;
        }

        public void Play(string aniName)
        {
            m_animation.Play(aniName);
        }
    }
}
