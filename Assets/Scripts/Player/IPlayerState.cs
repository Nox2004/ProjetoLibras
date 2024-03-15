public interface IPlayerState
{
    void EnterState(PlayerController me);
    void UpdateState(PlayerController me);
    void ExitState(PlayerController me);
}