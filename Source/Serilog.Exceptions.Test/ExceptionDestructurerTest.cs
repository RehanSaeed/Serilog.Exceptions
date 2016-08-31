namespace Serilog.Exceptions.Test
{
    using Destructurers;
    using System;
    using Xunit;

    public class ExceptionDestructurerTest
    {
        [Fact]
        public void TargetTypes()
        {
            // arrange
            var destructurer = new ExceptionDestructurer();

            // act
            var targetTypes = destructurer.TargetTypes;

            // assert
            if (Type.GetType("System.Diagnostics.Eventing.Reader.EventLogInvalidDataException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") != null)
            {
                // full .net
                Assert.Equal(125, targetTypes.Length);
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogNotFoundException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogReadingException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstanceNotFoundException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationBaseException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationException");
            }
            else
            {
                // mono
                Assert.Equal(118, targetTypes.Length);
            }

        }
    }
}
