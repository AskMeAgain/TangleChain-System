using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace TangleChainIXITest
{
    /// <summary>
    /// A simple ExpectedExceptionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExpectedExceptionAttribute : NUnitAttribute, IWrapTestMethod
    {
        private readonly Type _expectedExceptionType;
        private readonly string _message;

        public ExpectedExceptionAttribute(Type type, string message)
        {
            _expectedExceptionType = type;
            _message = message;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ExpectedExceptionCommand(command, _expectedExceptionType, _message);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand
        {
            private readonly Type _expectedType;
            private readonly string _message;

            public ExpectedExceptionCommand(TestCommand innerCommand, Type expectedType, string message)
                : base(innerCommand)
            {
                _expectedType = expectedType;
                _message = message;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                Type caughtType = null;
                string message = "";

                try
                {
                    innerCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    if (ex is NUnitException)
                        ex = ex.InnerException;
                    caughtType = ex.GetType();
                    message = ex.Message;
                }

                if (caughtType == _expectedType && _message.Equals(message))
                    context.CurrentResult.SetResult(ResultState.Success);
                else if (caughtType != null && caughtType == _expectedType)
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected Message {0} but got {1}", _message, message));
                else if (caughtType != null && caughtType != _expectedType)
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected Message {0} but got {1}", _expectedType.Name, caughtType.Name));
                else
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected {0} but no exception was thrown", _expectedType.Name));

                return context.CurrentResult;
            }
        }
    }
}
