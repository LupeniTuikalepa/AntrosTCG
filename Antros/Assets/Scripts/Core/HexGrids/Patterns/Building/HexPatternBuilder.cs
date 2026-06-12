using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns.Building
{
    public readonly struct HexPatternBuilder : IDisposable
    {
        private readonly HexCoordinates from;
        private readonly HashSet<HexCoordinates> coordinates;

        public HexPatternBuilder(HexCoordinates from)
        {
            this.from = from;
            coordinates = HashSetPool<HexCoordinates>.Get();
        }


        public bool Contains(HexCoordinates coord) => coordinates.Contains(coord);

        public IEnumerable<HexCoordinates> GetCoordinates() => coordinates;

        public IEnumerable<HexCell> GetCells(HexGrid hexGrid)
        {
            foreach (var coordinate in coordinates)
            {
                if(hexGrid.TryGetCell(coordinate, out HexCell cell))
                    yield return cell;
            }
        }


        public HexPatternBuilder Clear()
        {
            coordinates.Clear();
            return this;
        }

        public HexPatternBuilder With<T>(T pattern) where T : IHexPattern => With(pattern, from);

        public HexPatternBuilder With<T>(T pattern, HexCoordinates source) where T : IHexPattern
        {
            foreach (var coordinate in pattern.GetAll(source))
                coordinates.Add(coordinate);

            return this;
        }

        public HexPatternBuilder With(HexCoordinates point)
        {
            coordinates.Add(point);
            return this;
        }
        public HexPatternBuilder Without<T>(T pattern) where T : IHexPattern => With(pattern, from);

        public HexPatternBuilder Without<T>(T pattern, HexCoordinates source) where T : IHexPattern
        {
            foreach (var coordinate in pattern.GetAll(source))
                coordinates.Remove(coordinate);

            return this;
        }

        public HexPatternBuilder Without(HexCoordinates point)
        {
            coordinates.Remove(point);
            return this;
        }

        public void Dispose()
        {
            HashSetPool<HexCoordinates>.Release(coordinates);
        }
    }
}