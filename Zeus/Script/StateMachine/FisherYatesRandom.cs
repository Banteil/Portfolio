using UnityEngine;
namespace Zeus
{
    // Fisher-Yates shuffle -- makes sure all items are selected with equal probability and that the same item is not selected twice in a row.
    public class FisherYatesRandom
    {
        private int[] _randomIndices = null;
        private int _randomIndex = 0;
        private int _prevValue = -1;

        public int Next(int len)
        {
            if (len <= 1)
                return 0;

            if (_randomIndices == null || _randomIndices.Length != len)
            {
                _randomIndices = new int[len];
                for (int i = 0; i < _randomIndices.Length; i++)
                    _randomIndices[i] = i;
            }

            if (_randomIndex == 0)
            {
                int count = 0;
                do
                {
                    for (int i = 0; i < len - 1; i++)
                    {
                        int j = Random.Range(i, len);
                        if (j != i)
                        {
                            int tmp = _randomIndices[i];
                            _randomIndices[i] = _randomIndices[j];
                            _randomIndices[j] = tmp;
                        }
                    }
                } while (_prevValue == _randomIndices[0] && ++count < 10); // Make sure the new first element is different from the last one we played
            }

            int value = _randomIndices[_randomIndex];
            if (++_randomIndex >= _randomIndices.Length)
                _randomIndex = 0;

            _prevValue = value;
            return value;
        }

        public int Range(int min, int max)
        {
            var len = (max - min) + 1;
            if (len <= 1)
                return max;

            if (_randomIndices == null || _randomIndices.Length != len)
            {
                _randomIndices = new int[len];
                for (int i = 0; i < _randomIndices.Length; i++)
                    _randomIndices[i] = min + i;
            }

            if (_randomIndex == 0)
            {
                int count = 0;
                do
                {
                    for (int i = 0; i < len - 1; i++)
                    {
                        int j = Random.Range(i, len);
                        if (j != i)
                        {
                            int tmp = _randomIndices[i];
                            _randomIndices[i] = _randomIndices[j];
                            _randomIndices[j] = tmp;
                        }
                    }
                } while (_prevValue == _randomIndices[0] && ++count < 10); // Make sure the new first element is different from the last one we played
            }

            int value = _randomIndices[_randomIndex];
            if (++_randomIndex >= _randomIndices.Length)
                _randomIndex = 0;

            _prevValue = value;
            return value;
        }
    }

}