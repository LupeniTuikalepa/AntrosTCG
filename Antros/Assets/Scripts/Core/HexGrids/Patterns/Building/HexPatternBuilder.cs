using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.HexGrids.Grids;
using Helteix.Tools.DataMapping;
using UnityEngine.Pool;

namespace ATCG.HexGrids.Patterns.Building
{
    public readonly struct HexPatternBuilder<TController> : IDisposable
        where TController : IHexPatternController
    {
        public readonly TController controller;
        public readonly HexCoordinates origin;
        private readonly HashSet<HexCoordinates> coordinates;

        public HexPatternBuilder(IEnumerable<HexCoordinates> coordinates, TController controller)
        {
            this.controller = controller;
            this.coordinates = HashSetPool<HexCoordinates>.Get();
            foreach (var coordinate in coordinates)
                this.coordinates.Add(coordinate);
            origin = this.coordinates.FirstOrDefault();
        }

        public HexPatternBuilder(HexCoordinates origin, TController controller)
        {
            this.origin = origin;
            this.controller = controller;
            coordinates = HashSetPool<HexCoordinates>.Get();
        }

        public bool Contains(HexCoordinates coord) => coordinates.Contains(coord);
        public IEnumerable<HexCoordinates> GetCoordinates() => coordinates;

        public IEnumerable<HexCell> GetCells(HexGrid hexGrid)
        {
            foreach (var coordinate in coordinates)
                if (hexGrid.TryGetCell(coordinate, out HexCell cell))
                    yield return cell;
        }

        public HexPatternBuilder<TController> Clear()
        {
            coordinates.Clear();
            return this;
        }

        public HexPatternBuilder<TController> With(PatternGroup group) => With(group, origin);

        public HexPatternBuilder<TController> With(PatternGroup group, HexCoordinates source)
        {
            for (int i = 0; i < group.Data.Length; i++)
                With(group.Data[i], source);

            return this;
        }

        public HexPatternBuilder<TController> With(PatternData data, HexCoordinates source)
        {
            if (Mapper.TryGet(data, out IPatternContainer container))
                container.AddToBuilder(data, this, source);

            return this;
        }

        public HexPatternBuilder<TController> With(PatternData data)
        {
            return With(data, origin);
        }

        public HexPatternBuilder<TController> With<TPattern>(TPattern pattern, HexCoordinates source)
            where TPattern : IHexPattern
        {
            foreach (var coordinate in pattern.GetAll(source, controller))
                coordinates.Add(coordinate);

            return this;
        }
        public HexPatternBuilder<TController> With<TPattern>(TPattern pattern)
            where TPattern : IHexPattern
            => With(pattern, origin);

        public HexPatternBuilder<TController> With(HexCoordinates point)
        {
            coordinates.Add(point);
            return this;
        }
        public HexPatternBuilder<TController> Without<TPattern>(TPattern pattern) where TPattern : IHexPattern
            => Without(pattern, origin);

        public HexPatternBuilder<TController> Without<TPattern>(TPattern pattern, HexCoordinates source)
            where TPattern : IHexPattern
        {
            foreach (var coordinate in pattern.GetAll(source, controller))
                coordinates.Remove(coordinate);
            return this;
        }


        public HexPatternBuilder<TController> Without(HexCoordinates point)
        {
            coordinates.Remove(point);
            return this;
        }

        public void Dispose() => HashSetPool<HexCoordinates>.Release(coordinates);
    }
}