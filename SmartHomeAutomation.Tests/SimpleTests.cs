using Xunit;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void PaginationDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var pagination = new PaginationDto();

            // Assert
            Assert.Equal(1, pagination.Page);
            Assert.Equal(10, pagination.PageSize);
            Assert.Equal("asc", pagination.SortOrder);
            Assert.Null(pagination.Search);
            Assert.Null(pagination.SortBy);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 20)]
        public void PaginationDto_CustomValues_ShouldSetCorrectly(int page, int pageSize)
        {
            // Arrange & Act
            var pagination = new PaginationDto
            {
                Page = page,
                PageSize = pageSize
            };

            // Assert
            Assert.Equal(page, pagination.Page);
            Assert.Equal(pageSize, pagination.PageSize);
        }

        [Fact]
        public void PagedResult_TotalPages_ShouldCalculateCorrectly()
        {
            // Arrange
            var result = new PagedResult<string>
            {
                TotalCount = 25,
                PageSize = 10,
                Page = 1
            };

            // Act & Assert
            Assert.Equal(3, result.TotalPages); // 25/10 = 2.5 -> 3
            Assert.True(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }

        [Fact]
        public void PagedResult_HasNextPage_ShouldBeCorrect()
        {
            // Arrange
            var result1 = new PagedResult<string>
            {
                TotalCount = 25,
                PageSize = 10,
                Page = 2 // 2 of 3 pages
            };

            var result2 = new PagedResult<string>
            {
                TotalCount = 25,
                PageSize = 10,
                Page = 3 // Last page
            };

            // Act & Assert
            Assert.True(result1.HasNextPage);
            Assert.False(result2.HasNextPage);
        }
    }
} 