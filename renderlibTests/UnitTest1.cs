using Microsoft.VisualStudio.TestTools.UnitTesting;
using RenderLib;

namespace renderlibTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RenderParameters_modLessThanPoint5_TileCountExpected()
        {
            var sut = new RenderParameters();
            sut.nx = 300;
            sut.ny = 300;
            sut.tileSize = 16;
            Assert.AreEqual(19, sut.HorizTileCount);
            Assert.AreEqual(19, sut.VertTileCount);
            Assert.AreEqual(361, sut.TileCount);
        }

        [TestMethod]
        public void RenderParameters_1080p_Expected()
        {
            var sut = new RenderParameters();
            sut.nx = 1920;
            sut.ny = 1080;
            sut.tileSize = 64;
            Assert.AreEqual(30, sut.HorizTileCount);
            Assert.AreEqual(17, sut.VertTileCount);
            Assert.AreEqual(510, sut.TileCount);
        }

        [TestMethod]
        public void RenderParameters_row2col_TileDetailsExpected()
        {
            var sut = new RenderParameters();
            sut.nx = 300;
            sut.ny = 300;
            sut.tileSize = 16;

            var tileDetails = sut.GetTileDetails(19);

            Assert.AreEqual(0, tileDetails.minx);
            Assert.AreEqual(15, tileDetails.maxx);
            Assert.AreEqual(16, tileDetails.miny);
            Assert.AreEqual(31, tileDetails.maxy);
        }

        [TestMethod]
        public void RenderParameters_row1col19_TileDetailsExpected()
        {
            var sut = new RenderParameters();
            sut.nx = 300;
            sut.ny = 300;
            sut.tileSize = 16;

            var tileDetails = sut.GetTileDetails(18);

            Assert.AreEqual(288, tileDetails.minx);
            Assert.AreEqual(299, tileDetails.maxx);
            Assert.AreEqual(0, tileDetails.miny);
            Assert.AreEqual(15, tileDetails.maxy);
        }

        [TestMethod]
        public void RenderParameters_row19col19_TileDetailsExpected()
        {
            var sut = new RenderParameters();
            sut.nx = 300;
            sut.ny = 300;
            sut.tileSize = 16;

            var tileDetails = sut.GetTileDetails(19*19-1);

            Assert.AreEqual(288, tileDetails.minx);
            Assert.AreEqual(299, tileDetails.maxx);
            Assert.AreEqual(288, tileDetails.miny);
            Assert.AreEqual(299, tileDetails.maxy);
        }

    }
}
