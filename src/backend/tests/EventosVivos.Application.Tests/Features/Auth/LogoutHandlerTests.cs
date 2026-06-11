using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Auth.Logout;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Auth;

public class LogoutHandlerTests
{
    [Fact]
    public async Task Removes_the_session()
    {
        var sessions = Substitute.For<ISessionStore>();
        var handler = new LogoutHandler(sessions);
        var sessionId = Guid.CreateVersion7();

        var result = await handler.Handle(new LogoutCommand(sessionId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await sessions.Received(1).RemoveAsync(sessionId, Arg.Any<CancellationToken>());
    }
}
