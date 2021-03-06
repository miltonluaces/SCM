#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet  {

    internal class SupplyChain {

        #region Imports

        private Dictionary<ulong, List<Relation>> mainGraph;
        private Dictionary<ulong, List<BomRelation>> bomGraph;

        #endregion

        #region Constructor

        internal SupplyChain() {
            mainGraph = new Dictionary<ulong, List<Relation>>();
            bomGraph = new Dictionary<ulong, List<BomRelation>>();
        }

        #endregion

        #region internal Methods

        internal void LoadGraphs() {
            List<Relation> relations = LoadRelations();
            LoadMainGraph(relations);
            List<BomRelation> bomRelations = LoadBomRelations();
            LoadBomGraph(bomRelations);
        }

        internal List<Node> GetMainAdjacents(Node root) {
            if (!mainGraph.ContainsKey(root.Id)) { throw new Exception("Error. Id " + root.Id + " not included");  } 
            List<Node> branches = new List<Node>();
            foreach (Relation rel in mainGraph[root.Id]) { branches.Add(rel.Target); }
            return branches;
        }

        internal List<Node> GetBomAdjacents(Node root) {
            if (!bomGraph.ContainsKey(root.Id)) { throw new Exception("Error. Id " + root.Id + " not included"); }
            List<Node> branches = new List<Node>();
            foreach (BomRelation bomRel in bomGraph[root.Id]) { branches.Add(bomRel.Target); }
            return branches;
        }

        #endregion

        #region Private Methods

        private List<Relation> LoadRelations()  {
            List<Relation> relations = new List<Relation>();
            BrkrMgr.GetInstance().ReadMany(relations, "");
            return relations;
        }

        private List<BomRelation> LoadBomRelations() {
            List<BomRelation> bomRelations = new List<BomRelation>();
            BrkrMgr.GetInstance().ReadMany(bomRelations, "");
            return bomRelations;
        }

        private void LoadMainGraph(List<Relation> relations) {
            ulong nodeKey;
            foreach (Relation rel in relations) {
                nodeKey = rel.Origin.Id;
                if (!mainGraph.ContainsKey(nodeKey)) { mainGraph.Add(rel.Origin.Id, new List<Relation>()); }
                mainGraph[nodeKey].Add(rel);
            }
        }

        private void LoadBomGraph(List<BomRelation> bomRelations) {
            ulong nodeKey;
            foreach (BomRelation bomRel in bomRelations)
            {
                nodeKey = bomRel.Origin.Id;
                if (!bomGraph.ContainsKey(nodeKey)) { bomGraph.Add(bomRel.Origin.Id, new List<BomRelation>()); }
                bomGraph[nodeKey].Add(bomRel);
            }
        }

        #endregion
    }
}
