// <copyright file="MultiplicationTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework1.Tests
{
    [TestFixture]
    public class MultiplicationTests
    {
        [Test]
        public void SequentialMultiplication_SmallMatrices_ReturnsCorrectResult()
        {
            var a = new[,] { { 1, 2 }, { 3, 4 } };
            var b = new[,] { { 2, 0 }, { 1, 2 } };
            var expected = new[,] { { 4, 4 }, { 10, 8 } };

            var result = MatrixMultiplier.SequentialMultiplication(a, b);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SequentialMultiplication_RectangularMatrices_ReturnsCorrectResult()
        {
            var a = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            var b = new[,] { { 7, 8 }, { 9, 10 }, { 11, 12 } };
            var expected = new[,] { { 58, 64 }, { 139, 154 } };

            var result = MatrixMultiplier.SequentialMultiplication(a, b);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SequentialMultiplication_IdentityMatrix_ReturnsOriginalMatrix()
        {
            var original = new[,] { { 1, 2 }, { 3, 4 } };
            var identity = new[,] { { 1, 0 }, { 0, 1 } };

            var result = MatrixMultiplier.SequentialMultiplication(original, identity);

            Assert.That(result, Is.EqualTo(original));
        }

        [Test]
        [TestCase(2, 3, 4)]
        [TestCase(1, 5, 3)]
        [TestCase(10, 2, 8)]
        public void SequentialMultiplication_ReturnsMatrixWithCorrectDimensions(int rowsA, int colsA, int colsB)
        {
            var a = MatrixMaker.Generate(rowsA, colsA);
            var b = MatrixMaker.Generate(colsA, colsB);

            var result = MatrixMultiplier.SequentialMultiplication(a, b);

            Assert.That(result.GetLength(0), Is.EqualTo(rowsA));
            Assert.That(result.GetLength(1), Is.EqualTo(colsB));
        }

        [Test]
        public void SequentialMultiplication_ZeroMatrices_ReturnsZeroMatrix()
        {
            var a = new[,] { { 0, 0 }, { 0, 0 } };
            var b = new[,] { { 1, 2 }, { 3, 4 } };
            var expected = new[,] { { 0, 0 }, { 0, 0 } };

            var result = MatrixMultiplier.SequentialMultiplication(a, b);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SequentialMultiplication_LargeMatrices_DoesNotThrow()
        {
            var a = MatrixMaker.Generate(50, 50);
            var b = MatrixMaker.Generate(50, 50);

            Assert.That(() => MatrixMultiplier.SequentialMultiplication(a, b), Throws.Nothing);
        }

        [Test]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        public void SequentialAndParallelMultiplication_ProduceIdenticalResults(int size)
        {
            var a = MatrixMaker.Generate(size, size);
            var b = MatrixMaker.Generate(size, size);

            var sequentialResult = MatrixMultiplier.SequentialMultiplication(a, b);
            var parallelResult = MatrixMultiplier.ParallelMultiplication(a, b, 4);

            Assert.That(parallelResult, Is.EqualTo(sequentialResult));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void ParallelMultiplication_WithDifferentThreadCounts_ProducesCorrectResult(int threadCount)
        {
            var a = new[,] { { 1, 2 }, { 3, 4 } };
            var b = new[,] { { 2, 0 }, { 1, 2 } };
            var expected = new[,] { { 4, 4 }, { 10, 8 } };

            var result = MatrixMultiplier.ParallelMultiplication(a, b, threadCount);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ParallelMultiplication_WithThreadCountLessThanOrEqualToOne_UsesSequential()
        {
            var a = new[,] { { 1, 2 }, { 3, 4 } };
            var b = new[,] { { 2, 0 }, { 1, 2 } };
            var expected = new[,] { { 4, 4 }, { 10, 8 } };

            var result1 = MatrixMultiplier.ParallelMultiplication(a, b, 0);
            var result2 = MatrixMultiplier.ParallelMultiplication(a, b, 1);

            Assert.That(result1, Is.EqualTo(expected));
            Assert.That(result2, Is.EqualTo(expected));
        }

        [Test]
        public void SequentialMultiplication_NullFirstMatrix_ThrowsArgumentNullException()
        {
            var b = new[,] { { 1, 2 }, { 3, 4 } };

            Assert.That(() => MatrixMultiplier.SequentialMultiplication(null!, b),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SequentialMultiplication_NullSecondMatrix_ThrowsArgumentNullException()
        {
            var a = new[,] { { 1, 2 }, { 3, 4 } };

            Assert.That(() => MatrixMultiplier.SequentialMultiplication(a, null!),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SequentialMultiplication_IncompatibleDimensions_ThrowsArgumentException()
        {
            var a = new[,] { { 1, 2, 3 } };
            var b = new[,] { { 1, 2 }, { 3, 4 } };

            var exception = Assert.Throws<ArgumentException>(() =>
                MatrixMultiplier.SequentialMultiplication(a, b));
            Assert.That(exception.Message, Does.Contain("dimensions"));
        }

        [Test]
        public void ParallelMultiplication_NullFirstMatrix_ThrowsArgumentNullException()
        {
            var b = new[,] { { 1, 2 }, { 3, 4 } };

            Assert.That(() => MatrixMultiplier.ParallelMultiplication(null!, b, 2),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ParallelMultiplication_NullSecondMatrix_ThrowsArgumentNullException()
        {
            var a = new[,] { { 1, 2 }, { 3, 4 } };

            Assert.That(() => MatrixMultiplier.ParallelMultiplication(a, null!, 2),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ParallelMultiplication_IncompatibleDimensions_ThrowsArgumentException()
        {
            var a = new[,] { { 1, 2, 3 } };
            var b = new[,] { { 1, 2 }, { 3, 4 } };

            var exception = Assert.Throws<ArgumentException>(() =>
                MatrixMultiplier.ParallelMultiplication(a, b, 2));
            Assert.That(exception.Message, Does.Contain("dimensions"));
        }
    }
}