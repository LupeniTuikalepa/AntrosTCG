using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities.Components;
using UnityEngine;

namespace ATCG.Battle
{
    [Serializable]
    public class CommandGroup
    {
        public BattleID GroupID => groupID;
        public BattleID ParentGroupID => parentGroupID;
        public IEnumerable<CommandTree> Trees => trees;
        public string Label => label;

        [SerializeField]
        private BattleID groupID;
        [SerializeField]
        private BattleID parentGroupID;

        [SerializeField]
        private List<CommandTree> trees;
        [SerializeField]
        private string label;

        public CommandGroup(string label, CommandGroup parentGroup)
        {
            parentGroupID = parentGroup?.groupID ?? BattleID.None;

            groupID = BattleID.CreateNew();
            trees = new List<CommandTree>();
            this.label = label;
        }
        public CommandGroup(string label) : this(label, null)
        {
            parentGroupID = BattleID.None;
        }

        public void AddTree(CommandTree tree)
        {
            trees.Add(tree);
        }
    }
}