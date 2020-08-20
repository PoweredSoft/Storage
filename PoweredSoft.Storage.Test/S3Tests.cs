using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoweredSoft.Storage.S3;

namespace PoweredSoft.Storage.Test
{
    [TestClass]
    public class S3Tests
    {
        [TestMethod]
        public void NameValidation()
        {
            var space = GetMockS3Space();

            Assert.IsFalse(space.IsFileNameAllowed("Operations .pdf"), "Should not be valid");
            Assert.IsFalse(space.IsFileNameAllowed("Operations$$.pdf"), "Should not be valid");
        }

        [TestMethod]
        public void CanContainDash()
        {
            var space = GetMockS3Space();
            Assert.IsTrue(space.IsFileNameAllowed("Operations-yay.pdf"), "Should be allowed");
        }

        [TestMethod]
        public void NameSanitation()
        {
            var space = GetMockS3Space();

            Assert.AreEqual("Operations_.pdf", space.SanitizeFileName("Operations .pdf", "_"), "does not match sanitation expectations");
            Assert.AreEqual("Operations__.pdf", space.SanitizeFileName("Operations$$.pdf", "_"), "does not match sanitation expectations");
        }

        private static S3StorageProvider GetMockS3Space()
        {
            var space = new S3StorageProvider("http://localhost:9000", "mybucket", "myminio", "myexample");
            space.SetForcePathStyle(true);
            space.SetS3UsEast1RegionalEndpointValue(Amazon.Runtime.S3UsEast1RegionalEndpointValue.Legacy);
            return space;
        }
    }
}
