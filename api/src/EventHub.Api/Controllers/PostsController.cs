using EventHub.Domain.ReadModels;
using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Social;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public PostsController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    public sealed record CreatePostRequest(string ImageUrl, string Caption, Guid? ActivityId);
    public sealed record AddCommentRequest(string Text);

    /// <summary>Fil communautaire (GET /api/posts).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostDto>>> Feed(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetFeedQuery(), cancellationToken));

    /// <summary>Détail d'une publication (GET /api/posts/{id}).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var post = await _sender.Send(new GetPostByIdQuery(id), cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    /// <summary>Publier une photo (POST /api/posts).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostRequest body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(body.ImageUrl) || string.IsNullOrWhiteSpace(body.Caption))
            return BadRequest(new { error = "imageUrl et caption requis" });

        var id = await _sender.Send(
            new CreatePostCommand(userId.Value, body.ImageUrl, body.Caption, body.ActivityId),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Supprimer sa publication (DELETE /api/posts/{id}).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var status = await _sender.Send(new DeletePostCommand(userId.Value, id), cancellationToken);
        return status switch
        {
            DeletePostStatus.Deleted => NoContent(),
            DeletePostStatus.NotFound => NotFound(),
            // 403 explicite (Forbid() nécessiterait un schéma d'auth actif).
            DeletePostStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Aimer une publication (POST /api/posts/{id}/like).</summary>
    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> Like(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var result = await _sender.Send(new LikePostCommand(userId.Value, id), cancellationToken);
        return result.Status == LikeResultStatus.PostNotFound
            ? NotFound()
            : Ok(new { likesCount = result.LikesCount });
    }

    /// <summary>Retirer son « j'aime » (DELETE /api/posts/{id}/like).</summary>
    [HttpDelete("{id:guid}/like")]
    public async Task<IActionResult> Unlike(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var result = await _sender.Send(new UnlikePostCommand(userId.Value, id), cancellationToken);
        return result.Status == LikeResultStatus.PostNotFound
            ? NotFound()
            : Ok(new { likesCount = result.LikesCount });
    }

    /// <summary>Commenter (POST /api/posts/{id}/comments).</summary>
    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> Comment(
        Guid id, [FromBody] AddCommentRequest body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(body.Text))
            return BadRequest(new { error = "text requis" });

        var result = await _sender.Send(
            new AddCommentCommand(userId.Value, id, body.Text), cancellationToken);
        return result.Status == AddCommentStatus.PostNotFound
            ? NotFound()
            : Ok(new { commentId = result.CommentId });
    }
}
