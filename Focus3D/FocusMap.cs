using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Focus3D
{
    class FocusMap
    {
        private int[] _map;
        private bool[] _triggered;
        private int _regionsX;
        private int _regionsY;
        protected int length;

        public FocusMap(int x, int y)
        {
            _map = new int[x * y];
            _triggered = new bool[x * y];
            for (int i = 0; i < _triggered.Length; i++)
                _triggered[i] = false;
            _regionsX = x;
            _regionsY = y;
            length = _map.Length;
        }

        public int get(int i)
        {
            if (i < this.length)
                return _map[i];
            else
                throw new IndexOutOfRangeException("Index out of range\n");
        }

        public int[] get()
        {
                return _map;
        }

        public bool set(int[] i)
        {
            if (i.Length == this.length)
            {
                _map = i;
                return true;
            }
            return false;
            
        }

        public void setTrigger(bool[] b)
        { _triggered = b; }

        public void setTrigger(int i)
        { _triggered[i] = true; }

        public void cancelTrigger(int i)
        { _triggered[i] = false; }

        public bool triggered(int i)
        { return _triggered[i]; }

        public bool[] triggered()
        { return _triggered; }

        public void set(int i, int v)
        {
            this._map[i] = v;
        }

        public static bool[] operator > (FocusMap A, FocusMap B) {
            bool[] map;
            if (A._regionsY == B._regionsY && A._regionsX == B._regionsX)
            {
                map = new bool[A._regionsY * A._regionsX];
                for (int i = 0; i < A.length; i++)
                    map[i] = Math.Abs(A.get(i) - B.get(i)) > 50;
            }
            else
                map = new bool[1] {false};
            return map;
        }

        public static bool[] operator < (FocusMap A, FocusMap B)
        {
            bool[] map;
            if (A._regionsY == B._regionsY && A._regionsX == B._regionsX)
            {
                map = new bool[A._regionsY * A._regionsX];
                for (int i = 0; i < A.length; i++)
                    map[i] = A.get(i) < B.get(i);
            }
            else
                map = new bool[1] {false};
            return map;
        }
    }
}
