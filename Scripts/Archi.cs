using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archi.Core
{
    [RequireComponent(typeof(Grid))]
    [ExecuteInEditMode] 
    public class Archi : MonoBehaviour
    {
        [HideInInspector]
        public GameObject geometry;
        [HideInInspector]
        public List<Material> materials = new List<Material>();

        //Data
        [HideInInspector]
        public int selectedMaterial = 0;
        [HideInInspector]
        public int selectedTool = 0;
        [HideInInspector]
        public Grid grid;

        GameObject plane;
        private void Awake()
        {
            grid = GetComponent<Grid>();
            if (geometry == null)
            {
                geometry = new GameObject("ArchiGeometry");
            }
        }
        private void OnValidate()
        {
            grid = GetComponent<Grid>();
        }
        void Start()
        {

        }

        void Update()
        {
            if (geometry == null)
            {
                geometry = new GameObject("ArchiGeometry");
            }
            if (grid == null)
            {
                grid = GetComponent<Grid>();
            }
        }
    }
}