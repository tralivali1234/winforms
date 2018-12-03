using Xunit;

namespace System.Windows.Forms.Func.Tests
{
    public class AddNewTests
    {

        public const string PathToTestFromBin = "AddNew\\Debug\\netcoreapp3.0\\AddNew.exe";


        [Fact]
        public void AddNew_OpenAndClose()
        {
            var process = TestHelpers.StartProcess(PathToTestFromBin);
        }
    }
}
