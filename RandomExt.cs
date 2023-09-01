using System;

namespace CompleteChaos;

public class RandomExt : Random
{
    private uint _boolBits;

    public RandomExt()
    { }
    public RandomExt(int seed) : base(seed) { }

    public bool NextBoolean()
    {
        _boolBits >>= 1;
        if (_boolBits <= 1) _boolBits = (uint)~Next();
        return (_boolBits & 1) == 0;
    }
    
    public T NextEnum<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Next(values.Length));
    }
}
