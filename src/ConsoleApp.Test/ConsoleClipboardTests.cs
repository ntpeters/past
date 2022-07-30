using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace past.ConsoleApp.Test
{
    public class ConsoleClipboardTests
    {
        #region Constructors
        [Test]
        public void Constructor_Parameterless_Success()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void Constructor_NonNullClipboardManager_Success()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void Constructor_NullClipboardManager_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion Constructors

        #region GetClipboardHistoryStatus
        [Test]
        public void GetClipboardHistoryStatus_ValidParameters_WritesStatusAndSetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsHistoryEnabledThrowsWithQuietFalse_WritesErrorAndSetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsRoamingEnabledThrowsWithQuietFalse_SetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsHistoryEnabledThrowsWithQuietTrue_SetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsRoamingEnabledThrowsWithQuietTrue_WritesErrorAndSetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_NullInvocationContext_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_NullConsole_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion GetClipboardHistoryStatus

        #region GetCurrentClipboardValueAsync
        // TODO: Add tests for ansi and ansiResetType
        [Test]
        public void GetCurrentClipboardValueAsync_ValidParameters_WritesExpectedItemAndReturnsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        public void GetCurrentClipboardValueAsync_EmptyClipboardValue_WritesEmptyValueAndReturnsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        public void GetCurrentClipboardValueAsync_GetItemThrows_WritesErrorAndReturnsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        public void GetCurrentClipboardValueAsync_NullConsole_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion GetCurrentClipboardValueAsync

        #region GetClipboardHistoryItemAsync
        // TODO: Add remaining test stubs for GetClipboardHistoryItemAsync
        [Test]
        public void GetClipboardHistoryItemAsync_AnsiTrue_EnablesVirtualTerminalProcessing()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryItemAsync_FailToEnableVirtualTerminalProcessing_WritesErrorAndExpectedItem()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion GetClipboardHistoryItemAsync

        #region ListClipboardHistoryAsync
        // TODO: Add test stubs for ListClipboardHistoryAsync
        #endregion ListClipboardHistoryAsync
    }
}
