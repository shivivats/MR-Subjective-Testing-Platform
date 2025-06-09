using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

public class WeightCounter
{
    private List<float> _weightSummed;
    private int _count = 0;
    public int Count { get => _count; }
    public void Add(List<float> weight)
    {
        if (_weightSummed != null && _weightSummed.Count != weight.Count)
        {
            throw new Exception("_weightSummed and weight must have save count");
        }

        // first time
        if (_weightSummed == null)
        {
            _weightSummed = new List<float>();
            for (int i = 0; i < weight.Count; i++)
            {
                _weightSummed.Add(0f);
            }
        }

        float currSum = 0;
        for (int i = 0; i < _weightSummed.Count; i++)
        {
            _weightSummed[i] += weight[i];
            currSum += weight[i];
        }

        // if all the elements in weight are zero, do not count++
        if (currSum > 0)
        {
            _count++;
        }
    }

    public List<float> Sum()
    {
        return _weightSummed;
    }

    public List<float> Average()
    {
        if (_weightSummed != null && _count != 0)
            for (int i = 0; i < _weightSummed.Count; i++)
                _weightSummed[i] /= _count;
        return _weightSummed;
    }

}
