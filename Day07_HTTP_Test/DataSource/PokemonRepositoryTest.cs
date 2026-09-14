using Day07_HTTP;
//using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Day07_HTTP.obj;
using Moq;
using Xunit;

namespace Day07_HTTP_Test.DataSource;

//[TestFixture]
//[TestOf(typeof(PokemonRepository))]


public class PokemonRepositoryTests
{
    public Mock<IPokemonApiDataSource<Pokemon>> MockDataSource { get; }
    private readonly PokemonRepository _repository;

    public PokemonRepositoryTests()
    {
        // 1. Arrange: IPokemonApiDataSource의 Mock 객체 생성 및 Repository 주입
        MockDataSource = new Mock<IPokemonApiDataSource<Pokemon>>();
        _repository = new PokemonRepository(MockDataSource.Object);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenPokemonExists_ReturnsPokemon()
    {
        // Arrange (준비): Mock 데이터 생성 (피카츄 데이터)
        var fakePikachu = new Pokemon
        {
            Id = 25,
            Name = "pikachu",
            Types = new List<TypeSlot>
            {
                new TypeSlot { Type = new NamedApiResource { Name = "electric" } }
            }
        };

        var fakeResponse = new Response<Pokemon>(
            statusCode: 200,
            headers: new Dictionary<string, string>(),
            body: fakePikachu
        );

        // Mock 설정: GetByNameAsync("pikachu") 호출 시 fakeResponse 반환하도록 지정
        MockDataSource
            .Setup(ds => ds.GetByNameAsync("pikachu"))
            .ReturnsAsync(fakeResponse);

        // Act (실행)
        var result = await _repository.GetPokemonByNameAsync("pikachu");

        // Assert (검증: xUnit 기본 Assert 사용)
        Assert.NotNull(result);
        Assert.Equal(25, result.Id);
        Assert.Equal("pikachu", result.Name);
        
        Assert.NotNull(result.Types);
        Assert.NotEmpty(result.Types);
        Assert.Contains(result.Types, t => t.Type != null && t.Type.Name == "electric");

        // Verification: Mock 메서드가 정확히 1번 호출되었는지 검증
        MockDataSource.Verify(ds => ds.GetByNameAsync("pikachu"), Times.Once);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenApiFails_ReturnsNull()
    {
        // Arrange: API가 404 에러나 Null Body를 반환하는 상황 가공
        var fakeErrorResponse = new Response<Pokemon>(
            statusCode: 404,
            headers: new Dictionary<string, string>(),
            body: null!
        );

        MockDataSource
            .Setup(ds => ds.GetByNameAsync("unknown_pokemon"))
            .ReturnsAsync(fakeErrorResponse);

        // Act
        var result = await _repository.GetPokemonByNameAsync("unknown_pokemon");

        // Assert
        Assert.Null(result);
        MockDataSource.Verify(ds => ds.GetByNameAsync("unknown_pokemon"), Times.Once);
    }
}