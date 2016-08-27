using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Util.Extensions {
    public class Grid<T> : IEnumerable<T> {
        private Dictionary<IntVector2, T> values;

        public Grid() {
            values = new Dictionary<IntVector2,T>();
        }

        public T this[int i, int j] {
            get {
                T value;
                values.TryGetValue(new IntVector2(i, j), out value);
                return value;
            }
            set {
                values[new IntVector2(i, j)] = value;
            }
        }

        public int Height {
            get {
                return values.Max(v => v.Key.y) + 1;
            }
        }

        public int Width {
            get {
                return values.Max(v => v.Key.x) + 1;
            }
        }

        public T[,] ToArray() {
            var width = Width;
            var height = Height;
            var array = new T[width, height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    array[i, j] = this[i, j];
                }
            }

            return array;
        }

        public override string ToString()
        {
            var width = Width;
            var sb = new StringBuilder();
            foreach (var paddedRow in PaddedRows()) {
                sb.Append("{");
                int currentCol = 0;
                foreach (var value in paddedRow) {
                    sb.Append(value.ToString());
                    if (currentCol < width - 1) sb.Append(", ");
                    currentCol++;
                }
                sb.Append("}\r\n");
            }

            return sb.ToString();
        }

        public Grid<T> Copy() {
            var copy = new Grid<T>();
            foreach (var point in values) {
                copy[point.Key.x, point.Key.y] = point.Value;
            }
            return copy;
        }

        public void Remove(int i, int j) {
            Remove(new IntVector2(i, j));
        }

        public void Remove(IntVector2 coordinates) {
            values.Remove(coordinates);
        }

        public IEnumerable<T> GetRow(int j) {
            return values.Where(v => v.Key.y == j).Select(v => v.Value);
        }

        public IEnumerable<T> GetColumn(int i) {
            return values.Where(v => v.Key.x == i).Select(v => v.Value);
        }

        public IEnumerable<T> GetPaddedRow(int j) {
            var paddedRow = new List<T>();
            var unpaddedRow = values.Where(v => v.Key.y == j);
            for (int index = 0; index < Width; index++) {
                if (unpaddedRow.Any(v => v.Key.x == index && v.Key.y == j)) {
                    paddedRow.Add(this[index, j]);
                } else {
                    paddedRow.Add(default(T));
                }
            }
            return paddedRow;
        }

        public IEnumerable<T> GetPaddedColumn(int i) {
            var paddedColumn = new List<T>();
            var unpaddedColumn = values.Where(v => v.Key.x == i);
            for (int index = 0; index < Height; index++) {
                if (unpaddedColumn.Any(v => v.Key.x == i && v.Key.y == index)) {
                    paddedColumn.Add(this[i, index]);
                } else {
                    paddedColumn.Add(default(T));
                }
            }
            return paddedColumn;
        }

        public IEnumerable<IEnumerable<T>> Rows() {
            for (int i = 0; i < Height; i++) {
                yield return GetRow(i);
            }
        }

        public IEnumerable<IEnumerable<T>> Columns() {
            for (int i = 0; i < Width; i++) {
                yield return GetColumn(i);
            }
        }

        public IEnumerable<IEnumerable<T>> PaddedRows() {
            for (int i = 0; i < Height; i++) {
                yield return GetPaddedRow(i);
            }
        }

        public IEnumerable<IEnumerable<T>> PaddedColumns() {
            for (int i = 0; i < Width; i++) {
                yield return GetPaddedColumn(i);
            }
        }

        public IEnumerable<IntVector2> GetPositionsWhere(Func<T, bool> predicate) {
            return values.Where(v => predicate(v.Value)).Select(v => v.Key);
        }

        public IEnumerator<T> GetEnumerator() {
            foreach (var value in values) {
                yield return value.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void RotateClockwise() {
            var width = Width;
            var updatedValues = new Dictionary<IntVector2, T>();
            foreach (var item in values) {
                updatedValues.Add(new IntVector2(item.Key.y, width - 1 - item.Key.x), item.Value);
            }
            values = updatedValues;
        }
        public void RotateCounterclockwise() {
            var height = Height;
            var updatedValues = new Dictionary<IntVector2, T>();
            foreach (var item in values) {
                updatedValues.Add(new IntVector2(height - 1 - item.Key.y, item.Key.x), item.Value);
            }
            values = updatedValues;
        }

        public void HorizontalFlip() {
            var width = Width;
            var updatedValues = new Dictionary<IntVector2, T>();
            foreach (var item in values) {
                updatedValues.Add(new IntVector2(width - 1 - item.Key.x, item.Key.y), item.Value);
            }
            values = updatedValues;
        }

        public void VerticalFlip() {
            var height = Height;
            var updatedValues = new Dictionary<IntVector2, T>();
            foreach (var item in values) {
                updatedValues.Add(new IntVector2(item.Key.x, height - 1 - item.Key.y), item.Value);
            }
            values = updatedValues;
        }

        public void ShiftPositions(int xShift, int yShift) {
            if (xShift == 0 && yShift == 0) return;
            var updatedValues = new Dictionary<IntVector2, T>();
            foreach (var item in values) {
                updatedValues.Add(new IntVector2(item.Key.x + xShift, item.Key.y + yShift), item.Value);
            }
            values = updatedValues;
        }
    }
}
