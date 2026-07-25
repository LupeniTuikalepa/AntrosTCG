using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Capacities.Data.Status;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public struct HexPathfinder
    {
        public PathfindingTraversableRule[] customTraversableRules;

        public bool TryFindPath(PathfindingAgentAspect agentAspect, HexCoordinates start, HexCoordinates goal, List<HexCoordinates> path, int maxSteps = 128)
        {
            using PathfindingContext context = new PathfindingContext(maxSteps);
            BattleGrid battleGrid = agentAspect.GridMemberComponent.grid;

            if (!battleGrid.TryGetBattleCell(start, out BattleCellAspect startCell))
                return false;

            if (!battleGrid.TryGetBattleCell(goal, out BattleCellAspect goalCell))
                return false;

            context.frontier.Add(new PriorityHexCoordinates(start, 0));
            context.cameFrom[start] = start;
            context.costSoFar[start] = 0;

            while (context.frontier.Count > 0)
            {
                context.frontier.Sort();
                var current = context.frontier[0].coordinates;
                context.frontier.RemoveAt(0);

                if (current == goal)
                    break;

                if (!battleGrid.TryGetBattleCell(current, out _))
                    continue;

                foreach (HexCoordinates next in GetNeighbors(current, battleGrid))
                {
                    AgentMovementType movementType = agentAspect.MovementType;
                    if (!battleGrid.TryGetBattleCell(next, out BattleCellAspect nextCell))
                        continue;

                    if (!CanTraverse(agentAspect, nextCell))
                        continue;

                    if (!battleGrid.TryGetBattleCell(next, out BattleCellAspect currentCell))
                        continue;

                    bool nextIsGoal = next == goal;
                    HexCoordinates actual = next;

                    using (HashSetPool<HexCoordinates>.Get(out var redirectedCoord))
                    {
                        while (TryRedirect(agentAspect, currentCell, ref actual, ref movementType) && !nextIsGoal)
                        {
                            if (redirectedCoord.Add(actual))
                            {
                                context.costSoFar[next] = int.MaxValue;
                                context.cameFrom[next] = current;
                            }

                            //Redirected out of bound
                            if (!battleGrid.TryGetBattleCell(actual, out nextCell))
                                break;
                        }
                    }

                    int newCost = context.costSoFar[current] + GetCost(current, actual, nextCell);

                    if (newCost > maxSteps)
                        continue;

                    if (!context.costSoFar.ContainsKey(actual) || newCost < context.costSoFar[actual])
                    {
                        //Debug.Log($"Actual is start {actual == start}, Start {start}");
                        context.costSoFar[actual] = newCost;
                        int priority = newCost + actual.Distance(goal);
                        context.frontier.Add(new PriorityHexCoordinates(actual, priority));
                        context.cameFrom[actual] = current;
                    }
                }
            }

            if (!context.cameFrom.TryGetValue(goal, out HexCoordinates entryCase))
                return false;

            if (battleGrid.TryGetBattleCell(entryCase, out var beforeGoalCell))
            {
                AgentMovementType finalMovementType = agentAspect.MovementType;

                if (!TryRedirect(agentAspect, beforeGoalCell, ref goal, ref finalMovementType))
                    return ReconstructPath(start, goal, path, context);

                if (!ReconstructPath(start, goal, path, context))
                    return false;

                path.Add(goal);
            }
            return true;
        }

        private IEnumerable<HexCoordinates> GetNeighbors(
            HexCoordinates from,
            BattleGrid battleGrid)
        {
            for (var i = 0; i < HexOperations.Directions.Length; i++)
            {
                HexCoordinates neighbor = from + HexOperations.Directions[i];
                if (battleGrid.TryGetBattleCell(neighbor, out _))
                    yield return neighbor;
            }
        }

        private bool ReconstructPath(
            HexCoordinates start,
            HexCoordinates goal,
            List<HexCoordinates> path, PathfindingContext context)
        {
            if (!context.cameFrom.ContainsKey(goal))
                return false;
            if(!context.cameFrom.ContainsKey(start))
                return false;

            //Debug.Log($"Reconstruct path from {start} to {goal}");
            HexCoordinates current = goal;
            while (current != start)
            {
                path.Add(current);
                current = context.cameFrom[current];
            }

            path.Reverse();
            return path.Count > 0;
        }
        public bool CanTraverse(PathfindingAgentAspect agent, BattleCellAspect cell)
        {
            if (!cell.CanBeMovedOn())
                return false;

            for (int i = 0; i < agent.AgentRules.Length; i++)
            {

                if (!agent.AgentRules[i].CanTraverse(agent, cell))
                    return false;
            }


            if (customTraversableRules != null)
            {
                for (int i = 0; i < customTraversableRules.Length; i++)
                {
                    if (!customTraversableRules[i].CanTraverse(agent, cell))
                        return false;
                }
            }

            return true;
        }

        //TODO
        private int GetCost(HexCoordinates from, HexCoordinates to, BattleCellAspect cell) => 1;

        private bool TryRedirect(PathfindingAgentAspect agentAspect, BattleCellAspect from, ref HexCoordinates to, ref AgentMovementType segmentType)
        {
            if (!from.IsValid())
                return false;

            PathfindingRedirectionIterator iterator = new PathfindingRedirectionIterator(from, to, agentAspect);
            iterator.ForeachRedirectStatusComponent();

            if (!iterator.WasRedirected)
                return false;

            to = iterator.To;
            segmentType = iterator.SegmentType;
            return true;
        }
    }
}