using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace StrainTest
{
    /// <summary>
    /// A simple ExpectedExceptionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExpectedNoExceptionAttribute : NUnitAttribute, IWrapTestMethod
    {
        public ExpectedNoExceptionAttribute()
        {
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ExpectedExceptionCommand(command);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand
        {

            public ExpectedExceptionCommand(TestCommand innerCommand)
                : base(innerCommand)
            {

            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var flag = true;
                var message = "";
                try
                {
                    innerCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    if (ex is NUnitException)
                        ex = ex.InnerException;
                    flag = false;
                    message = ex.Message;
                }

                if (flag)
                    context.CurrentResult.SetResult(ResultState.Success);
                else
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected No Exception But Exception was thrown: " + message));

                return context.CurrentResult;
            }
        }
    }
}
