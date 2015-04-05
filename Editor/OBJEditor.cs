using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityExtension;


public class OBJWindow : EditorWindow
{
	//------------------------------------------------------------------------------------------------------------
	private GameObject m_root = null;

	//------------------------------------------------------------------------------------------------------------
	[MenuItem("OBJ-IO/OBJ Mesh Exporter")]
	public static void Execute()
	{
		OBJWindow.GetWindow<OBJWindow>();
	}

	//------------------------------------------------------------------------------------------------------------
	private void OnGUI()
	{
		m_root = (GameObject)EditorGUILayout.ObjectField("Root", m_root, typeof(GameObject), true);

		if (m_root != null)
		{
			var meshFilters = new List<MeshFilter>();
			meshFilters.AddRange(m_root.GetComponents<MeshFilter>());
			meshFilters.AddRange(m_root.GetComponentsInChildren<MeshFilter>());

			Mesh mesh;
			if (meshFilters.Count > 0) {
				CombineInstance[] combine = new CombineInstance[meshFilters.Count];
				for (int i = 0; i < meshFilters.Count; ++i) {
					combine[i].mesh = meshFilters[i].sharedMesh;
					combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				}

				mesh = new Mesh();
				mesh.CombineMeshes(combine);
				mesh.Optimize();
			} else {
				mesh = meshFilters[0].sharedMesh;
			}

			if (GUILayout.Button("Export OBJ"))
			{
				var lOutputPath = EditorUtility.SaveFilePanel("Save Mesh as OBJ", "", m_root.name + ".obj", "obj");

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
		else
		{
			GUILayout.Label("Please provide a GameObject which contains (including it's children)  at least on or more MeshFilter component");
		}
	}
}
