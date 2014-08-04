using UnityEngine;
using System.Collections;
using NodeCanvas;

namespace Sorcery
{
    [Name("Attack")]
    [Category("_FSMAction")]
    public class FSMActionAttack : FSMAction
    {
        protected override void OnExecute()
        {
            m_animControl.Play("Attack");
        }
    }
}


