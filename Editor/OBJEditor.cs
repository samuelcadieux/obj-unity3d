using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityExtension;


public class OBJWindow : EditorWindow
{
	//------------------------------------------------------------------------------------------------------------
	private Transform m_root = null;

	//------------------------------------------------------------------------------------------------------------
	[MenuItem("Window/OBJ-IO Mesh Exporter")]
	public static void Execute()
	{
		OBJWindow.GetWindow<OBJWindow>();
	}

	[MenuItem("CONTEXT/Transform/OBJ-IO Export Mesh")]
	public static void ExportTransform(MenuCommand command)
	{
		var selection = command.context as Transform;
		ExportMesh(selection);
	}

	[MenuItem("CONTEXT/Transform/OBJ-IO Export Mesh", true)]
	public static bool CheckTransformExport(MenuCommand command)
	{
		var selection = command.context as Transform;
		if (selection == null) 
		{
			return false;
		}

		return selection.GetComponent<MeshFilter>() != null || selection.GetComponentInChildren<MeshFilter>() != null;
	}

	[MenuItem("CONTEXT/MeshFilter/OBJ-IO Export Mesh")]
	public static void ExportMeshFilter(MenuCommand command)
	{
		var selection = command.context as MeshFilter;
		ExportMesh(selection.transform);
	}


	//------------------------------------------------------------------------------------------------------------
	private void OnGUI()
	{
		m_root = (Transform)EditorGUILayout.ObjectField("Root", m_root, typeof(Transform), true);

		if (m_root == null)
		{
			GUILayout.Label("Please provide a Transform  which contains (including it's children)  at least on or more MeshFilter component");
			return;
		}

		if (GUILayout.Button("Export OBJ"))
		{
			ExportMesh(m_root);
		}
	}

	private static void ExportMesh(Transform root) {
		var meshFilters = new List<MeshFilter>();
		meshFilters.AddRange(root.GetComponents<MeshFilter>());
		meshFilters.AddRange(root.GetComponentsInChildren<MeshFilter>());

		Mesh mesh;
		if (meshFilters.Count > 1) {
			CombineInstance[] combine = new CombineInstance[meshFilters.Count];
			for (int i = 0; i < meshFilters.Count; ++i) {
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			}

			mesh = new Mesh();
			mesh.CombineMeshes(combine);
			mesh.Optimize();
		} else if (meshFilters.Count > 0) {
			mesh = meshFilters[0].sharedMesh;
		} else {
			return;
		}

		var lOutputPath = EditorUtility.SaveFilePanel("Save Mesh as OBJ", "", root.name + ".obj", "obj");

		if (File.Exists(lOutputPath))
		{
			File.Delete(lOutputPath);
		}

		var lStream = new FileStream(lOutputPath, FileMode.Create);
		var lOBJData = mesh.EncodeOBJ();
		OBJLoader.ExportOBJ(lOBJData, lStream);
		lStream.Close();
}
}
