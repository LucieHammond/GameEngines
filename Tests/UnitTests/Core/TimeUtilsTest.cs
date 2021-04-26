using GameEngine.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace GameEnginesTest.UnitTests.Core
{
    [TestClass]
    public class TimeUtilsTest
    {
        [TestMethod]
        public void ToTimestampTest()
        {
            // The timestamp of Unix Epoch is zero
            Assert.AreEqual(0, TimeUtils.UnixEpoch.ToTimestamp());

            // The timestamp of the 1st January 2000 is (30 x 365 + 7) x 24 x 3600 = 946684800
            DateTime beginYear2000 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(946684800, beginYear2000.ToTimestamp());

            // A later time correspond to a greater timestamp, while an earlier time correspond to a lower timestamp
            DateTime datetime = new DateTime(20546548121548);
            Assert.IsTrue(datetime.ToTimestamp() < datetime.AddSeconds(1).ToTimestamp());
            Assert.IsTrue(datetime.ToTimestamp() > datetime.AddSeconds(-1).ToTimestamp());

            // The timestamp of a date before Unix Epoch is negative
            DateTime dateBeforeEpoch = new DateTime(1968, 5, 13);
            Assert.IsTrue(0 > dateBeforeEpoch.ToTimestamp());
        }

        [TestMethod]
        public void ToDateTimeTest()
        {
            // The datetime corresponding to a timestamp of 0 is Unix Epoch
            Assert.AreEqual(TimeUtils.UnixEpoch, TimeUtils.ToDateTime(0));

            // The datetime corresponding to a timestamp of 31536000 (365 x 24 x 3600) is the 1st january 1971
            DateTime beginYear1971 = new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(beginYear1971, TimeUtils.ToDateTime(31536000));

            // A greater timestamp correspond to a later time, while a lower timestamp correspond to an earlier time
            double timestamp = 21648798;
            Assert.IsTrue(DateTime.Compare(TimeUtils.ToDateTime(timestamp), TimeUtils.ToDateTime(timestamp + 1)) < 0);
            Assert.IsTrue(DateTime.Compare(TimeUtils.ToDateTime(timestamp), TimeUtils.ToDateTime(timestamp - 1)) > 0);

            // The datetime corresponding to the current timestamp is DateTime.Now (approximately)
            double currentTimestamp = TimeUtils.CurrentTimestamp();
            Assert.IsTrue((TimeUtils.ToDateTime(currentTimestamp) - DateTime.Now) < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void FormatTimeSpanTest()
        {
            double seconds = 3 * 3600 + 18 * 60 + 9;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            // With a standard TimeSpan format (constant format "c")
            Assert.AreEqual("03:18:09", TimeUtils.FormatTimeSpan(seconds, "c", culture));

            // With a custom TimeSpan format
            string format = "%h'h '%m'min '%s'sec'";
            Assert.AreEqual("3h 18min 9sec", TimeUtils.FormatTimeSpan(seconds, format));
        }

        [TestMethod]
        public void FormatDateTimeTest()
        {
            double timestamp = 773282160; // 4th July 1994 00:36
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            // With a standard DateTime format (constant format "g")
            Assert.AreEqual("7/4/1994 12:36 AM", TimeUtils.FormatDateTime(timestamp, "g", culture));

            // With a custom DateTime format
            string format = "dddd d MMMM yyyy, 'at' HH:mm";
            Assert.AreEqual("Monday 4 July 1994, at 00:36", TimeUtils.FormatDateTime(timestamp, format, culture));
        }
    }
}
