using System;
using System.Collections;
using System.Collections.Generic;

namespace ATCG.Battle.Grids
{
    public struct Path : IEnumerable<PathSegment>
    {
        private readonly List<PathSegment> segments;

        public Path(IEnumerable<PathSegment> segments)
        {
            this.segments = new List<PathSegment>(segments);
        }

        public IEnumerator<PathSegment> GetEnumerator() => this.segments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}