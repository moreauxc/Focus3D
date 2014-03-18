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
        public int[] Map
        {
            get
            {
                return _map;
            }
        }
        private Trigger[] _triggered;
        public Trigger[] Triggers
        {
            get
            {
                return _triggered;
            }
        }
        private int _regionsX;
        private int _regionsY;
        protected int length;
        public enum Trigger {Unseen = 0, Triggered, Drawn };
        public FocusMap(int x, int y)
        {
            _map = new int[x * y];
            _triggered = new Trigger[x * y];
            for (int i = 0; i < _triggered.Length; i++)
                _triggered[i] = Trigger.Unseen;
            _regionsX = x;
            _regionsY = y;
            length = _map.Length;
        }

        public bool setMap(int[] i)
        {
            if (i.Length == this.length)
            {
                i.CopyTo(_map, 0);
                return true;
            }
            return false;
            
        }

        public bool setTriggers(Trigger[] i)
        {
            if (i.Length == this.length)
            {
                i.CopyTo(_triggered, 0);
                return true;
            }
            return false;

        }

        public static bool[] operator > (FocusMap A, FocusMap B) {
            bool[] map;
            if (A._regionsY == B._regionsY && A._regionsX == B._regionsX)
            {
                map = new bool[A._regionsY * A._regionsX];
                for (int i = 0; i < A.length; i++)
                    map[i] = A.Map[i] - B.Map[i] > 10;
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
                    map[i] = Math.Abs(A.Map[i] - B.Map[i]) < Math.Abs(A.Map[i] / 2);
            }
            else
                map = new bool[1] {false};
            return map;
        }
    }
}
