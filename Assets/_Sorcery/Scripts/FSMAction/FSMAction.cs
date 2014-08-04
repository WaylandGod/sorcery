using UnityEngine;
using System.Collections;
using NodeCanvas;

namespace Sorcery
{
    public abstract class FSMAction : ActionTask
    {
        protected AnimControl m_animControl = new AnimControl();

        protected override string OnInit()
        {
            return m_animControl.Init(gameObject);
        }
    }
}


