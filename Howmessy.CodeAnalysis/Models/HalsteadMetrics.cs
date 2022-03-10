namespace Howmessy.CodeAnalysis.Models;

using System;

internal sealed class HalsteadMetrics
{
    public HalsteadMetrics(int numOperands, int numOperators, int numUniqueOperands, int numUniqueOperators)
    {
        NumberOfOperands = numOperands;
        NumberOfOperators = numOperators;
        NumberOfUniqueOperands = numUniqueOperands;
        NumberOfUniqueOperators = numUniqueOperators;
    }

    public int NumberOfOperands { get; }

    public int NumberOfOperators { get; }

    public int NumberOfUniqueOperands { get; }

    public int NumberOfUniqueOperators { get; }

    public int Bugs => (int)(Volume / 3000);

    public double Difficulty => NumberOfUniqueOperands == 0
            ? 0
            : (NumberOfUniqueOperators / 2.0 * (NumberOfOperands / (double)NumberOfUniqueOperands));

    public TimeSpan Effort => TimeSpan.FromSeconds(Difficulty * Volume / 18.0);

    public int Length => NumberOfOperators + NumberOfOperands;

    public int Vocabulary => NumberOfUniqueOperators + NumberOfUniqueOperands;

    public double Volume => Vocabulary == 0 ? 0.0 : Length * Math.Log(Vocabulary, 2.0);
}
