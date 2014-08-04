using UnityEngine;
using System.Collections;
using Sorcery;
using NodeCanvas;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Invoke("TestMove", 5f);
	}

	private void TestMove()
	{
		FSMInstruction inst = new FSMInstructionAttack();
        GetComponent<Blackboard>().SetDataValue("myObject", inst);
	}

}
