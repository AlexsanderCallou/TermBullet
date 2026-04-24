using TermBullet.Application.Items;

namespace TermBullet.Tui.Screens;

public sealed class MainDashboardActionHandler(
    MarkDoneItemUseCase markDoneItemUseCase,
    CancelItemUseCase cancelItemUseCase,
    MigrateItemUseCase migrateItemUseCase,
    DeleteItemUseCase deleteItemUseCase)
{
    public async Task<ActionResult> HandleDoneAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await markDoneItemUseCase.ExecuteAsync(publicRef, cancellationToken);
            return ActionResult.Ok();
        }
        catch (Exception ex)
        {
            return ActionResult.Fail(ex.Message);
        }
    }

    public async Task<ActionResult> HandleCancelAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await cancelItemUseCase.ExecuteAsync(publicRef, cancellationToken);
            return ActionResult.Ok();
        }
        catch (Exception ex)
        {
            return ActionResult.Fail(ex.Message);
        }
    }

    public async Task<ActionResult> HandleMigrateAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await migrateItemUseCase.ExecuteAsync(publicRef, cancellationToken);
            return ActionResult.Ok();
        }
        catch (Exception ex)
        {
            return ActionResult.Fail(ex.Message);
        }
    }

    public async Task<ActionResult> HandleDeleteAsync(
        string publicRef,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await deleteItemUseCase.ExecuteAsync(publicRef, cancellationToken);
            return ActionResult.Ok();
        }
        catch (Exception ex)
        {
            return ActionResult.Fail(ex.Message);
        }
    }
}
