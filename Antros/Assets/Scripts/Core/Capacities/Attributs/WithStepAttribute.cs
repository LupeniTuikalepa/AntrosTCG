using System;

namespace ATCG.Capacities.Attributs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WithStepAttribute : Attribute
    {
        public string StepName { get; }

        public WithStepAttribute(string stepName)
        {
            StepName = stepName;
        }

    }
}